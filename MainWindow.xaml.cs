using AddWaterMark.Beans;
using AddWaterMark.Config;
using AddWaterMark.DataBase.Beans;
using AddWaterMark.DataBase.Services;
using AddWaterMark.Utils;
using AddWaterMark.ViewModels;
using AddWaterMark.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            string fontIsGradient = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_IS_GRADIENT];
            string fontColor = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_COLOR];
            string fontGradientColor = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_GRADIENT_COLOR];
            string fontBold = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_BOLD];
            string fontItalic = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_ITALIC];
            string fontUnderline = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_UNDERLINE];
            string fontStrikeout = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_STRIKEOUT];
            string horizontalDisStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_HORIZONTAL_DIS];
            string verticalDisStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_VERTICAL_DIS];
            Configs.waterMarkText = string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text.Trim()) ? Constants.WATER_MARK_TEXT : text.Trim();
            Configs.waterMarkOpacity = string.IsNullOrEmpty(opacityStr) || !NumberUtils.IsByte(opacityStr, out byte opacity) ? Constants.WATER_MARK_OPACITY : opacity;
            Configs.waterMarkRotate = string.IsNullOrEmpty(rotateStr) || !NumberUtils.IsInt(rotateStr, out int rotate) ? Constants.WATER_MARK_ROTATE : rotate;
            Configs.waterMarkFontFamily = string.IsNullOrEmpty(fontFamily) || !mainViewModel.SystemFonts.Contains(fontFamily) ? Constants.WATER_MARK_FONT_FAMILY : fontFamily;
            Configs.waterMarkFontSize = string.IsNullOrEmpty(fontSizeStr) || !NumberUtils.IsInt(fontSizeStr, out int fontSize) ? Constants.WATER_MARK_FONT_SIZE : fontSize;
            Configs.waterMarkFontIsGradient = string.IsNullOrEmpty(fontIsGradient) ? false : Convert.ToBoolean(fontIsGradient);
            Configs.waterMarkFontColor = string.IsNullOrEmpty(fontColor) ? Constants.WATER_MARK_FONT_COLOR : fontColor;
            Configs.waterMarkFontGradientColor = string.IsNullOrEmpty(fontGradientColor) ? Constants.WATER_MARK_FONT_GRADIENT_COLOR : fontGradientColor;
            Configs.waterMarkFontBold = string.IsNullOrEmpty(fontBold) ? false : Convert.ToBoolean(fontBold);
            Configs.waterMarkFontItalic = string.IsNullOrEmpty(fontItalic) ? false : Convert.ToBoolean(fontItalic);
            Configs.waterMarkFontUnderline = string.IsNullOrEmpty(fontUnderline) ? false : Convert.ToBoolean(fontUnderline);
            Configs.waterMarkFontStrikeout = string.IsNullOrEmpty(fontStrikeout) ? false : Convert.ToBoolean(fontStrikeout);
            Configs.waterMarkHorizontalDis = string.IsNullOrEmpty(horizontalDisStr) || !NumberUtils.IsInt(horizontalDisStr, out int horizontalDis) ? Constants.WATER_MARK_HORIZONTAL_DIS : horizontalDis;
            Configs.waterMarkVerticalDis = string.IsNullOrEmpty(verticalDisStr) || !NumberUtils.IsInt(verticalDisStr, out int verticalDis) ? Constants.WATER_MARK_VERTICAL_DIS : verticalDis;
            mainViewModel.WaterMarkText = Configs.waterMarkText;
            mainViewModel.WaterMarkOpacity = Configs.waterMarkOpacity;
            mainViewModel.WaterMarkRotate = Configs.waterMarkRotate;
            mainViewModel.WaterMarkFontFamily = Configs.waterMarkFontFamily;
            mainViewModel.WaterMarkFontSize = Configs.waterMarkFontSize;
            mainViewModel.WaterMarkFontIsGradient = Configs.waterMarkFontIsGradient;
            mainViewModel.WaterMarkFontColor = Configs.waterMarkFontColor;
            mainViewModel.WaterMarkFontGradientColor = Configs.waterMarkFontGradientColor;
            mainViewModel.WaterMarkFontBold = Configs.waterMarkFontBold;
            mainViewModel.WaterMarkFontItalic = Configs.waterMarkFontItalic;
            mainViewModel.WaterMarkFontUnderline = Configs.waterMarkFontUnderline;
            mainViewModel.WaterMarkFontStrikeout = Configs.waterMarkFontStrikeout;
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
            if (MessageBoxResult.OK == MessageBox.Show("确认退出该程序吗？", Constants.MSG_WARN, MessageBoxButton.OKCancel)) {
                if (!Configs.inited) {
                    return;
                }
                // 页面GridSplitter位置获取
                mainViewModel.Tab2SplitDistance = ImgFilePaths_Row.Height.Value;
                if (mainViewModel.ConfigIsChanged) {
                    MessageBoxResult result = MessageBox.Show("存在修改的配置，保存配置吗？", Constants.MSG_WARN, MessageBoxButton.YesNoCancel);
                    if (MessageBoxResult.Yes == result) {
                        // 保存配置退出
                        SaveConfigs();
                    } else if (MessageBoxResult.No == result) {
                        // 不保存配置退出
                        CancelSaveConfig_Click(sender, null);
                        SaveConfigs();
                    } else {
                        // 取消退出
                        e.Cancel = true;
                    }
                }
                
            } else {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// 字体颜色选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            if (System.Windows.Forms.DialogResult.OK == cd.ShowDialog()) {
                mainViewModel.WaterMarkFontColor = System.Drawing.ColorTranslator.ToHtml(cd.Color);
                if (!Configs.waterMarkFontColor.Equals(mainViewModel.WaterMarkFontColor)) {
                    mainViewModel.ConfigIsChanged = true;
                }
            }
        }
        /// <summary>
        /// 渐变色窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontGradientColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            GradientColorWindow gradientColorWindow = new GradientColorWindow(mainViewModel.WaterMarkFontGradientColor);
            if(true == gradientColorWindow.ShowDialog()) {
                mainViewModel.WaterMarkFontGradientColor = gradientColorWindow.GradientColorResult;
                Configs_Changed(sender, e);
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
                // media
                // CreateWaterMarkImage(true, null, GetTestFormattedText());
                // drawing
                CreateWaterMarkImage(true, null, mainViewModel.WaterMarkText,
                    GetDrawingFont(mainViewModel.WaterMarkFontFamily, mainViewModel.WaterMarkFontSize, mainViewModel.WaterMarkFontBold, mainViewModel.WaterMarkFontItalic, mainViewModel.WaterMarkFontUnderline, mainViewModel.WaterMarkFontStrikeout)
                   );
                SetOperateMsg("生成水印成功");
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
                // media
                // CreateWaterMarkImage(true, openFileDialog.FileName, GetTestFormattedText());
                // drawing
                CreateWaterMarkImage(true, openFileDialog.FileName, mainViewModel.WaterMarkText,
                    GetDrawingFont(mainViewModel.WaterMarkFontFamily, mainViewModel.WaterMarkFontSize, mainViewModel.WaterMarkFontBold, mainViewModel.WaterMarkFontItalic, mainViewModel.WaterMarkFontUnderline, mainViewModel.WaterMarkFontStrikeout)
                    );
                SetOperateMsg("生成水印成功");
                testImgPath = openFileDialog.FileName;
            }
            mainViewModel.CanTestWaterMark = true;
        }

        private void RefreshWaterMark_Click(object sender, RoutedEventArgs e) {
            mainViewModel.CanTestWaterMark = false;
            // media
            // CreateWaterMarkImage(true, testImgPath, GetTestFormattedText());
            // drawing
            CreateWaterMarkImage(true, testImgPath, mainViewModel.WaterMarkText,
                    GetDrawingFont(mainViewModel.WaterMarkFontFamily, mainViewModel.WaterMarkFontSize, mainViewModel.WaterMarkFontBold, mainViewModel.WaterMarkFontItalic, mainViewModel.WaterMarkFontUnderline, mainViewModel.WaterMarkFontStrikeout)
                    );
            SetOperateMsg("刷新水印成功");
            mainViewModel.CanTestWaterMark = true;
        }

        private FormattedText GetTestFormattedText() {
            return GetFormattedText(mainViewModel.WaterMarkText, mainViewModel.WaterMarkFontFamily, mainViewModel.WaterMarkFontItalic, mainViewModel.WaterMarkFontBold
                , mainViewModel.WaterMarkFontSize, mainViewModel.WaterMarkFontIsGradient, mainViewModel.WaterMarkFontColor, mainViewModel.WaterMarkFontGradientColor, mainViewModel.WaterMarkOpacity);
        }
        private void ClearWaterMark_Click(object sender, RoutedEventArgs e) {
            mainViewModel.WaterMarkBitmap = null;
            SetOperateMsg("清除水印图片成功");
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
                    SetOperateMsg("保存水印文件成功");
                }
            } else {
                MessageBox.Show("不存在水印图片！", Constants.MSG_ERROR);
            }
        }

        private Brush GetWaterMarkBrush(bool isGradient, string fontColorStr, string gradientColor, byte opacity) {
            Brush brush;
            // 不透明度按100转成byte 255的数值范围
            opacity = (byte)(opacity * 255 / 100);
            if (isGradient) {
                // 渐变色
                GradientStopCollection gradients = new GradientStopCollection();
                if (!string.IsNullOrEmpty(gradientColor)) {
                    string[] gradientColorsArray = gradientColor.Split(';');
                    foreach (string gradientColorStr in gradientColorsArray) {
                        string[] gradientColorArray = gradientColorStr.Split(':');
                        float point = Convert.ToSingle(gradientColorArray[0]);
                        string colorHtml = gradientColorArray[1];
                        Color pointColor = (Color)ColorConverter.ConvertFromString(colorHtml);
                        Color pointOpacityColor = Color.FromArgb(opacity, pointColor.R, pointColor.G, pointColor.B);
                        gradients.Add(new GradientStop(pointOpacityColor, point));
                    }
                }
                brush = new LinearGradientBrush(gradients, 0D);
            } else {
                // 纯色
                Color fontColor = (Color)ColorConverter.ConvertFromString(fontColorStr);
                Color waterMarkColor = Color.FromArgb(opacity, fontColor.R, fontColor.G, fontColor.B);
                brush = new SolidColorBrush(waterMarkColor);
            }
            return brush;
        }

        private FormattedText GetFormattedText(string waterMark, string fontFamilyStr, bool isItalic, bool isBold, double fontSize, bool isGradient, string fontColor, string fontGradientColor, byte opacity) {
            // 字体
            FontFamily fontFamily = new FontFamily(fontFamilyStr);
            FontWeight fontWeight = FontWeights.Normal;
            if (isBold) {
                fontWeight = FontWeights.Bold;
            }
            FontStyle fontStyle = FontStyles.Normal;
            if (isItalic) {
                fontStyle = FontStyles.Italic;
            }
            Brush brush = GetWaterMarkBrush(isGradient, fontColor, fontGradientColor, opacity);
            Typeface typeface = new Typeface(fontFamily, fontStyle, fontWeight, FontStretches.Normal);
            return new FormattedText(
                waterMark,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                brush);
        }

        private System.Drawing.Font GetDrawingFont(string fontFamily, int fontSize, bool fontBold, bool fontItalic, bool fontUnderline, bool fontStrikeout) {
            System.Drawing.FontStyle fontStyle = System.Drawing.FontStyle.Regular;
            if (fontBold) {
                fontStyle |= System.Drawing.FontStyle.Bold;
            }
            if (fontItalic) {
                fontStyle |= System.Drawing.FontStyle.Italic;
            }
            if (fontUnderline) {
                fontStyle |= System.Drawing.FontStyle.Underline;
            }
            if (fontStrikeout) {
                fontStyle |= System.Drawing.FontStyle.Strikeout;
            }
            return new System.Drawing.Font(fontFamily, fontSize, fontStyle);
        }

        private System.Drawing.Brush GetDrawingBrush(byte opacity, bool isGradient, string fontColor, string fontGradientColor, int width, int height) {
            // 不透明度按100转成byte 255的数值范围
            opacity = (byte)(opacity * 255 / 100);
            // 画刷
            System.Drawing.Brush brush;
            if (isGradient) {
                System.Drawing.Drawing2D.LinearGradientBrush gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(new System.Drawing.Rectangle(0, 0, width, height), System.Drawing.Color.Black, System.Drawing.Color.White, 0f);

                if (!string.IsNullOrEmpty(fontGradientColor)) {
                    List<GradientColor> gradientColors = GradientColorUtils.GetList(fontGradientColor);
                    // 判断是否存在point0和1的，因为drawing的LinearGradientBrush必须要有0和1的颜色，但是media中不需要，需要补
                    if(gradientColors[0].Point != 0) {
                        gradientColors.Insert(0, new GradientColor(0, gradientColors[0].Color));
                    }
                    if (gradientColors[gradientColors.Count - 1].Point != 1) {
                        gradientColors.Add(new GradientColor(1, gradientColors[gradientColors.Count - 1].Color));
                    }
                    System.Drawing.Drawing2D.ColorBlend blend = new System.Drawing.Drawing2D.ColorBlend();

                    System.Drawing.Color[] colors = new System.Drawing.Color[gradientColors.Count];
                    float[] positions = new float[gradientColors.Count];
                    for (int i = 0; i < gradientColors.Count; i++) {
                        GradientColor gradientColor = gradientColors[i];
                        colors[i] = System.Drawing.Color.FromArgb(opacity, System.Drawing.ColorTranslator.FromHtml(gradientColor.Color));
                        positions[i] = gradientColor.Point;
                    }
                    blend.Colors = colors;
                    blend.Positions = positions;
                    gradientBrush.InterpolationColors = blend;
                }
                brush = gradientBrush;
            } else {
                // 设置颜色和透明度
                System.Drawing.Color waterMarkColor = System.Drawing.Color.FromArgb(opacity, System.Drawing.ColorTranslator.FromHtml(fontColor));
                brush = new System.Drawing.SolidBrush(waterMarkColor);
            }

            return brush;
        }

        /// <summary>
        /// 创建水印图
        /// </summary>
        /// <param name="isTest">测试水印</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="formattedText">水印文本样式（包含水印文本）</param>
        private void CreateWaterMarkImage(bool isTest, string filePath, FormattedText formattedText) {
            int waterMarkRotate = Configs.waterMarkRotate;
            int waterMarkHorizontalDis = Configs.waterMarkHorizontalDis;
            int waterMarkVerticalDis = Configs.waterMarkVerticalDis;
            if (isTest) {
                waterMarkRotate = mainViewModel.WaterMarkRotate;
                waterMarkHorizontalDis = mainViewModel.WaterMarkHorizontalDis;
                waterMarkVerticalDis = mainViewModel.WaterMarkVerticalDis;
                if (null != mainViewModel.WaterMarkBitmap) {
                    mainViewModel.WaterMarkBitmap = null;
                }
            }
            string ext = string.IsNullOrEmpty(filePath) ? ".jpg" : Path.GetExtension(filePath).ToLower();
            BitmapSource backPhoto;
            int photoWidth, photoHeight;
            if (string.IsNullOrEmpty(filePath)) {
                // 未指定图片，绘制白色背景图
                photoWidth = (int)WaterMarkBorder.ActualWidth;
                photoHeight = (int)WaterMarkBorder.ActualHeight;
                int stride = ((photoWidth * 32 + 31) & ~31) / 8;
                byte[] pixels = new byte[photoHeight * stride];
                BitmapPalette myPalette = new BitmapPalette(new List<Color> { Colors.White });
                backPhoto = BitmapSource.Create(photoWidth, photoHeight, 96, 96, PixelFormats.Indexed1, myPalette, pixels, stride);
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
            }

            // 水印图层
            RenderTargetBitmap composeImage = new RenderTargetBitmap(photoWidth, photoHeight, 96, 96, PixelFormats.Default);
            int circleDiameter = (int)Math.Sqrt(Math.Pow(photoWidth, 2D) + Math.Pow(photoHeight, 2D));
            DrawingVisual drawingVisual = new DrawingVisual();

            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(backPhoto, new Rect(0, 0, photoWidth, photoHeight));

            float x = (photoWidth - circleDiameter) / 2, y = (photoHeight - circleDiameter) / 2;

            // 设置旋转
            RotateTransform transform = new RotateTransform(waterMarkRotate, photoWidth / 2, photoHeight / 2);
            drawingContext.PushTransform(transform);

            int xcount = circleDiameter / waterMarkHorizontalDis + 1;
            int ycount = circleDiameter / waterMarkVerticalDis + 1;
            float ox = x;

            for (int k = 0; k < ycount; k++) {
                for (int i = 0; i < xcount; i++) {
                    drawingContext.DrawText(formattedText, new Point(x, y));
                    x += waterMarkHorizontalDis;
                }
                x = ox;
                y += waterMarkVerticalDis;
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
                mainViewModel.WaterMarkBitmap = bitmapImage;
            } else {
                // 水印文件保存
                using FileStream fileStream = new FileStream(filePath, FileMode.Create);
                bitMapEncoder.Save(fileStream);
            }
            backPhoto.Freeze();
        }

        /// <summary>
        /// 创建水印图
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
                waterMarkRotate = mainViewModel.WaterMarkRotate;
                waterMarkHorizontalDis = mainViewModel.WaterMarkHorizontalDis;
                waterMarkVerticalDis = mainViewModel.WaterMarkVerticalDis;
                waterMarkOpacity = mainViewModel.WaterMarkOpacity;
                waterMarkFontIsGradient = mainViewModel.WaterMarkFontIsGradient;
                waterMarkFontColor = mainViewModel.WaterMarkFontColor;
                waterMarkFontGradientColor = mainViewModel.WaterMarkFontGradientColor;
                if (null != mainViewModel.WaterMarkBitmap) {
                    mainViewModel.WaterMarkBitmap = null;
                }
            }

            System.Drawing.Image backPhoto;
            int photoWidth, photoHeight;
            if (string.IsNullOrEmpty(filePath)) {
                // 未指定图片，绘制白色背景图
                photoWidth = (int)WaterMarkBorder.ActualWidth;
                photoHeight = (int)WaterMarkBorder.ActualHeight;
                backPhoto = new System.Drawing.Bitmap(photoWidth, photoHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                System.Drawing.Graphics backPhotoGraphics = System.Drawing.Graphics.FromImage(backPhoto);
                backPhotoGraphics.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.White), new System.Drawing.Rectangle(0, 0, photoWidth, photoHeight));
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
            System.Drawing.Bitmap bmPhoto = new System.Drawing.Bitmap(photoWidth, photoHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(72, 72);
            System.Drawing.Graphics bmPhotoGraphics = System.Drawing.Graphics.FromImage(bmPhoto);
            bmPhotoGraphics.Clear(System.Drawing.Color.FromName("white"));
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
            System.Drawing.Brush brush = GetDrawingBrush(waterMarkOpacity, waterMarkFontIsGradient, waterMarkFontColor, waterMarkFontGradientColor, (int)crSize.Width, (int)crSize.Height);

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
            if (!isChange && Configs.waterMarkFontIsGradient != mainViewModel.WaterMarkFontIsGradient) {
                isChange = true;
            }
            if (!isChange && !Configs.waterMarkFontColor.Equals(mainViewModel.WaterMarkFontColor)) {
                isChange = true;
            }
            if (!isChange && !Configs.waterMarkFontGradientColor.Equals(mainViewModel.WaterMarkFontGradientColor)) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkFontBold != mainViewModel.WaterMarkFontBold) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkFontItalic != mainViewModel.WaterMarkFontItalic) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkFontUnderline != mainViewModel.WaterMarkFontUnderline) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkFontStrikeout != mainViewModel.WaterMarkFontStrikeout) {
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
                mainViewModel.WaterMarkFontIsGradient = false;
                mainViewModel.WaterMarkFontColor = Constants.WATER_MARK_FONT_COLOR;
                mainViewModel.WaterMarkFontGradientColor = Constants.WATER_MARK_FONT_GRADIENT_COLOR;
                mainViewModel.WaterMarkFontBold = false;
                mainViewModel.WaterMarkFontItalic = false;
                mainViewModel.WaterMarkFontUnderline = false;
                mainViewModel.WaterMarkFontStrikeout = false;
                mainViewModel.WaterMarkHorizontalDis = Constants.WATER_MARK_HORIZONTAL_DIS;
                mainViewModel.WaterMarkVerticalDis = Constants.WATER_MARK_VERTICAL_DIS;
                mainViewModel.ConfigIsChanged = false;
                SaveConfigs();
                SetOperateMsg("恢复默认配置成功");
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
            mainViewModel.WaterMarkFontIsGradient = Configs.waterMarkFontIsGradient;
            mainViewModel.WaterMarkFontColor = Configs.waterMarkFontColor;
            mainViewModel.WaterMarkFontGradientColor = Configs.waterMarkFontGradientColor;
            mainViewModel.WaterMarkFontBold = Configs.waterMarkFontBold;
            mainViewModel.WaterMarkFontItalic = Configs.waterMarkFontItalic;
            mainViewModel.WaterMarkFontUnderline = Configs.waterMarkFontUnderline;
            mainViewModel.WaterMarkFontStrikeout = Configs.waterMarkFontStrikeout;
            mainViewModel.WaterMarkHorizontalDis = Configs.waterMarkHorizontalDis;
            mainViewModel.WaterMarkVerticalDis = Configs.waterMarkVerticalDis;

            mainViewModel.ConfigIsChanged = false;
            SetOperateMsg("取消修改成功");
        }
        /// <summary>
        /// 保存当前修改的配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveConfig_Click(object sender, RoutedEventArgs e) {
            SaveConfigs();
            mainViewModel.ConfigIsChanged = false;
            SetOperateMsg("保存配置成功");
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
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_IS_GRADIENT, ref Configs.waterMarkFontIsGradient, mainViewModel.WaterMarkFontIsGradient);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_COLOR, ref Configs.waterMarkFontColor, mainViewModel.WaterMarkFontColor);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_GRADIENT_COLOR, ref Configs.waterMarkFontGradientColor, mainViewModel.WaterMarkFontGradientColor);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_BOLD, ref Configs.waterMarkFontBold, mainViewModel.WaterMarkFontBold);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_ITALIC, ref Configs.waterMarkFontItalic, mainViewModel.WaterMarkFontItalic);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_UNDERLINE, ref Configs.waterMarkFontUnderline, mainViewModel.WaterMarkFontUnderline);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WATER_MARK, Constants.INI_KEY_WATER_MARK_FONT_STRIKEOUT, ref Configs.waterMarkFontStrikeout, mainViewModel.WaterMarkFontStrikeout);
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
                SetOperateMsg("添加目录成功");
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
                    SetOperateMsg("修改目录成功");
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
                    SetOperateMsg("删除成功");
                }
            } else {
                MessageBox.Show("请先选择要删除路径！", Constants.MSG_ERROR);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenImgFilePath_Click(object sender, RoutedEventArgs e) {
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

        private void ImgFilePathClear_Click(object sender, RoutedEventArgs e) {
            if (mainViewModel.ImgFilePaths.Count > 0) {
                if (MessageBoxResult.OK == MessageBox.Show("确认清空所有路径？", Constants.MSG_WARN, MessageBoxButton.OKCancel)) {
                    ServiceFactory.GetImgFilePathService().Clear();
                    mainViewModel.ImgFilePaths.Clear();
                    SetOperateMsg("清空成功");
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
                    mainViewModel.SetTaskStatus(Colors.Green, "任务运行中");
                    // 立即执行一次
                    ImgWaterMark_Tick(sender, e);
                    ImgWaterMarkTimer.Start();
                    stop = false;
                    SetOperateMsg("水印定时器已开启");
                } else {
                    AddWaterMarkLog("图片加水印定时器已关闭...");
                    mainViewModel.SetTaskStatus(Colors.Red, "任务未运行");
                    ImgWaterMarkTimer.Stop();
                    stop = true;
                    SetOperateMsg("水印定时器已关闭");
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

        // key=waterMarkText， 
        private static readonly Dictionary<string, List<string>> fileListDic = new Dictionary<string, List<string>>();// 待添加水印文件
        private static bool isRun = false;// 任务是否运行
        private static bool stop = false;// 任务是否停止
        private static bool handExecute = false;// 手工执行
        private void ImgWaterMark_Tick(object sender, EventArgs e) {
            if (isRun) {
                return;
            }
            isRun = true;
            // 异步处理
            Task.Factory.StartNew(delegate {
                if (handExecute) {
                    AddWaterMarkLog("图片加水印开始执行...");
                }
                fileListDic.Clear();
                if (mainViewModel.ImgFilePaths.Count > 0) {
                    foreach (ImgFilePath imgFilePath in mainViewModel.ImgFilePaths) {
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
                            System.Drawing.Font font = GetDrawingFont(Configs.waterMarkFontFamily, Configs.waterMarkFontSize, Configs.waterMarkFontBold, Configs.waterMarkFontItalic, Configs.waterMarkFontUnderline, Configs.waterMarkFontStrikeout);
                            Stopwatch watch = new Stopwatch();
                            foreach (string waterMarkText in processListDic.Keys) {
                                FormattedText formattedText = GetFormattedText(waterMarkText, Configs.waterMarkFontFamily, Configs.waterMarkFontItalic, Configs.waterMarkFontBold, Configs.waterMarkFontSize, Configs.waterMarkFontIsGradient, Configs.waterMarkFontColor, Configs.waterMarkFontGradientColor, Configs.waterMarkOpacity);
                                List<string> processList = processListDic[waterMarkText];
                                foreach (string filePath in processList) {
                                    if (stop) {
                                        break;
                                    }
                                    watch.Restart();
                                    // media
                                    // CreateWaterMarkImage(false, filePath, formattedText);
                                    // drawing
                                    CreateWaterMarkImage(false, filePath, waterMarkText, font);
                                    watch.Stop();
                                    AddWaterMarkLog($"{filePath}:处理完成,耗时：{watch.ElapsedMilliseconds}ms");
                                }
                            }
                            stop = false;
                        }
                    }
                }
                if (handExecute) {
                    AddWaterMarkLog("图片加水印执行结束...");
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

        private void SetOperateMsg(string msg) {
            SetOperateMsg(Colors.Green, msg);
        }
        private void SetOperateMsg(Color color, string msg) {
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
            mainViewModel.InitOperateMsg(Colors.White, string.Empty);
        }

        /// <summary>
        /// 列出所有的图片文件
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filepath"></param>
        /// <param name="waterMark"></param>
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
            SetOperateMsg("日志清空成功");
        }

        private void Lnk_Click(object sender, RoutedEventArgs e) {
            Process.Start(((System.Windows.Documents.Hyperlink)sender).NavigateUri.ToString());
        }
    }
}