using System;

namespace AddWaterMark.Utils {
    class StringUtils {
        /// <summary>
        /// 判断字符串是否为空
        /// </summary>
        /// <param name="s"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool IsEmpty(string s, out string result) {
            bool isResult = false;
            if(string.IsNullOrEmpty(s) || string.IsNullOrEmpty(s.Trim())) {
                isResult = true;
                result = string.Empty;
            } else {
                result = s.Trim();
            }
            return isResult;
        }

        public static bool IsNotEmpty(string s, out string result) {
            bool isResult = false;
            if (!string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(s.Trim())) {
                isResult = true;
                result = s.Trim();
            } else {
                result = string.Empty;
            }
            return isResult;
        }

        public static bool IsBool(string s, out bool result) {
            bool isResult = false;
            result = false;
            try {
                if(!string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(s.Trim())) {
                    result = Convert.ToBoolean(s);
                    isResult = true;
                }
            } catch {

            }
            return isResult;
        }
    }
}
