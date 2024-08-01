using AddWaterMark.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
