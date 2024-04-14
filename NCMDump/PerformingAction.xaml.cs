using CommunityToolkit.Maui.Storage;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;

namespace NCMDump;

public partial class PerformingAction : ContentPage
{
    public ObservableCollection<MusicDescriptor> MusicListItems { get; set; } = new();
    private List<string> Files;
    private bool IsFinishWork = false;


    public PerformingAction(List<string> files)
    {
        InitializeComponent();
        this.BindingContext = this;
        Files = files;
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        await UpdateFileList();
    }

    private async void ProcessFiles()
    {
        Button.IsEnabled = false;
        Button.IsVisible = false;
        ActionProgressBar.IsVisible = true;
        var result = await FolderPicker.Default.PickAsync();
        string outdir;
        if (result.IsSuccessful)
        {
            outdir = result.Folder.Path;
        }
        else
        {
            await DisplayAlert("��ʾ", "��ѡ�񱣴�Ŀ¼", "ȷ��");
            Button.IsEnabled = true;
            Button.IsVisible = true;
            ActionProgressBar.IsVisible = false;
            return;
        }
        int totalItems = MusicListItems.Count;
        int processedItems = 0;
        await Task.Run(() =>
        {
            foreach (var item in MusicListItems)
            {
                item.Status = "Processing";

                if (item.CryptoMusic.WriteDecryptMusic(outdir))
                {
                    item.Status = "OK";
                }
                else
                {
                    item.Status = "Fail";
                }
                processedItems++;
                double progress = (double)processedItems / totalItems;
                MainThread.BeginInvokeOnMainThread(() => ActionProgressBar.Progress = progress);
                MainThread.BeginInvokeOnMainThread(() => MusicItemListView.ScrollTo(processedItems));

                //�µ���̫�챻������������
                if((GlobalVars.Configs.DownloadCoverImage && item.CryptoMusic.IsEnabedImages) || GlobalVars.Configs.DownloadLyric)
                {
                    Thread.Sleep(1000);
                }
            }
        });
        ActionProgressBar.IsVisible = false;
        Button.IsEnabled = true;
        Button.IsVisible = true;
        Button.Text = "����";
        IsFinishWork = true;
    }

    private string FileSizeToString(long len)
    {
        long fileSizeInBytes = len;
        double fileSizeInKB = fileSizeInBytes / 1024.0; // �ֽ�ת��Ϊǧ�ֽڣ�KB��
        double fileSizeInMB = fileSizeInKB / 1024.0; // ǧ�ֽ�ת��Ϊ���ֽڣ�MB��
        string FileSize = $"{Math.Round(fileSizeInMB, 2)} MB";
        return FileSize;
    }


    private Task UpdateFileList()
    {
        MusicListItems.Clear();
        return Task.Run(() =>
        {
            Files.ForEach(file =>
            {
                if (NeteaseCryptoMusic.CheckFile(file) == false)
                    return;

                MusicListItems.Add(new MusicDescriptor(file));
            });
        }
        );
    }


    private void Button_Clicked(object sender, EventArgs e)
    {
        if (IsFinishWork)
        {
            Navigation.PopToRootAsync();
            return;
        }
        ProcessFiles();
    }
}