using AddWaterMark.Beans;
using AddWaterMark.Utils;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.ComponentModel;

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
            if(null != GradientColorList) {
                WaterMarkFontGradientColor = GradientColorUtils.GetString(GradientColorList);
                if (GradientColorList.Count < 3) {
                    CanDelete = false;
                    DeleteTip = "渐变色最少2种颜色";
                } else {
                    CanDelete = true;
                    DeleteTip = "删除该渐变色块";
                }
                if (GradientColorList.Count>=10) {
                    CanAdd = false;
                    AddTip = "最多10个渐变色";
                } else {
                    CanAdd = true;
                    AddTip = "添加渐变色块";
                }
            } else {
                WaterMarkFontGradientColor = string.Empty;
                CanDelete = false;
                DeleteTip = "渐变色为空";
                CanAdd = true;
                AddTip = "添加渐变色块";
            }
            
        }
    }
}
