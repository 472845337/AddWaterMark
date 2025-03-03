using AddWaterMark.DataBase.Beans;
using PropertyChanged;
using System.Collections.Generic;

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

    }
}
