using NCMDump;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MusicDescriptor : ObservableObject
{
    private string musicName;
    private string fileInfo;
    private string fileName;
    private ImageSource coverImage;
    private bool isChecked;
    private string status;
    private NeteaseCryptoMusic cryptoMusic;

    public MusicDescriptor(string _fileName, string _musicName, string _fileInfo, ImageSource _coverImage, NeteaseCryptoMusic cryptoMusic)
    {
        fileName = _fileName;
        musicName = _musicName;
        fileInfo = _fileInfo;
        coverImage = _coverImage;
        isChecked = true;
        status = "Waiting";
        this.cryptoMusic = cryptoMusic;
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

    public ImageSource CoverImage
    {
        get => coverImage;
        set => Set(ref coverImage, value, nameof(CoverImage));
    }

    public bool IsItemChecked
    {
        get => isChecked;
        set => Set(ref isChecked,value, nameof(IsItemChecked));
    }
    public string Status
    {
        get => status;
        set => Set(ref status,value, nameof(Status));
    }

    public NeteaseCryptoMusic CryptoMusic
    {
        get => cryptoMusic;
    }
}


