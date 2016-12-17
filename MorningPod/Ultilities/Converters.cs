using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MorningPod.ViewModels;
using scb = System.Windows.Media.SolidColorBrush;
using col = System.Windows.Media.Color;

namespace MorningPod.Ultilities
{
    
    public abstract class basedconvtr<T> : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            T rs = Do(value, parameter); return rs;
        }
        public abstract T Do(object val, object par);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }

    /// Bool > Toggle Visible / Hidden
    public class BoolToggleVisH : basedconvtr<Visibility>
    {
        public static BoolToggleVisH Get => new BoolToggleVisH();
        public override Visibility Do(object val, object par)
        {
            bool bol = (bool) val;
            return bol ? Visibility.Visible : Visibility.Hidden;
        }
    }
    /// Bool > Toggle between 2 ColorBrushes
    public class BoolToggleColorBrs : basedconvtr<scb>
    {
        public static BoolToggleColorBrs Get => new BoolToggleColorBrs();
        public override scb Do(object val, object par)
        {
            bool bol = (bool) val;
            scb[] pars = (scb[]) par ?? new[] { new scb(Colors.Crimson) , new scb(Colors.Crimson) };
            return bol ? pars[0] : pars[1];
        }
    }
    /// Bool > Toggle between 2 Green or Trans
    public class BoolToggleGreenTrans : basedconvtr<scb>
    {
        public static BoolToggleGreenTrans Get => new BoolToggleGreenTrans();
        public override scb Do(object val, object par)
        {
            bool bol = (bool) val;
            return bol ? vals.MediumSeaGreen : vals.Trans;
        }
    }
    /// Bool > Toggle between 2 Green or White
    public class BoolToggleGreenWhite : basedconvtr<scb>
    {
        public static BoolToggleGreenWhite Get => new BoolToggleGreenWhite();
        public override scb Do(object val, object par)
        {
            bool bol = (bool) val;
            return bol ? vals.MediumSeaGreen : vals.White;
        }
    }
    /// Bool > Toggle between 2 gray or black 
    public class BoolToggleGrayBlack : basedconvtr<scb>
    {
        public static BoolToggleGrayBlack Get => new BoolToggleGrayBlack();
        public override scb Do(object val, object par)
        {
            bool bol = (bool) val;
            return bol ? vals.Gr7 : vals.Black;
        }
    }
    /// Bool > Toggle between 2 LightGray or White
    public class BoolToggleLightGrayWhite : basedconvtr<scb>
    {
        public static BoolToggleLightGrayWhite Get => new BoolToggleLightGrayWhite();
        public override scb Do(object val, object par)
        {
            bool bol = (bool) val;
            scb RS =  bol ? vals.Gr9 : vals.White;

            if (!bol && par != null) RS.Opacity = double.Parse((string) par);

            return RS;
        }
    }
    /// Bool > Toggle between 2 Strings
    public class BoolTogglePlayPauseStr : basedconvtr<string>
    {
        public static BoolTogglePlayPauseStr Get => new BoolTogglePlayPauseStr();
        public override string Do(object val , object par)
        {
            bool bol = (bool) val;
            return bol ? "■" : "▶";
        }
    }
    /// Bool > Toggle Bold
    public class BoolToggleBold : basedconvtr<FontWeight>
    {
        public static BoolToggleBold Get => new BoolToggleBold();
        public override FontWeight Do(object val, object par) { return (bool) val ? FontWeights.Bold : FontWeights.Normal; }
    }
    /// Bool > Toggle SemiBold
    public class BoolToggleSemiBold : basedconvtr<FontWeight>
    {
        public static BoolToggleSemiBold Get => new BoolToggleSemiBold();
        public override FontWeight Do(object val, object par) { return (bool) val ? FontWeights.SemiBold : FontWeights.Normal; }
    }
    /// Int > Toggle Bold
    public class IntLessOneToggleBold : basedconvtr<FontWeight>
    {
        public static IntLessOneToggleBold Get => new IntLessOneToggleBold();
        public override FontWeight Do(object val, object par) { return (int) val < 1 ? FontWeights.ExtraBold : FontWeights.Normal; }
    }
    /// Int > Toggle scb
    public class IntLessOneToggleSCB : basedconvtr<scb>
    {
        public static IntLessOneToggleSCB Get => new IntLessOneToggleSCB();
        public override scb Do(object val, object par) { return (int) val < 1 ? vals.MediumSlateBlue : vals.Gr7; }
    }
    /// Int > Toggle scb
    public class DoubleToMarginTop : basedconvtr<Thickness>
    {
        public static DoubleToMarginTop Get => new DoubleToMarginTop();
        public override Thickness Do(object val , object par)
        {
            double d = (double) val;
            return new Thickness(0 , d*-0.5 , 0 , 0);
        }
    }
    /// Color[] > LinearGradient
    public class ColorsToGradientSolid : basedconvtr<LinearGradientBrush>
    {
        public static ColorsToGradientSolid Get => new ColorsToGradientSolid();
        public override LinearGradientBrush Do(object val , object par)
        {
            LinearGradientBrush RS = new LinearGradientBrush { StartPoint = new Point(0 , 0) , EndPoint = new Point(1 , 0) };


            col[] cols = (col[]) val;

            if (cols == null)
            {
                return RS;
            }
            
            double off = 0;
            double step = 1d/cols.Length;
            
            col lastCol = default(col);
            foreach (col col in cols)
            {
                if (lastCol != default(col)) RS.GradientStops.Add(new GradientStop(lastCol , off));
                RS.GradientStops.Add(new GradientStop(col , off));
                lastCol = col;
                off += step;
            }


            

            return RS;
        }
    }
    /// Color[] > LinearGradient
    public class ColorsToGradientSoft : basedconvtr<RadialGradientBrush>
    {
        public static ColorsToGradientSoft Get => new ColorsToGradientSoft();
        public override RadialGradientBrush Do(object val , object par)
        {
            RadialGradientBrush RS = new RadialGradientBrush { /*StartPoint = new Point(0 , 0) , EndPoint = new Point(1 , 0)*/ };


            col[] cols = (col[]) val;

            if (cols == null)
            {
                return RS;
            }
            
            double off = 0;
            double step = 1d/cols.Length;
            
            col lastCol = default(col);
            foreach (col col in cols)
            {
                if (lastCol != default(col)) RS.GradientStops.Add(new GradientStop(lastCol , off));
                RS.GradientStops.Add(new GradientStop(col , off));
                lastCol = col;
                off += step;
            }


            

            return RS;
        }
    }

    /// DateTime > Medium String
    public class DatetimeToStringMed : basedconvtr<string>
    {
        public static DatetimeToStringMed Get => new DatetimeToStringMed();
        public override string Do(object val , object par)
        {
            DateTime dt = (DateTime) val;
            if (dt.Year == 1) return "";
            return dt.ToString(vals.DateStringFormat);
        }
    }

    /// DateTime > Medium String
    public class StateToSCB : basedconvtr<scb>
    {
        public static StateToSCB Get => new StateToSCB();
        public override scb Do(object val , object par)
        {
            State state = (State) val;
            switch (state)
            {
                case State.PullingDatas: return vals.SandyBrown;
                case State.PullingFiles: return vals.MediumSeaGreen;
                default: return vals.Gr9;
            }
        }
    }

    //todo this is dumb
    /// Double - 5
    public class DoubleMinus5 : basedconvtr<double>
    {
        public static DoubleMinus5 Get => new DoubleMinus5();
        public override double Do(object val, object par)
        {
            if (val == null) return 0;

            double rs = (double)val - 5;
            return rs;
        }
    }

}