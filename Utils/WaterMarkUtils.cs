using AddWaterMark.Beans;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace AddWaterMark.Utils {
    class WaterMarkUtils {


        internal static Brush GetWaterMarkBrush(bool isGradient, string fontColorStr, string gradientColor, byte opacity) {
            Brush brush;
            // 不透明度按100转成byte 255的数值范围
            opacity = (byte)(opacity * 255 / 100);
            if (isGradient) {
                // 渐变色
                GradientStopCollection gradients = new GradientStopCollection();
                if (!string.IsNullOrEmpty(gradientColor)) {
                    string[] gradientColorsArray = gradientColor.Split(';');
                    foreach (string gradientColorStr in gradientColorsArray) {
                        string[] gradientColorArray = gradientColorStr.Split(':');
                        float point = Convert.ToSingle(gradientColorArray[0]);
                        string colorHtml = gradientColorArray[1];
                        Color pointColor = (Color)ColorConverter.ConvertFromString(colorHtml);
                        Color pointOpacityColor = Color.FromArgb(opacity, pointColor.R, pointColor.G, pointColor.B);
                        gradients.Add(new GradientStop(pointOpacityColor, point));
                    }
                }
                brush = new LinearGradientBrush(gradients, 0D);
            } else {
                // 纯色
                Color fontColor = (Color)ColorConverter.ConvertFromString(fontColorStr);
                Color waterMarkColor = Color.FromArgb(opacity, fontColor.R, fontColor.G, fontColor.B);
                brush = new SolidColorBrush(waterMarkColor);
            }
            return brush;
        }

        internal static FormattedText GetFormattedText(string waterMark, string fontFamilyStr, bool isItalic, bool isBold, double fontSize, bool isGradient, string fontColor, string fontGradientColor, byte opacity) {
            // 字体
            FontFamily fontFamily = new FontFamily(fontFamilyStr);
            FontWeight fontWeight = FontWeights.Normal;
            if (isBold) {
                fontWeight = FontWeights.Bold;
            }
            FontStyle fontStyle = FontStyles.Normal;
            if (isItalic) {
                fontStyle = FontStyles.Italic;
            }
            Brush brush = GetWaterMarkBrush(isGradient, fontColor, fontGradientColor, opacity);
            Typeface typeface = new Typeface(fontFamily, fontStyle, fontWeight, FontStretches.Normal);
            return new FormattedText(
                waterMark,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                brush);
        }

        internal static System.Drawing.Brush GetDrawingBrush(int opacity, bool isGradient, string fontColor, string fontGradientColor, int width, int height) {
            // 不透明度按100转成byte 255的数值范围
            opacity = opacity * 255 / 100;
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
                    colors[i] = System.Drawing.Color.FromArgb(opacity, System.Drawing.ColorTranslator.FromHtml(gradientColor.Color));
                    positions[i] = gradientColor.Point;
                }
                blend.Colors = colors;
                blend.Positions = positions;
                gradientBrush.InterpolationColors = blend;
                brush = gradientBrush;
            } else {
                // 设置颜色和透明度
                System.Drawing.Color waterMarkColor = System.Drawing.Color.FromArgb(opacity, System.Drawing.ColorTranslator.FromHtml(fontColor));
                brush = new System.Drawing.SolidBrush(waterMarkColor);
            }

            return brush;
        }

    }
}
