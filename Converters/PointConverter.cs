using System;

namespace AddWaterMark.Converters {
    class PointConverter : BaseValueConverter<PointConverter> {
        #region IValueConverter Members

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            float point = (float)value;
            return point * 100;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            double point = (double)value;
            return point / 100.0F;
        }

        #endregion
    }
}
