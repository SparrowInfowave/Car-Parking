/// <summary>
/// By SwanDEV 2017
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

/// <summary> Files, Paths, Names and IO common methods. (Last update: 2019-06-22)..
/// *** Reminder! File IO methods do not work with streamingAssetsPath on Android.
/// </summary>
public class FilePathName
{
    private static string _lastGeneratedFileNameWithoutExt_fff = "";
    private static int _lastSameFileNameCounter_fff = 1;

    private static string _lastGeneratedFileNameWithoutExt = "";
    private static int _lastSameFileNameCounter = 1;

    public enum SaveFormat
    {
        NONE = -1,
        GIF = 0,
        JPG,
        PNG,
    }

    #region ----- Instance -----
    private static FilePathName fpn = null;
    public static FilePathName Instance
    {
        get
        {
            if (fpn == null) fpn = new FilePathName();
            return fpn;
        }
    }
    #endregion

    #region ----- Path & FileName -----
    public enum AppPath
    {
        /// <summary> The directory path where you can store data that you want to be kept between runs. </summary>
        PersistentDataPath = 0,

        /// <summary> The directory path where temporary data can be stored. </summary>
        TemporaryCachePath,

        /// <summary> The folder located at /Assets/StreamingAssets in the project. (Not work with System.IO methods when running on Android/WebGL) </summary>
        StreamingAssetsPath,

        /// <summary> The folder located at /Assets in the project. (Work on the Unity editor only) </summary>
        DataPath
    }
    public string GetAppPath(AppPath appPath)
    {
        string directory = "";
        switch (appPath)
        {
            case AppPath.PersistentDataPath:
                directory = Application.persistentDataPath;
                break;
            case AppPath.TemporaryCachePath:
                directory = Application.temporaryCachePath;
                break;
            case AppPath.StreamingAssetsPath:
                directory = Application.streamingAssetsPath;
                break;
            case AppPath.DataPath:
                directory = Application.dataPath;
                break;
        }
        return directory;
    }
    public string GetSaveDirectory(bool isTemporaryPath = false, string subFolder = "", bool createDirectoryIfNotExist = false)
    {
        string result = "";
        if (isTemporaryPath)
        {
            result = Application.temporaryCachePath;
        }
        else
        {
            //Available path: Application.persistentDataPath, Application.temporaryCachePath, Application.dataPath.
            //Do not allow sub-folder when a GIF is being created, but you can move it to any accessible sub-folder after the GIF is completely saved.
            //And/Or, you can filter the file names to include .gif only.
#if UNITY_EDITOR
            result = Application.dataPath;
            //result = Application.persistentDataPath;
#else
			result = Application.persistentDataPath;
#endif
        }

        result = string.IsNullOrEmpty(subFolder) ? result : Path.Combine(result, subFolder);

        if (createDirectoryIfNotExist && !Directory.Exists(result))
        {
            Directory.CreateDirectory(result);
        }

        return result;
    }

    public string GetFileNameWithoutExt(bool millisecond = false)
    {
        if (millisecond)
        {
            return _GetComparedFileName(DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"),
                _lastGeneratedFileNameWithoutExt_fff, _lastSameFileNameCounter_fff,
                out _lastGeneratedFileNameWithoutExt_fff, out _lastSameFileNameCounter_fff);
        }
        return _GetComparedFileName(DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"),
            _lastGeneratedFileNameWithoutExt, _lastSameFileNameCounter,
            out _lastGeneratedFileNameWithoutExt, out _lastSameFileNameCounter);
    }

    private string _GetComparedFileName(string newFileName, string lastGeneratedFileName, int sameFileNameCounter,
        out string outLastGeneratedFileName, out int outSameFileNameCounter)
    {
        if (lastGeneratedFileName == newFileName)
        {
            sameFileNameCounter++;
        }
        else
        {
            sameFileNameCounter = 1;
        }

        outLastGeneratedFileName = newFileName;
        outSameFileNameCounter = sameFileNameCounter;

        if (sameFileNameCounter > 1)
        {
            newFileName += " " + sameFileNameCounter;
        }

        return newFileName;
    }

