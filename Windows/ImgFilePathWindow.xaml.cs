using AddWaterMark.Config;
using AddWaterMark.DataBase.Beans;
using AddWaterMark.ViewModels;
using System;
using System.IO;
using System.Windows;


namespace AddWaterMark.Windows {
    /// <summary>
    /// ImgFilePathWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImgFilePathWindow : Window {
        public ImgFilePathViewModel vm = new ImgFilePathViewModel();
        public ImgFilePathWindow(ImgFilePath imgFilePath) {
            InitializeComponent();
            if (null != imgFilePath) {
                vm.Id = imgFilePath.Id;
                vm.FilePath = imgFilePath.FilePath;
                vm.WaterMark = imgFilePath.WaterMark;
            }
            Loaded += Window_Loaded;
            Closing += Window_Closing;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            DataContext = vm;
        }
        private void Window_Closing(object sender, EventArgs e) {
            DataContext = null;
        }
        private void SelectImgFilePath_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (System.Windows.Forms.DialogResult.OK == folderBrowserDialog.ShowDialog()) {
                vm.FilePath = folderBrowserDialog.SelectedPath;
            }
        }

        private void SaveImgFilePath_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(vm.FilePath)) {
                MessageBox.Show("图片目录不能为空！", Constants.MSG_ERROR);
                return;
            }
            if (Directory.Exists(vm.FilePath)) {
                DialogResult = true;
            } else {
                if (MessageBoxResult.OK == MessageBox.Show("当前配置目录不存在，确认添加吗？", Constants.MSG_WARN, MessageBoxButton.OKCancel)) {
                    DialogResult = true;
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
