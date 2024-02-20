using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCMDump
{
    public class FileViewControl : LinearLayout
    {
        private LinearLayout textLayout;
        private ImageView imageView;
        private TextView bigTextView;
        private TextView smallTextView;
        private CheckBox checkBox;

        public FileViewControl(Context context) : base(context)
        {
            Initialize(context);
        }

        private void Initialize(Context context)
        {
            // 创建线性布局
            LinearLayout mainLayout = new LinearLayout(context);
            mainLayout.Orientation = Orientation.Horizontal;
            mainLayout.SetPadding(10, 10, 10, 10); // 设置整个布局的上下左右空隙
            LayoutParams mainLayoutParams = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
            AddView(mainLayout, mainLayoutParams);

            // 计算基于屏幕密度的图标大小
            float density = Resources.DisplayMetrics.Density;
            int iconSize = (int)(60 * density); // 60dp × density 转换为px

            // 添加ImageView
            imageView = new ImageView(context);
            LayoutParams imageParams = new LayoutParams(iconSize, iconSize); // 使用计算出的大小
            imageParams.SetMargins(0, 0, 10, 0); // 在图标和文本之间设置空隙
            mainLayout.AddView(imageView, imageParams);

            // 添加包含大字体文本和小字体文本的垂直布局
            textLayout = new LinearLayout(context);
            textLayout.Orientation = Orientation.Vertical;
            LayoutParams textLayoutParams = new LayoutParams(0, LayoutParams.WrapContent, 1); // 使用权重确保文本占据剩余空间
            mainLayout.AddView(textLayout, textLayoutParams);

            // 添加大字体TextView
            bigTextView = new TextView(context);
            bigTextView.SetTextSize(ComplexUnitType.Sp, 20);
            textLayout.AddView(bigTextView);

            // 添加小字体TextView
            smallTextView = new TextView(context);
            smallTextView.SetTextSize(ComplexUnitType.Sp, 12);
            textLayout.AddView(smallTextView);

            // 添加CheckBox
            checkBox = new CheckBox(context);
            LayoutParams checkBoxParams = new LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);
            checkBoxParams.SetMargins(10, 0, 0, 0); // 在选择框和文本之间设置空隙
            mainLayout.AddView(checkBox, checkBoxParams);
        }
        public void SetImage(int resId) => imageView.SetImageResource(resId);

        public void SetImage(Bitmap bitmap) => imageView.SetImageBitmap(bitmap);

        public void SetImage(byte[] bytes)
        {
            var bitmap = ByteArrayToBitmap(bytes);
            imageView.SetImageBitmap(bitmap);
        }

        public void SetBigText(string text) => bigTextView.Text = text;

        public void SetSmallText(string text) => smallTextView.Text = text;

        public bool Checked { get => checkBox.Checked; set => checkBox.Checked = value; }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            // 设置控件的最小尺寸
            SetMeasuredDimension(MeasuredWidth, Math.Max(100, MeasuredHeight)); // 控件高度最小为100
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
