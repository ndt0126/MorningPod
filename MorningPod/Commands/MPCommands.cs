using System;
using System.IO;
using System.Windows.Input;
using MorningPod.Ultilities;
using MorningPod.ViewModels;
using can = System.Windows.Input.CanExecuteRoutedEventArgs;
using doa = System.Windows.Input.ExecutedRoutedEventArgs;

namespace MorningPod.Commands
{
    public static partial class MPCommands
    {
        public static RoutedCommand SkipBackward = new RoutedCommand("SkipBackward",typeof(ViewModel));
        public static RoutedCommand SkipForward = new RoutedCommand("SkipForward",typeof(ViewModel));
        public static RoutedCommand DownloadEpi = new RoutedCommand("DownloadEpi",typeof(ViewModel));
        public static RoutedCommand ShowEpi = new RoutedCommand("ShowEpi",typeof(ViewModel));
        public static RoutedCommand ToggleLogs = new RoutedCommand("ToggleLogs",typeof(MainWindow));
        public static RoutedCommand ShowHome = new RoutedCommand("ShowHome",typeof(App));
        public static RoutedCommand Exit = new RoutedCommand("Exit",typeof(App));

        //public static RoutedCommand PickUpdateTime = new RoutedCommand("PickUpdateTime",typeof(App));
        //public static RoutedCommand PickWakeTime = new RoutedCommand("PickWakeTime",typeof(App));
    }

    public static partial class MPCommands
    {
        public static void Init()
        {
            CommandManager.RegisterClassCommandBinding(typeof (object) , new CommandBinding(ShowHome , delegate { App.OpenHome(); } , vals.CanExe));
            CommandManager.RegisterClassCommandBinding(typeof (object) , new CommandBinding(Exit , delegate { App.ExitCompletly(); } , vals.CanExe));
        }

        static ViewModel VM;
        public static void Init(ViewModel vm)
        {
            VM = vm;
            CommandManager.RegisterClassCommandBinding(typeof(object), new CommandBinding(MediaCommands.TogglePlayPause, VM.TOOGLE_PLAY_PAUSE, CAN_TOOGLE_PLAY_PAUSE));
            CommandManager.RegisterClassCommandBinding(typeof(object), new CommandBinding(MediaCommands.NextTrack, VM.PLAY_NEXT, CAN_TOOGLE_PLAY_PAUSE));
            CommandManager.RegisterClassCommandBinding(typeof(object), new CommandBinding(MediaCommands.PreviousTrack, VM.PLAY_PREV, CAN_TOOGLE_PLAY_PAUSE));
            CommandManager.RegisterClassCommandBinding(typeof(object), new CommandBinding(SkipBackward, VM.SKIP_BACK, CAN_SKIP));
            CommandManager.RegisterClassCommandBinding(typeof(object), new CommandBinding(SkipForward, VM.SKIP_FORWARD, CAN_SKIP));

            CommandManager.RegisterClassCommandBinding(typeof (object) , new CommandBinding(ApplicationCommands.Delete , DELETE_FILE , CAN_DELETE_EPI));
            CommandManager.RegisterClassCommandBinding(typeof (object) , new CommandBinding(DownloadEpi, VM.DOWNLOAD_EPI , CAN_DOWNLOAD_EPI));
            CommandManager.RegisterClassCommandBinding(typeof (object) , new CommandBinding(ShowEpi, SHOW_EPI_FILE , CAN_DELETE_EPI));
        }

        static MainWindow MW;
        public static void Init(MainWindow mw)
        {
            MW = mw;
            CommandManager.RegisterClassCommandBinding(typeof (object) , new CommandBinding(ToggleLogs , MW.ToggleLogs , vals.CanExe));
            //CommandManager.RegisterClassCommandBinding(typeof (object) , new CommandBinding(PickUpdateTime , MW.PickUpdateTime , vals.CanExe));
            //CommandManager.RegisterClassCommandBinding(typeof (object) , new CommandBinding(PickWakeTime , MW.PickWakeTime , vals.CanExe));
        }
    }

    public static partial class MPCommands
    {
        static void CAN_DOWNLOAD_EPI(object s , can e) { e.CanExecute = VM.CurEpi != null && !VM.CurEpi.IsDLed; }
        static void CAN_DELETE_EPI(object s , can e) { e.CanExecute = VM.CurEpi != null && VM.CurEpi.IsDLed; }
        static void CAN_SKIP(object s ,can e) { e.CanExecute = VM.CurMP != null && VM.PlayingEpi != null; }
        static void CAN_TOOGLE_PLAY_PAUSE(object s ,can e) { e.CanExecute = VM.CurMP != null && VM.PlayingEpi != null; }

        static void DELETE_FILE(object s ,doa e)
        {
            if ((string) e.Parameter == "delEpi")
            {
                var filePath = VM.CurEpi.GetLocalPath();
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                        VM.CurEpi.IsDLed = false;
                    }
                    catch (Exception ex) { mt.Log(ex); }
                }
            }
        }

        static void SHOW_EPI_FILE(object s ,doa e)
        {
            var filePath = VM.CurEpi.GetLocalPath();
            if (!File.Exists(filePath)) { VM.Log("File not Exists : ", filePath); }
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
        }
    }
}
