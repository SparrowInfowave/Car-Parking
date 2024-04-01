using System;
using UnityEngine;

namespace IMBX
{
    /// <summary>
    /// The loader Cache Management and Loading Settings.
    /// </summary>
    [Serializable]
    public class LoaderManagement
    {
        public LoaderManagement() { }

        public LoaderManagement(LoaderManagement LM)
        {
            this.CacheDirectoryEnum = LM.CacheDirectoryEnum;
            this.CacheMode = LM.CacheMode;
            this.FileExtension = LM.FileExtension;
            this.FileIndexFormatDigitsCount = LM.FileIndexFormatDigitsCount;
            this.FileNameAndIndexSeparator = LM.FileNameAndIndexSeparator;
            this.FileNamePrefix = LM.FileNamePrefix;
            this.FileNameStartingIndex = LM.FileNameStartingIndex;
            this.FolderName = LM.FolderName;
            this.LoadingRetry = LM.LoadingRetry;
            this.LoadingTimeOut = LM.LoadingTimeOut;
            this.MaxCacheFilePerFolder = LM.MaxCacheFilePerFolder;
            this.MaxTimeForKeepingFiles = LM.MaxTimeForKeepingFiles;
            this.MinTimeForKeepingFiles = LM.MinTimeForKeepingFiles;
        }

        /// <summary>
        /// The behavior for handling Load and Cache files. (NoCache: do not auto save the image; UseCached: use the locally cached file if exist; Replace: download and replace the locally cached file is exist)
        /// </summary>
        [Tooltip("The behavior for handling Load and Cache files. (NoCache: do not auto save the image; UseCached: use the locally cached file if exist; Replace: download and replace the locally cached file is exist)")]
        public ImageLoader.CacheMode CacheMode = ImageLoader.CacheMode.UseCached;

        /// <summary>
        /// The enum that determines which application path to load and save(cache) file to.
        /// </summary>
        [Tooltip("The enum that determines which application path to load and save(cache) file to.")]
        public FilePathName.AppPath CacheDirectoryEnum = FilePathName.AppPath.PersistentDataPath;

        /// <summary>
        /// The directory for loading and storing(caching) files.
        /// </summary>
        public string CacheDirectory
        {
            get
            {
                return FilePathName.Instance.GetAppPath(CacheDirectoryEnum);
            }
        }

        /// <summary>
        /// The sub-folder under cache directory for loading and storing(caching) files to.
        /// </summary>
        [Tooltip("The sub-folder under cache directory for loading and storing(caching) files to.")]
        public string FolderName = "";

        /// <summary>
        /// The filename prefix for storing(caching) the image files. The final filename will be combined by this prefix, separator, and the index together.
        /// (For ImageBatchLoader)
        /// </summary>
        [Tooltip("The filename prefix for storing(caching) the image files. The final filename will be combined by this prefix, separator, and the index together. (For ImageBatchLoader)")]
        public string FileNamePrefix = "Pic";

        /// <summary>
        /// Set .jpg or .png.
        /// </summary>
        [Tooltip("Set .jpg or .png.")]
        public string FileExtension = ".png";

        /// <summary>
        /// Number of Digits for the Index follow the Filename Prefix.
        /// </summary>
        [Tooltip("Number of Digits for the Index follow the Filename Prefix.")]
        public uint FileIndexFormatDigitsCount = 4;
        /// <summary>
        /// File Name Starting Index. (Set this value to set an offset for the filename index. The default starting index is 0.)
        /// </summary>
        [Tooltip("File Name Starting Index. (Set this value to set an offset for the filename index. The default starting index is 0.)")]
        public uint FileNameStartingIndex = 0;
        /// <summary>
        /// Separator text between the File Name and Index. (Please use filename friendly characters only)
        /// </summary>
        [Tooltip("Separator text between the File Name and Index. (Please use filename friendly characters only)")]
        public string FileNameAndIndexSeparator = "_";

        /// <summary>
        /// The maximum number of files can be stored(Cached) in the folder. ( 0 means no limit )
        /// </summary>
        [Tooltip("The maximum number of files can be stored(Cached) in the folder. ( 0 means no limit )")]
        public uint MaxCacheFilePerFolder = 0;

        /// <summary>
        /// Time duration in seconds for keeping files not being deleted. (eg. 86400 = 3600s * 24h = 1 day.)
        /// (Zero means infinite)
        /// </summary>
        [Tooltip("Time duration in seconds for keeping files not being deleted. (eg. 86400 = 3600s * 24h = 1 day.) (Zero means infinite)")]
        public uint MinTimeForKeepingFiles = 0;

        /// <summary>
        /// Time duration in seconds, files must be deleted if the last modify time from now more than this duration.
        /// For example, set this value = 3600(1 hour), then all files in the folder those modified 1 hour ago will be deleted.
        /// (Zero means infinite)
        /// </summary> 
        [Tooltip("Time duration in seconds, files must be deleted if the last modify time from now more than this duration. For example, set this value = 3600(1 hour), then all files in the folder those modified 1 hour ago will be deleted. (Zero means infinite)")]
        public uint MaxTimeForKeepingFiles = 0;

        /// <summary>
        /// The times to retry if fails to load the file. Retry per second until retry value is Zero.
        /// </summary>
        [Tooltip("The times to retry if fails to load the file. Retry per second until retry value is Zero.")]
        public uint LoadingRetry = 0;

        /// <summary>
        /// The time duration for waiting for a loading/downloading, stop and kill the loader if time exceeds.
        /// </summary>
        [Tooltip("The time duration for waiting for a loading/downloading, stop and kill the loader if time exceeds.")]
        public float LoadingTimeOut = 0;


        public void SetFileNameFormat(uint fileIndexFormatDigitsCount, uint fileNameStartingIndex = 0, string fileNameAndIndexSeparator = "_")
        {
            FileIndexFormatDigitsCount = fileIndexFormatDigitsCount;
            FileNameStartingIndex = fileNameStartingIndex;
            FileNameAndIndexSeparator = fileNameAndIndexSeparator;
        }

        /// <summary>
        /// Generate a FileName base on FileNamePrefix, FileIndexFormatDigitsCount, FileNameStartingIndex, FileNameAndIndexSeparator and file index.
        /// </summary>
        public string GenerateFileName(int fileIndex)
        {
            FileIndexFormatDigitsCount = (uint)Mathf.Clamp(FileIndexFormatDigitsCount, 0, 18);
            string fileIndexFormat = "{0," + FileIndexFormatDigitsCount + ":D" + FileIndexFormatDigitsCount + "}";
            string fileName = FileNamePrefix + FileNameAndIndexSeparator + String.Format(fileIndexFormat, FileNameStartingIndex + fileIndex); // e.g. "Pic" + "-" + string format "0000" with fileIndex 12 = "Pic-0012"
            return fileName;
        }
        public string GenerateFileName(uint fileIndex)
        {
            return GenerateFileName((int)fileIndex);
        }
    }

}
