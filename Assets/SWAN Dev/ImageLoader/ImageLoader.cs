/// <summary>
/// By SwanDEV 2018
/// </summary>

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using UnityEngine.Networking;

namespace IMBX
{
    /// <summary>
    /// Load image from web or local, with cache, retry and timeout options.
    /// </summary>
    public class ImageLoader : MonoBehaviour
    {
        public enum CacheMode
        {
            /// <summary>Do not save the download image to the local cache directory. And, do not check & load the cached image from local.</summary>
            NoCache = 0,

            /// <summary>Check & load the cached image from local cache directory. Save the download image to the local cache directory if no same filename image exists.</summary>
            UseCached,

            /// <summary>Download and save the image to the local cache directory without checking cached images. Replace the local image if same filename.</summary>
            Replace,
        }

        public string DetectedFileMime { get; private set; }

        public string DetecedFileExtension { get; private set; }

        private uint index = 0;
        private float timeOut = 10f;
        private Action<Texture2D, uint> onComplete = null;

        /// <summary>
        /// The loader Cache Management and Loading Settings.
        /// </summary>
        public LoaderManagement LMGT = new LoaderManagement();

        /// <summary>
        /// Create a GameObject to setup an ImageLoader for loading image from web or local. 
        /// This GameObject and ImageLoader will be automatically destroyed when finished.
        /// </summary>
        /// <param name="name"> The GameObject name of this ImageLoader. </param>
        /// <param name="cacheDirectory"> The application path, default is Application.persistentDataPath. </param>
        /// <param name="maxCacheFilePerFolder"> Number of file allow being cached in a folder. </param>
        /// <returns></returns>
        public static ImageLoader Create(uint maxCacheFilePerFolder = 0, FilePathName.AppPath cacheDirectoryEnum = FilePathName.AppPath.PersistentDataPath, string name = "")
        {
            ImageLoader loader = new GameObject(string.IsNullOrEmpty(name) ? "[ImageLoader]" : name).AddComponent<ImageLoader>();
            loader.LMGT.CacheDirectoryEnum = cacheDirectoryEnum;
            loader.LMGT.MaxCacheFilePerFolder = maxCacheFilePerFolder;
            return loader;
        }
        public static ImageLoader Create()
        {
            ImageLoader loader = new GameObject("[ImageLoader]").AddComponent<ImageLoader>();
            return loader;
        }

        /// <summary>
        /// Start to load the image with full loader management features, return the image as Texture2D with the onComplete callback.
        /// </summary>
        /// <param name="index"> A number specified by you. Can be used to indicate the identity or purpose of this Imageloader. It will be returned with the onComplete callback when finished. </param>
        /// <param name="url"> The url or local path of the image. </param>
        /// <param name="LM"> The loader Cache Management and Loading Settings. </param>
        /// <param name="onComplete"> The callback for returning the loaded Texture2D and Index. </param>
        public void Load(uint index, string url, LoaderManagement LM, Action<Texture2D, uint> onComplete)
        {
            LMGT = new LoaderManagement(LM);
            Load(index, url, LM.GenerateFileName((int)index), LM.FolderName, LM.CacheMode, onComplete, LM.LoadingRetry, LM.LoadingTimeOut);
        }

        /// <summary>
        /// Start to load the image, and return it as Texture2D with the onComplete callback.
        /// </summary>
        /// <param name="index"> A number specified by you. Can be used to indicate the identity or purpose of this Imageloader. It will be returned with the onComplete callback when finished. </param>
        /// <param name="url"> The url or local path of the image. </param>
        /// <param name="retry"> How many time to retry if fail to load the file. </param>
        /// <param name="timeOut"> The maximum duration in seconds for waiting for a load. </param>
        /// <param name="onComplete"> The callback for returning the loaded Texture2D and Index. </param>
        public void Load(uint index, string url, Action<Texture2D, uint> onComplete, uint retry = 0, float timeOut = 10f)
        {
            Load(index, url, "", "", CacheMode.NoCache, onComplete, retry, timeOut);
        }