    public string EnsureValidPath(string pathOrUrl)
    {
        string path = pathOrUrl;
        if (path.StartsWith("http"))
        {
            // WEB
        }
        else if (path.StartsWith("/idbfs/"))
        {
            // WebGL index DB
        }
        else
        {
            // Local path
            path = EnsureLocalPath(path);
        }
        return path;
    }

    public string EnsureLocalPath(string path)
    {
        if (path.ToLower().StartsWith("jar:")) // Android streamingAssetsPath
        { }
        else if (!path.ToLower().StartsWith("file:///"))
        {
            while (path.StartsWith("/"))
            {
                path = path.Remove(0, 1);
            }
            path = "file:///" + path;
        }
        return path;
    }

    public String EnsureValidFileName(String fileName)
    {
        string replaceChars = "[:\\\\/*\"?|<>']";
        for (int i = 0; i < replaceChars.Length; i++)
        {
            if (fileName.Contains(replaceChars[i]))
            {
                fileName = fileName.Replace(replaceChars[i], '_');
            }
        }
        return fileName;
    }

    public string GetGifFileName()
    {
        string timestamp = GetFileNameWithoutExt();
        return "GIF_" + timestamp;
    }
    public string GetGifFullPath(string subFolder = "", bool createDirectoryIfNotExist = false)
    {
        return GetSaveDirectory(false, subFolder, createDirectoryIfNotExist) + "/" + GetGifFileName() + ".gif";
    }
    public string GetDownloadedGifSaveFullPath(string subFolder = "", bool createDirectoryIfNotExist = false)
    {
        return GetSaveDirectory(false, subFolder, createDirectoryIfNotExist) + "/" + GetGifFileName() + ".gif";
    }

    public string GetJpgFileName(string subFolder = "", bool createDirectoryIfNotExist = false)
    {
        string timestamp = GetFileNameWithoutExt(true);
        return "Photo_" + timestamp;
    }
    public string GetJpgFullPath(string subFolder = "", bool createDirectoryIfNotExist = false)
    {
        return GetSaveDirectory(false, subFolder, createDirectoryIfNotExist) + "/" + GetJpgFileName() + ".jpg";
    }

    public string GetPngFileName(string subFolder = "", bool createDirectoryIfNotExist = false)
    {
        string timestamp = GetFileNameWithoutExt(true);
        return "Photo_" + timestamp;
    }
    public string GetPngFullPath(string subFolder = "", bool createDirectoryIfNotExist = false)
    {
        return GetSaveDirectory(false, subFolder, createDirectoryIfNotExist) + "/" + GetPngFileName() + ".png";
    }

    #endregion

    #region ----- IO -----
    public byte[] ReadFileToBytes(string fromFullPath)
    {
        if (!File.Exists(fromFullPath))
        {
#if UNITY_EDITOR
            Debug.LogWarning("File Not Found: " + fromFullPath);
#endif
            return null;
        }
        return File.ReadAllBytes(fromFullPath);
    }

    public void WriteBytesToFile(string toFullPath, byte[] byteArray)
    {
        CheckToCreateDirectory(Path.GetDirectoryName(toFullPath));
        File.WriteAllBytes(toFullPath, byteArray);
    }

    public void CopyFile(string fromFullPath, string toFullPath, bool overwrite = false)
    {
        if (!File.Exists(fromFullPath))
        {
#if UNITY_EDITOR
            Debug.LogWarning("File Not Found: " + fromFullPath);
#endif
            return;
        }
        CheckToCreateDirectory(Path.GetDirectoryName(toFullPath));
        File.Copy(fromFullPath, toFullPath, overwrite);
    }

