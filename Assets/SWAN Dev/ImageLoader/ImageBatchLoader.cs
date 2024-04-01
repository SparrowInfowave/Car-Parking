/// <summary>
/// By SwanDEV 2018
/// </summary>

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace IMBX
{
    /// <summary>
    /// Load multiple images from web or local, and/or save to the local storage.
    /// </summary>
    public class ImageBatchLoader
    {
        public class Results
        {
            /// <summary>
            /// The dictionary for containing all loaded textures.
            /// </summary>
            public Dictionary<int, Texture2D> m_TextureDict = new Dictionary<int, Texture2D>();
            private Dictionary<int, Result> _resultDict = new Dictionary<int, Result>();

            public void SetResult(uint index, Result result)
            {
                _resultDict.Add((int)index, result);
                m_TextureDict.Add((int)index, result.m_Texture);
            }

            /// <summary>
            /// Get all textures in a list. (Reminded to check null before using the textures)
            /// </summary>
            public List<Texture2D> GetTextureList()
            {
                return m_TextureDict.Values.ToList() ;
            }
            
            /// <summary>
            /// Get particular texture by index. (Reminded to check null before using the texture)
            /// </summary>
            public Texture2D GetTexture(int index)
            {
                Texture2D texture = null;
                m_TextureDict.TryGetValue(index, out texture);
                return texture;
            }

            /// <summary>
            /// Get MIME type of a particular image by index.
            /// </summary>
            public string GetMimeType(int index)
            {
                Result result = null;
                _resultDict.TryGetValue(index, out result);
                return result.m_DetectedFileMime;
            }

            /// <summary>
            /// Get file extension name of a particular image by index.
            /// </summary>
            public string GetExtensionName(int index)
            {
                Result result = null;
                _resultDict.TryGetValue(index, out result);
                return result.m_DetectedFileExtension;
            }
        }

        public class Result
        {
            /// <summary>
            /// To avoid error at fail to load/download the image, you should Check Null before access this texture.
            /// </summary>
            public Texture2D m_Texture;

            /// <summary>
            /// The index of the image, the same as in the image URL/path list.
            /// </summary>
            public uint m_Index;

            public float m_Progress;
            public string m_DetectedFileMime;
            public string m_DetectedFileExtension;

            public Result(Texture2D texture, uint index, float progress, string mime, string extension)
            {
                m_Texture = texture;
                m_Index = index;
                m_Progress = progress;
                m_DetectedFileMime = mime;
                m_DetectedFileExtension = extension;
            }
        }
        
        /// <summary>
        /// The loading progress. 
        /// </summary>
        private float _progress = 0f;

        /// <summary>
        /// The loader Cache Management and Loading Settings.
        /// </summary>
        public LoaderManagement LMGT = new LoaderManagement();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ImageBatchLoader(uint maxCacheFilePerFolder = 0, FilePathName.AppPath cacheDirectoryEnum = FilePathName.AppPath.PersistentDataPath)
        {
            LMGT.CacheDirectoryEnum = cacheDirectoryEnum;
            LMGT.MaxCacheFilePerFolder = maxCacheFilePerFolder;
        }

        /// <summary>
        /// Load multiple images from web or local with full loader management features, return images as Texture2D with the onComplete and onProgress callback.
        /// </summary>
        /// <param name="imageUrls"> Image urls or local paths. </param>
        /// <param name="LM"> The loader Cache Management and Loading Settings. </param>
        /// <param name="onComplete"> The callback for receiving all the loaded results. </param>
        /// <param name="onProgress"> Update the loading progress and receive the latest loaded texture. </param>
        public void Load(List<string> imageUrls, LoaderManagement LM, Action<Results> onComplete, Action<Result> onProgress = null)
        {
            LMGT = new LoaderManagement(LM);
            Load(imageUrls, LM.FileNamePrefix, LM.FolderName, LM.CacheMode, onComplete, onProgress, LM.LoadingRetry, LM.LoadingTimeOut);
        }

        /// <summary>
        /// Load multiple images from web or local.
        /// </summary>
        /// <param name="imageUrls"> Image urls or local paths. </param>
        /// <param name="onComplete"> The callback for receiving all the loaded results. </param>
        /// <param name="retry"> The retry number for each image. </param>
        /// <param name="timeOut"> The timout for each image. </param>
        /// <param name="onProgress"> Update the loading progress and receive the latest loaded texture. </param>
        public void Load(List<string> imageUrls, Action<Results> onComplete, uint retry = 0, float timeOut = 10f, Action<Result> onProgress = null)
        {
            Results results = new Results();
            
            LMGT.LoadingRetry = retry;
            LMGT.LoadingTimeOut = timeOut;

            for (int i = 0; i < imageUrls.Count; i++)
            {
                ImageLoader loader = ImageLoader.Create(LMGT.MaxCacheFilePerFolder, LMGT.CacheDirectoryEnum);
                loader.Load((uint)i, imageUrls[i], (texture, index) =>
                {
                    _progress = (float)(results.m_TextureDict.Count + 1) / imageUrls.Count;

                    // Reminded: If the texture cannot be loaded, it will return a null. So check null before use it.
                    Result result = new Result(texture, index, _progress, loader.DetectedFileMime, loader.DetecedFileExtension);
                    results.SetResult(index, result);

                    if (onProgress != null) onProgress(result); // On Progress

                    if (results.m_TextureDict.Count >= imageUrls.Count) // On Complete
                    {
                        if (onComplete != null)
                        {
                            var ordered = results.m_TextureDict.OrderBy(item => item.Key);
                            results.m_TextureDict = ordered.ToDictionary((k) => k.Key, (v) => v.Value);
                            onComplete(results);
                        }
                    }
                }, retry, timeOut);
            }
        }

        /// <summary>
        /// Load multiple images from web or local, and save to the local storage.
        /// </summary>
        /// <param name="imageUrls"> Image urls or local paths. </param>
        /// <param name="filenamePrefix"> The filename prefix for storing(caching) the image files. The final filename will be combined by this prefix and the index together. </param>
        /// <param name="folderName"> The target folder for storing(caching) the image file. </param>
        /// <param name="cacheMode"> The behavior for handling Load and Cache files. (NoCache: do not auto save the image; UseCached: use the locally cached file if exist; Replace: download and replace the locally cached file is exist) </param>
        /// <param name="onComplete"> The callback for receiving all the loaded results. </param>
        /// <param name="onProgress"> Update the loading progress and receive the latest loaded texture. </param>
        /// <param name="retry"> The retry number for each image. </param>
        /// <param name="timeOut"> The timout for each image. </param>
        public void Load(List<string> imageUrls, string filenamePrefix, string folderName, ImageLoader.CacheMode cacheMode = ImageLoader.CacheMode.Replace, 
            Action<Results> onComplete = null, Action<Result> onProgress = null, uint retry = 0, float timeOut = 10f)
        {
            Results results = new Results();

            LMGT.FileNamePrefix = filenamePrefix;
            LMGT.FolderName = folderName;
            LMGT.CacheMode = cacheMode;
            LMGT.LoadingRetry = retry;
            LMGT.LoadingTimeOut = timeOut;
            LMGT.FileIndexFormatDigitsCount = (uint)Mathf.Clamp(LMGT.FileIndexFormatDigitsCount, 0, 18);

            for (int i = 0; i < imageUrls.Count; i++)
            {
                ImageLoader loader = ImageLoader.Create(LMGT.MaxCacheFilePerFolder, LMGT.CacheDirectoryEnum);
                loader.LMGT = new LoaderManagement(LMGT);
                string fileName = loader.LMGT.GenerateFileName(i);
                
                loader.Load((uint)i, imageUrls[i], fileName, folderName, cacheMode, (texture, index) =>
                {
                    _progress = (float)(results.m_TextureDict.Count + 1) / imageUrls.Count;

                    // Reminded: If the texture cannot be loaded, it will return a null. So check null before use it.
                    Result result = new Result(texture, index, _progress, loader.DetectedFileMime, loader.DetecedFileExtension);
                    results.SetResult(index, result);

                    if (onProgress != null) onProgress(result); // On Progress

                    if (results.m_TextureDict.Count >= imageUrls.Count) // On Complete
                    {
                        if (onComplete != null)
                        {
                            var ordered = results.m_TextureDict.OrderBy(item => item.Key);
                            results.m_TextureDict = ordered.ToDictionary((k) => k.Key, (v) => v.Value);
                            onComplete(results);
                        }
                    }
                }, retry, timeOut );
            }
        }
    }
}
