using AddWaterMark.Config;
using AddWaterMark.DataBase.Beans;
using AddWaterMark.DataBase.Services;
using AddWaterMark.Utils;
using AddWaterMark.ViewModels;
using AddWaterMark.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace AddWaterMark {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        // 图片加水印定时器 10分钟执行一次
        private readonly System.Windows.Threading.DispatcherTimer ImgWaterMarkTimer = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMinutes(10) };
        // 自动GC定时器 5分钟执行一次
        private readonly System.Windows.Threading.DispatcherTimer AutoGcTimer = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMinutes(5) };
        // 操作信息定时器
        private readonly System.Windows.Threading.DispatcherTimer OperateMessageTimer = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromSeconds(5) };
        // VM
        readonly MainViewModel mainViewModel = new MainViewModel();
        public MainWindow() {
            InitializeComponent();
            Configs.Handler = new WindowInteropHelper(this).Handle;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            ImgWaterMarkTimer.Tick += ImgWaterMark_Tick;
            // 自动GC
            AutoGcTimer.Tick += AutoGcTimer_Tick;
            AutoGcTimer.Start();
            // 操作消息（在设置消息的时候启动，计时完成后停止）
            OperateMessageTimer.Tick += OperateMessageTimer_Tick;
        }

        /// <summary>
        /// 窗口加载 获取ini配置项
        /// 和目录配置项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            var iniData = IniParserUtils.GetIniData(Constants.SET_FILE);
            #region 窗口尺寸和位置
            string mainLeftStr = iniData[Constants.INI_SECTION_WINDOW][Constants.INI_KEY_LEFT];
            string mainTopStr = iniData[Constants.INI_SECTION_WINDOW][Constants.INI_KEY_TOP];
            string mainHeightStr = iniData[Constants.INI_SECTION_WINDOW][Constants.INI_KEY_HEIGHT];
            string mainWidthStr = iniData[Constants.INI_SECTION_WINDOW][Constants.INI_KEY_WIDTH];
            Configs.mainHeight = string.IsNullOrEmpty(mainHeightStr) || !NumberUtils.IsNumeric(mainHeightStr, out double mainHeight) ? Constants.MAIN_HEIGHT : mainHeight;
            Configs.mainWidth = string.IsNullOrEmpty(mainWidthStr) || !NumberUtils.IsNumeric(mainWidthStr, out double mainWidth) ? Constants.MAIN_WIDTH : mainWidth;
            Configs.mainLeft = string.IsNullOrEmpty(mainLeftStr) || !NumberUtils.IsNumeric(mainLeftStr, out double mainLeft) ? Constants.MAIN_LEFT : mainLeft;
            Configs.mainTop = string.IsNullOrEmpty(mainTopStr) || !NumberUtils.IsNumeric(mainTopStr, out double mainTop) ? Constants.MAIN_TOP : mainTop;
            mainViewModel.MainHeight = Configs.mainHeight;
            mainViewModel.MainWidth = Configs.mainWidth;
            mainViewModel.MainLeft = Configs.mainLeft;
            mainViewModel.MainTop = Configs.mainTop;
            #endregion
            #region 水印设置项
            string text = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_TEXT];
            string opacityStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_OPACITY];
            string rotateStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_ROTATE];
            string fontFamily = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_FAMILY];
            string fontSizeStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_SIZE];
            string fontColor = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_COLOR];
            string horizontalDisStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_HORIZONTAL_DIS];
            string verticalDisStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_VERTICAL_DIS];
            Configs.waterMarkText = string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text.Trim()) ? Constants.WATER_MARK_TEXT : text.Trim();
            Configs.waterMarkOpacity = string.IsNullOrEmpty(opacityStr) || !NumberUtils.IsInt(opacityStr, out int opacity) ? Constants.WATER_MARK_OPACITY : opacity;
            Configs.waterMarkRotate = string.IsNullOrEmpty(rotateStr) || !NumberUtils.IsInt(rotateStr, out int rotate) ? Constants.WATER_MARK_ROTATE : rotate;
            Configs.waterMarkFontFamily = string.IsNullOrEmpty(fontFamily) || !mainViewModel.SystemFonts.Contains(fontFamily) ? Constants.WATER_MARK_FONT_FAMILY : fontFamily;
            Configs.waterMarkFontSize = string.IsNullOrEmpty(fontSizeStr) || !NumberUtils.IsInt(fontSizeStr, out int fontSize) ? Constants.WATER_MARK_FONT_SIZE : fontSize;
            Configs.waterMarkFontColor = string.IsNullOrEmpty(fontColor) ? Constants.WATER_MARK_FONT_COLOR : fontColor;
            Configs.waterMarkHorizontalDis = string.IsNullOrEmpty(horizontalDisStr) || !NumberUtils.IsInt(horizontalDisStr, out int horizontalDis) ? Constants.WATER_MARK_HORIZONTAL_DIS : horizontalDis;
            Configs.waterMarkVerticalDis = string.IsNullOrEmpty(verticalDisStr) || !NumberUtils.IsInt(verticalDisStr, out int verticalDis) ? Constants.WATER_MARK_VERTICAL_DIS : verticalDis;
            mainViewModel.WaterMarkText = Configs.waterMarkText;
            mainViewModel.WaterMarkOpacity = Configs.waterMarkOpacity;
            mainViewModel.WaterMarkRotate = Configs.waterMarkRotate;
            mainViewModel.WaterMarkFontFamily = Configs.waterMarkFontFamily;
            mainViewModel.WaterMarkFontSize = Configs.waterMarkFontSize;
            mainViewModel.WaterMarkFontColor = Configs.waterMarkFontColor;
            mainViewModel.WaterMarkHorizontalDis = Configs.waterMarkHorizontalDis;
            mainViewModel.WaterMarkVerticalDis = Configs.waterMarkVerticalDis;
            #endregion
            #region 页面以及水印目录加载
            // 加载上次打开的TabPage
            string lastOpenTabStr = iniData[Constants.INI_SECTION_PAGE][Constants.INI_KEY_LAST_OPEN_TAB];
            Configs.lastOpenTab = string.IsNullOrEmpty(lastOpenTabStr) || !NumberUtils.IsInt(lastOpenTabStr, out int lastOpenTab) ? Constants.LAST_OPEN_TAB : lastOpenTab;
            mainViewModel.LastOpenTab = Configs.lastOpenTab;
            // 加载上次页面的GridSplitter位置
            string tab2SplitDistanceStr = iniData[Constants.INI_SECTION_PAGE][Constants.INI_KEY_TAB2_SPLIT_DISTANCE];
            Configs.tab2SplitDistance = string.IsNullOrEmpty(tab2SplitDistanceStr) || !NumberUtils.IsNumeric(tab2SplitDistanceStr, out double tab2SplitDistance) ? Constants.TAB2_SPLIT_DISTANCE : tab2SplitDistance;
            mainViewModel.Tab2SplitDistance = Configs.tab2SplitDistance;
            ImgFilePaths_Row.Height = new GridLength(Configs.tab2SplitDistance, GridUnitType.Pixel);
            // 图片水印目录数据
            List<ImgFilePath> imgFilePathList = ServiceFactory.GetImgFilePathService().SelectList(null);
            mainViewModel.ImgFilePaths = new System.Collections.ObjectModel.ObservableCollection<ImgFilePath>(imgFilePathList);
            // 水印目录GridView栏目宽度
            string pathsViewColumn1 = iniData[Constants.INI_SECTION_PAGE][Constants.INI_KEY_PATHS_VIEW_COLUMN_1];
            string pathsViewColumn2 = iniData[Constants.INI_SECTION_PAGE][Constants.INI_KEY_PATHS_VIEW_COLUMN_2];
            Configs.pathsViewColumn1 = string.IsNullOrEmpty(pathsViewColumn1) ? Constants.PATHS_VIEW_COLUMN1 : pathsViewColumn1;
            Configs.pathsViewColumn2 = string.IsNullOrEmpty(pathsViewColumn2) ? Constants.PATHS_VIEW_COLUMN1 : pathsViewColumn2;
            mainViewModel.PathsViewColumn1 = Configs.pathsViewColumn1;
            mainViewModel.PathsViewColumn2 = Configs.pathsViewColumn2;
            #endregion
            Configs.inited = true;
            DataContext = mainViewModel;
        }

        /// <summary>
        /// 窗口关闭前保存配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!Configs.inited) {
                return;
            }
            // 页面GridSplitter位置获取
            mainViewModel.Tab2SplitDistance = ImgFilePaths_Row.Height.Value;
            SaveConfigs();
        }

        /// <summary>
        /// 字体颜色选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            if (System.Windows.Forms.DialogResult.OK == cd.ShowDialog()) {
                mainViewModel.WaterMarkFontColor = ColorTranslator.ToHtml(cd.Color);
                if (!Configs.waterMarkFontColor.Equals(mainViewModel.WaterMarkFontColor)) {
                    mainViewModel.ConfigIsChanged = true;
                }
            }
        }

        private string testImgPath;
        /// <summary>
        /// 空白图片的水印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateWaterMark_Click(object sender, RoutedEventArgs e) {
            mainViewModel.CanTestWaterMark = false;
            Task.Factory.StartNew(delegate {
                CreateWaterMarkImage(true, null, mainViewModel.WaterMarkText);
                SetOperateMsg(System.Windows.Media.Colors.Green, "生成水印成功");
                mainViewModel.CanTestWaterMark = true;
                testImgPath = null;
            });
        }

        /// <summary>
        /// 自定义图片的水印
        /// </summary>
        private void CreateImgWaterMark_Click(object sender, RoutedEventArgs e) {
            mainViewModel.CanTestWaterMark = false;
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog { Filter = "图片文件|*.jpg;*.jpeg;*.bmp;*.png" };
            if (System.Windows.Forms.DialogResult.OK == openFileDialog.ShowDialog()) {
                CreateWaterMarkImage(true, openFileDialog.FileName, mainViewModel.WaterMarkText);
                SetOperateMsg(System.Windows.Media.Colors.Green, "生成水印成功");
                testImgPath = openFileDialog.FileName;
            }
            mainViewModel.CanTestWaterMark = true;
        }

        private void ClearWaterMark_Click(object sender, RoutedEventArgs e) {
            mainViewModel.WaterMarkBitmap = null;
            SetOperateMsg(System.Windows.Media.Colors.Green, "清除水印图片成功");
        }

        private void RefreshWaterMark_Click(object sender, RoutedEventArgs e) {
            mainViewModel.CanTestWaterMark = false;
            CreateWaterMarkImage(true, testImgPath, mainViewModel.WaterMarkText);
            SetOperateMsg(System.Windows.Media.Colors.Green, "刷新水印成功");
            mainViewModel.CanTestWaterMark = true;
        }

        private void SaveWaterMark_Click(object sender, RoutedEventArgs e) {
            if (null != mainViewModel.WaterMarkBitmap) {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog {
                    FileName = "测试水印.jpg",
                    DefaultExt = "*.jpg",
                    Filter = "JPG|*.jpg|BMP|*.bmp|PNG|*.png"
                };
                if (System.Windows.Forms.DialogResult.OK == saveFileDialog.ShowDialog()) {
                    ImageUtils.SaveBitmapImageIntoFile(mainViewModel.WaterMarkBitmap, saveFileDialog.FileName);
                    SetOperateMsg(System.Windows.Media.Colors.Green, "保存水印文件成功");
                }
            } else {
                MessageBox.Show("不存在水印图片！", Constants.MSG_ERROR);
            }
        }

        /// <summary>
        /// 创建水印图
        /// </summary>
        /// <param name="isTest">测试水印</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="waterMark">水印文本</param>
        private void CreateWaterMarkImage(bool isTest, string filePath, string waterMark) {
            if (isTest && null != mainViewModel.WaterMarkBitmap) {
                mainViewModel.WaterMarkBitmap = null;
            }
            Image backPhoto;
            int photoWidth, photoHeight;
            if (string.IsNullOrEmpty(filePath)) {
                // 未指定图片，绘制白色背景图
                photoWidth = (int)WaterMarkBorder.ActualWidth;
                photoHeight = (int)WaterMarkBorder.ActualHeight;
                backPhoto = new Bitmap(photoWidth, photoHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Graphics backPhotoGraphics = Graphics.FromImage(backPhoto);
                backPhotoGraphics.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, photoWidth, photoHeight));
                backPhotoGraphics.Dispose();
            } else {
                // 图片水印任务处理，原文件改名
                string ext = Path.GetExtension(filePath);
                FileStream fs = new FileStream(filePath, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);
                byte[] bytes = br.ReadBytes((int)fs.Length);
                br.Dispose();
                fs.Dispose();
                if (!isTest) {
                    string newpath = Path.GetDirectoryName(filePath) + "\\" + Path.GetFileNameWithoutExtension(filePath) + "_原文件" + ext;
                    File.Move(filePath, newpath);
                }
                using MemoryStream ms = new MemoryStream(bytes);
                using Image imgPhoto = Image.FromStream(ms);
                photoWidth = imgPhoto.Width;
                photoHeight = imgPhoto.Height;
                backPhoto = Image.FromStream(ms);
            }

            // 水印图层
            int circleDiameter = (int)Math.Sqrt(Math.Pow(photoWidth, 2D) + Math.Pow(photoHeight, 2D));
            Bitmap bmPhoto = new Bitmap(photoWidth, photoHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(72, 72);
            Graphics bmPhotoGraphics = Graphics.FromImage(bmPhoto);
            bmPhotoGraphics.Clear(Color.FromName("white"));
            bmPhotoGraphics.InterpolationMode = InterpolationMode.High;
            bmPhotoGraphics.SmoothingMode = SmoothingMode.HighQuality;
            bmPhotoGraphics.DrawImage(backPhoto, new Rectangle(0, 0, photoWidth, photoHeight), 0, 0, photoWidth, photoHeight, GraphicsUnit.Pixel);
            float x = (photoWidth - circleDiameter) / 2, y = (photoHeight - circleDiameter) / 2;
            // 设置颜色和透明度
            Color waterMarkColor = Color.FromArgb(Configs.waterMarkOpacity, ColorTranslator.FromHtml(Configs.waterMarkFontColor));
            SolidBrush semiTransBrush = new SolidBrush(waterMarkColor);
            // 设置旋转
            Matrix matrix = bmPhotoGraphics.Transform;
            matrix.RotateAt(Configs.waterMarkRotate, new System.Drawing.Point(photoWidth / 2, photoHeight / 2));
            bmPhotoGraphics.Transform = matrix;

            int xcount = circleDiameter / Configs.waterMarkHorizontalDis + 1;
            int ycount = circleDiameter / Configs.waterMarkVerticalDis + 1;
            float ox = x;
            for (int k = 0; k < ycount; k++) {
                for (int i = 0; i < xcount; i++) {
                    for (int j = 0; j < xcount; j++) {
                        bmPhotoGraphics.DrawString(waterMark, new Font(Configs.waterMarkFontFamily, Configs.waterMarkFontSize), semiTransBrush, x, y);
                    }
                    x += Configs.waterMarkHorizontalDis;
                }
                x = ox;
                y += Configs.waterMarkVerticalDis;
            }
            if (isTest) {
                // 水印测试的，输出到水印图
                mainViewModel.WaterMarkBitmap = ImageUtils.BitmapToBitmapImage(bmPhoto);
            } else {
                // 水印文件保存
                string ext = Path.GetExtension(filePath).ToLower();
                if (Constants.IMG_EXT_PNG.Equals(ext)) {
                    bmPhoto.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                } else if (Constants.IMG_EXT_BMP.Equals(ext)) {
                    bmPhoto.Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);
                } else {
                    bmPhoto.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            }
            bmPhotoGraphics.Dispose();
            backPhoto.Dispose();
            bmPhoto.Dispose();
        }

        private void Configs_Changed(object sender, RoutedEventArgs e) {
            bool isChange = false;
            if (!isChange && !Configs.waterMarkText.Equals(mainViewModel.WaterMarkText)) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkOpacity != mainViewModel.WaterMarkOpacity) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkRotate != mainViewModel.WaterMarkRotate) {
                isChange = true;
            }
            if (!isChange && !Configs.waterMarkFontFamily.Equals(mainViewModel.WaterMarkFontFamily)) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkFontSize != mainViewModel.WaterMarkFontSize) {
                isChange = true;
            }
            if (!isChange && !Configs.waterMarkFontColor.Equals(mainViewModel.WaterMarkFontColor)) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkHorizontalDis != mainViewModel.WaterMarkHorizontalDis) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkVerticalDis != mainViewModel.WaterMarkVerticalDis) {
                isChange = true;
            }
            mainViewModel.ConfigIsChanged = isChange;
        }

        private void DefaultConfig_Click(object sender, RoutedEventArgs e) {
            if (MessageBoxResult.OK == MessageBox.Show("确认恢复初始值？", Constants.MSG_WARN, MessageBoxButton.OKCancel)) {
                mainViewModel.WaterMarkText = Constants.WATER_MARK_TEXT;
                mainViewModel.WaterMarkOpacity = Constants.WATER_MARK_OPACITY;
                mainViewModel.WaterMarkRotate = Constants.WATER_MARK_ROTATE;
                mainViewModel.WaterMarkFontFamily = Constants.WATER_MARK_FONT_FAMILY;
                mainViewModel.WaterMarkFontSize = Constants.WATER_MARK_FONT_SIZE;
                mainViewModel.WaterMarkFontColor = Constants.WATER_MARK_FONT_COLOR;
                mainViewModel.WaterMarkHorizontalDis = Constants.WATER_MARK_HORIZONTAL_DIS;
                mainViewModel.WaterMarkVerticalDis = Constants.WATER_MARK_VERTICAL_DIS;
                mainViewModel.ConfigIsChanged = false;
                SetOperateMsg(System.Windows.Media.Colors.Green, "恢复默认配置成功");
            }

        }
        /// <summary>
        /// 取消当前所有的配置修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelSaveConfig_Click(object sender, RoutedEventArgs e) {
            mainViewModel.WaterMarkText = Configs.waterMarkText;
            mainViewModel.WaterMarkOpacity = Configs.waterMarkOpacity;
            mainViewModel.WaterMarkRotate = Configs.waterMarkRotate;
            mainViewModel.WaterMarkFontFamily = Configs.waterMarkFontFamily;
            mainViewModel.WaterMarkFontSize = Configs.waterMarkFontSize;
            mainViewModel.WaterMarkFontColor = Configs.waterMarkFontColor;
            mainViewModel.WaterMarkHorizontalDis = Configs.waterMarkHorizontalDis;
            mainViewModel.WaterMarkVerticalDis = Configs.waterMarkVerticalDis;

            mainViewModel.ConfigIsChanged = false;
            SetOperateMsg(System.Windows.Media.Colors.Green, "取消修改成功");
        }
        /// <summary>
        /// 保存当前修改的配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveConfig_Click(object sender, RoutedEventArgs e) {
            SaveConfigs();
            mainViewModel.ConfigIsChanged = false;
            SetOperateMsg(System.Windows.Media.Colors.Green, "保存配置成功");
        }

        private void SaveConfigs() {
            IniParser.Model.IniData iniData = new IniParser.Model.IniData();
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WINDOW, Constants.INI_KEY_LEFT, ref Configs.mainLeft, mainViewModel.MainLeft);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WINDOW, Constants.INI_KEY_TOP, ref Configs.mainTop, mainViewModel.MainTop);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WINDOW, Constants.INI_KEY_HEIGHT, ref Configs.mainHeight, mainViewModel.MainHeight);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WINDOW, Constants.INI_KEY_WIDTH, ref Configs.mainWidth, mainViewModel.MainWidth);

            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_TEXT, ref Configs.waterMarkText, mainViewModel.WaterMarkText);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_OPACITY, ref Configs.waterMarkOpacity, mainViewModel.WaterMarkOpacity);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_ROTATE, ref Configs.waterMarkRotate, mainViewModel.WaterMarkRotate);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_FAMILY, ref Configs.waterMarkFontFamily, mainViewModel.WaterMarkFontFamily);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_SIZE, ref Configs.waterMarkFontSize, mainViewModel.WaterMarkFontSize);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_COLOR, ref Configs.waterMarkFontColor, mainViewModel.WaterMarkFontColor);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_HORIZONTAL_DIS, ref Configs.waterMarkHorizontalDis, mainViewModel.WaterMarkHorizontalDis);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_VERTICAL_DIS, ref Configs.waterMarkVerticalDis, mainViewModel.WaterMarkVerticalDis);

            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_PAGE, Constants.INI_KEY_LAST_OPEN_TAB, ref Configs.lastOpenTab, mainViewModel.LastOpenTab);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_PAGE, Constants.INI_KEY_PATHS_VIEW_COLUMN_1, ref Configs.pathsViewColumn1, mainViewModel.PathsViewColumn1);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_PAGE, Constants.INI_KEY_PATHS_VIEW_COLUMN_2, ref Configs.pathsViewColumn2, mainViewModel.PathsViewColumn2);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_PAGE, Constants.INI_KEY_TAB2_SPLIT_DISTANCE, ref Configs.tab2SplitDistance, mainViewModel.Tab2SplitDistance);
            IniParserUtils.SaveIniData(Constants.SET_FILE, iniData);
        }

        /// <summary>
        /// 添加监听目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddImgFilePath_Click(object sender, RoutedEventArgs e) {
            ImgFilePathWindow imgFilePathWindow = new ImgFilePathWindow(null, mainViewModel.ImgFilePaths);
            if (true == imgFilePathWindow.ShowDialog()) {
                ImgFilePath imgFilePath = new ImgFilePath {
                    Id = Guid.NewGuid().ToString(),
                    FilePath = imgFilePathWindow.vm.FilePath,
                    WaterMark = imgFilePathWindow.vm.WaterMark
                };
                ServiceFactory.GetImgFilePathService().Insert(imgFilePath);
                mainViewModel.ImgFilePaths.Add(imgFilePath);
                SetOperateMsg(System.Windows.Media.Colors.Green, "添加目录成功");
            }
        }
        /// <summary>
        /// 修改监听目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateImgFilePath_Click(object sender, RoutedEventArgs e) {
            if (ImgFilePath_ListView.SelectedIndex > -1) {
                ImgFilePath imgFilePath = (ImgFilePath)ImgFilePath_ListView.SelectedItem;
                ImgFilePathWindow imgFilePathWindow = new ImgFilePathWindow((ImgFilePath)ImgFilePath_ListView.SelectedItem, mainViewModel.ImgFilePaths);
                if (true == imgFilePathWindow.ShowDialog()) {
                    imgFilePath.FilePath = imgFilePathWindow.vm.FilePath;
                    imgFilePath.WaterMark = imgFilePathWindow.vm.WaterMark;
                    ServiceFactory.GetImgFilePathService().Update(imgFilePath);
                    SetOperateMsg(System.Windows.Media.Colors.Green, "修改目录成功");
                }
            } else {
                MessageBox.Show("请先选择目录！", Constants.MSG_ERROR);
            }
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteImgFilePath_Click(object sender, RoutedEventArgs e) {
            if (ImgFilePath_ListView.SelectedIndex > -1) {
                if (MessageBoxResult.OK == MessageBox.Show("确认删除该路径？", Constants.MSG_WARN, MessageBoxButton.OKCancel)) {
                    ImgFilePath selected = (ImgFilePath)ImgFilePath_ListView.SelectedItem;
                    ServiceFactory.GetImgFilePathService().Delete(selected.Id);
                    mainViewModel.ImgFilePaths.Remove(selected);
                    SetOperateMsg(System.Windows.Media.Colors.Green, "删除成功");
                }
            } else {
                MessageBox.Show("请先选择要删除路径！", Constants.MSG_ERROR);
            }
        }

        private void ImgFilePathClear_Click(object sender, RoutedEventArgs e) {
            if (mainViewModel.ImgFilePaths.Count > 0) {
                if (MessageBoxResult.OK == MessageBox.Show("确认清空所有路径？", Constants.MSG_WARN, MessageBoxButton.OKCancel)) {
                    ServiceFactory.GetImgFilePathService().Clear();
                    mainViewModel.ImgFilePaths.Clear();
                    SetOperateMsg(System.Windows.Media.Colors.Green, "清空成功");
                }
            } else {
                MessageBox.Show("当前路径为空，无需清空！", Constants.MSG_ERROR);
            }
        }


        private void Img_WaterMark_Toggle_Click(object sender, RoutedEventArgs e) {
            if (mainViewModel.ImgFilePaths.Count == 0) {
                MessageBox.Show("当前路径为空，请添加文件目录！", Constants.MSG_ERROR);
            } else {
                // 水印定时任务切换状态
                if (mainViewModel.ImgWaterMarkTimerCanRun) {
                    AddWaterMarkLog("图片加水印定时器已开启...");
                    mainViewModel.SetTaskStatus(System.Windows.Media.Colors.Green, "任务运行中");
                    // 立即执行一次
                    ImgWaterMark_Tick(sender, e);
                    ImgWaterMarkTimer.Start();
                    stop = false;
                    SetOperateMsg(System.Windows.Media.Colors.Green, "水印定时器已开启");
                } else {
                    AddWaterMarkLog("图片加水印定时器已关闭...");
                    mainViewModel.SetTaskStatus(System.Windows.Media.Colors.Red, "任务未运行");
                    ImgWaterMarkTimer.Stop();
                    stop = true;
                    SetOperateMsg(System.Windows.Media.Colors.Pink, "水印定时器已关闭");
                }
                mainViewModel.ImgWaterMarkTimerCanRun = !mainViewModel.ImgWaterMarkTimerCanRun;
            }
        }

        private void Img_WaterMark_Execute_Click(object sender, RoutedEventArgs e) {
            if (mainViewModel.ImgFilePaths.Count == 0) {
                MessageBox.Show("当前路径为空，请添加文件目录！", Constants.MSG_ERROR);
            } else {
                if (mainViewModel.ImgWaterMarkTimerCanRun) {
                    mainViewModel.ImgWaterMarkTimerCanRun = false;
                    // 解发一次水印定时任务
                    handExecute = true;
                    ImgWaterMark_Tick(sender, e);
                }
            }
        }

        private int lines = 0;// 日志总行数
        private void AddWaterMarkLog(string lineLog) {
            mainViewModel.WaterMarkLog += $"{DateTime.Now:yy-M-d HH:mm:ss}-{lineLog}\r\n";
            lines++;
            if (lines > Constants.WATER_MARK_LOG_LIMIT) {
                System.Text.RegularExpressions.Regex myRegex = new System.Text.RegularExpressions.Regex("\r\n");
                System.Text.RegularExpressions.Match myMatch = myRegex.Match(mainViewModel.WaterMarkLog, 0);
                int matchCount = 0;
                while (myMatch.Success && matchCount < Constants.WATER_MARK_LOG_CACHE) {
                    myMatch = myMatch.NextMatch();
                    matchCount++;
                }
                mainViewModel.WaterMarkLog = mainViewModel.WaterMarkLog.Substring(myMatch.Index + 2);
                lines = Constants.WATER_MARK_LOG_LIMIT - matchCount;
            }
        }

        private static readonly Hashtable filelst = new Hashtable();
        private static bool isRun = false;
        private static bool stop = false;
        private static bool handExecute = false;
        private void ImgWaterMark_Tick(object sender, EventArgs e) {
            if (isRun) {
                return;
            }
            isRun = true;
            // 异步处理
            Task.Factory.StartNew(delegate {
                AddWaterMarkLog("图片加水印开始执行...");
                filelst.Clear();
                if (mainViewModel.ImgFilePaths.Count > 0) {
                    foreach (ImgFilePath imgFilePath in mainViewModel.ImgFilePaths) {
                        AddImgFileList(imgFilePath.FilePath, string.IsNullOrEmpty(imgFilePath.WaterMark) ? mainViewModel.WaterMarkText : imgFilePath.WaterMark);
                    }
                    if (filelst.Count > 0) {
                        Hashtable processlst = new Hashtable();
                        foreach (string onefile in filelst.Keys) {
                            string ext = Path.GetExtension(onefile);
                            string marktext = (string)filelst[onefile];
                            string filename = Path.GetFileName(onefile);
                            if (!filename.Contains("_原文件" + ext)) {
                                string ywj = Path.GetDirectoryName(onefile) + "\\" + Path.GetFileNameWithoutExtension(onefile) + "_原文件" + ext;
                                if (!File.Exists(ywj)) {
                                    processlst.Add(onefile, marktext);
                                }
                            }
                        }
                        if (processlst.Count > 0) {
                            foreach (string filepath in processlst.Keys) {
                                if (stop) {
                                    break;
                                }
                                string marktext = (string)processlst[filepath];
                                CreateWaterMarkImage(false, filepath, marktext);
                                AddWaterMarkLog($"{filepath}:处理完成");
                            }
                            stop = false;
                        }
                    }
                }
                AddWaterMarkLog("图片加水印执行结束...");
                if (handExecute) {
                    handExecute = false;
                    mainViewModel.ImgWaterMarkTimerCanRun = true;
                }
                isRun = false;
            });
        }

        private void AutoGcTimer_Tick(object sender, EventArgs e) {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                //  配置工作使用空间
                DllUtils.SetProcessWorkingSetSize(Configs.Handler, -1, -1);
            }
        }
        private void SetOperateMsg(System.Windows.Media.Color color, string msg) {
            if (!string.IsNullOrEmpty(mainViewModel.OperateMsg)) {
                // 存在上次未结束的状态报告
                OperateMessageTimer_Tick(null, null);
            }
            OperateMessageTimer.Start();
            mainViewModel.InitOperateMsg(color, msg);
        }
        private void OperateMessageTimer_Tick(object sender, EventArgs e) {
            // 执行完成后,定时器停止，清除消息
            OperateMessageTimer.Stop();
            mainViewModel.InitOperateMsg(System.Windows.Media.Colors.White, string.Empty);
        }

        private void AddImgFileList(string filepath, string waterMark) {
            if (Directory.Exists(filepath)) {
                // 列出指定路径下的所有文件
                foreach (string file in Directory.GetFiles(filepath, "*.jpg")) {
                    filelst.Add(file, waterMark);
                }
                foreach (string file in Directory.GetFiles(filepath, "*.jpeg")) {
                    filelst.Add(file, waterMark);
                }
                foreach (string file in Directory.GetFiles(filepath, "*.png")) {
                    filelst.Add(file, waterMark);
                }
                foreach (string file in Directory.GetFiles(filepath, "*.bmp")) {
                    filelst.Add(file, waterMark);
                }
                // 递归列出所有子文件夹
                foreach (string directory in Directory.GetDirectories(filepath)) {
                    AddImgFileList(directory, waterMark);
                }
            }
        }


        /// <summary>
        /// 清空日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaterMarkLogClear_Click(object sender, RoutedEventArgs e) {
            mainViewModel.WaterMarkLog = string.Empty;
            lines = 0;
            SetOperateMsg(System.Windows.Media.Colors.Green, "日志清空成功");
        }
    }
}