using AddWaterMark.Config;
using AddWaterMark.DataBase.Beans;
using AddWaterMark.DataBase.Services;
using AddWaterMark.Utils;
using AddWaterMark.ViewModels;
using AddWaterMark.Windows;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Linq;

namespace AddWaterMark {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        // 自动GC定时器 5分钟执行一次
        private readonly System.Windows.Threading.DispatcherTimer AutoGcTimer = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMinutes(5) };
        // VM
        readonly MainViewModel vm = new MainViewModel();
        public MainWindow() {
            InitializeComponent();
            Configs.Handler = new WindowInteropHelper(this).Handle;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            // 自动GC
            AutoGcTimer.Tick += AutoGcTimer_Tick;
            AutoGcTimer.Start();
            // 模型赋值
            DataContext = vm;
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
            Configs.mainHeight = NumberUtils.IsNumeric(mainHeightStr, out double mainHeight) ? mainHeight : Constants.MAIN_HEIGHT;
            Configs.mainWidth = NumberUtils.IsNumeric(mainWidthStr, out double mainWidth) ? mainWidth : Constants.MAIN_WIDTH;
            Configs.mainLeft = NumberUtils.IsNumeric(mainLeftStr, out double mainLeft) ? mainLeft : Constants.MAIN_LEFT;
            Configs.mainTop = NumberUtils.IsNumeric(mainTopStr, out double mainTop) ? mainTop : Constants.MAIN_TOP;
            vm.MainHeight = Configs.mainHeight;
            vm.MainWidth = Configs.mainWidth;
            vm.MainLeft = Configs.mainLeft;
            vm.MainTop = Configs.mainTop;
            #endregion
            #region 水印设置项
            string text = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_TEXT];
            string opacityStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_OPACITY];
            string rotateStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_ROTATE];
            string fontFamilyStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_FAMILY];
            string fontSizeStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_SIZE];
            string fontIsGradientStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_IS_GRADIENT];
            string fontColorStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_COLOR];
            string fontGradientColorStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_GRADIENT_COLOR];
            string fontBoldStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_BOLD];
            string fontItalicStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_ITALIC];
            string fontUnderlineStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_UNDERLINE];
            string fontStrikeoutStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_FONT_STRIKEOUT];
            string horizontalDisStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_HORIZONTAL_DIS];
            string verticalDisStr = iniData[Constants.INI_SECTION_WATER_MARK][Constants.INI_KEY_WATER_MARK_VERTICAL_DIS];
            Configs.waterMarkText = StringUtils.IsNotEmpty(text, out string waterMarkText) ? waterMarkText : Constants.WATER_MARK_TEXT;
            Configs.waterMarkOpacity = NumberUtils.IsByte(opacityStr, out byte opacity) ? opacity : Constants.WATER_MARK_OPACITY;
            Configs.waterMarkRotate = NumberUtils.IsInt(rotateStr, out int rotate) ? rotate : Constants.WATER_MARK_ROTATE;
            Configs.waterMarkFontFamily = StringUtils.IsNotEmpty(fontFamilyStr, out string fontFamily) && vm.SystemFonts.Contains(fontFamily) ? fontFamily : Constants.WATER_MARK_FONT_FAMILY;
            Configs.waterMarkFontSize = NumberUtils.IsInt(fontSizeStr, out int fontSize) ? fontSize : Constants.WATER_MARK_FONT_SIZE;
            Configs.waterMarkFontIsGradient = StringUtils.IsBool(fontIsGradientStr, out bool fontIsGradient) ? fontIsGradient : false;
            Configs.waterMarkFontColor = StringUtils.IsNotEmpty(fontColorStr, out string fontColor) ? fontColor : Constants.WATER_MARK_FONT_COLOR;
            Configs.waterMarkFontGradientColor = StringUtils.IsNotEmpty(fontGradientColorStr, out string fontGradientColor) ? fontGradientColor : Constants.WATER_MARK_FONT_GRADIENT_COLOR;
            Configs.waterMarkFontBold = StringUtils.IsBool(fontBoldStr, out bool fontBold) ? fontBold : false;
            Configs.waterMarkFontItalic = StringUtils.IsBool(fontItalicStr, out bool fontItalic) ? fontItalic : false;
            Configs.waterMarkFontUnderline = StringUtils.IsBool(fontUnderlineStr, out bool fontUnderline) ? fontUnderline : false;
            Configs.waterMarkFontStrikeout = StringUtils.IsBool(fontStrikeoutStr, out bool fontStrikeout) ? fontStrikeout : false;
            Configs.waterMarkHorizontalDis = NumberUtils.IsInt(horizontalDisStr, out int horizontalDis) ? horizontalDis : Constants.WATER_MARK_HORIZONTAL_DIS;
            Configs.waterMarkVerticalDis = NumberUtils.IsInt(verticalDisStr, out int verticalDis) ? verticalDis : Constants.WATER_MARK_VERTICAL_DIS;
            vm.WaterMarkText = Configs.waterMarkText;
            vm.WaterMarkOpacity = Configs.waterMarkOpacity;
            vm.WaterMarkRotate = Configs.waterMarkRotate;
            vm.WaterMarkFontFamily = Configs.waterMarkFontFamily;
            vm.WaterMarkFontSize = Configs.waterMarkFontSize;
            vm.WaterMarkFontIsGradient = Configs.waterMarkFontIsGradient;
            vm.WaterMarkFontColor = Configs.waterMarkFontColor;
            vm.WaterMarkFontGradientColor = Configs.waterMarkFontGradientColor;
            vm.WaterMarkFontBold = Configs.waterMarkFontBold;
            vm.WaterMarkFontItalic = Configs.waterMarkFontItalic;
            vm.WaterMarkFontUnderline = Configs.waterMarkFontUnderline;
            vm.WaterMarkFontStrikeout = Configs.waterMarkFontStrikeout;
            vm.WaterMarkHorizontalDis = Configs.waterMarkHorizontalDis;
            vm.WaterMarkVerticalDis = Configs.waterMarkVerticalDis;
            #endregion
            #region 页面以及水印目录加载
            // 加载上次打开的TabPage
            string lastOpenTabStr = iniData[Constants.INI_SECTION_PAGE][Constants.INI_KEY_LAST_OPEN_TAB];
            Configs.lastOpenTab = NumberUtils.IsInt(lastOpenTabStr, out int lastOpenTab) ? lastOpenTab : Constants.LAST_OPEN_TAB;
            vm.LastOpenTab = Configs.lastOpenTab;
            // 加载上次页面的GridSplitter位置
            string tab2SplitDistanceStr = iniData[Constants.INI_SECTION_PAGE][Constants.INI_KEY_TAB2_SPLIT_DISTANCE];
            Configs.tab2SplitDistance = NumberUtils.IsNumeric(tab2SplitDistanceStr, out double tab2SplitDistance) ? tab2SplitDistance : Constants.TAB2_SPLIT_DISTANCE;
            vm.Tab2SplitDistance = Configs.tab2SplitDistance;
            ImgFilePaths_Row.Height = new GridLength(Configs.tab2SplitDistance, GridUnitType.Pixel);
            // 图片水印目录数据
            List<ImgFilePath> imgFilePathList = ServiceFactory.GetImgFilePathService().SelectList(null);
            vm.ImgFilePaths = new System.Collections.ObjectModel.ObservableCollection<ImgFilePath>(imgFilePathList);
            vm.AllSelect = vm.ImgFilePaths.Where(a => !a.IsSelect).ToList().Count == 0;
            // 配置目录列表的GridView栏目宽度
            string pathsViewColumn1Str = iniData[Constants.INI_SECTION_PAGE][Constants.INI_KEY_PATHS_VIEW_COLUMN_1];
            string pathsViewColumn2Str = iniData[Constants.INI_SECTION_PAGE][Constants.INI_KEY_PATHS_VIEW_COLUMN_2];
            Configs.pathsViewColumn1 = NumberUtils.IsNumeric(pathsViewColumn1Str, out double pathsViewColumn1) ? pathsViewColumn1 : -1;
            Configs.pathsViewColumn2 = NumberUtils.IsNumeric(pathsViewColumn2Str, out double pathsViewColumn2) ? pathsViewColumn2 : -1;
            vm.PathsViewColumn1 = Configs.pathsViewColumn1;
            vm.PathsViewColumn2 = Configs.pathsViewColumn2;
            // 任务频率
            string scrollEndStr = iniData[Constants.INI_SECTION_TASK][Constants.INI_KEY_SCROLL_END];
            string taskIntervalStr = iniData[Constants.INI_SECTION_TASK][Constants.INI_KEY_INTERVAL];
            Configs.scrollEnd = StringUtils.IsBool(scrollEndStr, out bool scrollEnd) ? scrollEnd : false;
            Configs.taskInterval = NumberUtils.IsInt(taskIntervalStr, out int taskInterval) ? taskInterval : Constants.TASK_INTERVAL;
            vm.ScrollEnd = Configs.scrollEnd;
            vm.TaskInterval = Configs.taskInterval;
            vm.ImgWaterMarkTaskTimer.Interval = TimeSpan.FromMinutes(Configs.taskInterval);
            #endregion
            vm.WaterMarkBorder = WaterMarkBorder;
            vm.ImgFilePath_ListView = ImgFilePath_ListView;
            vm.TaskLog_RichTextBox = TaskLog_RichTextBox;
            Configs.inited = true;
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
                vm.Tab2SplitDistance = ImgFilePaths_Row.Height.Value;
                if (vm.ConfigIsChanged) {
                    MessageBoxResult result = MessageBox.Show("存在修改的配置，保存配置吗？", Constants.MSG_WARN, MessageBoxButton.YesNoCancel);
                    if (MessageBoxResult.Yes == result) {
                        // 保存配置退出
                        vm.SaveConfigs();
                    } else if (MessageBoxResult.Cancel == result) {
                        // 取消退出
                        e.Cancel = true;
                        return;
                    }
                }
                // 窗口设置保存
                SavePageConfig();
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
                vm.WaterMarkFontColor = System.Drawing.ColorTranslator.ToHtml(cd.Color);
                if (!Configs.waterMarkFontColor.Equals(vm.WaterMarkFontColor)) {
                    vm.ConfigIsChanged = true;
                }
            }
        }
        /// <summary>
        /// 渐变色窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontGradientColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            GradientColorWindow gradientColorWindow = new GradientColorWindow(vm.WaterMarkFontGradientColor);
            if (true == gradientColorWindow.ShowDialog()) {
                vm.WaterMarkFontGradientColor = gradientColorWindow.GradientColorResult;
                Configs_Changed(sender, e);
            }
        }

        private void Configs_Changed(object sender, RoutedEventArgs e) {
            bool isChange = false;
            if (!isChange && !Configs.waterMarkText.Equals(vm.WaterMarkText)) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkOpacity != vm.WaterMarkOpacity) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkRotate != vm.WaterMarkRotate) {
                isChange = true;
            }
            if (!isChange && !Configs.waterMarkFontFamily.Equals(vm.WaterMarkFontFamily)) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkFontSize != vm.WaterMarkFontSize) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkFontIsGradient != vm.WaterMarkFontIsGradient) {
                isChange = true;
            }
            if (!isChange && !Configs.waterMarkFontColor.Equals(vm.WaterMarkFontColor)) {
                isChange = true;
            }
            if (!isChange && !Configs.waterMarkFontGradientColor.Equals(vm.WaterMarkFontGradientColor)) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkFontBold != vm.WaterMarkFontBold) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkFontItalic != vm.WaterMarkFontItalic) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkFontUnderline != vm.WaterMarkFontUnderline) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkFontStrikeout != vm.WaterMarkFontStrikeout) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkHorizontalDis != vm.WaterMarkHorizontalDis) {
                isChange = true;
            }
            if (!isChange && Configs.waterMarkVerticalDis != vm.WaterMarkVerticalDis) {
                isChange = true;
            }
            if (vm.WaterMarkFontIsGradient) {
                vm.SetOperateMsg(Colors.OrangeRed, "注意：渐变色不支持PDF！");
            }
            vm.ConfigIsChanged = isChange;
        }

        private void SavePageConfig() {
            IniParser.Model.IniData iniData = new IniParser.Model.IniData();
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WINDOW, Constants.INI_KEY_LEFT, ref Configs.mainLeft, vm.MainLeft);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WINDOW, Constants.INI_KEY_TOP, ref Configs.mainTop, vm.MainTop);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WINDOW, Constants.INI_KEY_HEIGHT, ref Configs.mainHeight, vm.MainHeight);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_WINDOW, Constants.INI_KEY_WIDTH, ref Configs.mainWidth, vm.MainWidth);

            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_PAGE, Constants.INI_KEY_LAST_OPEN_TAB, ref Configs.lastOpenTab, vm.LastOpenTab);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_PAGE, Constants.INI_KEY_PATHS_VIEW_COLUMN_1, ref Configs.pathsViewColumn1, vm.PathsViewColumn1);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_PAGE, Constants.INI_KEY_PATHS_VIEW_COLUMN_2, ref Configs.pathsViewColumn2, vm.PathsViewColumn2);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_PAGE, Constants.INI_KEY_TAB2_SPLIT_DISTANCE, ref Configs.tab2SplitDistance, vm.Tab2SplitDistance);

            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_TASK, Constants.INI_KEY_SCROLL_END, ref Configs.scrollEnd, vm.ScrollEnd);
            IniParserUtils.ConfigIniData(iniData, Constants.INI_SECTION_TASK, Constants.INI_KEY_INTERVAL, ref Configs.taskInterval, vm.TaskInterval);
            IniParserUtils.SaveIniData(Constants.SET_FILE, iniData);
        }

        private void ImgFilePath_Selected(object sender, RoutedEventArgs e) {
            vm.ImgFilePathSelectedChanged();
        }

        private void AutoGcTimer_Tick(object sender, EventArgs e) {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                //  配置工作使用空间
                DllUtils.SetProcessWorkingSetSize(Configs.Handler, -1, -1);
            }
        }

        private void TaskIntervalSlider_ValueChanged(object sender, RoutedEventArgs e) {
            vm.ImgWaterMarkTaskTimer.Interval = TimeSpan.FromMinutes(vm.TaskInterval);
            if (Configs.inited) {
                vm.AddWaterMarkLog($"当前频率调整为:{vm.TaskInterval}");
            }
        }

        private void ScrollEnd_Checked(object sender, RoutedEventArgs e) {
            TaskLog_RichTextBox.ScrollToEnd();
        }

        private void WaterMarkImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (vm.WaterMarkBitmap != null) {
                ImageWindow imageWindow = new ImageWindow(vm.WaterMarkHeight, vm.WaterMarkWidth, vm.WaterMarkBitmap);
                imageWindow.Show();
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            foreach (ImgFilePath path in vm.ImgFilePaths) {
                path.IsSelect = true;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
            foreach (ImgFilePath path in vm.ImgFilePaths) {
                path.IsSelect = false;
            }
        }

        private void ImgFilePath_ListView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            ImgFilePath_ListView.SelectedItem = null;
        }

        private void ImgFilePath_ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            vm.UpdateImgFilePathCommand.Execute(null);
        }
    }
}