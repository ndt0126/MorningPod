using System;
using System.Windows;
using System.Windows.Input;
using MorningPod.ViewModels;

namespace MorningPod
{
    public partial class MediaUC
    {
        private ViewModel _vm; public ViewModel VM { set { _vm = value; } get { if (_vm == null) { _vm = (ViewModel) DataContext; } return _vm; } }

        public MediaUC()
        {
            InitializeComponent();

            prog.PreviewMouseLeftButtonUp += delegate(object s , MouseButtonEventArgs a)
            {
                double x = a.GetPosition(prog).X;
                double perc = x/prog.ActualWidth;
                VM.Skip_To(perc);

                prog.Value = perc * 100;
            };

            vol.PreviewMouseLeftButtonUp += delegate(object s , MouseButtonEventArgs a)
            {
                double x = a.GetPosition(vol).X;
                double perc = x/vol.ActualWidth;

                VM.Change_Vol(perc);

                //vol.Value = perc;
            };

            vol.PreviewMouseWheel += delegate(object s , MouseWheelEventArgs a)
            {
                double newVol = VM.CurVolume + (0.2*(a.Delta > 0 ? 1 : -1));
                if (newVol < 0) newVol = 0; if (newVol > 1) newVol = 1;
                VM.Change_Vol(newVol);
            };
        }


        public MainWindow mainWin;
        private void ButtShowLog_OnClick(object sender , RoutedEventArgs e)
        {
            //todo why command didn't work after reopen window???
            mainWin.ToggleLogs(null , null);
        }
    }
}
