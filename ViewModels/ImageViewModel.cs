using System.Windows.Media.Imaging;
using PropertyChanged;

namespace AddWaterMark.ViewModels {
    [AddINotifyPropertyChangedInterface]
    class ImageViewModel {
        public double ImageHeight { get; set; }
        public double ImageWidth { get; set; }
        public BitmapImage WaterMarkImage { get; set; }
    }
}
