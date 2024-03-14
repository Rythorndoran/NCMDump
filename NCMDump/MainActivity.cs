using Android;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Views;

namespace NCMDump
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public partial class MainActivity : Activity
    {
        private string CurrentFolder = "";
        private string SaveFolder = "";

        private LinearLayout fileListLinearLayout;
        private ScrollView scrollView;
        private LinearLayout scrollContentLayout;

        private Dictionary<NCMFileViewControl, string> fileDictionary = new Dictionary<NCMFileViewControl, string>();

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Initialize(this);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.Menu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.AboutDialogActionItem:
                    ShowAboutDialog();
                    return true;

                case Resource.Id.StartDumpActionItem:
                    ShowToast("选择保存目录");
                    StartDumpFiles();
                    return true;

                case Resource.Id.SelectDumpPathActionItem:
                    UpdateCurrentFolder();
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode is FilePickerRequestCode)
            {
                FilePickerOnActivityResultAction?.Invoke(requestCode, resultCode, data);
            }
            if (requestCode is ManageAppAllFilesPermissionRequestCode)
            {
                ManageAppAllFilesPermissionOnActivityResultAction?.Invoke(requestCode, resultCode, data);
            }
        }

        private async void Initialize(Context context)
        {
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            fileListLinearLayout = FindViewById<LinearLayout>(Resource.Id.FileListLinearLayout);

            // Create ScrollView and add it to the LinearLayout
            scrollView = new ScrollView(this);
            LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.MatchParent);
            scrollView.LayoutParameters = layoutParams;
            fileListLinearLayout.AddView(scrollView);

            // Create a new LinearLayout to put inside the ScrollView
            scrollContentLayout = new LinearLayout(this);
            scrollContentLayout.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
            scrollContentLayout.Orientation = Android.Widget.Orientation.Vertical;
            scrollView.AddView(scrollContentLayout);
            if (CheckPermissionGranted() is false)
            {

                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                AlertDialog alertDialog = builder.Create();
                alertDialog.SetMessage("文件访问权限未授予，程序可能无法正常运行,请授予文件访问权限");
                alertDialog.SetButton("确定", async (s, e) =>
                {
                    alertDialog.Dismiss();
                    bool status = await RequestPermission();
                    if (status is false)
                    {
                        ShowToast("请授予文件权限！");
                    }
                });
                alertDialog.Show();
            }
            else
            {
                UpdateCurrentFolder();
            }
        }

        private void ShowToast(string msg)
        {
            var toast = Toast.MakeText(this, msg, ToastLength.Short);
            if (toast != null)
            {
                toast.Show();
            }
        }

        private void ShowAboutDialog()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            AlertDialog alertDialog = builder.Create();
            alertDialog.SetTitle("关于");
            alertDialog.SetMessage("作者：酷安@Nafany");
            alertDialog.SetButton("关闭", (s, e) =>
            {
                alertDialog.Dismiss();
            });
            alertDialog.Show();
        }

        private async void UpdateCurrentFolder()
        {
            if (CheckPermissionGranted() is false)
            {

                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                AlertDialog alertDialog = builder.Create();
                alertDialog.SetMessage("文件访问权限未授予，程序可能无法正常运行,请授予文件访问权限");
                alertDialog.SetButton("确定", async (s, e) =>
                {
                    alertDialog.Dismiss();
                    bool status = await RequestPermission();
                    if (status is false)
                    {
                        ShowToast("请授予文件权限！");
                    }
                });
                alertDialog.Show();
                return;
            }

            CurrentFolder = await SelectPath();
            Console.WriteLine("Path:" + CurrentFolder);
            if(CurrentFolder is not "")
            {
                UpdateFilesFromFolder();
            }
        }

        private void AddMusicItem(string filepath)
        {
            //NCMFileViewControl

            var fileInfo = new FileInfo(filepath);
            var ncm_metadata = NeteaseCrypto.GetMetaData(filepath);
            var ViewControl = new NCMFileViewControl(this);
            var Image = NeteaseCrypto.GetImageBytes(filepath);
            if (Image is not null)
            {
                ViewControl.SetImage(Image);
            }
            else
            {
                ViewControl.SetImage(Resource.Mipmap.ic_launcher);
            }

            string MusicName = NeteaseCrypto.GetMusicNameFromMetaData(ncm_metadata);
            string ArtistNames = NeteaseCrypto.GetArtistNamesFromMetaData(ncm_metadata);
            string FileExtensionName = NeteaseCrypto.GetFormatFromMetaData(ncm_metadata);

            long fileSizeInBytes = fileInfo.Length;
            double fileSizeInKB = fileSizeInBytes / 1024.0; // 字节转换为千字节（KB）
            double fileSizeInMB = fileSizeInKB / 1024.0; // 千字节转换为兆字节（MB）
            string FileSize = $"{Math.Round(fileSizeInMB, 2)} MB";

            ViewControl.SetMusicNameText(MusicName);
            ViewControl.SetMusicInfoText($"Artist：{ArtistNames}");
            ViewControl.SetFileInfoText($"Size: {FileSize} Format: {FileExtensionName}");
            ViewControl.SetCheckBoxChecked(true);
            scrollContentLayout.AddView(ViewControl);
            fileDictionary.Add(ViewControl, filepath);
        }

        private void UpdateFilesFromFolder()
        {
            scrollContentLayout.RemoveAllViews();
            fileDictionary.Clear();

            Console.WriteLine(CurrentFolder);
            var files = Directory.EnumerateFiles(CurrentFolder);
            int count = 0;
            foreach (var item in files)
            {
                FileInfo fileInfo = new FileInfo(item);
                if (fileInfo.Extension != ".ncm")
                    continue;
                AddMusicItem(item);
                count++;
            }
            if (count == 0)
            {
                ShowToast("当前未找到NCM文件，请检查路径或者文件权限！");
            }
        }

        private async void StartDumpFiles()
        {
            SaveFolder = await SelectPath();
            List<string> list = new List<string>();
            foreach (var item in fileDictionary)
            {
                if (item.Key.GetCheckBoxChecked())
                {
                    list.Add(item.Value);
                }
            }
            var totalFiles = list.Count;
            var processedFiles = 0;
            var decryptedcount = 0;


            // 创建一个进度条对话框
            ProgressDialog progressDialog = new ProgressDialog(this);
            progressDialog.SetMessage("Loading...");
            progressDialog.SetCancelable(false);
            progressDialog.SetTitle("");
            progressDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            progressDialog.Max = totalFiles; // 设置最大进度为总文件数

            progressDialog.Show();

            var task = Task.Run(() =>
            {
                foreach (var item in list)
                {
                    processedFiles++;
                    // 在UI线程上更新进度条和消息
                    RunOnUiThread(() =>
                    {
                        progressDialog.Progress = processedFiles;
                        progressDialog.SetMessage(item);
                    });

                    if (NeteaseCrypto.WriteMusicFile(item, SaveFolder))
                    {
                        decryptedcount++;
                    }
                }
                if (decryptedcount == 0)
                {
                    RunOnUiThread(() => { progressDialog.Dismiss(); ShowToast("没有任何文件被解密，请检查路径或者文件权限！"); });
                }
                else
                {
                    // 在UI线程上关闭进度条对话框
                    RunOnUiThread(() => { progressDialog.Dismiss(); ShowToast("成功解密" + decryptedcount + "个文件"); });
                }
            });
            await task;
        }
    }
}