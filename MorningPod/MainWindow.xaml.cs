using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using MorningPod.Commands;
using PlaybackShortcut;
using MorningPod.Models;
using MorningPod.Ultilities;
using MorningPod.ViewModels;
using Ultilities;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Path = System.IO.Path;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace MorningPod
{
    public partial class MainWindow
    {
        //private IKeyboardMouseEvents m_GlobalHook;

        public bool IsActivated;

        public ViewModel ViewModel;

        public MainWindow(ViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;

            MPCommands.Init(this);

            InitializeComponent();

            MediaUc.DataContext = DataContext;
            MediaUc.mainWin = this;
            ScheUc.DataContext = App.Config;

            KeyUp += OnKeyUp;

            DGEpis.PreviewMouseDoubleClick += delegate { ViewModel.PLAY(false); };

            ViewModel.OnDownloadedNewFile += delegate { DGEpis.Items.Refresh(); };

            Activated += delegate { IsActivated = true; };
            Deactivated += delegate { IsActivated = false; };

            ButtUpdate.Click += delegate { ViewModel.PullFeedFiles(false); };
            ButtUpdateAll.Click += delegate { App.UpdateNow(); };
            ButtPlayAll.Click += delegate { App.PlayAllNow(); };
        }


        void LBPods_OnSelectionChanged(object sender , SelectionChangedEventArgs e) { }

        void DGEpis_OnSelectionChanged(object sender , SelectionChangedEventArgs e) { }

        void OnKeyUp(object s , KeyEventArgs a)
        {
            if (a.Key == Key.Up || a.Key == Key.Down)
            {
                if (((FrameworkElement) Keyboard.FocusedElement).DataContext.GetType() != typeof (Epi)) DGEpis.Focus();
            }
            else if (a.Key == Key.Enter || a.Key == Key.Return)
            {
                ViewModel.PLAY(false);
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) && a.Key == Key.U)
            {
                ViewModel.PullFeedFiles(true);
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && a.Key == Key.U)
            {
                ViewModel.PullFeedFiles();
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && a.Key == Key.A)
            {
                ViewModel.PlayAll(1);
            }
        }


        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            
            //m_GlobalHook = Hook.GlobalEvents();

            //m_GlobalHook.KeyUp += GlobalHookKeyUp;
        }

        //private void GlobalHookKeyUp(object s , System.Windows.Forms.KeyEventArgs a)
        //{
        //    switch (a.KeyCode)
        //    {
        //    case Keys.Space:
        //        if (IsActivated) ViewModel.TOOGLE_PLAY_PAUSE(null , null);
        //        break;
        //    case Keys.MediaPlayPause:
        //        ViewModel.TOOGLE_PLAY_PAUSE(null , null);
        //        break;
        //    case Keys.MediaNextTrack:
        //        ViewModel.PLAY_NEXT(null , null);
        //        break;
        //    case Keys.MediaPreviousTrack:
        //        ViewModel.PLAY_PREV(null , null);
        //        break;
        //    }
        //}

        bool ShowingLog = false;
        public void ToggleLogs(object sender , ExecutedRoutedEventArgs e)
        {
            ShowingLog = !ShowingLog;

            DGEpis.Visibility = ShowingLog ? Visibility.Hidden : Visibility.Visible;
            DBGr.Visibility = !ShowingLog ? Visibility.Hidden : Visibility.Visible;
           
            MediaUc.ButtShowLog.Content = DBGr.Visibility == Visibility.Visible ? "Close logs" : ViewModel.LastVMLog;

        }


        //private void asdfasdfasdfasdf(object sender , RoutedEventArgs e)
        //{
        //    Grid src = (Grid) e.Source;
        //    ColorX col = (ColorX) src.DataContext;
        //    System.Windows.Media.Color conCol = System.Windows.Media.Color.FromArgb(col.col.A, col.col.R, col.col.G, col.col.B);

        //    src.Children.Add(new Rectangle { Fill = new SolidColorBrush(conCol), Width = 44, Height = 44, HorizontalAlignment = HorizontalAlignment.Left});

        //    TextBlock tbl = new TextBlock { Text = string.Join(" , " , col.col.A , col.col.R , col.col.G , col.col.B) + "  :: [ " + col.grade + " ~ " + col.count + " ]" };
        //    src.Children.Add(tbl);

        //    if (col.col.A < 100) tbl.TextDecorations.Add(TextDecorations.Strikethrough);
        //}


        //public void PickUpdateTime(object sender , ExecutedRoutedEventArgs e)
        //{
        //    PickTimeDialog.Create(ViewModel.Config.UpdateScheMin, delegate(object s , OnPicked_Time_EventArgs a)
        //    {
        //        ViewModel.SetUpdateTime(a.TotMins);

        //        //ViewModel.Config.UpdateScheMin = a.TotMins;
        //        //DateTime now = vals.now;
        //        //ViewModel.NextUpdate = new DateTime(now.Year , now.Month , now.Day).AddMinutes(ViewModel.Config.UpdateScheMin);
        //    });
        //}

        //public void PickWakeTime(object sender , ExecutedRoutedEventArgs e)
        //{
        //    PickTimeDialog.Create(ViewModel.Config.WakeScheMin, delegate(object s , OnPicked_Time_EventArgs a)
        //    {
        //        ViewModel.SetWakeTime(a.TotMins);

        //        //ViewModel.Config.WakeScheMin = a.TotMins;
        //        //DateTime now = vals.now;
        //        //ViewModel.NextWake = new DateTime(now.Year , now.Month , now.Day).AddMinutes(ViewModel.Config.WakeScheMin);
        //    });
        //}

        protected override void OnClosing(CancelEventArgs e)
        {
            App.curHomeWin = null;
            base.OnClosing(e);
        }
    }
}