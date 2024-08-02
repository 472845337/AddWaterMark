using PropertyChanged;
using System.Windows.Media.Imaging;

namespace AddWaterMark.ViewModels {
    [AddINotifyPropertyChangedInterface]
    class ImageViewModel {
        public double ImageHeight { get; set; }
        public double ImageWidth { get; set; }
        public BitmapImage WaterMarkImage { get; set; }
    }
}
