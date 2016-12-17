using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MorningPod.Ultilities;

// "yy-MM-dd : HH:mm"

namespace MorningPod.CustomUI
{
    public class TimeHeavyDateTimeButton : basedconvtr<StackPanel>
    {
        public static TimeHeavyDateTimeButton Get => new TimeHeavyDateTimeButton();
        public override StackPanel Do(object val , object par)
        {
            DateTime dt = (DateTime) val;

            StackPanel RS = new StackPanel { MinHeight = 22, MinWidth = 100, Orientation = Orientation.Horizontal , Margin = new Thickness(10 , 5 , 10 , 5) };

            if (dt.Year == 1) { return RS; }

            TextBlock txtD = new TextBlock { Text = dt.ToString("yy-MM-dd : "), FontSize = 13 };
            txtD.Align(null , 0);
            RS.Children.Add(txtD);

            TextBlock txtT = new TextBlock { Text = dt.ToString("HH:mm"), FontSize = 16, FontWeight = FontWeights.SemiBold };
            txtT.Align(null , 0);
            RS.Children.Add(txtT);

            return RS;
        }
    }
}
