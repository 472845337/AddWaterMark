using AddWaterMark.Beans;
using AddWaterMark.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace AddWaterMark.Converters {
    public class GradientColorConverter : BaseValueConverter<GradientColorConverter> {
        #region IValueConverter Members

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            ICollection<GradientColor> colors = GradientColorUtils.GetList((string)value);
            GradientStopCollection gradients = new GradientStopCollection();
            foreach (GradientColor gradientColor in colors) {
                gradients.Add(new GradientStop((Color)ColorConverter.ConvertFromString(gradientColor.Color), gradientColor.Point));
            }
            LinearGradientBrush brush = new LinearGradientBrush(gradients, 0D);
            return brush;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }

        #endregion
    }
}
