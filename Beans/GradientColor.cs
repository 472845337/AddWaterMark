using PropertyChanged;

namespace AddWaterMark.Beans {

    [AddINotifyPropertyChangedInterface]
    internal class GradientColor {
        public GradientColor(float point, string color) {
            Point = point;
            Color = color;
        }
        public float Point { get; set; }
        public string Color { get; set; }
    }
}
