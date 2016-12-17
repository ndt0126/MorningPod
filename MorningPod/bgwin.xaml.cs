using System;
using System.ComponentModel;
using System.Windows;
using MorningPod.ViewModels;

namespace MorningPod
{
    public partial class bgwin
    {
        ViewModel SViewModel;

        public bgwin(ViewModel sViewModel)
        {
            SViewModel = sViewModel;
            Visibility = Visibility.Hidden;
            ShowInTaskbar = false;
            InitializeComponent();

            //Hide();
            //new MainWindow(SViewModel).Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //if (App.IsExitingCompletly) { base.OnClosing(e); return; }
            Hide();
            e.Cancel = true;
            base.OnClosing(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            Hide();

            MainWindow newMainWindow = App.curHomeWin = new MainWindow(SViewModel);

            newMainWindow.Show();
        }
    }
}
