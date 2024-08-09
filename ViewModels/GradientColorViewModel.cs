using AddWaterMark.Beans;
using AddWaterMark.Utils;
using PropertyChanged;
using System.Collections.ObjectModel;

namespace AddWaterMark.ViewModels {
    [AddINotifyPropertyChangedInterface]
    class GradientColorViewModel {

        public string WaterMarkFontGradientColor { get; set; }// 渐变色

        public ObservableCollection<GradientColor> GradientColorList { get; set; }

        public bool CanAdd { get; set; }
        public string AddTip { get; set; }
        public bool CanDelete { get; set; }
        public string DeleteTip { get; set; }

        public void ChangeGradient() {
            if (null != GradientColorList) {
                WaterMarkFontGradientColor = GradientColorUtils.GetString(GradientColorList);
                if (GradientColorList.Count < 3) {
                    CanDelete = false;
                    DeleteTip = Lang.Find("GradientCountTooltip");
                } else {
                    CanDelete = true;
                    DeleteTip = Lang.Find("GradientDeleteTooltip");
                }
                if (GradientColorList.Count >= 10) {
                    CanAdd = false;
                    AddTip = Lang.Find("GradientCountMaxTooltip");
                } else {
                    CanAdd = true;
                    AddTip = Lang.Find("GradientAddTooltip");
                }
            } else {
                WaterMarkFontGradientColor = string.Empty;
                CanDelete = false;
                DeleteTip = Lang.Find("GradientEmptyTooltip");
                CanAdd = true;
                AddTip = Lang.Find("GradientAddTooltip");
            }

        }
    }
}
