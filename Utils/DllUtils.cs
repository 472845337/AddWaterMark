using System;
using System.Runtime.InteropServices;

namespace AddWaterMark.Utils {
    class DllUtils {
        public const string
            User32 = "user32.dll",
            Gdi32 = "gdi32.dll",
            GdiPlus = "gdiplus.dll",
            Kernel32 = "kernel32.dll",
            Shell32 = "shell32.dll",
            MsImg = "msimg32.dll",
            NTdll = "ntdll.dll",
            DwmApi = "dwmapi.dll",
            DbghHelp = "dbghelp.dll",
            PsApi = "psapi.dll",
            WinMm = "winmm.dll",
            Crypt = "crypt32.dll";
        /** 强制GC API函数**/
        [DllImport(Kernel32)]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        /// <summary>
        /// 窗口置最前，最小化也会置
        /// </summary>
        /// <param name="hWnd">窗口的句柄</param>
        /// <param name="fAltTab">此参数的 TRUE 表示正在使用 Alt/Ctl+Tab 键序列将窗口切换到 。 否则，此参数应为 FALSE</param>
        /// <returns></returns>
        [DllImport(User32, CharSet = CharSet.Auto)]
        public static extern bool SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
        /// <summary>
        /// 窗口操作，关闭请不要随意使用
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nCmdShow"></param>
        /// <returns></returns>
        [DllImport(User32, EntryPoint = "ShowWindow", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, ushort nCmdShow);
    }
}
