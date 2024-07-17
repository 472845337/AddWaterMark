using AddWaterMark.DataBase.Beans;
using PropertyChanged;
using System.Collections.Generic;

namespace AddWaterMark.ViewModels {

    [AddINotifyPropertyChangedInterface]
    public class ImgFilePathViewModel {
        [DoNotNotify]
        public List<ImgFilePath> CurImgFilePaths { get; set; }
        public string Id { get; set; }
        public string FilePath { get; set; }
        public string WaterMark { get; set; }
    }
}
