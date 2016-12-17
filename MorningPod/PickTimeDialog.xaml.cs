using System;
using System.Windows;
using System.Windows.Input;
using MorningPod.Ultilities;

namespace MorningPod.ViewModels
{
    public class PickTimeDialogVM : NotificationObject
    {
        public RoutedCommand AddHour { get; set; }
        public RoutedCommand AddMinute { get; set; }
        public RoutedCommand SubHour { get; set; }
        public RoutedCommand SubMinute { get; set; }

        private int _hours; public int Hours { get { return _hours; } set { _hours = value; RPC("Hours"); } }
        private int _mins; public int Mins { get { return _mins; } set { _mins = value; RPC("Mins"); } }

        public PickTimeDialogVM()
        {
            AddHour = mt.RegCmd<PickTimeDialogVM>("AddHour" , DoAddHour);
            AddMinute = mt.RegCmd<PickTimeDialogVM>("AddMinute" , DoAddMinute);
            SubHour = mt.RegCmd<PickTimeDialogVM>("SubHour" , DoSubHour);
            SubMinute = mt.RegCmd<PickTimeDialogVM>("SubMinute" , DoSubMinute);
        }


        public const int minStep = 1;

        void DoAddHour(object sender , ExecutedRoutedEventArgs e) { if (Hours < 23) Hours++; else Hours = 0; }
        void DoAddMinute(object sender , ExecutedRoutedEventArgs e) { if (Mins < 60-minStep) Mins += minStep; else Mins = 0; }
        void DoSubHour(object sender , ExecutedRoutedEventArgs e) { if (Hours > 0) Hours--; else Hours = 23; }
        void DoSubMinute(object sender , ExecutedRoutedEventArgs e) { if (Mins > 0) Mins -= minStep; else Mins = 60-minStep; }


        public void SetOrg(double orgTimeInMins) { TimeSpan TS = TimeSpan.FromMinutes(orgTimeInMins); Hours = TS.Hours; Mins = TS.Minutes; }

        public TimeSpan GetResult() { return new TimeSpan(0 , Hours , Mins , 0 , 0); }
    }
}

namespace MorningPod
{
    public class OnPicked_Time_EventArgs : EventArgs { public double TotMins; public OnPicked_Time_EventArgs(double totMins) { TotMins = totMins; } }

    public partial class PickTimeDialog
    {
        double OrgTimeInMins;

        public event EventHandler<OnPicked_Time_EventArgs> OnPicked_Time;

        public static PickTimeDialog Create(double orgTimeInMins, EventHandler<OnPicked_Time_EventArgs> handler) { PickTimeDialog RS = new PickTimeDialog(orgTimeInMins, handler); RS.ShowDialog(); return RS; }
        public PickTimeDialog(double orgTimeInMins, EventHandler<OnPicked_Time_EventArgs> handler)
        {
            InitializeComponent();

            Vm.SetOrg(orgTimeInMins);

            if (handler != null) OnPicked_Time += handler;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            ButtAddHour.Focus();
        }

        
        
        void Done(object sender , RoutedEventArgs routedEventArgs) { TimeSpan TS = Vm.GetResult(); OnPicked_Time?.Invoke(this, new OnPicked_Time_EventArgs(TS.TotalMinutes)); Close(); }

    }
}
