using AddWaterMark.Beans;
using AddWaterMark.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;


namespace AddWaterMark.Windows {
    /// <summary>
    /// ImgFilePathWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImgFilePathWindow : Window {
        public ImgFilePathViewModel vm = new ImgFilePathViewModel();
        public ImgFilePathWindow(ImgFilePath imgFilePath, ObservableCollection<ImgFilePath> imgFilePaths) {
            InitializeComponent();
            SourceInitialized += Window_SourceInitialized;
            if (null != imgFilePath) {
                vm.Id = imgFilePath.Id;
                vm.FilePath = imgFilePath.FilePath;
                vm.WaterMark = imgFilePath.WaterMark;
                vm.IsChild = imgFilePath.IsChild;
                vm.IncludeExt = imgFilePath.IncludeExt;
                vm.ExcludeExt = imgFilePath.ExcludeExt;
                vm.ExcludeChild = imgFilePath.ExcludeChild;
                // 排除子目录解析成多行编辑
                List<string> excludeChildList = ImgFilePath.GetExcludeChildList(imgFilePath.ExcludeChild);
                if (null != excludeChildList) {
                    foreach (string excludeChild in excludeChildList) {
                        vm.ExcludeChildList.Add(new ExcludeChildItem { Value = excludeChild });
                    }
                }
            }
            vm.CurImgFilePaths = new List<ImgFilePath>(imgFilePaths);
            Loaded += Window_Loaded;
            Closing += Window_Closing;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            DataContext = vm;
        }

        #region 窗口尺寸：自适应内容高度，用户手动调整后切换为普通窗口，由列表区域吸收多出的空间
        private const int WM_SIZING = 0x0214;// 用户拖动边框改变大小
        private const int WM_SIZE = 0x0005;
        private const int SIZE_MAXIMIZED = 2;
        private bool manualSize = false;

        private void Window_SourceInitialized(object sender, EventArgs e) {
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            System.Windows.Interop.HwndSource.FromHwnd(handle).AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            // 用户拖动边框或最大化时，放弃高度自适应，转成普通窗口（否则内容不会跟随拉伸，底部留白）
            if (!manualSize && (msg == WM_SIZING || (msg == WM_SIZE && (int)wParam == SIZE_MAXIMIZED))) {
                manualSize = true;
                SizeToContent = SizeToContent.Manual;
                // 以自适应完成时的紧凑尺寸作为最小尺寸
                if (MinHeight < ActualHeight) {
                    MinHeight = ActualHeight;
                }
                if (MinWidth < ActualWidth) {
                    MinWidth = ActualWidth;
                }
            }
            return IntPtr.Zero;
        }
        #endregion
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
                MessageBox.Show(Lang.Find("PathEmpty"), Lang.Find("Msgbox_Error"));
                return;
            }
            if (string.IsNullOrEmpty(vm.WaterMark)) {
                MessageBox.Show(Lang.Find("TextEmpty"), Lang.Find("Msgbox_Error"));
                return;
            }
            if (!Directory.Exists(vm.FilePath)) {
                if (MessageBoxResult.Cancel == MessageBox.Show(Lang.Find("PathNotExist"), Lang.Find("Msgbox_Warn"), MessageBoxButton.OKCancel)) {
                    return;
                }
            }
            // 排除子目录多行归一化后拼接成存储形式（重复校验需要用到，先拼接）
            vm.ExcludeChild = ImgFilePath.JoinExcludeChildList(vm.ExcludeChildList.Select(item => ImgFilePath.NormalizeExcludeChild(item.Value, vm.FilePath)).ToList());
            bool isRepeatPath = GetRepeatPath(vm.Id, vm.FilePath, out string repeatPath, out string childPath, out string rootPath);
            if (isRepeatPath) {
                if (!string.IsNullOrEmpty(repeatPath)) {
                    MessageBox.Show($"{Lang.Find("SamePathExist")}{repeatPath}");
                } else if (!string.IsNullOrEmpty(childPath)) {
                    // 提示冲突原因和两种解决方法
                    MessageBox.Show($"{Lang.Find("ChildPath")}{childPath}{Environment.NewLine}{string.Format(Lang.Find("ChildPathTip"), rootPath, childPath)}");
                }
                return;
            }
            DialogResult = true;
        }

        #region 排除子目录编辑
        /// <summary>
        /// 新增一行排除子目录
        /// </summary>
        private void AddExcludeChild_Click(object sender, RoutedEventArgs e) {
            vm.ExcludeChildList.Add(new ExcludeChildItem());
        }

        /// <summary>
        /// 删除一行排除子目录
        /// </summary>
        private void DeleteExcludeChild_Click(object sender, RoutedEventArgs e) {
            FrameworkElement element = sender as FrameworkElement;
            ExcludeChildItem item = element?.DataContext as ExcludeChildItem;
            if (null != item) {
                vm.ExcludeChildList.Remove(item);
            }
        }

        /// <summary>
        /// 选择目录填入当前行：主目录之内取相对路径，主目录之外只取目录名
        /// </summary>
        private void SelectExcludeChild_Click(object sender, RoutedEventArgs e) {
            FrameworkElement element = sender as FrameworkElement;
            ExcludeChildItem item = element?.DataContext as ExcludeChildItem;
            if (null == item) {
                return;
            }
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (System.Windows.Forms.DialogResult.OK == folderBrowserDialog.ShowDialog()) {
                string normalized = ImgFilePath.NormalizeExcludeChild(folderBrowserDialog.SelectedPath, vm.FilePath);
                if (string.IsNullOrEmpty(normalized)) {
                    MessageBox.Show(Lang.Find("ExcludeChildPickRootError"), Lang.Find("Msgbox_Error"));
                    return;
                }
                item.Value = normalized;
            }
        }
        #endregion



        /// <summary>
        /// 匹配到相同的目录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool GetRepeatPath(long? id, string filePath, out string repeatPath, out string childPath, out string rootPath) {
            bool isRepeat = false;
            repeatPath = string.Empty;
            childPath = string.Empty;
            rootPath = string.Empty;

            foreach (ImgFilePath single in vm.CurImgFilePaths) {
                if (null != id && id.Equals(single.Id)) {
                    continue;
                }
                string singlePathLogFilePath = Path.Combine(single.FilePath, "log.txt");
                string filePathLogFilePath = Path.Combine(filePath, "log.txt");
                try {
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
                            // 长路径是子目录，短路径是主目录
                            bool singleIsRoot = singlePathLogFilePath.Length < filePathLogFilePath.Length;
                            string childDir = singleIsRoot ? filePath : single.FilePath;
                            string rootDir = singleIsRoot ? single.FilePath : filePath;
                            string rootExcludeChild = singleIsRoot ? single.ExcludeChild : vm.ExcludeChild;
                            // 子目录已被主目录方的排除名单排除，双方不会重复处理同一批文件，允许添加
                            if (ImgFilePath.IsExcludedChild(childDir, rootDir, ImgFilePath.GetExcludeChildList(rootExcludeChild))) {
                                continue;
                            }
                            isRepeat = true;
                            childPath = childDir;
                            rootPath = rootDir;
                            break;
                        }
                    }
                } catch (Exception) {

                }
            }
            return isRepeat;
        }
    }
}
