using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;

namespace AddWaterMark.Utils {
    class FontsUtils {

        internal static List<string> GetSystemFonts() {
            InstalledFontCollection installedFontCollection = new InstalledFontCollection();
            List<string> systemFonts = new List<string>();
            foreach (FontFamily fontFamily in installedFontCollection.Families) {
                if (fontFamily.IsStyleAvailable(FontStyle.Regular)) {
                    systemFonts.Add(fontFamily.Name);
                }
            }
            systemFonts.Reverse();
            return systemFonts;
        }
    }
}
