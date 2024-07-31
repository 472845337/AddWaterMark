namespace AddWaterMark.Utils {
    class NumberUtils {
        // 是否数字
        public static bool IsNumeric(string s, out double result) {
            bool bReturn = false;
            result = 0;
            try {
                if (!string.IsNullOrEmpty(s)) {
                    result = double.Parse(s);
                    bReturn = true;
                }
            } catch {

            }
            return bReturn;
        }
        //判断是否为正整数
        public static bool IsInt(string s, out int result) {
            bool bReturn = false;
            result = 0;
            try {
                if (!string.IsNullOrEmpty(s)) {
                    result = int.Parse(s);
                    bReturn = true;
                }
            } catch {

            }
            return bReturn;
        }

        public static bool IsByte(string s, out byte result) {
            bool bReturn = false;
            result = 0;
            try {
                if (!string.IsNullOrEmpty(s)) {
                    result = byte.Parse(s);
                    bReturn = true;
                }
            } catch {

            }
            return bReturn;
        }
    }
}
