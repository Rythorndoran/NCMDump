using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCMDump
{
    internal class NCMFileViewControl : LinearLayout
    {
        private ImageView imageView;
        private TextView musicNameTextView;
        private TextView musicInfoTextView;
        private TextView fileInfoTextView;
        private LinearLayout linearLayout;
        private CheckBox checkBox;

        public NCMFileViewControl(Context context) : base(context)
        {

            Initialize(context);
        }

        private void Initialize(Context context)
        {
            LayoutInflater.From(context).Inflate(Resource.Layout.NCMFileItem, this, true);
            imageView = FindViewById<ImageView>(Resource.Id.MusicImageView);
            musicNameTextView = FindViewById<TextView>(Resource.Id.MusicNameTextView);
            musicInfoTextView = FindViewById<TextView>(Resource.Id.MusicInfoTextView);
            fileInfoTextView = FindViewById<TextView>(Resource.Id.FileInfoTextView);
            linearLayout = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
            checkBox = FindViewById<CheckBox>(Resource.Id.CheckBox1);

            this.Click += NCMFileViewClick; // 添加点击事件
        }

        // 添加其他操作方法
        public void SetImage(int resId)
        {
            imageView.SetImageResource(resId);
        }

        public void SetImage(Bitmap bitmap) => imageView.SetImageBitmap(bitmap);
        public void SetImage(byte[] bytes)
        {
            var bitmap = ByteArrayToBitmap(bytes);
            imageView.SetImageBitmap(bitmap);
        }

        public void SetMusicNameText(string musicName)
        {
            musicNameTextView.Text = musicName;
        }

        public void SetFileInfoText(string fileInfo)
        {
            fileInfoTextView.Text = fileInfo;
        }

        public void SetMusicInfoText(string musicInfo)
        {
            musicInfoTextView.Text = musicInfo;
        }

        public void SetCheckBoxChecked(bool isChecked)
        {
            checkBox.Checked = isChecked;
        }

        public bool GetCheckBoxChecked()
        {
            return checkBox.Checked;
        }

        private void NCMFileViewClick(object sender, EventArgs e)
        {
            SetCheckBoxChecked(!GetCheckBoxChecked());
        }
        private Bitmap ByteArrayToBitmap(byte[] byteArray)
        {
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InMutable = true;

            Bitmap bitmap = BitmapFactory.DecodeByteArray(byteArray, 0, byteArray.Length, options);

            return bitmap;
        }
    }
}
