using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if __ANDROID__
using Android.OS;
#endif

namespace NCMDump
{
    internal class PermissionHelper
    {
        public static async Task<bool> RequestPermission()
        {
#if __ANDROID__
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                if (!Android.OS.Environment.IsExternalStorageManager)
                {
                    Android.Content.Intent intent = new Android.Content.Intent(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);
                    Android.Net.Uri uri = Android.Net.Uri.FromParts("package", AppInfo.PackageName, null);
                    intent.SetData(uri);
                    Platform.CurrentActivity.StartActivity(intent);
                }
                return (Android.OS.Environment.IsExternalStorageManager);
            }
            else
            {
                var statusW = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                if (statusW != PermissionStatus.Granted)
                {
                    statusW = await Permissions.RequestAsync<Permissions.StorageWrite>();
                }
                if (statusW != PermissionStatus.Granted)
                {
                    return false;
                }

                statusW = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                if (statusW != PermissionStatus.Granted)
                {
                    statusW = await Permissions.RequestAsync<Permissions.StorageRead>();
                }
                if (statusW != PermissionStatus.Granted)
                {
                    return false;
                }
            }
            return true;
#else
            return true;
#endif
        }

        public static async Task<bool> CheckPermission()
        {
#if __ANDROID__
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                return (Android.OS.Environment.IsExternalStorageManager);
            }
            else
            {
                var statusW = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                if (statusW != PermissionStatus.Granted)
                {
                    return false;
                }
                statusW = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                if (statusW != PermissionStatus.Granted)
                {
                    return false;
                }
            }
            return true;
#else
            return true;
#endif
        }
    }
}