        /// <summary>
        /// Start to load the image with cache feature, and return it as Texture2D with the onComplete callback.
        /// </summary>
        /// <param name="index"> A number specified by you. Can be used to indicate the identity or purpose of this Imageloader. It will be returned with the onComplete callback when finished. </param>
        /// <param name="url"> The url or local path of the image. </param>
        /// <param name="fileName"> The filename for storing(caching) the image file. </param>
        /// <param name="folderName"> The target folder for storing(caching) the image file. </param>
        /// <param name="cacheMode"> The behavior for handling Load and Cache files. (NoCache: do not auto save the image; UseCached: use the locally cached file if exist; Replace: download and replace the locally cached file is exist)  </param>
        /// <param name="retry"> How many time to retry if fail to load the file. </param>
        /// <param name="timeOut"> The maximum duration in seconds for waiting for a load. </param>
        /// <param name="onComplete"> The callback for returning the loaded Texture2D and Index. </param>
        public void Load(uint index, string url, string fileName, string folderName, CacheMode cacheMode, Action<Texture2D, uint> onComplete = null, uint retry = 0, float timeOut = 10f)
        {
            LMGT.FolderName = folderName;
            LMGT.CacheMode = cacheMode;
            LMGT.LoadingRetry = retry;
            LMGT.LoadingTimeOut = timeOut;

            this.name += " - " + index;
            this.timeOut = timeOut;
            this.index = index;
            this.onComplete = onComplete;
            
            string fullFolderPath = "";
            if (cacheMode != CacheMode.NoCache)
            {
                fullFolderPath = Path.Combine(LMGT.CacheDirectory, folderName);

                if (string.IsNullOrEmpty(fileName))
                {
                    Debug.LogWarning("Trying to load image with cache feature enabled, but without specifying the filename!");
                    if (onComplete != null) onComplete(null, index);
                    Destroy(gameObject);
                    return;
                }
                fileName = fileName + LMGT.FileExtension;
#if UNITY_EDITOR
                Debug.Log("fullFolderPath: " + fullFolderPath);
#endif
            }

            _LoadFile(index, url, fileName, fullFolderPath, cacheMode, retry, onComplete);
        }
        
