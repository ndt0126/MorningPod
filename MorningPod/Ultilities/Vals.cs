using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using scb = System.Windows.Media.SolidColorBrush;
using col = System.Windows.Media.Color;

namespace MorningPod.Ultilities
{

    public class vals
    {
        public static string LogPath;

        public static void Init()
        {
            var a = ObjDir;
            var b = PodUrlListFilePath;
        }

        private static string _baseDir;
        public static string BaseDir
        {
            set { _baseDir = value; }
            get
            {
                if (_baseDir == null)
                {
                    DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                    _baseDir = di.FullName;
                }
                return _baseDir;
            }
        }

        private static string _dataDir;
        public static string DataDir
        {
            get
            {
                if (_dataDir == null)
                {
                    _dataDir = Path.Combine(BaseDir , "PodDatas");
                }
                if (!Directory.Exists(_dataDir)) Directory.CreateDirectory(_dataDir);
                return _dataDir;
            }
        }

        private static string _objDir;
        public static string ObjDir
        {
            get
            {
                if (_objDir == null)
                {
                    _objDir = Path.Combine(DataDir, "OBJS");
                }
                if (!Directory.Exists(_objDir)) Directory.CreateDirectory(_objDir);
                return _objDir;
            }
        }

        private static string _mediaDir;
        public static string MediaDir
        {
            get
            {
                if (_mediaDir == null)
                {
                    _mediaDir = Path.Combine(DataDir, "Medias");
                }
                if (!Directory.Exists(_mediaDir)) Directory.CreateDirectory(_mediaDir);
                return _mediaDir;
            }
        }

        private static string _podUrlListFilePath;
        public static string PodUrlListFilePath
        {
            get
            {
                if (_podUrlListFilePath == null)
                {
                    _podUrlListFilePath = Path.Combine(BaseDir, "PodList.txt");
                    //if (!File.Exists(_podUrlListFilePath)) File.Create(_podUrlListFilePath);
                }
                return _podUrlListFilePath;
            }
        }

        public const int MaxEpsEach = 5;




        public static DateTime now
        {
            get
            {
                return DateTime.Now;
            }
        }




        private static BooleanToVisibilityConverter _boolToVis;
        public static BooleanToVisibilityConverter BoolToVis { get { if(_boolToVis == null) _boolToVis = new BooleanToVisibilityConverter(); return _boolToVis; } }
        


        public static readonly scb

            Trans = make.scb(null, 0),

            Gr1 = make.scb(("#1b1b1b")),
            Gr2 = make.scb(("#303030")),
            Gr3 = make.scb(("#474747")),
            Gr4 = make.scb(("#5e5e5e")),
            Gr5 = make.scb(("#777777")),
            Gr5_5 = make.scb(("#777777"), 0.5),
            Gr5_3 = make.scb(("#777777"), 0.3),
            Gr6 = make.scb(("#919191")),
            Gr7 = make.scb(("#ababab")),
            Gr8 = make.scb(("#c6c6c6")),
            Gr9 = make.scb(("#e2e2e2")),
            Gr95 = make.scb(("#F0F0F0")),
            Gr975 = make.scb(("#F8F8F8")),

            DarkSlateGray = make.scb(Colors.DarkSlateGray),
            Gray = make.scb(Colors.Gray),
            LightGray = make.scb(Colors.LightGray),
            CusGray = make.scb(("#CCCCCC")),
            //BGGray = make.scb(("#EEEEEE")),
            BGGray = make.scb(("#EEEEEE")),

            VOCPurple = make.scb(("#894189")),
            BrightPurple = make.scb(("#F3E2F3")),

            MediumSlateBlue = make.scb(Colors.MediumSlateBlue),
            SandyBrown = make.scb(Colors.SandyBrown),
            SandyBrownD = make.scb(("#F18E37")),
            MediumPurple = make.scb(Colors.MediumPurple),
            MediumSeaGreen = make.scb(Colors.MediumSeaGreen),
            Crimson = make.scb(Colors.Crimson),
            Black = make.scb(Colors.Black),
            Black_1 = make.scb(Colors.Black, 0.1),
            Black_75 = make.scb(Colors.Black, 0.75),
            White = make.scb(Colors.White),
            White_75 = make.scb(Colors.White, 0.75),
            HalfWhite = make.scb(Colors.White, 0.5),
            HalfBlack = make.scb(Colors.Black, 0.5),
            HalfCrimson = make.scb(Colors.Crimson, 0.5)
            ;


        public const string DateStringFormat = "yy-MM-dd : HH:mm";
        public static void CanExe(object sender , CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
    }
}
