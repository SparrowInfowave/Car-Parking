
using System.Linq;
using System.IO;
using UnityEngine;

public class FileMimeAndExtension
{
    private readonly byte[] BMP = { 66, 77 };
    private readonly byte[] DOC = { 208, 207, 17, 224, 161, 177, 26, 225 };
    private readonly byte[] EXE_DLL = { 77, 90 };
    private readonly byte[] GIF = { 71, 73, 70, 56 };
    private readonly byte[] ICO = { 0, 0, 1, 0 };
    private readonly byte[] JPG = { 255, 216, 255 };
    private readonly byte[] MP3 = { 255, 251, 48 };
    private readonly byte[] OGG = { 79, 103, 103, 83, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0 };
    private readonly byte[] PDF = { 37, 80, 68, 70, 45, 49, 46 };
    private readonly byte[] PNG = { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82 };
    private readonly byte[] RAR = { 82, 97, 114, 33, 26, 7, 0 };
    private readonly byte[] SWF = { 70, 87, 83 };
    private readonly byte[] TIFF = { 73, 73, 42, 0 };
    private readonly byte[] TORRENT = { 100, 56, 58, 97, 110, 110, 111, 117, 110, 99, 101 };
    private readonly byte[] TTF = { 0, 1, 0, 0, 0 };
    private readonly byte[] WAV_AVI = { 82, 73, 70, 70 };
    private readonly byte[] WMV_WMA = { 48, 38, 178, 117, 142, 102, 207, 17, 166, 217, 0, 170, 0, 98, 206, 108 };
    private readonly byte[] ZIP_DOCX = { 80, 75, 3, 4 };
    private readonly byte[] RTF = { 123, 92, 114, 116, 102, 49, 125 };


    public string GetFileExtension(string fullFilePath, string fileName = "")
    {
        string mime = "";
        string extensionName = "";
        GetFileMimeAndExtension(fullFilePath, ref mime, ref extensionName, fileName);
        return extensionName;
    }
    public string GetFileExtension(byte[] fileBytes, string fileName = "")
    {
        string mime = "";
        string extensionName = "";
        GetFileMimeAndExtension(fileBytes, ref mime, ref extensionName, fileName);
        return extensionName;
    }

    public string GetFileMime(string fullFilePath, string fileName = "")
    {
        string mime = "";
        string extensionName = "";
        GetFileMimeAndExtension(fullFilePath, ref mime, ref extensionName, fileName);
        return mime;
    }
    public string GetFileMime(byte[] fileBytes, string fileName = "")
    {
        string mime = "";
        string extensionName = "";
        GetFileMimeAndExtension(fileBytes, ref mime, ref extensionName, fileName);
        return mime;
    }

    public void GetFileMimeAndExtension(string fullFilePath, ref string mime, ref string extensionName, string fileName = "")
    {
        GetFileMimeAndExtension(File.ReadAllBytes(fullFilePath), ref mime, ref extensionName, fileName);
    }

    private void _LogByteSequence(byte[] fileBytes, string fileName = "")
    {
        byte[] buffer = new byte[20];
        System.Array.Copy(fileBytes, buffer, Mathf.Min(20, fileBytes.Length));
        Debug.Log(fileName + " - Log File ByteArray Sequence: " + buffer[0] + ", " + buffer[1] + ", " + buffer[2] + ", " + buffer[3] + ", "
            + buffer[4] + ", " + buffer[5] + ", " + buffer[6] + ", " + buffer[7] + ", "
            + buffer[8] + ", " + buffer[9] + ", " + buffer[10] + ", " + buffer[11] + ", "
            + buffer[12] + ", " + buffer[13] + ", " + buffer[14] + ", " + buffer[15]);
    }

