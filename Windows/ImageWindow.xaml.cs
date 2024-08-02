using AddWaterMark.ViewModels;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AddWaterMark.Windows {
    /// <summary>
    /// ImageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImageWindow : Window {
        ImageViewModel vm = new ImageViewModel();
        private double imageHeight, imageWidth;
        private BitmapImage imageSource;
        public ImageWindow(double height, double width, BitmapImage image) {
            InitializeComponent();
            imageHeight = height;
            imageWidth = width;
            imageSource = image;
            Loaded += Window_Loaded;
            DataContext = vm;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            vm.ImageHeight = imageHeight;
            vm.ImageWidth = imageWidth;
            vm.WaterMarkImage = imageSource;
        }
    }
}
