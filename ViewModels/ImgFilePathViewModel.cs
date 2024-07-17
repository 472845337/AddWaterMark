using PropertyChanged;

namespace AddWaterMark.ViewModels {

    [AddINotifyPropertyChangedInterface]
    public class ImgFilePathViewModel {
        public string Id { get; set; }
        public string FilePath { get; set; }
        public string WaterMark { get; set; }
    }
}
