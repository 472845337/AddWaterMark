using AddWaterMark.Beans;
using System;
using System.Collections.Generic;
using System.Windows;

namespace AddWaterMark.Utils {
    class WaterMarkUtils {


        internal static System.Windows.Media.Brush GetWaterMarkBrush(bool isGradient, string fontColorStr, string gradientColor, byte opacity) {
            System.Windows.Media.Brush brush;
            // 不透明度按100转成byte 255的数值范围
            opacity = (byte)(opacity * 255 / 100);
            if (isGradient) {
                // 渐变色
                System.Windows.Media.GradientStopCollection gradients = new System.Windows.Media.GradientStopCollection();
                if (!string.IsNullOrEmpty(gradientColor)) {
                    string[] gradientColorsArray = gradientColor.Split(';');
                    foreach (string gradientColorStr in gradientColorsArray) {
                        string[] gradientColorArray = gradientColorStr.Split(':');
                        float point = Convert.ToSingle(gradientColorArray[0]);
                        string colorHtml = gradientColorArray[1];
                        System.Windows.Media.Color pointColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorHtml);
                        System.Windows.Media.Color pointOpacityColor = System.Windows.Media.Color.FromArgb(opacity, pointColor.R, pointColor.G, pointColor.B);
                        gradients.Add(new System.Windows.Media.GradientStop(pointOpacityColor, point));
                    }
                }
                brush = new System.Windows.Media.LinearGradientBrush(gradients, 0D);
            } else {
                // 纯色
                System.Windows.Media.Color fontColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(fontColorStr);
                System.Windows.Media.Color waterMarkColor = System.Windows.Media.Color.FromArgb(opacity, fontColor.R, fontColor.G, fontColor.B);
                brush = new System.Windows.Media.SolidColorBrush(waterMarkColor);
            }
            return brush;
        }

        internal static System.Windows.Media.FormattedText GetFormattedText(string waterMark, string fontFamilyStr, bool isItalic, bool isBold, double fontSize, System.Windows.Media.Brush brush) {
            // 字体
            System.Windows.Media.FontFamily fontFamily = new System.Windows.Media.FontFamily(fontFamilyStr);
            FontWeight fontWeight = FontWeights.Normal;
            if (isBold) {
                fontWeight = FontWeights.Bold;
            }
            FontStyle fontStyle = FontStyles.Normal;
            if (isItalic) {
                fontStyle = FontStyles.Italic;
            }
            System.Windows.Media.Typeface typeface = new System.Windows.Media.Typeface(fontFamily, fontStyle, fontWeight, FontStretches.Normal);
            return new System.Windows.Media.FormattedText(
                waterMark,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                brush);
        }

        [Obsolete("drawing里的渐变色不支持不透明度")]
        internal static System.Drawing.Brush GetDrawingBrush(bool isGradient, string fontColor, string fontGradientColor, int width, int height) {
            // 画刷
            System.Drawing.Brush brush;
            if (isGradient) {
                List<GradientColor> gradientColors = GradientColorUtils.GetList(fontGradientColor);
                System.Drawing.Drawing2D.LinearGradientBrush gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new System.Drawing.Rectangle(0, 0, width, height),
                    System.Drawing.Color.Black,
                    System.Drawing.Color.White,
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
                // 判断是否存在point0和1的，因为drawing的LinearGradientBrush必须要有0和1的颜色，但是media中不需要，需要补
                if (gradientColors[0].Point != 0) {
                    gradientColors.Insert(0, new GradientColor(0, gradientColors[0].Color));
                }
                if (gradientColors[gradientColors.Count - 1].Point != 1) {
                    gradientColors.Add(new GradientColor(1, gradientColors[gradientColors.Count - 1].Color));
                }
                System.Drawing.Drawing2D.ColorBlend blend = new System.Drawing.Drawing2D.ColorBlend();

                System.Drawing.Color[] colors = new System.Drawing.Color[gradientColors.Count];
                float[] positions = new float[gradientColors.Count];
                for (int i = 0; i < gradientColors.Count; i++) {
                    GradientColor gradientColor = gradientColors[i];
                    colors[i] = System.Drawing.Color.FromArgb(255, System.Drawing.ColorTranslator.FromHtml(gradientColor.Color));
                    positions[i] = gradientColor.Point;
                }
                blend.Colors = colors;
                blend.Positions = positions;
                gradientBrush.InterpolationColors = blend;
                brush = gradientBrush;
            } else {
                // 设置颜色和透明度
                System.Drawing.Color waterMarkColor = System.Drawing.Color.FromArgb(255, System.Drawing.ColorTranslator.FromHtml(fontColor));
                brush = new System.Drawing.SolidBrush(waterMarkColor);
            }

            return brush;
        }

    }
}
