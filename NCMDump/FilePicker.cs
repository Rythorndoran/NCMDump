using Android;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Views;


namespace NCMDump
{
    public partial class MainActivity : Activity
    {
        private const int FilePickerRequestCode = 2333;

        private Action<int, Result, Intent> FilePickerOnActivityResultAction;

        public string GetPathFromUri(Android.Net.Uri treeUri)
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

        public Task<string> SelectPath()
        {
            var intent = new Intent(Intent.ActionOpenDocumentTree);
            var tcs = new TaskCompletionSource<string>();
            FilePickerOnActivityResultAction = (requestCode, resultCode, data) =>
            {
                string DirectoryPath = GetPathFromUri(data.Data);
                tcs.SetResult(DirectoryPath);
            };
            StartActivityForResult(intent, FilePickerRequestCode);
            return tcs.Task;
        }
    }
}