        private void _LoadFile(uint index, string url, string fileName, string fullFolderPath, CacheMode cacheMode, uint retry = 0, Action<Texture2D, uint> onComplete = null)
        {
            string fullFilePath = Path.Combine(fullFolderPath, fileName);
            if (cacheMode != CacheMode.NoCache && !Directory.Exists(fullFolderPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }

            bool isLoadCache = false;
            bool supportSystemIO = true;
            if (fullFolderPath.Contains(Application.streamingAssetsPath) && _IsAndroidOrWebGL)
            {
                //Debug.LogWarning("Un-Support SystemIO for saving and loading files on current platform!");
                supportSystemIO = false;
                isLoadCache = true;

                StartCoroutine(_LoadRoutine(index, fullFilePath, fileName, fullFolderPath, fullFilePath, true, cacheMode, retry, (texture, idx) => {
                    if (texture == null)
                    {
                        StartCoroutine(_LoadRoutine(index, url, fileName, fullFolderPath, fullFilePath, false, cacheMode, retry, onComplete));
                    }
                    else
                    {
                        onComplete(_GetImageByPath(fullFilePath), index);
                        Destroy(gameObject);
                    }
                }));
            }

            if (cacheMode == CacheMode.UseCached && supportSystemIO)
            {
                // Use cached file if exist (file with same fileName as requested).
                if (File.Exists(fullFilePath))
                {
                    if (onComplete != null)
                    {
                        onComplete(_GetImageByPath(fullFilePath), index);
                        Destroy(gameObject);
                        isLoadCache = true;
                    }
                }
            }

            if (!isLoadCache)
            {
                StartCoroutine(_LoadRoutine(index, url, fileName, fullFolderPath, fullFilePath, false, cacheMode, retry, onComplete));
            }
        }

        private IEnumerator _LoadRoutine(uint index, string url, string fileName, string fullFolderPath, string fileSavePath, bool isLoadLocal, CacheMode cacheMode, uint retry = 0, Action<Texture2D, uint> onComplete = null)
        {
            if (!string.IsNullOrEmpty(url))
            {
                url = FilePathName.Instance.EnsureValidPath(url);

                bool isSameFolder = false;
                if (url.ToLower().Contains(fullFolderPath.ToLower()))
                {
                    //Debug.LogWarning("The loading & caching folder is the same!");
                    isSameFolder = true;
                }

#if UNITY_4 || UNITY_5 //|| UNITY_2017
                if (timeOut > 0) Invoke("_StopDownload", timeOut);
                WWW www = new WWW(url);
                yield return www;
                if (timeOut > 0) CancelInvoke("_StopDownload");

                if (www.error == null)
                {
                    FileMimeAndExtension fme = new FileMimeAndExtension();
                    byte[] data = www.bytes;
                    string mime = "";
                    string extensionName = "";
                    fme.GetFileMimeAndExtension(data, ref mime, ref extensionName);
                    DetectedFileMime = mime;
                    DetecedFileExtension = extensionName;

                    if (cacheMode != CacheMode.NoCache)
                    {
                        if (!isLoadLocal && !isSameFolder)
                        {
                            File.WriteAllBytes(fileSavePath, data);
                        }
                        _DeleteOldestFiles(fullFolderPath, LMGT.MaxCacheFilePerFolder, LMGT.MinTimeForKeepingFiles, LMGT.MaxTimeForKeepingFiles);
                    }
                    if (onComplete != null) onComplete(www.texture, index);
                    Destroy(gameObject);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Failed to download file! Retry: " + retry);
#endif
                    if (retry > 0)
                    {
                        yield return new WaitForSeconds(1);
                        retry--;
                        StartCoroutine(_LoadRoutine(index, url, fileName, fullFolderPath, fileSavePath, isLoadLocal, cacheMode, retry, onComplete));
                    }
                    else
                    {
                        _StopDownload();
                    }
                }
                www.Dispose();
                www = null;
#else
                using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
                {
                    if (timeOut > 0) uwr.timeout = (int)timeOut;
                    yield return uwr.SendWebRequest();

                    if (uwr.isNetworkError || uwr.isHttpError)
                    {
#if UNITY_EDITOR
                        Debug.LogWarning("Failed to download file! Retry: " + retry);
#endif
                        if (retry > 0)
                        {
                            yield return new WaitForSeconds(1);
                            retry--;
                            StartCoroutine(_LoadRoutine(index, url, fileName, fullFolderPath, fileSavePath, isLoadLocal, cacheMode, retry, onComplete));
                        }
                        else
                        {
                            _StopDownload();
                        }
                    }
                    else
                    {
                        FileMimeAndExtension fme = new FileMimeAndExtension();
                        byte[] data = uwr.downloadHandler.data;
                        string mime = "";
                        string extensionName = "";
                        fme.GetFileMimeAndExtension(data, ref mime, ref extensionName);
                        DetectedFileMime = mime;
                        DetecedFileExtension = extensionName;

                        if (cacheMode != CacheMode.NoCache)
                        {
                            if (!isLoadLocal && !isSameFolder)
                            {
                                File.WriteAllBytes(fileSavePath, data);
                            }
                            _DeleteOldestFiles(fullFolderPath, LMGT.MaxCacheFilePerFolder, LMGT.MinTimeForKeepingFiles, LMGT.MaxTimeForKeepingFiles);
                        }
                        if (onComplete != null) onComplete(DownloadHandlerTexture.GetContent(uwr), index);
                        Destroy(gameObject);
                    }
                }
#endif
            }
            else
            {
                _StopDownload();
            }
        }

        private void _DeleteOldestFiles(string fullFolderPath, uint keepFileNum, uint minKeepTime, uint maxKeepTime)
        {
            if (!Directory.Exists(fullFolderPath)) return;
            string[] allFileNames = Directory.GetFiles(fullFolderPath).Where(file => (Path.GetExtension(file).ToLower() == LMGT.FileExtension.ToLower())).ToArray();
            if (allFileNames != null && allFileNames.Length >= keepFileNum && keepFileNum > 0)
            {
                int exceedNum = allFileNames.Length - (int)keepFileNum;  // Set the maximum number of files can be deleted.

                List<FileInfo> fileInfoList = new List<FileInfo>();
                for (int i = 0; i < allFileNames.Length; i++)
                {
                    fileInfoList.Add(new FileInfo(allFileNames[i]));
                }

                fileInfoList.Sort(delegate (FileInfo a, FileInfo b) { // Sort ascending by last write time, so the oldest files in front.
                    if (a.LastWriteTime == b.LastWriteTime) return 0;
                    else if (a.LastWriteTime < b.LastWriteTime) return -1;
                    else return 1;
                });

                if (exceedNum > 0)
                {
                    for (int i = 0; i < exceedNum; i++)
                    {
                        if (i < fileInfoList.Count && fileInfoList[i] != null)
                        {
                            if (minKeepTime <= 0 || fileInfoList[i].LastWriteTime < DateTime.Now.AddSeconds(-minKeepTime)) // Ensure files do not being deleted before expire.
                            {
#if UNITY_EDITOR
                                Debug.Log(i + " - Delete: Max. Exceed=" + exceedNum + " - FileName & LastWriteTime : " + fileInfoList[i].Name + " - " + fileInfoList[i].LastWriteTime);
#endif
                                File.Delete(fileInfoList[i].FullName);
                            }
                        }
                    }
                }

                if (maxKeepTime > 0 && maxKeepTime > minKeepTime && fileInfoList != null && fileInfoList.Count > 0)
                {
                    List<FileInfo> hardExpiredFileList = fileInfoList.FindAll(delegate (FileInfo fi)
                    {
                        return fi.LastWriteTime < DateTime.Now.AddSeconds(-maxKeepTime);
                    });
                    for (int i = 0; i < hardExpiredFileList.Count; i++) // Delete hard expired files (Files those must delete)
                    {
#if UNITY_EDITOR
                        Debug.Log(i + " - Delete - FileName & LastWriteTime : " + hardExpiredFileList[i].Name + " - " + hardExpiredFileList[i].LastWriteTime);
#endif
                        File.Delete(hardExpiredFileList[i].FullName);
                    }
                }

            }
        }

        private Texture2D _GetImageByPath(string fullFilePath)
        {
            if (File.Exists(fullFilePath))
            {
                Texture2D getTexture = new Texture2D(1, 1);
                getTexture.LoadImage(File.ReadAllBytes(fullFilePath));
                return getTexture;
            }
            return null;
        }

        private void _StopDownload()
        {
            if (onComplete != null)
            {
                onComplete(null, index);
            }
            Destroy(gameObject);
        }

        private bool _IsAndroidOrWebGL
        {
            get
            {
                bool flag = false;
#if UNITY_ANDROID || UNITY_WEBGL
                flag = true;
#endif
                return flag;
            }
        }
    }

}
