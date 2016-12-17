using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using scb = System.Windows.Media.SolidColorBrush;
using col = System.Windows.Media.Color;

namespace MorningPod.Ultilities
{
    public class make
    {
        public static scb scb(string colorString, double opacity = 1)
        {
            if (colorString == null) return new scb() { Opacity = opacity };
            return new scb((col) ColorConverter.ConvertFromString(colorString)) {Opacity = opacity};
        }
        public static scb scb(col color, double opacity = 1) { return new scb(color) {Opacity = opacity}; }
    }
}
