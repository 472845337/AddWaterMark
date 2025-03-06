
using AddWaterMark.Beans;
using PropertyChanged;
using System.Collections.Generic;

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
        public string IsChildShow {  get; set; }
        // 包含扩展名
        [TableParam("include_ext", "VARCHAR")]
        public string IncludeExt { get; set; }
        // 排除扩展名
        [TableParam("exclude_ext", "VARCHAR")]
        public string ExcludeExt { get; set; }
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
}
