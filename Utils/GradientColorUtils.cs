using AddWaterMark.Beans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
