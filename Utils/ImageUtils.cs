using AddWaterMark.Config;
using System.IO;

namespace AddWaterMark.Utils {
    class ImageUtils {
        /// <summary>
        /// bitmap转捣成BitmapImage
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static System.Windows.Media.Imaging.BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap) {
            System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            using (MemoryStream ms = new MemoryStream()) {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
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
        public static void SaveBitmapImageIntoFile(System.Windows.Media.Imaging.BitmapImage bitmapImage, string filePath) {
            System.Windows.Media.Imaging.BitmapEncoder encoder;
            string ext = Path.GetExtension(filePath);
            if (Constants.IMG_EXT_JPG.Equals(ext) || Constants.IMG_EXT_JPEG.Equals(ext)) {
                encoder = new System.Windows.Media.Imaging.JpegBitmapEncoder();
            } else if (Constants.IMG_EXT_BMP.Equals(ext)) {
                encoder = new System.Windows.Media.Imaging.BmpBitmapEncoder();
            } else if (Constants.IMG_EXT_PNG.Equals(ext)) {
                encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            } else {
                throw new System.Exception("不支持的图片格式");
            }
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapImage));

            using FileStream fileStream = new FileStream(filePath, FileMode.Create);
            encoder.Save(fileStream);
        }


    }
}
