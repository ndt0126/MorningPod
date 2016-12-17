using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using MorningPod.Commands;
using MorningPod.Models;
using MorningPod.Ultilities;
using MorningPod.ViewModels;
using PlaybackShortcut;
using bgw = System.ComponentModel.BackgroundWorker;

namespace MorningPod
{
    public partial class App
    {
        private static IKeyboardMouseEvents m_GlobalHook;

        public static MPConf Config { get; set; }

        private static ViewModel _svm; public static ViewModel SViewModel { set { _svm = value; } get { if (_svm == null) { _svm = new ViewModel(); } return _svm; } }

        static void InitApp()
        {
            vals.LogPath = Path.Combine(vals.BaseDir , "log.txt");

            AppDomain.CurrentDomain.UnhandledException += delegate(object s , UnhandledExceptionEventArgs a)
            {
                Exception ex = (Exception) a.ExceptionObject;
                mt.Log(ex);
            };

            Config = MPConf.TryRead();

            MPCommands.Init();

            InitTray();
            OpenHome();

            BACKGROUND.Start();
            
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.KeyUp += GlobalHookKeyUp;
        }

        
        private static void GlobalHookKeyUp(object s , System.Windows.Forms.KeyEventArgs a)
        {
            switch (a.KeyCode)
            {
            case Keys.Space:
                if (curHomeWin != null && curHomeWin.IsActivated) SViewModel.TOOGLE_PLAY_PAUSE(null , null);
                break;
            case Keys.Left:
                if (curHomeWin != null && SViewModel.IsPlaying) SViewModel.SKIP_BACK(null , null);
                break;
            case Keys.Right:
                if (curHomeWin != null && SViewModel.IsPlaying) SViewModel.SKIP_FORWARD(null , null);
                break;

            case Keys.MediaPlayPause:
                if (SViewModel.IsPlaying || curHomeWin != null) SViewModel.TOOGLE_PLAY_PAUSE(null , null);
                break;
            case Keys.MediaNextTrack:
                if (curHomeWin != null) SViewModel.PLAY_NEXT(null , null);
                break;
            case Keys.MediaPreviousTrack:
                if (curHomeWin != null) SViewModel.PLAY_PREV(null , null);
                break;
            }
        }




        static bgwin bgwin ;public static MainWindow curHomeWin;
        public static void OpenHome()
        {
            Current.Dispatcher.Invoke(new Action(delegate
            {
                if (bgwin == null) { bgwin = new bgwin(SViewModel); }
                bgwin.Show();
            }));
        }

        
        internal static void ExitCompletly() { bgwin.Close(); Current.Shutdown(); }



        public static TaskbarIcon TBIcon = null;
        public static void InitTray()
        {
            if (TBIcon != null) return;
            TBIcon = (TaskbarIcon)Current.FindResource("TaskbarIcon");/*need*/
            TBIcon.TrayMouseDoubleClick += delegate { OpenHome(); };
            TBIcon.TrayRightMouseUp += delegate
            {
                TBIcon.ContextMenu.IsOpen = true;
                //AppMenu.Open(false, true);
            };
        }




        
        /// -------------------------------------------------- SET SCHEDULE ----------------------------------------------
        public void SetUpdateTime(double totMins) { Config.SetUpdateTime(totMins); }

        public void SetWakeTime(double totMins) { Config.SetWakeTime(totMins); }




        /// -------------------------------------------------- MEET SCHEDULE ----------------------------------------------
        public static void UpdateNow()
        {
            string extra = Path.Combine(vals.BaseDir , "update.exe");
            if (File.Exists(extra)) System.Diagnostics.Process.Start(extra);
            Current.Dispatcher.Invoke(new Action(delegate { SViewModel.PullFeedFiles(true); }));
            Config.JustUpdated();
        }

        public static void PlayAllNow()
        {
            OpenHome();
            mt.DoDelay(() =>
            {
                string extra = Path.Combine(vals.BaseDir , "wake.exe");
                if (File.Exists(extra)) System.Diagnostics.Process.Start(extra);
                if (SViewModel.Pods == null) return;
                StartVolumeRamp();
                Current.Dispatcher.Invoke(new Action(delegate { App.SViewModel.PlayAll(0); }));
                Config.JustWaked();
            } 
            , 1000);
        }

        static void StartVolumeRamp()
        {
            mt.StartAWorker(() =>
            {
                double fxVol = 0;
                while (fxVol < 1)
                {
                    Current.Dispatcher.Invoke(new Action(delegate
                    {
                        if (SViewModel?.CurMP != null)
                        {
                            if (fxVol >= 0.99) fxVol = 1;
                            SViewModel.Change_Vol(fxVol);
                            fxVol += 0.005;
                        }
                    }));
                    Thread.Sleep(2000);
                }
            }
            , () =>
            {
                
            });

        }


        /// making sure only 1 instance
        Mutex mutex; EventWaitHandle eventWaitHandle;
        const string UniqueEventName = "{5974E2FF-51F4-4F68-A2B3-2C734CF37EEF}";/*need*/ const string UniqueMutexName = "{18BBDCD5-0F4E-4D68-A490-F1CD49893FDB}";/*need*/
        void App_OnStartup(object sender, StartupEventArgs e)
        {
            bool isOwned;
            mutex = new Mutex(true, UniqueMutexName, out isOwned);
            eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            // So, R# would not give a warning that this variable is not used.
            GC.KeepAlive(mutex);
            if (isOwned)
            {
                Thread thread = PausableThread;

                // It is important mark it as background otherwise it will prevent app from exiting.
                thread.IsBackground = true;
                thread.Start();

                InitApp();

                return;
            }
            // Notify other instance so it could bring itself to foreground.
            eventWaitHandle.Set();
            // Terminate this instance.
            Shutdown();
        }
        Thread PausableThread
        {
            get
            {
                return new Thread(
                    () =>
                    {
                        while (eventWaitHandle.WaitOne())
                        {
                            Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(OpenHome));
                        }
                    });
            }
        }

    }

    public class BACKGROUND
    {
        public static BACKGROUND CacheSelf;

        public static void Start()
        {
            if (CacheSelf == null) CacheSelf = new BACKGROUND();
            CreateWorker();
        }


        const int Interval_ForeGround = 3000;

        private static bool _running = false;
        private static bgw BgWorker;
        static void CreateWorker()
        {
            BgWorker = new bgw();
            BgWorker.DoWork += worker_DoWork;
            BgWorker.RunWorkerCompleted += delegate { mt.Log("restart worker"); CreateWorker(); };
            _running = true; BgWorker.RunWorkerAsync();
        }


        static void worker_DoWork(object s, DoWorkEventArgs a)
        {
            while (_running)
            {
                Check();

                Thread.Sleep(Interval_ForeGround);
            }

            mt.Log("THIS SHOULD NOT SHOW AAAA");
        }


        private static void Check()
        {

            if (App.Config.NextUpdate != App.Config.LastUpdate)
            {
                if (App.Config.NextUpdate <= vals.now)
                {
                    App.UpdateNow();
                }
            }


            if (App.Config.NextWake != App.Config.LastWake)
            {
                if (App.Config.NextWake <= vals.now)
                {
                    App.PlayAllNow();
                }
            }
            
        }
    }
}
