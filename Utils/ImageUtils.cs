using AddWaterMark.Config;
using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace AddWaterMark.Utils {
    class ImageUtils {
        /// <summary>
        /// bitmap转捣成BitmapImage
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap) {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream ms = new MemoryStream()) {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            return bitmapImage;
        }
        /// <summary>
        /// 把内存里的BitmapImage数据保存到硬盘中
        /// </summary>
        /// <param name="bitmapImage">BitmapImage数据</param>
        /// <param name="filePath">输出的文件路径</param>
        public static void SaveBitmapImageIntoFile(BitmapImage bitmapImage, string filePath) {
            string ext = Path.GetExtension(filePath);
            BitmapEncoder encoder = GetEncoder(ext);
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            using FileStream fileStream = new FileStream(filePath, FileMode.Create);
            encoder.Save(fileStream);
        }

        /// <summary>
        /// 获取图片的编码器
        /// </summary>
        /// <param name="imgExt"></param>
        /// <returns></returns>
        public static BitmapEncoder GetEncoder(string imgExt) {
            BitmapEncoder encoder;
            if (Constants.IMG_EXT_PNG.Equals(imgExt)) {
                encoder = new PngBitmapEncoder();
            } else if (Constants.IMG_EXT_BMP.Equals(imgExt)) {
                encoder = new BmpBitmapEncoder();
            } else if (Constants.IMG_EXT_GIF.Equals(imgExt)) {
                encoder = new GifBitmapEncoder();
            } else if (Constants.IMG_EXT_TIFF.Equals(imgExt)) {
                encoder = new TiffBitmapEncoder();
            } else if (Constants.IMG_EXT_WMP.Equals(imgExt)) {
                encoder = new WmpBitmapEncoder();
            } else {
                encoder = new JpegBitmapEncoder();
            }
            return encoder;
        }

        public static int[] GetFrameDelays(System.Drawing.Image gifImage) {
            // 获取GIF帧延迟
            System.Drawing.Imaging.PropertyItem frameDelayItem = gifImage.GetPropertyItem(0x5100); // 0x5100 is the PropertyTagFrameDelay
            int[] frameDelays = new int[gifImage.GetFrameCount(new System.Drawing.Imaging.FrameDimension(gifImage.FrameDimensionsList[0]))];
            for (int i = 0; i < frameDelays.Length; i++) {
                frameDelays[i] = BitConverter.ToInt32(frameDelayItem.Value, i * 4); // Convert to milliseconds
            }
            return frameDelays;
        }

        public static BitmapSource ImageToImageSource(System.Drawing.Image image) {
            using var bitmap = new System.Drawing.Bitmap(image);
            // 获取 Bitmap 的句柄
            var hBitmap = bitmap.GetHbitmap();

            try {
                // 使用 Imaging 创建 BitmapSource
                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                return bitmapSource;
            } finally {
                // 释放 GDI 对象
                DllUtils.DeleteObject(hBitmap);
            }
        }

        public static System.Drawing.Image ConvertToImage(RenderTargetBitmap renderTargetBitmap) {
            // 创建一个 BitmapEncoder（例如 PngBitmapEncoder）
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            // 将 BitmapEncoder 编码到内存流
            using var memoryStream = new MemoryStream();
            encoder.Save(memoryStream);

            // 从内存流加载 System.Drawing.Image
            memoryStream.Seek(0, SeekOrigin.Begin); // 重置流位置
            return System.Drawing.Image.FromStream(memoryStream);
        }
    }
}
