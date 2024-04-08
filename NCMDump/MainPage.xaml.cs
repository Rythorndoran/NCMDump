using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
namespace NCMDump
{
    public partial class MainPage : ContentPage
    {
        public string PickerMode
        {
            get;
            set;
        } = "Single";

        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = this;
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            if (PickerMode.Equals("Single"))
            {
                var result = await FilePicker.PickAsync();
                if (result != null)
                {
                    List<string> files = new List<string>();
                    files.Add(result.FullPath);
                    await Navigation.PushAsync(new FileListPage(files));
                }

            }
            else if (PickerMode.Equals("Multiple"))
            {
                var result = await FilePicker.PickMultipleAsync();
                if (result != null)
                {
                    List<string> files = new List<string>();
                    result.ToList().ForEach(file => { files.Add(file.FullPath); });
                    await Navigation.PushAsync(new FileListPage(files));
                }
            }
            else if (PickerMode.Equals("Folder"))
            {
                var result = await FolderPicker.Default.PickAsync();
                if (result.IsSuccessful)
                {
                    List<string> files = Directory.EnumerateFiles(result.Folder.Path).ToList();
                    await Navigation.PushAsync(new FileListPage(files));
                }
            }
        }

        private async void ContentPage_Loaded(object sender, EventArgs e)
        {
            if(await PermissionHelper.CheckPermission() == false)
            {
                await DisplayAlert("提示", "应用没有文件权限，点击授予权限", "确定");
                await PermissionHelper.RequestPermission();
            }
        }
    }
}
