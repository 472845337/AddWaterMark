using AddWaterMark.Beans;
using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AddWaterMark.ViewModels {

    public class ImgFilePathViewModel : AbstractViewModel {
        [DoNotNotify]
        public List<ImgFilePath> CurImgFilePaths { get; set; }
        public long? Id { get; set; }
        public string FilePath { get; set; }
        public string WaterMark { get; set; }
        public bool? IsChild { get; set; }
        public string IncludeExt { get; set; }
        public string ExcludeExt { get; set; }
        // 排除子目录的存储形式（多项拼接），编辑时解析到ExcludeChildList，保存时由后台重新拼接
        public string ExcludeChild { get; set; }
        // 排除子目录编辑行
        public ObservableCollection<ExcludeChildItem> ExcludeChildList { get; set; } = new ObservableCollection<ExcludeChildItem>();

    }
}
