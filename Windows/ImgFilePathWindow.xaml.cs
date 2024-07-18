using AddWaterMark.Config;
using AddWaterMark.DataBase.Beans;
using AddWaterMark.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;


namespace AddWaterMark.Windows {
    /// <summary>
    /// ImgFilePathWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImgFilePathWindow : Window {
        public ImgFilePathViewModel vm = new ImgFilePathViewModel();
        public ImgFilePathWindow(ImgFilePath imgFilePath, ObservableCollection<ImgFilePath> imgFilePaths) {
            InitializeComponent();
            if (null != imgFilePath) {
                vm.Id = imgFilePath.Id;
                vm.FilePath = imgFilePath.FilePath;
                vm.WaterMark = imgFilePath.WaterMark;
            }
            vm.CurImgFilePaths = new List<ImgFilePath>(imgFilePaths);
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
            if (string.IsNullOrEmpty(vm.WaterMark)) {
                MessageBox.Show("水印不能为空！", Constants.MSG_ERROR);
                return;
            }
            if (!Directory.Exists(vm.FilePath)) {
                if (MessageBoxResult.Cancel == MessageBox.Show("当前配置目录不存在，确认继续添加吗？", Constants.MSG_WARN, MessageBoxButton.OKCancel)) {
                    return;
                }
            }
            bool isRepeatPath = GetRepeatPath(null, vm.FilePath, out string repeatPath, out string childPath);
            if (isRepeatPath) {
                if (!string.IsNullOrEmpty(repeatPath)) {
                    MessageBox.Show($"您已添加相同的目录：{repeatPath}");
                } else if (!string.IsNullOrEmpty(childPath)) {
                    MessageBox.Show($"您已添加目录存在包含关系：{childPath}");
                }
                return;
            }
            DialogResult = true;
        }

        /// <summary>
        /// 匹配到相同的目录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool GetRepeatPath(string id, string filePath, out string repeatPath, out string childPath) {
            bool isRepeat = false;
            repeatPath = string.Empty;
            childPath = string.Empty;

            foreach (ImgFilePath single in vm.CurImgFilePaths) {
                if (null != id && id.Equals(single.Id)) {
                    continue;
                }
                string singlePathLogFilePath = Path.Combine(single.FilePath, "log.txt");
                string filePathLogFilePath = Path.Combine(filePath, "log.txt");
                // 判断目录是否存在重复
                Uri singlePathLogFileUri = new Uri(singlePathLogFilePath);
                Uri filePathLogFileUri = new Uri(filePathLogFilePath);
                if (singlePathLogFileUri == filePathLogFileUri) {
                    isRepeat = true;
                    repeatPath = single.FilePath;
                    break;
                }
                // 判断是否子目录,如果目录长度相同，表示不可能是子目录
                if (singlePathLogFilePath.Length != filePathLogFilePath.Length) {
                    FileInfo singlePathLogFileInfo = new FileInfo(singlePathLogFilePath);
                    FileInfo filePathLogFileInfo = new FileInfo(filePathLogFilePath);
                    // 长目录
                    DirectoryInfo dir1 = singlePathLogFilePath.Length > filePathLogFilePath.Length ? singlePathLogFileInfo.Directory : filePathLogFileInfo.Directory;
                    // 短目录
                    DirectoryInfo dir2 = singlePathLogFilePath.Length < filePathLogFilePath.Length ? singlePathLogFileInfo.Directory : filePathLogFileInfo.Directory;
                    bool isChild = false;
                    while (dir1.FullName.Length > dir2.FullName.Length) {
                        if (string.Equals(dir1.Parent.FullName, dir2.FullName, StringComparison.CurrentCultureIgnoreCase)) {
                            isChild = true;
                            break;
                        } else {
                            dir1 = dir1.Parent;
                        }
                    }
                    if (isChild) {
                        isRepeat = true;
                        childPath = single.FilePath;
                        break;
                    }
                }

            }
            return isRepeat;
        }


        private void Cancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
