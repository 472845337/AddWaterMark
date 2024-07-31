using AddWaterMark.Beans;
using System;
using System.Collections.Generic;

namespace AddWaterMark.Utils {
    class GradientColorUtils {

        public static List<GradientColor> GetList(string gradientColor) {
            List<GradientColor> list = new List<GradientColor>();
            if (!string.IsNullOrEmpty(gradientColor)) {
                string[] gradientColorsArray = gradientColor.Split(';');
                foreach (string gradientColorStr in gradientColorsArray) {
                    string[] gradientColorArray = gradientColorStr.Split(':');
                    float point = Convert.ToSingle(gradientColorArray[0]);
                    string colorHtml = gradientColorArray[1];
                    list.Add(new GradientColor(point, colorHtml));
                }
            }
            return list;
        }

        public static string GetString(ICollection<GradientColor> gradientColorList) {
            string gradientColorStr = string.Empty;
            foreach (GradientColor gradientColor in gradientColorList) {
                if (gradientColorStr.Length > 0) {
                    gradientColorStr += ";";
                }
                gradientColorStr += $"{gradientColor.Point}:{gradientColor.Color}";
            }
            return gradientColorStr;
        }

        public static void GetPdfColor(string gradientColor, int opacity, out iTextSharp.text.pdf.PdfDeviceNColor pdfDeviceNColor, out float[] tints) {
            List<GradientColor> gradientColors = GetList(gradientColor);
            tints = new float[gradientColors.Count];
            iTextSharp.text.pdf.PdfSpotColor[] pdfSpotColorArray = new iTextSharp.text.pdf.PdfSpotColor[gradientColors.Count];
            for (int i = 0; i < gradientColors.Count; i++) {
                GradientColor a = gradientColors[i];
                tints[i] = a.Point;
                System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(a.Color);
                iTextSharp.text.BaseColor baseColor = new iTextSharp.text.BaseColor(color.R, color.G, color.B, opacity);
                pdfSpotColorArray[i] = new iTextSharp.text.pdf.PdfSpotColor(i.ToString(),baseColor);
            }
            pdfDeviceNColor = new iTextSharp.text.pdf.PdfDeviceNColor(pdfSpotColorArray);
        }
    }
}