    public void GetFileMimeAndExtension(byte[] fileBytes, ref string mime, ref string extensionName, string fileName = "")
    {
#if UNITY_EDITOR
        //_LogByteSequence(fileBytes);
#endif
        string extension = Path.GetExtension(fileName) == null ? string.Empty : Path.GetExtension(fileName).ToUpper();
        
        if (fileBytes.Take(2).SequenceEqual(BMP))
        {
            mime = "image/bmp";
            extensionName = "bmp";
        }
        else if (fileBytes.Take(7).SequenceEqual(RTF))
        {
            mime = "application/rtf";
            extensionName = "rtf";
        }
        else if (fileBytes.Take(8).SequenceEqual(DOC))
        {
            mime = "application/msword";
            extensionName = "doc";
        }
        else if (fileBytes.Take(2).SequenceEqual(EXE_DLL))
        {
            mime = "application/x-msdownload"; //both use same mime type
            extensionName = "exe";
        }
        else if (fileBytes.Take(4).SequenceEqual(GIF))
        {
            mime = "image/gif";
            extensionName = "gif";
        }
        else if (fileBytes.Take(4).SequenceEqual(ICO))
        {
            mime = "image/x-icon";
            extensionName = "ico";
        }
        else if (fileBytes.Take(3).SequenceEqual(JPG))
        {
            mime = "image/jpeg";
            extensionName = "jpg";
        }
        else if (fileBytes.Take(3).SequenceEqual(MP3))
        {
            mime = "audio/mpeg";
            extensionName = "mpg";
        }
        else if (fileBytes.Take(14).SequenceEqual(OGG))
        {
            if (extension == ".OGX")
            {
                mime = "application/ogg";
                extensionName = "ogx";
            }
            else if (extension == ".OGA")
            {
                mime = "audio/ogg";
                extensionName = "oga";
            }
            else
            {
                mime = "video/ogg";
                extensionName = "ogg";
            }
        }
        else if (fileBytes.Take(7).SequenceEqual(PDF))
        {
            mime = "application/pdf";
            extensionName = "pdf";
        }
        else if (fileBytes.Take(16).SequenceEqual(PNG))
        {
            mime = "image/png";
            extensionName = "png";
        }
        else if (fileBytes.Take(7).SequenceEqual(RAR))
        {
            mime = "application/x-rar-compressed";
            extensionName = "rar";
        }
        else if (fileBytes.Take(3).SequenceEqual(SWF))
        {
            mime = "application/x-shockwave-flash";
            extensionName = "swf";
        }
        else if (fileBytes.Take(4).SequenceEqual(TIFF))
        {
            mime = "image/tiff";
            extensionName = "tiff";
        }
        else if (fileBytes.Take(11).SequenceEqual(TORRENT))
        {
            mime = "application/x-bittorrent";
            extensionName = "torrent";
        }
        else if (fileBytes.Take(5).SequenceEqual(TTF))
        {
            mime = "application/x-font-ttf";
            extensionName = "ttf";
        }
        else if (fileBytes.Take(4).SequenceEqual(WAV_AVI))
        {
            if (extension == ".AVI")
            {
                mime = "video/x-msvideo";
                extensionName = "avi";
            }
            else
            {
                mime = "audio/x-wav";
                extensionName = "wav";
            }
        }
        else if (fileBytes.Take(16).SequenceEqual(WMV_WMA))
        {
            if (extension == ".WMA")
            {
                mime = "audio/x-ms-wma";
                extensionName = "wma";
            }
            else
            {
                mime = "video/x-ms-wmv";
                extensionName = "wmv";
            }

        }
        else if (fileBytes.Take(4).SequenceEqual(ZIP_DOCX))
        {
            if (extension == ".DOCX")
            {
                mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                extensionName = "docx";
            }
            else
            {
                mime = "application/x-zip-compressed";
                extensionName = "zip";
            }
        }
        else
        {
            mime = "application/octet-stream";  //DEFAULT UNKNOWN MIME TYPE
            extensionName = ".unknown";         //DEFAULT UNKNOWN FILE EXTENSION
        }
    }
}
