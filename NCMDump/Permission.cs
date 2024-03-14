using Android.OS;
using Android.Content.PM;
using Android.Runtime;
using Android.Content;

namespace NCMDump
{
    public partial class MainActivity : Activity
    {
        private const int ManageAppAllFilesPermissionRequestCode = 3333;

        private Action<int, string[], Permission[]> OnRequestPermissionsResultAction;
        private Action<int, Result, Intent> ManageAppAllFilesPermissionOnActivityResultAction;

        public bool CheckPermissionGranted()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                return (Android.OS.Environment.IsExternalStorageManager);
            }
            else
            {
                if (base.CheckSelfPermission(Android.Manifest.Permission.WriteExternalStorage) != Permission.Granted)
                {
                    return false;
                }
                if (base.CheckSelfPermission(Android.Manifest.Permission.ReadExternalStorage) != Permission.Granted)
                {
                    return false;
                }
                return true;
            }
        }

        public Task<bool> RequestManageAppAllFilesPermission()
        {
            Android.Content.Intent intent = new Android.Content.Intent(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);
            Android.Net.Uri uri = Android.Net.Uri.FromParts("package", base.PackageName, null);
            intent.SetData(uri);
            var tcs = new TaskCompletionSource<bool>();
            ManageAppAllFilesPermissionOnActivityResultAction = (requestCode, resultCode, data) =>
            {
                tcs.SetResult(Android.OS.Environment.IsExternalStorageManager);
            };
            StartActivityForResult(intent, ManageAppAllFilesPermissionRequestCode);
            return tcs.Task;
        }

        public Task<bool> RequestPermissionAsync(string permission)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (CheckSelfPermission(permission) == Permission.Granted)
            {
                tcs.SetResult(true); // 直接返回true，因为权限已经被授予
            }
            else
            {
                // 重写OnRequestPermissionsResult以处理权限请求结果
                OnRequestPermissionsResultAction = (requestCode, permissions, grantResults) =>
                {
                    if (requestCode == 1)
                    {
                        var granted = grantResults.Length > 0 && grantResults[0] == Permission.Granted;
                        tcs.SetResult(granted);
                    }
                };
                base.RequestPermissions(new string[] { permission }, 0);
            }

            return tcs.Task;
        }

        public async Task<bool> RequestPermission()
        {
            if (Android.OS.Build.VERSION.SdkInt > BuildVersionCodes.Q)
            {
                bool status = true;
                if (Android.OS.Environment.IsExternalStorageManager is false)
                     status = await RequestManageAppAllFilesPermission();
                return status;
            }
            else
            {
                bool status = true;
                if (base.CheckSelfPermission(Android.Manifest.Permission.WriteExternalStorage) != Permission.Granted)
                {
                    status = await RequestPermissionAsync(Android.Manifest.Permission.WriteExternalStorage);
                }
                if (base.CheckSelfPermission(Android.Manifest.Permission.ReadExternalStorage) != Permission.Granted)
                {
                    status = await RequestPermissionAsync(Android.Manifest.Permission.ReadExternalStorage);
                }
                return status;
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            OnRequestPermissionsResultAction?.Invoke(requestCode, permissions, grantResults);
        }
    }
}
