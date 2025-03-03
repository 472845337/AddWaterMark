using System;

namespace AddWaterMark.Converters {
    internal class IsChildIncludeConverter : BaseValueConverter<IsChildIncludeConverter> {
        #region IValueConverter Members

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (null == value || (bool)value) {
                return true;
            } else {
                return false;
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return (bool)value;
        }

        #endregion
    }
}
