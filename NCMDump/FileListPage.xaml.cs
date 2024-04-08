using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Storage;
using System.Collections.Immutable;
using System.Diagnostics;

namespace NCMDump;

public partial class FileListPage : ContentPage
{
    private List<string> Files;
    public MusicItemList MusicItems { get; set; } = new();

    private int SelectedFileCount = 0;

    public FileListPage(List<string> files)
    {
        InitializeComponent();
        this.BindingContext = this;
        Files = files;
    }

    private string FileSizeToString(long len)
    {
        long fileSizeInBytes = len;
        double fileSizeInKB = fileSizeInBytes / 1024.0; // 字节转换为千字节（KB）
        double fileSizeInMB = fileSizeInKB / 1024.0; // 千字节转换为兆字节（MB）
        string FileSize = $"{Math.Round(fileSizeInMB, 2)} MB";
        return FileSize;
    }

    private Task UpdateFileList()
    {
        MusicItems.MusicDescriptorList.Clear();
        return Task.Run(() =>
        {
            Files.ForEach(file =>
            {
                Debug.WriteLine(file);
                var cryptoMusic = NeteaseCryptoMusic.FromFile(file);

                if (cryptoMusic == null)
                    return;

                var fileinfo = new System.IO.FileInfo(file);
                string fileinfo_string = cryptoMusic.MetaData.Format +  " | " + FileSizeToString(fileinfo.Length)  ;
                MusicItems.MusicDescriptorList.Add(new MusicDescriptor(file, cryptoMusic.MetaData.MusicName, fileinfo_string, cryptoMusic.CoverImage, cryptoMusic));
            });
        }
        );
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        await UpdateFileList();
        if(MusicItems.MusicDescriptorList.Count == 0)
        {
            await DisplayAlert("警告", "没有有效的文件！", "确定");
        }
        else
        {
            NextStepButton.IsEnabled = true;
            SelectedFileCount = MusicItems.MusicDescriptorList.Count;
        }
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        var grid = sender as Grid;
        var descriptor = (grid.BindingContext as MusicDescriptor);
        descriptor.IsItemChecked = !descriptor.IsItemChecked;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new PerformingAction(MusicItems.MusicDescriptorList.Where(x => x.IsItemChecked).Select(x => x.FileName).ToList()));
    }

    private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value == false)
        {
            SelectedFileCount--;
        }
        else
        {
            SelectedFileCount++;
        }
        if (SelectedFileCount == 0)
        {
            NextStepButton.IsEnabled = false;
        }
        else
        {
            NextStepButton.IsEnabled = true;
        }
    }
}