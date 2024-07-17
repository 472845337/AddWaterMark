using System;
using System.Runtime.InteropServices;

namespace AddWaterMark.Utils {
    class DllUtils {
        /** 强制GC API函数**/
        [DllImport("kernel32.dll")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
    }
}
