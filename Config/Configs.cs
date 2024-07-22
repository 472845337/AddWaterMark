
namespace AddWaterMark.Config {
    class Configs {
        public static string AppStartPath = System.AppDomain.CurrentDomain.BaseDirectory;// 当前程序的启动目录
        public static bool inited = false;
        public static System.IntPtr Handler = System.IntPtr.Zero;// 当前窗口句柄

        public static double mainHeight;// 主窗口高
        public static double mainWidth;// 主窗口宽
        public static double mainTop;// 主窗口顶部位置
        public static double mainLeft;// 主窗口左边位置

        public static string waterMarkText;
        public static byte waterMarkOpacity;
        public static int waterMarkRotate;
        public static string waterMarkFontFamily;
        public static int waterMarkFontSize;
        public static bool waterMarkFontIsGradient;
        public static string waterMarkFontColor;
        public static string waterMarkFontGradientColor;
        public static bool waterMarkFontBold;
        public static bool waterMarkFontItalic;
        public static bool waterMarkFontUnderline;
        public static bool waterMarkFontStrikeout;
        public static int waterMarkHorizontalDis;
        public static int waterMarkVerticalDis;

        public static int lastOpenTab;
        public static double pathsViewColumn1;
        public static double pathsViewColumn2;
        public static double tab2SplitDistance;

        public static int taskInterval;
    }
}
