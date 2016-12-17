using System.Windows.Input;
using MorningPod.Ultilities;
using MorningPod.ViewModels;

namespace MorningPod
{
    public partial class ScheduleUC
    {
        private ViewModel _vm; public ViewModel VM { set { _vm = value; } get { if (_vm == null) { _vm = (ViewModel) DataContext; } return _vm; } }
        
        public RoutedCommand PickUpdateTime { get; set; }
        public RoutedCommand PickWakeTime { get; set; }

        public ScheduleUC()
        {
            PickUpdateTime = mt.RegCmd<PickTimeDialogVM>("PickUpdateTime" , DoPickUpdateTime);
            PickWakeTime = mt.RegCmd<PickTimeDialogVM>("PickWakeTime" , DoPickWakeTime);

            InitializeComponent();
        }


        void DoPickUpdateTime(object sender , ExecutedRoutedEventArgs e)
        {
            PickTimeDialog.Create(App.Config.NextUpdateInMins, delegate(object s , OnPicked_Time_EventArgs a)
            {
                App.Config.SetUpdateTime(a.TotMins);
            });
        }
        void DoPickWakeTime(object sender , ExecutedRoutedEventArgs e)
        {
            PickTimeDialog.Create(App.Config.NextWakeInMins, delegate(object s , OnPicked_Time_EventArgs a)
            {
                App.Config.SetWakeTime(a.TotMins);
            });
        }
    }
}
