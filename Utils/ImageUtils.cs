using AddWaterMark.Config;
using System.IO;
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
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.Collect();
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
    }
}
