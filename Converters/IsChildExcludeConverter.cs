using System;

namespace AddWaterMark.Converters {
    internal class IsChildExcludeConverter : BaseValueConverter<IsChildExcludeConverter> {
        #region IValueConverter Members

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (null == value || (bool)value) {
                return false;
            } else {
                return true;
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return !(bool)value;
        }

        #endregion
    }
}
