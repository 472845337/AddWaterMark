using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;

namespace AddWaterMark.Utils {
    internal class FontsUtils {

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

        internal static Font GetDrawingFont(string fontFamily, int fontSize, bool fontBold, bool fontItalic, bool fontUnderline, bool fontStrikeout) {
            FontStyle fontStyle = FontStyle.Regular;
            if (fontBold) {
                fontStyle |= FontStyle.Bold;
            }
            if (fontItalic) {
                fontStyle |= FontStyle.Italic;
            }
            if (fontUnderline) {
                fontStyle |= FontStyle.Underline;
            }
            if (fontStrikeout) {
                fontStyle |= FontStyle.Strikeout;
            }
            return new Font(fontFamily, fontSize, fontStyle);
        }

        public static BaseFont ConvertFont2BaseFont(Font _font) {
            var fontFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

            if (!iTextSharp.text.FontFactory.IsRegistered(_font.Name)) iTextSharp.text.FontFactory.RegisterDirectory(fontFolderPath);
            // BaseFont.IDENTITY_H 这里不加中文会有问题
            BaseFont baseFont = iTextSharp.text.FontFactory.GetFont(_font.Name, BaseFont.IDENTITY_H, _font.Size, ConvertFontStyle(_font.Style)).BaseFont;
            return baseFont;
        }

        private static int ConvertFontStyle(FontStyle _fontStyle) {
            int style = -1;

            if ((_fontStyle & FontStyle.Regular) != 0) {
                style |= iTextSharp.text.Font.NORMAL;
            }
            if ((_fontStyle & FontStyle.Bold) != 0) {
                style |= iTextSharp.text.Font.BOLD;
            }
            if ((_fontStyle & FontStyle.Italic) != 0) {
                style |= iTextSharp.text.Font.ITALIC;
            }
            if ((_fontStyle & FontStyle.Underline) != 0) {
                style |= iTextSharp.text.Font.UNDERLINE;
            }
            if ((_fontStyle & FontStyle.Strikeout) != 0) {
                style |= iTextSharp.text.Font.STRIKETHRU;
            }
            return style;
        }
    }
}
