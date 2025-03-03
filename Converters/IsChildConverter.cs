using AddWaterMark.Beans;
using System;

namespace AddWaterMark.Converters {
    internal class IsChildConverter : BaseValueConverter<IsChildConverter> {
        #region IValueConverter Members

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (null == value || (bool)value) {
                return Lang.Find("IncludeChild");
            } else {
                return Lang.Find("ExcludeChild");
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }

        #endregion
    }
}
