
using AddWaterMark.Beans;
using PropertyChanged;
using SqliteUtils;
using System;
using System.Collections.Generic;
using System.IO;

namespace AddWaterMark.Beans {
    [AddINotifyPropertyChangedInterface]
    [Table("t_img_file_path")]
    public class ImgFilePath : SqliteTableData {

        [TableParam(true, "id", "INTEGER", true)]
        public long? Id { get; set; }
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
        /// 归一化排除子目录项：主目录下的绝对路径转相对路径，主目录外的绝对路径只保留目录名，主目录本身返回空。
        /// 兼容手输、粘贴（含资源管理器“复制文件地址”的引号形式），选择目录与保存共用
        /// </summary>
        public static string NormalizeExcludeChild(string value, string rootPath) {
            if (string.IsNullOrWhiteSpace(value)) {
                return string.Empty;
            }
            string item = value.Trim().Trim('"').Replace('/', '\\').Trim().TrimEnd('\\');
            string root = string.IsNullOrWhiteSpace(rootPath) ? string.Empty : rootPath.Trim().Replace('/', '\\').TrimEnd('\\');
            if (item.Length == 0) {
                return string.Empty;
            }
            if (root.Length > 0) {
                if (item.Equals(root, StringComparison.OrdinalIgnoreCase)) {
                    return string.Empty;// 主目录本身不能作为排除项
                }
                if (item.StartsWith(root + "\\", StringComparison.OrdinalIgnoreCase)) {
                    return item.Substring(root.Length + 1);// 主目录之下：转相对路径
                }
            }
            if (Path.IsPathRooted(item)) {
                return Path.GetFileName(item);// 主目录之外的绝对路径：只保留目录名（任意层级同名生效）
            }
            return item;// 已是相对路径或纯目录名
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
