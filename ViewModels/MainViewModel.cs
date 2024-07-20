using AddWaterMark.Beans;
using AddWaterMark.DataBase.Beans;
using AddWaterMark.Utils;
using PropertyChanged;
using System.Collections.ObjectModel;

namespace AddWaterMark.ViewModels {
    [AddINotifyPropertyChangedInterface]
    class MainViewModel {
        public MainViewModel() {
            SystemFonts = new ObservableCollection<string>(FontsUtils.GetSystemFonts());
        }
        public double MainHeight { get; set; }// 主窗口高度
        public double MainWidth { get; set; }// 主窗口宽度
        public double MainLeft { get; set; }// 主窗口左边位置
        public double MainTop { get; set; }// 主窗口顶部位置
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
        public bool CanTestWaterMark { get; set; } = true;// 是否可以测试水印
        public System.Windows.Media.Imaging.BitmapImage WaterMarkBitmap { get; set; }// 水印位图
        public int LastOpenTab { get; set; }// 上次打开Tab页
        public double Tab2SplitDistance { get; set; }// Tab2页 GridSplitter距离
        public string PathsViewColumn1 { get; set; }// 水印目录视图第一栏宽度
        public string PathsViewColumn2 { get; set; }// 水印目录视图第﻿二栏宽度
        public ObservableCollection<ImgFilePath> ImgFilePaths { get; set; }// 自动添加水印目录数据集合
        public bool ImgWaterMarkTimerCanRun { get; set; } = true;// 自动水印定时器是否可执行
        public string WaterMarkLog { get; set; } = "";
        public string TaskStatus { get;private set; } = "任务未运行";
        public System.Windows.Media.Brush TaskStatusColor { get;private set; } = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
        public System.Windows.Media.Brush OperateMsgColor { get; private set; }// 操作信息颜色
        public string OperateMsg { get; private set; }// 操作信息
        public void SetTaskStatus(System.Windows.Media.Color color, string msg) {
            TaskStatusColor = new System.Windows.Media.SolidColorBrush(color);
            TaskStatusColor.Freeze();
            TaskStatus = msg;
        }
        public void InitOperateMsg(System.Windows.Media.Color color, string msg) {
            OperateMsgColor = new System.Windows.Media.SolidColorBrush(color);
            OperateMsgColor.Freeze();
            OperateMsg = msg;
        }
    }
}