    public void MoveFile(string fromFullPath, string toFullPath)
    {
        if (!File.Exists(fromFullPath))
        {
#if UNITY_EDITOR
            Debug.LogWarning("File Not Found: " + fromFullPath);
#endif
            return;
        }
        CheckToCreateDirectory(Path.GetDirectoryName(toFullPath));
        File.Move(fromFullPath, toFullPath);
    }

    public void DeleteFile(string fileFullPath)
    {
        if (!File.Exists(fileFullPath))
        {
#if UNITY_EDITOR
            Debug.LogWarning("File Not Found: " + fileFullPath);
#endif
            return;
        }
        File.Delete(fileFullPath);
    }

    public void CheckToCreateDirectory(string directory)
    {
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }

    /// <summary> Determine whether a given path is a directory. </summary>
    public bool PathIsDirectory(string path)
    {
        FileAttributes attr = File.GetAttributes(path);
        if ((attr & FileAttributes.Directory) == FileAttributes.Directory) return true; else return false;
    }

    public void RenameFile(string originFilePath, string newFileName)
    {
        string directory = Path.GetDirectoryName(originFilePath);
        string newFilePath = Path.Combine(directory, newFileName);
        CopyFile(originFilePath, newFilePath, true);
    }

    public bool FileStreamTo(string fileFullpath, byte[] byteArray)
    {
        try
        {
            CheckToCreateDirectory(Path.GetDirectoryName(fileFullpath));
            using (FileStream fs = new FileStream(fileFullpath, FileMode.Create, FileAccess.Write))
            {
                fs.Write(byteArray, 0, byteArray.Length);
                return true; // success
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception caught in process: {0}", e);
            return false;   // fail
        }
    }

    public void WriteBytesToText(byte[] bytes, string toFileFullPath, string separator = "", bool toChar = true)
    {
        //		int bkCount = 0;

        StringBuilder sb = new StringBuilder();
        if (string.IsNullOrEmpty(separator))
        {
            if (toChar)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append((char)bytes[i]);
                }
            }
            else
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i]);

                    //Test 
                    //					bkCount++;
                    //					if(bkCount == 3) 
                    //					{
                    //						bkCount = 0;
                    //						sb.Append(" (" + (i+1)/3 + ")\n");
                    //					}
                }
            }
        }
        else
        {
            if (toChar)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append((char)bytes[i]);
                    sb.Append(separator);
                }
            }
            else
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i]);
                    sb.Append(separator);

                    //Test
                    //					bkCount++;
                    //					if(bkCount == 3) 
                    //					{
                    //						bkCount = 0;
                    //						sb.Append(" (" + ((i+1)/3-1) + ")\n");
                    //					}
                }
            }
        }
        CheckToCreateDirectory(Path.GetDirectoryName(toFileFullPath));
        File.WriteAllText(toFileFullPath, sb.ToString());
    }

    public string SaveTextureAs(Texture2D texture2D, SaveFormat format = SaveFormat.JPG)
    {
        string savePath = string.Empty;
        switch (format)
        {
            case SaveFormat.JPG:
                savePath = GetJpgFullPath();
                WriteBytesToFile(savePath, texture2D.EncodeToJPG(90));
                break;
            case SaveFormat.PNG:
                savePath = GetPngFullPath();
                WriteBytesToFile(savePath, texture2D.EncodeToPNG());
                break;
            case SaveFormat.GIF:
#if PRO_GIF
                savePath = ProGifTexturesToGIF.Instance.Save(new List<Texture2D> { texture2D }, texture2D.width, texture2D.height, 1, -1, 10);
#endif
                break;
        }
        return savePath;
    }
    
    public string SaveTextureAs(Texture2D texture2D, AppPath appPath, string subFolder, bool isJPG)
    {
        string savePath = GetAppPath(appPath);
        if (!string.IsNullOrEmpty(subFolder)) savePath = Path.Combine(savePath, subFolder);
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
        savePath = Path.Combine(savePath, GetFileNameWithoutExt(true) + (isJPG ? ".jpg" : ".png"));
        WriteBytesToFile(savePath, (isJPG ? texture2D.EncodeToJPG(90) : texture2D.EncodeToPNG()));
        return savePath;
    }

