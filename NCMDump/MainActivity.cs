using Android.Content;
using Android.Graphics;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Java.Interop;
using static Android.Icu.Text.CaseMap;
using Android;
using Android.Text.Method;
using Android.Text.Style;
using Android.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace NCMDump
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
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

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            GetPermission();

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


            ShowToast("请选择路径");
            Intent intent = new Intent(Intent.ActionOpenDocumentTree);
            StartActivityForResult(intent, 0);
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
                    ShowToast("请选择输出路径");
                    var intent = new Intent(Intent.ActionOpenDocumentTree);
                    StartActivityForResult(intent, 1);
                    return true;

                case Resource.Id.SelectDumpPathActionItem:
                    intent = new Intent(Intent.ActionOpenDocumentTree);
                    StartActivityForResult(intent, 0);
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Ok)
            {
                if (requestCode == 0)
                {
                    var selectedPath = GetRealPathFromTreeUri(data.Data);
                    UpdateCurrentFolder(selectedPath);
                    Console.WriteLine(selectedPath);
                    ShowToast("选择路径：" + selectedPath);
                }
                else if (requestCode == 1)
                {
                    SaveFolder = GetRealPathFromTreeUri(data.Data);
                    Console.WriteLine(SaveFolder);
                    ShowToast("输出路径：" + SaveFolder);
                    DumpFiles();
                }
                if (requestCode == 2)
                {
                    Console.WriteLine("权限已授权");
                }
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

        private void UpdateCurrentFolder(string? newfolder)
        {
            if (newfolder != null)
            {
                CurrentFolder = newfolder;
            }
            UpdateFilesFromFolder();
        }

        private void AddMusicItem2(string filepath)
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
                AddMusicItem2(item);
                count ++;
            }
            if(count == 0)
            {
                ShowToast("当前未找到NCM文件，请检查路径或者文件权限！");
            }
        }

        private async void DumpFiles()
        {
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
                if(decryptedcount == 0)
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

        private string GetRealPathFromTreeUri(Android.Net.Uri treeUri)
        {
            // 获取Uri的路径部分
            var docId = DocumentsContract.GetTreeDocumentId(treeUri);
            // 分割得到的文档ID来获得路径段
            var parts = docId.Split(':');

            if (parts.Length >= 2 && "primary".Equals(parts[0], StringComparison.OrdinalIgnoreCase))
            {
                // 构造真实路径，这依赖于内部存储的根目录被挂载在 /storage/emulated/0/
                return Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/" + parts[1];
            }

            return null;
        }

        private void GetPermission()
        {
            if (base.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) != Permission.Granted)
            {
                base.RequestPermissions(new string[] {
                    Manifest.Permission.WriteExternalStorage,
                    Manifest.Permission.ReadExternalStorage }, 0);
            }

            if (Android.OS.Build.VERSION.SdkInt > BuildVersionCodes.Q)
            {
                if (Android.OS.Environment.IsExternalStorageManager != true)
                {
                    var intent = new Intent(Settings.ActionManageAllFilesAccessPermission);
                    StartActivityForResult(intent, 2);
                }
            }
        }
    }
}