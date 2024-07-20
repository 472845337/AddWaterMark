namespace AddWaterMark.Utils {
    class NumberUtils {
        // 是否数字
        public static bool IsNumeric(string s, out double result) {
            bool bReturn = true;
            try {
                result = double.Parse(s);
            } catch {
                result = 0;
                bReturn = false;
            }
            return bReturn;
        }
        //判断是否为正整数
        public static bool IsInt(string s, out int result) {
            bool bReturn = true;
            try {
                result = int.Parse(s);
            } catch {
                result = 0;
                bReturn = false;
            }
            return bReturn;
        }

        public static bool IsByte(string s, out byte result) {
            bool bReturn = true;
            try {
                result = byte.Parse(s);
            } catch {
                result = 0;
                bReturn = false;
            }
            return bReturn;
        }
    }
}