#if PRO_GIF
    public string SaveTexturesAsGIF(List<Texture2D> textureList, int width, int height, int fps, int loop, int quality,
        Action<int, string> onFileSaved = null, Action<int, float> onFileSaveProgress = null,
        ProGifTexturesToGIF.ResolutionHandle resolutionHandle = ProGifTexturesToGIF.ResolutionHandle.ResizeKeepRatio)
    {
        return ProGifTexturesToGIF.Instance.Save(textureList, width, height, fps, loop, quality, onFileSaved, onFileSaveProgress, resolutionHandle);
    }
#endif

    public Texture2D LoadImage(string fullFilePath)
    {
        if (!File.Exists(fullFilePath))
        {
#if UNITY_EDITOR
            Debug.LogWarning("File not exist! " + fullFilePath);
#endif
            return null;
        }
        else
        {
            Texture2D tex2D = new Texture2D(1, 1); //, TextureFormat.ARGB32, false);
            tex2D.LoadImage(ReadFileToBytes(fullFilePath));
            return tex2D;
        }
    }

    /// <summary>
    /// Load images in the target directory, to a texture2D list.
    /// </summary>
    /// <returns>The images.</returns>
    /// <param name="directory">Directory.</param>
    /// <param name="fileExtensions">A list of file extension names, indicating the type of files to be loaded. Load jpg, png and gif if Null or Empty.</param>
    public List<Texture2D> LoadImages(string directory, List<string> fileExtensions = null)
    {
        if (fileExtensions != null && fileExtensions.Count > 0) { }
        else
        {
            fileExtensions = new List<string> { ".jpg", ".png", ".gif" };
        }

        List<Texture2D> textureList = new List<Texture2D>();

        foreach (string f in GetFilePaths(directory, fileExtensions))
        {
            if (fileExtensions.Contains(Path.GetExtension(f).ToLower()))
            {
                textureList.Add(LoadImage(f));
            }
        }
        return textureList;
    }

    /// <summary>
    /// Load files in the target directory, to a byte[] list.
    /// </summary>
    /// <returns>Files in byte[].</returns>
    /// <param name="directory">Directory.</param>
    /// <param name="fileExtensions">A list of file extension names, indicating the type of files to be loaded. Load all files if Null or Empty.</param>
    public List<byte[]> LoadFiles(string directory, List<string> fileExtensions = null)
    {
        List<byte[]> fileByteList = new List<byte[]>();

        foreach (string f in GetFilePaths(directory, fileExtensions))
        {
            fileByteList.Add(ReadFileToBytes(f));
        }
        return fileByteList;
    }

    /// <summary>
    /// Get file paths in the target directory.
    /// </summary>
    /// <returns>File paths list.</returns>
    /// <param name="directory">Directory.</param>
    /// <param name="fileExtensions">A list of file extension names, indicating the type of file paths to get. Get all file paths if Null or Empty.</param>
    public List<string> GetFilePaths(string directory, List<string> fileExtensions = null)
    {
        if (!Directory.Exists(directory))
        {
            //throw new DirectoryNotFoundException("Directory not found at " + directory);
#if UNITY_EDITOR
            Debug.Log("Directory Not Found: " + directory);
#endif
            return null;
        }

        string[] allFiles_src = Directory.GetFiles(directory);

        bool loadAllFile = (fileExtensions == null) ? true : ((fileExtensions.Count <= 0) ? true : false);
        if (loadAllFile)
        {
#if UNITY_EDITOR
            Debug.Log("Load ALL");
#endif
            return allFiles_src.ToList();
        }

#if UNITY_EDITOR
        Debug.Log("Load Filtered");
#endif

        if (fileExtensions == null)
        {
            fileExtensions = new List<string>();
        }
        else
        {
            for (int i = 0; i < fileExtensions.Count; i++)
            {
                fileExtensions[i] = fileExtensions[i].ToLower();
            }
        }

        List<string> filteredFilePathList = new List<string>();
        foreach (string f in allFiles_src)
        {
            if (fileExtensions.Contains(Path.GetExtension(f).ToLower()))
            {
                filteredFilePathList.Add(f);
            }
        }
        return filteredFilePathList;
    }

