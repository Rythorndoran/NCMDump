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

    private Task UpdateFileList()
    {
        MusicItems.MusicDescriptorList.Clear();

        return Task.Run(() =>
        {
            GC.Collect();
            Files.ForEach(file =>
            {
                if (NeteaseCryptoMusic.CheckFile(file) == false)
                    return;
                MusicItems.MusicDescriptorList.Add(new MusicDescriptor(file));
            });
        }
        );
    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
         UpdateFileList();
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        var grid = sender as Grid;
        var descriptor = (grid.BindingContext as MusicDescriptor);
        descriptor.IsItemChecked = !descriptor.IsItemChecked;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        var newFileList = MusicItems.MusicDescriptorList.Where(x => x.IsItemChecked).Select(x => x.FileName).ToList();
        Navigation.PushAsync(new PerformingAction(newFileList));
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

    private void MusicItemListView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
    {
    
    }
}