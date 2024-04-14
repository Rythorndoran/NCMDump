using NCMDump;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MusicDescriptor : ObservableObject
{
    private string musicName;
    private string fileInfo;
    private string fileName;
    private bool isChecked;
    private string status;

    public MusicDescriptor(string _fileName)
    {
        fileName = _fileName;
        var metaData = NeteaseCryptoMusic.MetaDataFromFile(_fileName);
        musicName = metaData.MusicName;
        var info = new System.IO.FileInfo(_fileName);
        fileInfo = metaData.Format + " | " + FileSizeToString(info.Length);
        isChecked = true;
        status = "Waiting";
    }

    public string FileName { get => fileName; }

    public string MusicName
    {
        get => musicName;
        set => Set(ref musicName, value, nameof(MusicName));
    }

    public string FileInfo
    {
        get => fileInfo;
        set => Set(ref fileInfo, value, nameof(FileInfo));
    }

    public bool IsItemChecked
    {
        get => isChecked;
        set => Set(ref isChecked, value, nameof(IsItemChecked));
    }

    public string Status
    {
        get => status;
        set => Set(ref status, value, nameof(Status));
    }

    public NeteaseCryptoMusic CryptoMusic
    {
        get => NeteaseCryptoMusic.FromFile(fileName);
    }

    public ImageSource CoverImage
    {
        get => NeteaseCryptoMusic.FromFile(fileName).CoverImage;
    }

    private string FileSizeToString(long len)
    {
        long fileSizeInBytes = len;
        double fileSizeInKB = fileSizeInBytes / 1024.0; // 字节转换为千字节（KB）
        double fileSizeInMB = fileSizeInKB / 1024.0; // 千字节转换为兆字节（MB）
        string FileSize = $"{Math.Round(fileSizeInMB, 2)} MB";
        return FileSize;
    }
}