#if UNITY_4 || UNITY_5 || UNITY_2017 || UNITY_2018
    /// <summary>
    /// Loads file using WWW. Return the byte array of the file in onLoadCompleted callback.
    /// ( IEnumerator: Remember to call this method in StartCoroutine )
    /// </summary>
    /// <returns>The file byte array.</returns>
    /// <param name="url">Local file path or http/https link.</param>
    /// <param name="onLoadCompleted">On load completed callback. Return the byte array of the downloaded file.</param>
    /// <param name="onLoadCompletedWWW">On load completed callback. Return the WWW object.</param>
    public IEnumerator LoadFileWWW(string url, Action<byte[]> onLoadCompleted = null, Action<WWW> onLoadCompletedWWW = null)
    {
        string path = url;
        if (path.StartsWith("http"))
        {
            // from WEB
        }
        else
        {
            // from Local
            path = EnsureLocalPath(path);

#if UNITY_EDITOR
            Debug.Log("Local file path: " + path);
#endif
        }

        using (WWW www = new WWW(path))
        {
            yield return www;
            if (onLoadCompletedWWW != null) onLoadCompletedWWW(www);
            if (string.IsNullOrEmpty(www.error) == false)
            {
                Debug.LogError("File load error.\n" + www.error);
                onLoadCompleted(null);
                yield break;
            }
            onLoadCompleted(www.bytes);
        }
    }
#endif

    /// <summary>
    /// Loads file using UnityWebRequest. Return the byte array of the file in onLoadCompleted callback.
    /// ( IEnumerator: Remember to call this method in StartCoroutine )
    /// </summary>
    /// <returns>The file byte array.</returns>
    /// <param name="url">Local file path or http/https link.</param>
    /// <param name="onLoadCompleted">On load completed callback. Return the byte array of the downloaded file.</param>
    /// <param name="onLoadCompletedUWR">On load completed callback. Return the UnityWebRequest object.</param>
    public IEnumerator LoadFileUWR(string url, Action<byte[]> onLoadCompleted = null, Action<UnityWebRequest> onLoadCompletedUWR = null)
    {
        string path = url;
        if (path.StartsWith("http"))
        {
            // from WEB
        }
        else
        {
            // from Local
            path = EnsureLocalPath(path);
#if UNITY_EDITOR
            Debug.Log("Local file path: " + path);
#endif
        }

        using (UnityWebRequest uwr = UnityWebRequest.Get(path))
        {
#if UNITY_5
            uwr.Send();
#else
            uwr.SendWebRequest();
#endif
            while (!uwr.isDone)
            {
                //Debug.Log(uwr.downloadProgress);
                yield return null;
            }

            if (uwr.isDone)
            {
                //Debug.Log("UWR load completed..");
            }

            if (onLoadCompletedUWR != null) onLoadCompletedUWR(uwr);

#if UNITY_5
            if (uwr.isError)
#else
            if (uwr.isNetworkError || uwr.isHttpError)
#endif
            {
                Debug.LogError("File load error.\n" + uwr.error);
                onLoadCompleted(null);
            }
            else
            {
                onLoadCompleted(uwr.downloadHandler.data);
            }
        }
    }

#endregion

    public Sprite Texture2DToSprite(Texture2D texture2D)
    {
        if (texture2D == null) return null;

        Vector2 pivot = new Vector2(0.5f, 0.5f);
        float pixelPerUnit = 100;
        return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), pivot, pixelPerUnit);
    }

}
