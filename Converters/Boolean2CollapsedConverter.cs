using System;

namespace AddWaterMark.Converters {
    public class Boolean2CollapsedConverter : BaseValueConverter<Boolean2CollapsedConverter> {
        #region IValueConverter Members

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (targetType != typeof(System.Windows.Visibility)) {
                throw new InvalidOperationException("The target must be a System.Windows.Visibility");
            }
            bool boolValue = (bool)value;
            return boolValue ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }

        #endregion
    }
}
