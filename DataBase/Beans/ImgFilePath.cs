
using AddWaterMark.Beans;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.IO;

namespace AddWaterMark.DataBase.Beans {
    [Table("t_img_file_path")]
    public class ImgFilePath : TableData {
        // 路径
        [TableParam("file_path", "VARCHAR")]
        public string FilePath { get; set; }
        // 水印文本
        [TableParam("water_mark", "VARCHAR")]
        public string WaterMark { get; set; }
        // 包含子目录
        [TableParam("is_child", "BIT")]
        [OnChangedMethod(nameof(IsChildChange))]
        public bool? IsChild { get; set; }
        public string IsChildShow { get; set; }
        // 包含扩展名
        [TableParam("include_ext", "VARCHAR")]
        public string IncludeExt { get; set; }
        // 排除扩展名
        [TableParam("exclude_ext", "VARCHAR")]
        public string ExcludeExt { get; set; }
        // 排除子目录（目录名或相对路径，分隔符支持,;|，命中的子目录整棵跳过）
        [TableParam("exclude_child", "VARCHAR")]
        public string ExcludeChild { get; set; }
        // 是否选中
        public bool IsSelect { get; set; } = true;

        private void IsChildChange() {
            IsChildShow = GetIsChildShow(IsChild);
        }

        public static string GetIsChildShow(bool? isChild) {
            if (null == isChild || true == isChild) {
                return Lang.Find("IncludeChild");
            } else {
                return Lang.Find("ExcludeChild");
            }
        }

        public static List<string> GetExtList(string ext) {
            if (string.IsNullOrEmpty(ext)) {
                return null;
            } else {
                string[] extArray = ext.Split(new char[] { ',', ';', '\\', '/', '|' });
                List<string> result = new List<string>();
                for (int i = 0; i < extArray.Length; i++) {
                    string extSingle = extArray[i];
                    if (string.IsNullOrEmpty(extSingle)) {
                        continue;
                    }
                    result.Add(extSingle.StartsWith(".") ? extSingle : $".{extSingle}");
                }
                return result;
            }
        }

        /// <summary>
        /// 子目录是否命中排除名单：不含\的项按目录名任意层级匹配，含\的项按相对主目录的路径精确匹配。
        /// 水印任务遍历与添加目录的重复校验(GetRepeatPath)共用
        /// </summary>
        public static bool IsExcludedChild(string directory, string rootPath, List<string> excludeChildList) {
            if (null == excludeChildList || excludeChildList.Count == 0) {
                return false;
            }
            string dirName = Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string relative = directory.Substring(rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            foreach (string item in excludeChildList) {
                if (item.IndexOf('\\') > -1 || item.IndexOf('/') > -1) {
                    if (string.Equals(relative, item.Replace('/', '\\'), StringComparison.OrdinalIgnoreCase)) {
                        return true;
                    }
                } else if (string.Equals(dirName, item, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 拼接排除子目录名单为存储字符串，空项忽略
        /// </summary>
        public static string JoinExcludeChildList(List<string> list) {
            List<string> result = new List<string>();
            if (null != list) {
                for (int i = 0; i < list.Count; i++) {
                    string item = null == list[i] ? string.Empty : list[i].Trim();
                    if (!string.IsNullOrEmpty(item)) {
                        result.Add(item);
                    }
                }
            }
            return string.Join("|", result);
        }

        /// <summary>
        /// 解析排除子目录名单。分隔符不能用\和/，它们留给相对路径项使用
        /// </summary>
        public static List<string> GetExcludeChildList(string value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return null;
            } else {
                string[] valueArray = value.Split(new char[] { ',', ';', '|' });
                List<string> result = new List<string>();
                for (int i = 0; i < valueArray.Length; i++) {
                    string item = valueArray[i].Trim();
                    if (string.IsNullOrEmpty(item)) {
                        continue;
                    }
                    result.Add(item);
                }
                return result;
            }
        }
    }
}
