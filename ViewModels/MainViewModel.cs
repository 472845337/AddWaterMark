using AddWaterMark.Beans;
using AddWaterMark.Commands;
using AddWaterMark.Config;
using AddWaterMark.DataBase.Beans;
using AddWaterMark.DataBase.Services;
using AddWaterMark.Utils;
using AddWaterMark.Windows;
using iTextSharp.text.pdf;
using PropertyChanged;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AddWaterMark.ViewModels {
    [AddINotifyPropertyChangedInterface]
    class MainViewModel {
        public MainViewModel() {
            SystemFonts = new ObservableCollection<string>(FontsUtils.GetSystemFonts());// 系统字体
            #region 水印配置项命令
            DefaultConfigCommand = new RelayCommand(SetDefaultConfig);
            CancelConfigCommand = new RelayCommand(CancelSaveConfig, CanCancelOrSave);
            SaveConfigCommand = new RelayCommand(SaveConfig, CanCancelOrSave);
            CreateWaterMarkCommand = new RelayCommand(CreateWaterMark, CanTest);
            RefreshWaterMarkCommand = new RelayCommand(RefreshTestWaterMark, CanTest);
            CreateImgWaterMarkCommand = new RelayCommand(CreateImgWaterMark, CanTest);
            ClearWaterMarkCommand = new RelayCommand(ClearWaterMark);
            SaveWaterMarkCommand = new RelayCommand(SaveWaterMark);
            #endregion
            #region 图片目录命令
            AddImgFilePathCommand = new RelayCommand(AddImgFilePath);
            UpdateImgFilePathCommand = new RelayCommand(UpdateImgFilePath, ImgFilePathSelected);
            DeleteImgFilePathCommand = new RelayCommand(DeleteImgFilePath, ImgFilePathSelected);
            OpenImgFilePathCommand = new RelayCommand(OpenImgFilePath, ImgFilePathSelected);
            ClearImgFilePathCommand = new RelayCommand(ClearImgFilePath, (obj) => { return ImgFilePaths.Count > 0; });

            #endregion
            #region 水印执行命令
            ImgWaterMarkExecuteCommand = new RelayCommand(ImgWaterMarkExecute);// 该命令不可设置Enable，因为是Task处理水印任务，赋值ImgWaterMarkTimerCanRun时会异常
            ImgWaterMarkTaskToggleCommand = new RelayCommand(ImgWaterMarkTaskToggle);
            ClearWaterMarkLogCommand = new RelayCommand(ClearWaterMarkLog, (obj) => { return TaskLogs.Count > 0; });
            #endregion
            OperateMessageTimer.Tick += OperateMessageTimer_Tick;
            ImgWaterMarkExecuteTimer.Tick += ImgWaterMarkHand_Tick;
            ImgWaterMarkTaskTimer.Tick += ImgWaterMarkTask_Tick;
        }
        public RelayCommand DefaultConfigCommand { get; set; }
        public RelayCommand CancelConfigCommand { get; set; }
        public RelayCommand SaveConfigCommand { get; set; }
        public RelayCommand CreateWaterMarkCommand { get; set; }
        public RelayCommand RefreshWaterMarkCommand { get; set; }
        public RelayCommand CreateImgWaterMarkCommand { get; set; }
        public RelayCommand ClearWaterMarkCommand { get; set; }
        public RelayCommand SaveWaterMarkCommand { get; set; }

        public RelayCommand AddImgFilePathCommand { get; set; }
        public RelayCommand UpdateImgFilePathCommand { get; set; }
        public RelayCommand DeleteImgFilePathCommand { get; set; }
        public RelayCommand ClearImgFilePathCommand { get; set; }
        public RelayCommand OpenImgFilePathCommand { get; set; }

        public RelayCommand ImgWaterMarkExecuteCommand { get; set; }
        public RelayCommand ImgWaterMarkTaskToggleCommand { get; set; }
        public RelayCommand ClearWaterMarkLogCommand { get; set; }

        public readonly DispatcherTimer OperateMessageTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(5) };
        // 手工执行也是定时任务跑一下，只不过是跑完一次后，停止该计时器
        public readonly DispatcherTimer ImgWaterMarkExecuteTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };
        // 水印任务定时器
        public readonly DispatcherTimer ImgWaterMarkTaskTimer = new DispatcherTimer();
        public double MainHeight { get; set; }// 主窗口高度
        public double MainWidth { get; set; }// 主窗口宽度
        public double MainLeft { get; set; }// 主窗口左边位置
        public double MainTop { get; set; }// 主窗口顶部位置
        [OnChangedMethod("CancelOrSaveCommandChanged")]
        public bool ConfigIsChanged { get; set; }// 配置是否修改
        public string WaterMarkText { get; set; }// 水印文本
        public byte WaterMarkOpacity { get; set; }// 不透明度
        public int WaterMarkRotate { get; set; }// 旋转角度
        public ObservableCollection<string> SystemFonts { get; set; }// 系统字体
        public string WaterMarkFontFamily { get; set; }// 水印字体
        public int WaterMarkFontSize { get; set; }// 水印字体大小
        public bool WaterMarkFontIsGradient { get; set; }// 是否渐变色
        public string WaterMarkFontColor { get; set; }// 水印颜色
        public string WaterMarkFontGradientColor { get; set; }// 渐变色
        public bool WaterMarkFontBold { get; set; }// 字体加粗
        public bool WaterMarkFontItalic { get; set; }// 字体加粗
        public bool WaterMarkFontUnderline { get; set; }// 下划线
        public bool WaterMarkFontStrikeout { get; set; }// 中划线
        public int WaterMarkHorizontalDis { get; set; }// 水平距离
        public int WaterMarkVerticalDis { get; set; }// 垂直距离
        [OnChangedMethod("CanTestChanged")]
        public bool CanTestWaterMark { get; set; } = true;// 是否可以测试水印
        [DoNotNotify]
        public System.Windows.Controls.Border WaterMarkBorder { get; set; }
        public BitmapImage WaterMarkBitmap { get; set; }// 水印位图
        public double WaterMarkWidth { get; set; }
        public double WaterMarkHeight { get; set; }
        [DoNotNotify]
        public System.Windows.Controls.ListView ImgFilePath_ListView { get; set; }
        public int LastOpenTab { get; set; }// 上次打开Tab页
        public double Tab2SplitDistance { get; set; }// Tab2页 GridSplitter距离
        public double PathsViewColumn1 { get; set; }// 水印目录视图第一栏宽度
        public double PathsViewColumn2 { get; set; }// 水印目录视图第﻿二栏宽度
        public bool AllSelect { get; set; }
        public ObservableCollection<ImgFilePath> ImgFilePaths { get; set; }// 自动添加水印目录数据集合
        public bool ImgWaterMarkTimerCanRun { get; set; } = true;// 自动水印定时器是否可执行
        public bool ScrollEnd { get; set; }
        public ObservableCollection<Log> TaskLogs { get; set; } = new ObservableCollection<Log>();
        public System.Windows.Controls.RichTextBox TaskLog_RichTextBox { get; set; }
        public int TaskInterval { get; set; }
        public string TaskStatus { get; private set; } = "任务未运行";
        public Brush TaskStatusColor { get; private set; } = new SolidColorBrush(Colors.Red);
        public Brush OperateMsgColor { get; private set; }// 操作信息颜色
        public string OperateMsg { get; private set; }// 操作信息
        public void SetTaskStatus(Color color, string msg) {
            TaskStatusColor = new SolidColorBrush(color);
            TaskStatusColor.Freeze();
            TaskStatus = msg;
        }
        public void InitOperateMsg(Color color, string msg) {
            OperateMsgColor = new SolidColorBrush(color);
            OperateMsgColor.Freeze();
            OperateMsg = msg;
        }

        /// <summary>
        /// 恢复默认的配置项
        /// </summary>
        /// <param name="obj"></param>
        private void SetDefaultConfig(object obj) {
            if (MessageBoxResult.OK == MessageBox.Show("确认恢复初始值？", Constants.MSG_WARN, MessageBoxButton.OKCancel)) {
                WaterMarkText = Constants.WATER_MARK_TEXT;
                WaterMarkOpacity = Constants.WATER_MARK_OPACITY;
                WaterMarkRotate = Constants.WATER_MARK_ROTATE;
                WaterMarkFontFamily = Constants.WATER_MARK_FONT_FAMILY;
                WaterMarkFontSize = Constants.WATER_MARK_FONT_SIZE;
                WaterMarkFontIsGradient = false;
                WaterMarkFontColor = Constants.WATER_MARK_FONT_COLOR;
                WaterMarkFontGradientColor = Constants.WATER_MARK_FONT_GRADIENT_COLOR;
                WaterMarkFontBold = false;
                WaterMarkFontItalic = false;
                WaterMarkFontUnderline = false;
                WaterMarkFontStrikeout = false;
                WaterMarkHorizontalDis = Constants.WATER_MARK_HORIZONTAL_DIS;
                WaterMarkVerticalDis = Constants.WATER_MARK_VERTICAL_DIS;
                ConfigIsChanged = false;
                SaveConfigs();
                SetOperateMsg("恢复默认配置成功");
            }
        }

        /// <summary>
        /// 取消当前所有的配置修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelSaveConfig(object obj) {
            WaterMarkText = Configs.waterMarkText;
            WaterMarkOpacity = Configs.waterMarkOpacity;
            WaterMarkRotate = Configs.waterMarkRotate;
            WaterMarkFontFamily = Configs.waterMarkFontFamily;
            WaterMarkFontSize = Configs.waterMarkFontSize;
            WaterMarkFontIsGradient = Configs.waterMarkFontIsGradient;
            WaterMarkFontColor = Configs.waterMarkFontColor;
            WaterMarkFontGradientColor = Configs.waterMarkFontGradientColor;
            WaterMarkFontBold = Configs.waterMarkFontBold;
            WaterMarkFontItalic = Configs.waterMarkFontItalic;
            WaterMarkFontUnderline = Configs.waterMarkFontUnderline;
            WaterMarkFontStrikeout = Configs.waterMarkFontStrikeout;
            WaterMarkHorizontalDis = Configs.waterMarkHorizontalDis;
            WaterMarkVerticalDis = Configs.waterMarkVerticalDis;
            ConfigIsChanged = false;
            SetOperateMsg("取消修改成功");
        }
        /// <summary>
        /// 保存当前修改的配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveConfig(object obj) {
            SaveConfigs();
            ConfigIsChanged = false;
            SetOperateMsg("保存配置成功");
        }
        private bool CanCancelOrSave(object obj) {
            return ConfigIsChanged;
        }
        private bool CanTest(object obj) {
            return CanTestWaterMark;
        }
        private bool ImgFilePathSelected(object obj) {
            return ImgFilePath_ListView.SelectedIndex > -1;
        }
        /// <summary>
        /// 取消和保存按钮可用刷新（OnChangedMethod调用）
        /// </summary>
        void CancelOrSaveCommandChanged() {
            CancelConfigCommand.RaiseCanExecuteChanged();
            SaveConfigCommand.RaiseCanExecuteChanged();
        }
        // 刷新控件可用状态（OnChangedMethod调用）
        void CanTestChanged() {
            CreateWaterMarkCommand.RaiseCanExecuteChanged();
            CreateImgWaterMarkCommand.RaiseCanExecuteChanged();
            RefreshWaterMarkCommand.RaiseCanExecuteChanged();
        }
        /// <summary>
        /// 图片目录选中后相关命令变更可执行状态
        /// </summary>
        public void ImgFilePathSelectedChanged() {
            UpdateImgFilePathCommand.RaiseCanExecuteChanged();
            DeleteImgFilePathCommand.RaiseCanExecuteChanged();
            OpenImgFilePathCommand.RaiseCanExecuteChanged();
        }
        private void ImgFilePathsChanged() {
            ClearImgFilePathCommand.RaiseCanExecuteChanged();
        }
        private void WaterMarkLogChanged() {
            ClearWaterMarkLogCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// 保存当前配置到ini文件
        /// </summary>
        internal void SaveConfigs() {
            IniParser.Model.IniData iniData = new IniParser.Model.IniData();
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_TEXT, ref Configs.waterMarkText, WaterMarkText);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_OPACITY, ref Configs.waterMarkOpacity, WaterMarkOpacity);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_ROTATE, ref Configs.waterMarkRotate, WaterMarkRotate);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_FAMILY, ref Configs.waterMarkFontFamily, WaterMarkFontFamily);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_SIZE, ref Configs.waterMarkFontSize, WaterMarkFontSize);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_IS_GRADIENT, ref Configs.waterMarkFontIsGradient, WaterMarkFontIsGradient);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_COLOR, ref Configs.waterMarkFontColor, WaterMarkFontColor);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_GRADIENT_COLOR, ref Configs.waterMarkFontGradientColor, WaterMarkFontGradientColor);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_BOLD, ref Configs.waterMarkFontBold, WaterMarkFontBold);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_ITALIC, ref Configs.waterMarkFontItalic, WaterMarkFontItalic);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_UNDERLINE, ref Configs.waterMarkFontUnderline, WaterMarkFontUnderline);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_STRIKEOUT, ref Configs.waterMarkFontStrikeout, WaterMarkFontStrikeout);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_HORIZONTAL_DIS, ref Configs.waterMarkHorizontalDis, WaterMarkHorizontalDis);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_VERTICAL_DIS, ref Configs.waterMarkVerticalDis, WaterMarkVerticalDis);
            IniParserUtils.SaveIniData(Constants.SET_FILE, iniData);
        }
        private string testImgPath;
        /// <summary>
        /// 空白图片的水印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateWaterMark(object _) {
            CanTestWaterMark = false;
            // media
            Brush brush = WaterMarkUtils.GetWaterMarkBrush(WaterMarkFontIsGradient, WaterMarkFontColor, WaterMarkFontGradientColor, WaterMarkOpacity);
            CreateWaterMarkImage(true, null, GetTestFormattedText(brush), WaterMarkFontSize, brush);
            // drawing
            //CreateWaterMarkImage(true, null, WaterMarkText,
            //    FontsUtils.GetDrawingFont(WaterMarkFontFamily, WaterMarkFontSize, WaterMarkFontBold, WaterMarkFontItalic, WaterMarkFontUnderline, WaterMarkFontStrikeout)
            //   );
            SetOperateMsg("生成水印成功");
            CanTestWaterMark = true;
            testImgPath = null;
        }

        private void RefreshTestWaterMark(object obj) {
            CanTestWaterMark = false;
            // media
            Brush brush = WaterMarkUtils.GetWaterMarkBrush(WaterMarkFontIsGradient, WaterMarkFontColor, WaterMarkFontGradientColor, WaterMarkOpacity);
            CreateWaterMarkImage(true, testImgPath, GetTestFormattedText(brush), WaterMarkFontSize, brush);
            brush.Freeze();
            // drawing
            //CreateWaterMarkImage(true, testImgPath, WaterMarkText,
            //        FontsUtils.GetDrawingFont(WaterMarkFontFamily, WaterMarkFontSize, WaterMarkFontBold, WaterMarkFontItalic, WaterMarkFontUnderline, WaterMarkFontStrikeout)
            //        );
            SetOperateMsg("刷新水印成功");
            CanTestWaterMark = true;
        }

        /// <summary>
        /// 自定义图片的水印
        /// </summary>
        private void CreateImgWaterMark(object _) {
            CanTestWaterMark = false;
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog { Filter = "图片文件|*.jpg;*.jpeg;*.bmp;*.png" };
            if (System.Windows.Forms.DialogResult.OK == openFileDialog.ShowDialog()) {
                // media
                Brush brush = WaterMarkUtils.GetWaterMarkBrush(WaterMarkFontIsGradient, WaterMarkFontColor, WaterMarkFontGradientColor, WaterMarkOpacity);
                CreateWaterMarkImage(true, openFileDialog.FileName, GetTestFormattedText(brush), WaterMarkFontSize, brush);
                brush.Freeze();
                // drawing
                //CreateWaterMarkImage(true, openFileDialog.FileName, WaterMarkText,
                //    FontsUtils.GetDrawingFont(WaterMarkFontFamily, WaterMarkFontSize, WaterMarkFontBold, WaterMarkFontItalic, WaterMarkFontUnderline, WaterMarkFontStrikeout)
                //    );
                SetOperateMsg("生成水印成功");
                testImgPath = openFileDialog.FileName;
            }
            CanTestWaterMark = true;
        }

        private void ClearWaterMark(object _) {
            WaterMarkBitmap = null;
            testImgPath = null;
            SetOperateMsg("清除水印图片成功");
        }

        private void SaveWaterMark(object _) {
            if (null != WaterMarkBitmap) {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog {
                    FileName = "测试水印.jpg",
                    DefaultExt = "*.jpg",
                    Filter = "JPG|*.jpg|BMP|*.bmp|PNG|*.png"
                };
                if (System.Windows.Forms.DialogResult.OK == saveFileDialog.ShowDialog()) {
                    ImageUtils.SaveBitmapImageIntoFile(WaterMarkBitmap, saveFileDialog.FileName);
                    SetOperateMsg("保存水印文件成功");
                }
            } else {
                MessageBox.Show("不存在水印图片！", Constants.MSG_ERROR);
            }
        }



        /// <summary>
        /// Media Drawing方式创建水印图
        /// dpi处理中进行了对wpf的默认96的缩放，调整请注意
        /// </summary>
        /// <param name="isTest">测试水印</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="formattedText">水印文本样式（包含水印文本）</param>
        private void CreateWaterMarkImage(bool isTest, string filePath, FormattedText formattedText, int fontSize, Brush brush) {
            int waterMarkRotate = Configs.waterMarkRotate;
            int waterMarkHorizontalDis = Configs.waterMarkHorizontalDis;
            int waterMarkVerticalDis = Configs.waterMarkVerticalDis;
            bool waterMarkUnderline = Configs.waterMarkFontUnderline;
            bool waterMarkStrikeout = Configs.waterMarkFontStrikeout;
            if (isTest) {
                waterMarkRotate = WaterMarkRotate;
                waterMarkHorizontalDis = WaterMarkHorizontalDis;
                waterMarkVerticalDis = WaterMarkVerticalDis;
                waterMarkUnderline = WaterMarkFontUnderline;
                waterMarkStrikeout = WaterMarkFontStrikeout;
                if (null != WaterMarkBitmap) {
                    WaterMarkBitmap = null;
                }
            }
            string ext = string.IsNullOrEmpty(filePath) ? ".jpg" : Path.GetExtension(filePath).ToLower();
            BitmapSource backPhoto;
            double photoWidth, photoHeight;
            double backDpiX = 72, backDpiY = 72;
            if (string.IsNullOrEmpty(filePath)) {
                // 未指定图片，绘制白色背景图
                photoWidth = WaterMarkBorder.ActualWidth;
                photoHeight = WaterMarkBorder.ActualHeight;
                int stride = (((int)photoWidth * 32 + 31) & ~31) / 8;
                byte[] pixels = new byte[(int)photoHeight * stride];
                BitmapPalette myPalette = new BitmapPalette(new List<Color> { Colors.White });
                backPhoto = BitmapSource.Create((int)photoWidth, (int)photoHeight, backDpiX, backDpiY, PixelFormats.Indexed1, myPalette, pixels, stride);
            } else {
                FileStream fs = new FileStream(filePath, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);
                byte[] bytes = br.ReadBytes((int)fs.Length);
                br.Close();
                fs.Close();
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(bytes);
                bmp.EndInit();
                backPhoto = bmp;
                // 图片水印任务处理，原文件改名
                if (!isTest) {
                    string newpath = Path.GetDirectoryName(filePath) + "\\" + Path.GetFileNameWithoutExtension(filePath) + "_原文件" + ext;
                    File.Move(filePath, newpath);
                }
                photoWidth = backPhoto.PixelWidth;
                photoHeight = backPhoto.PixelHeight;
                if(backPhoto.DpiX != 0) {
                    backDpiX = backPhoto.DpiX;
                }
                if(backPhoto.DpiY != 0) {
                    backDpiY = backPhoto.DpiY;
                }
            }

            // wpf默认的dpi是96，设置固定绽放
            double scaleX = 96f / backDpiX;
            double scaleY = 96f / backDpiY;
            double drawWidth = photoWidth * scaleX;// 绘制时的宽度
            double drawHeight = photoHeight * scaleY;// 绘制时的高度
            formattedText.SetFontSize(fontSize * scaleX);
            // 水印图层，图片尺寸依旧是原图的宽高
            RenderTargetBitmap composeImage = new RenderTargetBitmap((int)photoWidth, (int)photoHeight, backDpiX, backDpiY, PixelFormats.Pbgra32);
            // 计算绘制图片的范围圆直径
            int circleDiameter = (int)Math.Sqrt(Math.Pow(drawWidth, 2D) + Math.Pow(drawHeight, 2D));

            // 定义绘制对象
            DrawingVisual drawingVisual = new DrawingVisual();
            // 获取绘制内容
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            // 背景图绘制
            drawingContext.DrawImage(backPhoto, new Rect(0, 0, drawWidth, drawHeight));
            // 水印的起始坐标
            double x = (drawWidth - circleDiameter) / 2, y = (drawHeight - circleDiameter) / 2;
            // 设置旋转（水印圆直径的中心点位置）
            RotateTransform transform = new RotateTransform(waterMarkRotate, drawWidth / 2, drawHeight / 2);
            drawingContext.PushTransform(transform);

            double xcount = circleDiameter / waterMarkHorizontalDis / scaleX + 1;
            double ycount = circleDiameter / waterMarkVerticalDis / scaleY + 1;
            double ox = x;
            for (int k = 0; k < ycount; k++) {
                for (int i = 0; i < xcount; i++) {
                    drawingContext.DrawText(formattedText, new Point(x, y));
                    if (waterMarkUnderline) {
                        // 下划线
                        drawingContext.DrawLine(new Pen(brush, 1), new Point(x, y + formattedText.Height), new Point(x + formattedText.Width, y + formattedText.Height));
                    }
                    if (waterMarkStrikeout) {
                        // 中划线
                        drawingContext.DrawLine(new Pen(brush, 1), new Point(x, y + formattedText.Height / 2), new Point(x + formattedText.Width, y + formattedText.Height / 2));
                    }
                    x += waterMarkHorizontalDis * scaleX;
                }
                x = ox;
                y += waterMarkVerticalDis * scaleY;
            }
            drawingContext.Close();
            composeImage.Render(drawingVisual);
            BitmapEncoder bitMapEncoder = ImageUtils.GetEncoder(ext);
            //加入第一帧
            bitMapEncoder.Frames.Add(BitmapFrame.Create(composeImage));
            if (isTest) {
                BitmapImage bitmapImage = new BitmapImage();
                using (var memoryStream = new MemoryStream()) {
                    bitMapEncoder.Save(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                }
                WaterMarkBitmap = bitmapImage;
                WaterMarkWidth = photoWidth;
                WaterMarkHeight = photoHeight;
            } else {
                // 水印文件保存
                using FileStream fileStream = new FileStream(filePath, FileMode.Create);
                bitMapEncoder.Save(fileStream);
            }
            backPhoto.Freeze();
        }

        /// <summary>
        /// Graphics Drawing方式创建水印图
        /// </summary>
        /// <param name="isTest">测试水印</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="waterMark">水印文本</param>
        private void CreateWaterMarkImage(bool isTest, string filePath, string waterMark, System.Drawing.Font font) {
            int waterMarkRotate = Configs.waterMarkRotate;
            int waterMarkHorizontalDis = Configs.waterMarkHorizontalDis;
            int waterMarkVerticalDis = Configs.waterMarkVerticalDis;
            byte waterMarkOpacity = Configs.waterMarkOpacity;
            bool waterMarkFontIsGradient = Configs.waterMarkFontIsGradient;
            string waterMarkFontColor = Configs.waterMarkFontColor;
            string waterMarkFontGradientColor = Configs.waterMarkFontGradientColor;
            if (isTest) {
                waterMarkRotate = WaterMarkRotate;
                waterMarkHorizontalDis = WaterMarkHorizontalDis;
                waterMarkVerticalDis = WaterMarkVerticalDis;
                waterMarkOpacity = WaterMarkOpacity;
                waterMarkFontIsGradient = WaterMarkFontIsGradient;
                waterMarkFontColor = WaterMarkFontColor;
                waterMarkFontGradientColor = WaterMarkFontGradientColor;
                if (null != WaterMarkBitmap) {
                    WaterMarkBitmap = null;
                }
            }

            System.Drawing.Image backPhoto;
            int photoWidth, photoHeight;
            if (string.IsNullOrEmpty(filePath)) {
                // 未指定图片，绘制白色背景图
                photoWidth = (int)WaterMarkBorder.ActualWidth;
                photoHeight = (int)WaterMarkBorder.ActualHeight;
                backPhoto = new System.Drawing.Bitmap(photoWidth, photoHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                System.Drawing.Graphics backPhotoGraphics = System.Drawing.Graphics.FromImage(backPhoto);
                backPhotoGraphics.Clear(System.Drawing.Color.White);
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
                using System.Drawing.Image imgPhoto = System.Drawing.Image.FromStream(ms);
                photoWidth = imgPhoto.Width;
                photoHeight = imgPhoto.Height;
                backPhoto = System.Drawing.Image.FromStream(ms);
            }

            // 水印图层
            int circleDiameter = (int)Math.Sqrt(Math.Pow(photoWidth, 2D) + Math.Pow(photoHeight, 2D));
            System.Drawing.Bitmap bmPhoto = new System.Drawing.Bitmap(photoWidth, photoHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bmPhoto.SetResolution(72, 72);
            System.Drawing.Graphics bmPhotoGraphics = System.Drawing.Graphics.FromImage(bmPhoto);
            bmPhotoGraphics.Clear(System.Drawing.Color.White);
            bmPhotoGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            bmPhotoGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            bmPhotoGraphics.DrawImage(backPhoto, new System.Drawing.Rectangle(0, 0, photoWidth, photoHeight), 0, 0, photoWidth, photoHeight, System.Drawing.GraphicsUnit.Pixel);
            float x = (photoWidth - circleDiameter) / 2, y = (photoHeight - circleDiameter) / 2;
            // 设置旋转
            System.Drawing.Drawing2D.Matrix matrix = bmPhotoGraphics.Transform;
            matrix.RotateAt(waterMarkRotate, new System.Drawing.Point(photoWidth / 2, photoHeight / 2));
            bmPhotoGraphics.Transform = matrix;
            // 画刷
            System.Drawing.SizeF crSize = bmPhotoGraphics.MeasureString(waterMark, font);
            System.Drawing.Brush brush = WaterMarkUtils.GetDrawingBrush(waterMarkOpacity, waterMarkFontIsGradient, waterMarkFontColor, waterMarkFontGradientColor, (int)crSize.Width, (int)crSize.Height);

            int xcount = circleDiameter / waterMarkHorizontalDis + 1;
            int ycount = circleDiameter / waterMarkVerticalDis + 1;
            float ox = x;
            for (int k = 0; k < ycount; k++) {
                for (int i = 0; i < xcount; i++) {
                    bmPhotoGraphics.DrawString(waterMark, font, brush, x, y);
                    x += waterMarkHorizontalDis;
                }
                x = ox;
                y += waterMarkVerticalDis;
            }
            brush.Dispose();
            if (isTest) {
                // 水印测试的，输出到水印图
                WaterMarkBitmap = ImageUtils.BitmapToBitmapImage(bmPhoto);
                WaterMarkWidth = photoWidth;
                WaterMarkHeight = photoHeight;
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

        /// <summary>
        /// 添加文本水印,渐变色功能未实现
        /// iTextSharp旋转的角度需要转换，不然和图片的旋转角度就反了，赋值时取负
        /// </summary>
        /// <param name="isTest">暂不支持测试</param>
        /// <param name="pdfPath">pdf文件</param>
        /// <param name="addText">水印文字</param>
        /// <param name="font">字体</param>
        private void PdfAddWatermark(bool isTest, string pdfPath, string addText, System.Drawing.Font font) {
            int waterMarkRotate = -Configs.waterMarkRotate;// 为了保证和图片的旋转一致取反
            int waterMarkHorizontalDis = Configs.waterMarkHorizontalDis;
            int waterMarkVerticalDis = Configs.waterMarkVerticalDis;
            byte waterMarkOpacity = Configs.waterMarkOpacity;
            //bool waterMarkFontIsGradient = Configs.waterMarkFontIsGradient;
            string waterMarkFontColor = Configs.waterMarkFontColor;
            //string waterMarkFontGradientColor = Configs.waterMarkFontGradientColor;
            if (isTest) {
                waterMarkRotate = -WaterMarkRotate;// 为了保证和图片的旋转一致取反
                waterMarkHorizontalDis = WaterMarkHorizontalDis;
                waterMarkVerticalDis = WaterMarkVerticalDis;
                waterMarkOpacity = WaterMarkOpacity;
                //waterMarkFontIsGradient = WaterMarkFontIsGradient;
                waterMarkFontColor = WaterMarkFontColor;
                //waterMarkFontGradientColor = WaterMarkFontGradientColor;
                if (null != WaterMarkBitmap) {
                    WaterMarkBitmap = null;
                }
            }
            string ext = Path.GetExtension(pdfPath);
            string newpath = Path.GetDirectoryName(pdfPath) + "\\" + Path.GetFileNameWithoutExtension(pdfPath) + "_原文件" + ext;
            File.Move(pdfPath, newpath);

            //读取pdf
            PdfReader reader = new PdfReader(newpath);
            //创建新pdf
            Stream outStream = new FileStream(pdfPath, FileMode.Create, FileAccess.Write, FileShare.None);
            PdfStamper stamper = new PdfStamper(reader, outStream); ;
            int pdfTotalPage = reader.NumberOfPages;//总页数

            // 透明度
            PdfGState gs = new PdfGState();
            // 字体
            System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(waterMarkFontColor);
            iTextSharp.text.BaseColor baseColor = new iTextSharp.text.BaseColor(color.R, color.G, color.B, waterMarkOpacity * 255 / 100);

            BaseFont baseFont = FontsUtils.ConvertFont2BaseFont(font);
            PdfContentByte content;
            for (int i = 1; i <= pdfTotalPage; i++) {
                //当前页pdf尺寸
                iTextSharp.text.Rectangle psize = reader.GetPageSize(i);
                float width = psize.Width;
                float height = psize.Height;
                int circleDiameter = (int)Math.Sqrt(Math.Pow(width, 2D) + Math.Pow(height, 2D));
                float x = (width - circleDiameter) / 2, y = (height - circleDiameter) / 2;
                //GetUnderContent内容下层
                //GetOverContent内容上层
                content = stamper.GetOverContent(i);

                content.SetGState(gs);
                //开始写入文本
                content.BeginText();
                //设置颜色
                //if (waterMarkFontIsGradient) {
                //    // 渐变色，这个功能不符合预期 
                //    GradientColorUtils.GetPdfColor(waterMarkFontGradientColor, waterMarkOpacity * 255 / 100, out PdfDeviceNColor ncolor, out float[] tints);
                //    content.SetColorFill(ncolor, tints);
                //} else {
                content.SetColorFill(baseColor);
                //}
                //字体大小
                content.SetFontAndSize(baseFont, font.Size);
                //设置文本矩阵
                content.SetTextMatrix(0, 0);
                //水印文本位置
                int xcount = circleDiameter / waterMarkHorizontalDis + 1;
                int ycount = circleDiameter / waterMarkVerticalDis + 1;
                float ox = x;
                for (int k = 0; k < ycount; k++) {
                    for (int l = 0; l < xcount; l++) {
                        content.ShowTextAligned(iTextSharp.text.Element.ALIGN_LEFT, addText, x, y, waterMarkRotate);
                        x += waterMarkHorizontalDis;
                    }
                    x = ox;
                    y += waterMarkVerticalDis;
                }
                content.EndText();
            }

            stamper.Close();
            reader.Close();
        }


        private FormattedText GetTestFormattedText(Brush brush) {
            return WaterMarkUtils.GetFormattedText(WaterMarkText, WaterMarkFontFamily, WaterMarkFontItalic, WaterMarkFontBold
                , WaterMarkFontSize, brush);
        }


        /// <summary>
        /// 添加监听目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddImgFilePath(object _) {
            ImgFilePathWindow imgFilePathWindow = new ImgFilePathWindow(null, ImgFilePaths);
            if (true == imgFilePathWindow.ShowDialog()) {
                ImgFilePath imgFilePath = new ImgFilePath {
                    Id = Guid.NewGuid().ToString(),
                    FilePath = imgFilePathWindow.vm.FilePath,
                    WaterMark = imgFilePathWindow.vm.WaterMark
                };
                ServiceFactory.GetImgFilePathService().Insert(imgFilePath);
                ImgFilePaths.Add(imgFilePath);
                SetOperateMsg("添加目录成功");
                ImgFilePathsChanged();
            }
        }
        /// <summary>
        /// 修改监听目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateImgFilePath(object _) {
            if (ImgFilePath_ListView.SelectedIndex > -1) {
                ImgFilePath imgFilePath = (ImgFilePath)ImgFilePath_ListView.SelectedItem;
                ImgFilePathWindow imgFilePathWindow = new ImgFilePathWindow((ImgFilePath)ImgFilePath_ListView.SelectedItem, ImgFilePaths);
                if (true == imgFilePathWindow.ShowDialog()) {
                    imgFilePath.FilePath = imgFilePathWindow.vm.FilePath;
                    imgFilePath.WaterMark = imgFilePathWindow.vm.WaterMark;
                    ServiceFactory.GetImgFilePathService().Update(imgFilePath);
                    SetOperateMsg("修改目录成功");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteImgFilePath(object _) {
            if (ImgFilePath_ListView.SelectedIndex > -1) {
                if (MessageBoxResult.OK == MessageBox.Show("确认删除该路径？", Constants.MSG_WARN, MessageBoxButton.OKCancel)) {
                    ImgFilePath selected = (ImgFilePath)ImgFilePath_ListView.SelectedItem;
                    ServiceFactory.GetImgFilePathService().Delete(selected.Id);
                    ImgFilePaths.Remove(selected);
                    SetOperateMsg("删除成功");
                    ImgFilePathsChanged();
                }
            } else {
                MessageBox.Show("请先选择要删除路径！", Constants.MSG_ERROR);
            }
        }

        private void ClearImgFilePath(object _) {
            if (ImgFilePaths.Count > 0) {
                if (MessageBoxResult.OK == MessageBox.Show("确认清空所有路径？", Constants.MSG_WARN, MessageBoxButton.OKCancel)) {
                    ServiceFactory.GetImgFilePathService().Clear();
                    ImgFilePaths.Clear();
                    SetOperateMsg("清空成功");
                    ImgFilePathsChanged();
                }
            } else {
                MessageBox.Show("当前路径为空，无需清空！", Constants.MSG_ERROR);
            }
        }

        /// <summary>
        /// 打开目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenImgFilePath(object _) {
            if (ImgFilePath_ListView.SelectedIndex > -1) {
                ImgFilePath selected = (ImgFilePath)ImgFilePath_ListView.SelectedItem;
                if (Directory.Exists(selected.FilePath)) {
                    Process.Start(selected.FilePath);
                } else {
                    MessageBox.Show("当前目录不存在！", Constants.MSG_ERROR);
                }
            } else {
                MessageBox.Show("请先选择路径！", Constants.MSG_ERROR);
            }
        }

        private void ImgWaterMarkExecute(object _) {
            if (ImgFilePaths.Count == 0) {
                MessageBox.Show("当前路径为空，请添加文件目录！", Constants.MSG_ERROR);
            } else {
                if (ImgWaterMarkTimerCanRun) {
                    handExecute = true;
                    ImgWaterMarkTimerCanRun = false;
                    ImgWaterMarkExecuteTimer.Start();
                }
            }
        }
        private void ImgWaterMarkTaskToggle(object _) {
            if (ImgFilePaths.Count == 0) {
                MessageBox.Show("当前路径为空，请添加文件目录！", Constants.MSG_ERROR);
            } else {
                // 水印定时任务切换状态
                if (ImgWaterMarkTimerCanRun) {
                    AddWaterMarkLog("图片加水印定时器已开启...");
                    SetTaskStatus(Colors.Green, "任务运行中");
                    stop = false;
                    // 立即执行一次
                    ImgWaterMarkExecuteTimer.Start();
                    ImgWaterMarkTaskTimer.Start();
                    SetOperateMsg("水印定时器已开启");
                } else {
                    AddWaterMarkLog("图片加水印定时器已关闭...");
                    SetTaskStatus(Colors.Red, "任务未运行");
                    stop = true;
                    ImgWaterMarkExecuteTimer.Stop();
                    ImgWaterMarkTaskTimer.Stop();
                    SetOperateMsg("水印定时器已关闭");
                }
                ImgWaterMarkTimerCanRun = !ImgWaterMarkTimerCanRun;
            }
        }



        private void ImgWaterMarkHand_Tick(object sender, EventArgs e) {
            if (isRun) {
                return;
            }
            isRun = true;
            Task.Factory.StartNew(delegate {
                if (handExecute) { 
                    AddWaterMarkLog("图片加水印开始执行..."); 
                }
                ImgWaterMarkExecute();
                if (handExecute) { 
                    AddWaterMarkLog("图片加水印执行结束...");
                    ImgWaterMarkTimerCanRun = true;
                }
                ImgWaterMarkExecuteTimer.Stop();
                isRun = false;
                handExecute = false;
            });
        }
        private void ImgWaterMarkTask_Tick(object sender, EventArgs e) {
            if (isRun) {
                return;
            }
            isRun = true;
            Task.Factory.StartNew(delegate {
                ImgWaterMarkExecute();
                isRun = false;
            });
        }

        private static readonly Dictionary<string, List<string>> fileListDic = new Dictionary<string, List<string>>();// 待添加水印文件
        private static bool isRun = false;// 任务是否运行
        private static bool stop = false;// 任务是否停止
        private static bool handExecute = false;
        private void ImgWaterMarkExecute() {
            if (ImgFilePaths.Count == 0 || ImgFilePaths.Where(a => a.IsSelect).ToList().Count == 0) {
                AddWaterMarkLog("目录列表为空或未勾选任何目录...");
            } else {
                foreach (ImgFilePath imgFilePath in ImgFilePaths.Where(a => a.IsSelect).ToList()) {
                    AddImgFileList(imgFilePath.FilePath, imgFilePath.WaterMark);
                }
                if (fileListDic.Count > 0) {
                    Dictionary<string, List<string>> processListDic = new Dictionary<string, List<string>>();
                    foreach (string waterMarkText in fileListDic.Keys) {
                        List<string> fileList = fileListDic[waterMarkText];
                        foreach (string onefile in fileList) {
                            string ext = Path.GetExtension(onefile);
                            string filename = Path.GetFileName(onefile);
                            if (!filename.Contains("_原文件" + ext)) {
                                string ywj = Path.GetDirectoryName(onefile) + "\\" + Path.GetFileNameWithoutExtension(onefile) + "_原文件" + ext;
                                if (!File.Exists(ywj)) {
                                    bool hasValue = processListDic.TryGetValue(waterMarkText, out List<string> processList);
                                    if (!hasValue) {
                                        processList = new List<string>();
                                        processListDic.Add(waterMarkText, processList);
                                    }
                                    processList.Add(onefile);
                                }
                            }
                        }
                    }
                    if (processListDic.Count > 0) {
                        System.Drawing.Font font = FontsUtils.GetDrawingFont(Configs.waterMarkFontFamily, Configs.waterMarkFontSize, Configs.waterMarkFontBold, Configs.waterMarkFontItalic, Configs.waterMarkFontUnderline, Configs.waterMarkFontStrikeout);

                        Brush brush = WaterMarkUtils.GetWaterMarkBrush(Configs.waterMarkFontIsGradient, Configs.waterMarkFontColor, Configs.waterMarkFontGradientColor, Configs.waterMarkOpacity);
                        Stopwatch watch = new Stopwatch();
                        foreach (string waterMarkText in processListDic.Keys) {
                            if (stop) {
                                break;
                            }
                            FormattedText formattedText = WaterMarkUtils.GetFormattedText(waterMarkText, Configs.waterMarkFontFamily, Configs.waterMarkFontItalic, Configs.waterMarkFontBold, Configs.waterMarkFontSize, brush);
                            List<string> processList = processListDic[waterMarkText];
                            foreach (string filePath in processList) {
                                if (stop) {
                                    break;
                                }
                                watch.Restart();

                                string ext = Path.GetExtension(filePath).ToLower();
                                if (".pdf".Equals(ext)) {
                                    // PDF文件加水印
                                    try {
                                        PdfAddWatermark(false, filePath, waterMarkText, font);
                                    } catch (Exception e) {
                                        Console.WriteLine(e.Message);
                                    }
                                } else {
                                    // 图片加水印
                                    // media
                                    CreateWaterMarkImage(false, filePath, formattedText, Configs.waterMarkFontSize, brush);
                                    // drawing
                                    // CreateWaterMarkImage(false, filePath, waterMarkText, font);
                                }
                                watch.Stop();
                                AddWaterMarkLog($"{filePath}:处理完成,耗时：{watch.ElapsedMilliseconds}ms");
                            }
                        }
                        font.Dispose();
                        brush.Freeze();
                        stop = false;
                    }
                    // 执行结束清空集合
                    processListDic.Clear();
                }
                fileListDic.Clear();
            }
        }

        /// <summary>
        /// 列出所有的图片(jpg,bmp,png)文件和pdf文件
        /// </summary>
        /// <param name="filepath">目录</param>
        /// <param name="waterMark">水印文本</param>
        private void AddImgFileList(string filepath, string waterMark) {
            bool hasList = fileListDic.TryGetValue(waterMark, out List<string> list);
            if (!hasList) {
                list = new List<string>();
                fileListDic.Add(waterMark, list);
            }
            if (Directory.Exists(filepath)) {
                // 列出指定路径下的所有文件
                foreach (string file in Directory.GetFiles(filepath, "*.jpg")) {
                    list.Add(file);
                }
                foreach (string file in Directory.GetFiles(filepath, "*.jpeg")) {
                    list.Add(file);
                }
                foreach (string file in Directory.GetFiles(filepath, "*.png")) {
                    list.Add(file);
                }
                foreach (string file in Directory.GetFiles(filepath, "*.bmp")) {
                    list.Add(file);
                }
                foreach (string file in Directory.GetFiles(filepath, "*.pdf")) {
                    list.Add(file);
                }
                // 递归列出所有子文件夹
                foreach (string directory in Directory.GetDirectories(filepath)) {
                    AddImgFileList(directory, waterMark);
                }
            }
        }

        private void OperateMessageTimer_Tick(object sender, EventArgs e) {
            // 执行完成后,定时器停止，清除消息
            OperateMessageTimer.Stop();
            InitOperateMsg(Colors.White, string.Empty);
        }
        public void SetOperateMsg(string msg) {
            SetOperateMsg(Colors.Green, msg);
        }
        public void SetOperateMsg(Color color, string msg) {
            if (!string.IsNullOrEmpty(OperateMsg)) {
                // 存在上次未结束的状态报告
                OperateMessageTimer_Tick(null, null);
            }
            OperateMessageTimer.Start();
            InitOperateMsg(color, msg);
        }

        public void AddWaterMarkLog(string log) {
            AddWaterMarkLog(Colors.LightGreen, log);
        }
        public void AddWaterMarkLog(Color color, string log) {
            ThreadPool.QueueUserWorkItem(delegate {
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Application.Current.Dispatcher));
                SynchronizationContext.Current.Post(pl => {
                    //里面写真正的业务内容
                    TaskLogs.Add(new Log { ColorBrush = new SolidColorBrush(color), Msg = $"{DateTime.Now:yy-M-d HH:mm:ss}-{log}" });
                    if (ScrollEnd) {
                        TaskLog_RichTextBox.ScrollToEnd();
                    }
                    if (TaskLogs.Count > Constants.LOG_LIMIT) {
                        for (int i = Constants.LOG_CACHE; i >= 0; i--) {
                            TaskLogs.RemoveAt(i);
                        }
                    }
                    WaterMarkLogChanged();
                }, null);
            });


        }
        /// <summary>
        /// 清空日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearWaterMarkLog(object _) {
            TaskLogs.Clear();
            SetOperateMsg("日志清空成功");
            WaterMarkLogChanged();
        }
    }
}
