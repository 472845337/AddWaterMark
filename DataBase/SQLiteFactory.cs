using System.Collections.Generic;

namespace AddWaterMark.DataBase {
    internal static class SqLiteFactory {
        private static readonly Dictionary<string, SqLiteHelper> SqLiteHelperDic = new Dictionary<string, SqLiteHelper>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbName">数据库名，在当前运行位置生成dbName，示例："/database.db"</param>
        /// <param name="password">数据库密码</param>
        /// <returns></returns>
        public static SqLiteHelper GetSqLiteHelper(string dbName, string password) {
            if (SqLiteHelperDic.ContainsKey(dbName)) {
                // 存在该库，直接返回库工具类
                return SqLiteHelperDic[dbName];
            } else {
                // 没有该库，创建库并打开库
                var sqliteHeper = string.IsNullOrEmpty(password) ? new SqLiteHelper(dbName) : new SqLiteHelper(dbName, password);
                sqliteHeper.Open();
                SqLiteHelperDic.Add(dbName, sqliteHeper);
                return sqliteHeper;
            }

        }

        public static void CloseAllSqLite() {
            foreach (var pair in SqLiteHelperDic) {
                pair.Value.Close();
            }
            SqLiteHelperDic.Clear();
        }
    }
}
