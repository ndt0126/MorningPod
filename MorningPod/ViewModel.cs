using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using MorningPod.Commands;
using MorningPod.Models;
using MorningPod.Ultilities;
using MorningPod.ViewModels;
using Ultilities;
using can = System.Windows.Input.CanExecuteRoutedEventArgs;
using doa = System.Windows.Input.ExecutedRoutedEventArgs;
using obj = System.Object;
#pragma warning disable 1587

namespace MorningPod.ViewModels
{
    public enum State { None, PullingDatas, PullingFiles }

    public class NotificationObject: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary> RaisePropertyChanged </summary>
        public void RPC(string propertyName) { if (PropertyChanged != null) { this.PropertyChanged(this,new PropertyChangedEventArgs(propertyName)); } }
    }

    public class ViewModel : NotificationObject
    {
        public EventHandler<GenericEventArgs> OnDownloadedNewFile;

        
        //private DateTime _lastUpdate; public DateTime LastUpdate { get { return _lastUpdate; } set { _lastUpdate = value; RPC("LastUpdate"); } }
        //private DateTime _lastWake; public DateTime LastWake { get { return _lastWake; } set { _lastWake = value; RPC("LastWake"); } }
        //private DateTime _nextUpdate; public DateTime NextUpdate { get { return _nextUpdate; } set { _nextUpdate = value; RPC("NextUpdate"); } }
        //private DateTime _nextWake; public DateTime NextWake { get { return _nextWake; } set { _nextWake = value; RPC("NextWake"); } }

        private State _curState = State.None; public State CurState { get { return _curState; } set { _curState = value; RPC("CurState"); } }

        Pod PodTotal = new Pod { Title = "All Podcasts" };
        Pod PodDLing = new Pod { Title = "Downloading" };

        private Color[] _playingPodColors; public Color[] PlayingPodColors { get { return _playingPodColors; } set { _playingPodColors = value; RPC("PlayingPodColors"); } }
        private string _playingPodImgPath; public string PlayingPodImgPath { get { return _playingPodImgPath; } set { _playingPodImgPath = value; RPC("PlayingPodImgPath"); } }
        private string _playingPodTitle; public string PlayingPodTitle { get { return _playingPodTitle; } set { _playingPodTitle = value; RPC("PlayingPodTitle"); } }
        private string _playingEpiTitle; public string PlayingEpiTitle { get { return _playingEpiTitle; } set { _playingEpiTitle = value; RPC("PlayingEpiTitle"); } }

        private bool _isPlaying; public bool IsPlaying { get { return _isPlaying; } set { _isPlaying = value; RPC("IsPlaying"); } }
        
        private double _curPosi; public double CurPosi { get { return _curPosi; } set { _curPosi = value; RPC("CurPosi"); } }
        private double _curVolume = 1; public double CurVolume { get { return _curVolume; } set { _curVolume = value; RPC("CurVolume"); } }

        private string _playingPosi; public string PlayingPosi { get { return _playingPosi; } set { _playingPosi = value; RPC("PlayingPosi"); } }
        private string _playingDur; public string PlayingDur { get { return _playingDur; } set { _playingDur = value; RPC("PlayingDur"); } }


        private List<Pod> _pods;        public List<Pod> Pods { get { return _pods; }       set { _pods = value; RPC("Pods"); } }
        private Pod _curPod;            public Pod CurPod { get { return _curPod; }         set { _curPod = value; RPC("CurPod"); } }
        private List<Epi> _curEpis;     public List<Epi> CurEpis { get { return _curEpis; } set { _curEpis = value; RPC("CurEpis"); } }
        private Epi _curEpi;            public Epi CurEpi { get { return _curEpi; }         set { _curEpi = value; RPC("CurEpi"); } }

        private List<Epi> _curDLEpis;   public List<Epi> CurDLEpis { get { return _curDLEpis; } set { _curDLEpis = value; RPC("CurDLEpis"); } }

        private string _vmlog; public string VMLog { get { return _vmlog; } set { _vmlog = value; RPC("VMLog"); } }
        private string _lastVMLog; public string LastVMLog { get { return _lastVMLog; } set { _lastVMLog = value; RPC("LastVMLog"); } }
        public void Log(params object[] objs) { LastVMLog = string.Join(" " , objs.Select(w => w.ToString())); LastVMLog = ">" + LastVMLog; LastVMLog = LastVMLog.Replace("\n" , "\n   "); VMLog = LastVMLog + "\n" + VMLog; mt.Log(LastVMLog); }

        


        /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public ViewModel()
        {
            //LastUpdate = App.Config.LastUpdate;
            //LastWake = App.Config.LastWake;
            //NextUpdate = App.Config.UpdateScheMin.ConvertMinsToNextDateTime();
            //NextWake = App.Config.WakeScheMin.ConvertMinsToNextDateTime();

            PropertyChanged += delegate(object s , PropertyChangedEventArgs a)
            {
                if (a.PropertyName == "CurPod")
                {
                    CurEpis = CurPod == PodDLing ? CurDLEpis 
                                                 : CurPod?.Epis;

                    CurEpi = CurEpis != null && CurEpis.Count > 0 ? CurEpis?.First() : null;
                }
                //else if (a.PropertyName == "CurDLEpis") { StartDownloadQueue(); }
            };

            mt.DoDelay(UpdateFeedDatas);

            worker = mt.StartAWorker(() => MediaTicker() , ()=> { });
            worker.ProgressChanged += MediaTickerProgressChanged;

            MPCommands.Init(this);
        }






        /// -------------------------------------------------- UPDATE ----------------------------------------------
        public void UpdateFeedDatas()
        {
            GetLocalPods();

            List<Pod> newPods = new List<Pod>();

            if (!File.Exists(vals.PodUrlListFilePath)) { File.Create(vals.PodUrlListFilePath); return; }
            string[] podUrlList = File.ReadAllLines(vals.PodUrlListFilePath);

            bool skipFromHere = false;
            foreach (string podUrl in podUrlList)
            {
                if (podUrl == "--") skipFromHere = true;
                else if (podUrl[0] == "-"[0] && podUrl[1] == "-"[0]) continue;
                if (skipFromHere) continue;
                if (Pods.ToList().Find(p => p?.URL == podUrl) != null) continue;
                newPods.Add(new Pod { URL = podUrl, Epis = new List<Epi>() });
            }

            if (newPods.Count == 0) return;
            PullFeedDatas(newPods, null, false, false, GetLocalPods);
        }


        public void PullFeedDatas(IList<Pod> pods, Pod specPod, bool force, bool episOnly, Action action)
        {
            BackgroundWorker udWorker = null;

            udWorker = mt.StartAWorker(() =>
            {
                XmlDocument doc;

                foreach (Pod pod in pods)
                {
                    if (specPod != null && specPod != PodTotal && specPod != PodDLing && pod != specPod) { /*Log("notwanted: " , pod.Title);*/ continue; }

                    if (!force && episOnly && (vals.now - pod.LastUpdate).TotalHours < 1) { Log("skippod: " , pod.LastUpdate); continue; }

                    if (pod != PodTotal && pod != PodDLing) Log("Pulling : " , pod.Title ?? pod.URL ?? "???");

                    if (pod.URL == null) continue;

                    bool didPull = false , didDeNode = false , didGetImg = false, didSerial = false;
                    
                    try 
                    {
                        CurState = State.PullingDatas;
                        doc = new XmlDocument();
                        doc.Load(pod.URL);
                        if (pod.SafeTitle.NotBlank()) doc.Save(Path.Combine(vals.ObjDir , pod.SafeTitle + ".lastest.xml"));
                        didPull = true;
                        pod.Epis = new List<Epi>();

                        XmlNode chan = doc.SelectSingleNode("//channel");
                        foreach (XmlNode node in chan.ChildNodes)
                        {
                            /// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ title ---------------------
                            if (!episOnly && node.Name == "title")
                            {
                                pod.Title = node.InnerText;
                                udWorker.ReportProgress(0 , "Pulled : " + (pod.Title ?? pod.URL ?? "???"));
                                pod.SafeTitle = pod.Title.PathSafe();
                                if (!Directory.Exists(pod.MediaPath)) Directory.CreateDirectory(pod.MediaPath);
                                doc.Save(Path.Combine(vals.ObjDir , pod.SafeTitle + ".lastest.xml"));
                            }
                            /// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ description ---------------------
                            else if (!episOnly && node.Name == "description") pod.Description = node.InnerText;
                            /// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ itunes:image ---------------------
                            else if (!episOnly && node.Name == "itunes:image") { foreach (XmlAttribute att in node.Attributes) { if (att.Name == "href") { pod.ImgUrl = att.Value; break; } } }
                            /// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ item ---------------------
                            else if (!episOnly && node.Name == "image") { foreach (XmlNode inode in node.ChildNodes) { if (inode.Name == "url") pod.ImgUrl = inode.InnerText; break; } }
                            /// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ item ---------------------
                            else if (pod.Epis.Count < vals.MaxEpsEach && node.Name == "item")
                            {
                                Epi newEpi = new Epi(pod); bool usable = true;
                                foreach (XmlNode enode in node.ChildNodes)
                                {
                                    /**/ if (enode.Name == "title") newEpi.Title = enode.InnerText;
                                    else if (enode.Name == "link")
                                    {
                                        newEpi.Url = enode.InnerText; usable = mt.IsNotDuplicated(newEpi.Url , pod.Epis);
                                    }
                                    else if (enode.Name == "description") newEpi.Description = enode.InnerText;
                                    else if (enode.Name == "pubDate")
                                    {
                                        newEpi.PubDate = DateTime.ParseExact(enode.InnerText , "ddd, dd MMM yyyy HH:mm:ss zzz" , CultureInfo.InvariantCulture);
                                    }
                                    else if (enode.Name == "enclosure" /*&& newEpi.Url.IsBlank()*/)
                                    {
                                        newEpi.Url = enode.Attributes["url"].Value; usable = mt.IsNotDuplicated(newEpi.Url , pod.Epis);
                                    }
                                }
                                if (usable) pod.Epis.Add(newEpi);
                            }
                        }
                        didDeNode = true;


                        /// Download Image
                        if(!File.Exists(pod.ImgPath + "max") && !string.IsNullOrWhiteSpace(pod.ImgUrl))
                        {
                            WebClient webClient = new WebClient();
                            webClient.DownloadFile(pod.ImgUrl,pod.ImgPath + "max");
                        }
                        if (!File.Exists(pod.ImgPath) && !string.IsNullOrWhiteSpace(pod.ImgUrl))
                        {
                            ImageUtilities.CreateResizedImage(pod.ImgPath + "max" , 66 , pod.ImgPath);
                            pod.Colors = ImageUtilities.FindDominantColors(pod.ImgPath);
                        }
                        didGetImg = true;


                        /// Basic Init for its Epis
                        foreach (Epi epi in pod.Epis)
                        {
                            epi.PodImgPath = pod.ImgPath;
                            epi.PodColors = pod.Colors;
                        }


                        /// sort by Time
                        pod.Epis.Sort();


                        /// ................................................................................... Serialize to Disk ...................................................................................
                        pod.LastUpdate = vals.now;
                        mt.SerializeObject(pod , Path.Combine(vals.ObjDir , pod.Title.PathSafe()));
                        didSerial = true;

                        if (!didPull || !didDeNode || !didGetImg || !didSerial) udWorker.ReportProgress(0 , "done:   <" + pod.SafeTitle + ">  " + pod.URL + "\n" + "didPull:" + didPull + " didDeNode:" + didDeNode + " didGetImg:" + didGetImg + " didSerial:" + didSerial);
                        udWorker.ReportProgress(0 , pod);
                    } 
                    catch (Exception ex) { CurState = State.None; mt.Log( pod.SafeTitle+"\n"+ pod.URL +"\n"+  " didPull:" + didPull + " didDeNode:" + didDeNode + " didGetImg:" + didGetImg + " didSerial:" + didSerial + "\n" +ex); }
                }
            }
            , () =>
            {
                CurState = State.None; 
                action?.Invoke();
            });
            

            udWorker.ProgressChanged += delegate(object s , ProgressChangedEventArgs a)
            {
                if (a.UserState is string) Log((string) a.UserState);

                else if (!episOnly && a.UserState is Pod) Pods.Add((Pod) a.UserState);
            };
        }


        public void PullFeedFiles(bool force = true)
        {
            if (force) Log("###################################### UPDATE ALL ##################################################################################################################");

            PullFeedDatas(Pods, CurPod, force, true, () =>
            {
                Log(" --------------------------------------------- DONE PullFeedDatas");
                GetLocalPods();
                Log(" --------------------------------------------- DONE GetLocalPods");

                if (CurPod == null || CurPod == PodTotal || CurPod == PodDLing)
                {
                    foreach (Epi epi in PodTotal.Epis)
                    {
                        if (epi.IsDLed) { continue; }
                        if (epi.Url == null) { Log("skip: " , "Url" + (epi.Url != null ? "1" : "0") , "IsDLed" + (epi.IsDLed ? "1" : "0") , epi.LocalFileName); continue; }
                        AddToDownloadQueue(epi);
                    }
                }
                else
                {
                    foreach (Epi epi in CurPod.Epis)
                    {
                        if (epi.IsDLed) { continue; }
                        if (epi.Url == null) { Log("skip: " , "Url" + (epi.Url != null ? "1" : "0") , "IsDLed" + (epi.IsDLed ? "1" : "0") , epi.LocalFileName); continue; }
                        AddToDownloadQueue(epi);
                    }
                }

                Log(" --------------------------------------------- " + "Added to DLQueue: " + (CurDLEpis?.Count??0) +" ------------------------------------------------------------------------------------------------------------------");
            });
        }


        


        /// -------------------------------------------------- DESERIALIZE LOCAL ----------------------------------------------
        void GetLocalPods()
        {
            PodTotal = new Pod { Title = "All Podcasts" }; List<Epi> epiTotal = new List<Epi>();
            PodDLing = new Pod { Title = "Downloading" };

            ulong lasSelectPodId = CurPod?.Id??0;

            Pods = new List<Pod>(); Pods.Add(PodTotal); Pods.Add(PodDLing);

            string[] files = Directory.GetFiles(vals.ObjDir);
            foreach (string file in files)
            {
                if (file.Contains(".thumb") || file.Contains(".lastest")) continue;
                Pod pod = mt.DeSerializeObject<Pod>(file); 
                Pods.Add(pod);

                pod.Epis.Sort();

                foreach (Epi epi in pod.Epis)
                {
                    epi.PodTitle = pod.Title;
                    epi.PodSafeTitle = pod.SafeTitle;
                    epi.PodImgPath = pod.ImgPath;
                    epi.PodColors = pod.Colors;
                    epi.LocalFileName = epi.Title.PathSafe().GetMaxLength(33) + ".mp3";
                    epiTotal.Add(epi);
                }
            }

            epiTotal.Sort(); PodTotal.Epis = epiTotal;

            IEnumerable<Pod> find = lasSelectPodId != 0 ? Pods.Where(p => p.Id == lasSelectPodId) : null;

            CurPod = find != null && find.Count() > 0 ? find.First() : PodTotal;
        }


        


        /// -------------------------------------------------- PLAYBACK ----------------------------------------------
        double _duration; string _playingDurStr; TimeSpan _playingDurTS; public Epi PlayingEpi; public Pod PlayingPod;
        public MediaPlayer CurMP;
        public void PLAY(bool force, double startVol = 1)
        {
            CurMP?.Stop();
            CurMP = new MediaPlayer();
            
            if (PlayingEpi != null) PlayingEpi.IsPlaying = false;
            PlayingEpi = CurEpi;
            PlayingPod = CurEpi.FindPod(Pods);

            PlayingPodImgPath = PlayingEpi.PodImgPath;
            PlayingPodTitle = PlayingEpi.PodTitle;
            PlayingEpiTitle = PlayingEpi.Title;

            Uri uri = new Uri(PlayingEpi.IsDLed ? PlayingEpi.GetLocalPath() : PlayingEpi.Url);

            try
            {
                CurMP.Open(uri);
                CurMP.Volume = startVol;
                CurMP.Play();
                IsPlaying = true;
                PlayingPosi = ""; PlayingDur = ""; _playingDurStr = ""; _playingDurTS = default(TimeSpan); PlayingPodColors = null;
                PlayingEpi.IsPlaying = true;
                Log("play: ", PlayingEpi.Title);
                CurMP.MediaOpened += delegate
                {
                    try { PlayingEpi.Duration = _duration = CurMP.NaturalDuration.TimeSpan.TotalSeconds; } catch (Exception ex) { mt.Log(ex); }
                    if (PlayingEpi?.CurPosi != 0) Skip_To((PlayingEpi?.CurPosi ?? 0)*0.01);
                    MediaTickerProgressChanged(null , null);
                    CurMP.Volume = CurVolume;
                };
                CurMP.MediaEnded += delegate
                {
                    CurMP?.Stop();
                    PlayingEpi.CurPosi = 0; PlayingEpi.PC++;
                    PlayingEpi.WriteThingsDown(Pods);
                    PLAY_NEXT(null,null);
                };
            }
            catch(Exception ex) { Log("load failed !"); mt.Log(ex); if (force) PLAY_NEXT(null, null, true); }
        }

        DateTime _lastToggle;
        public void TOOGLE_PLAY_PAUSE(object s ,doa e)
        {
            if ((vals.now - _lastToggle).TotalMilliseconds < 200) return; _lastToggle = vals.now;

            if (CurMP == null) { PLAY(false); return; }
            if (IsPlaying) CurMP.Pause();
            else CurMP.Play();
            IsPlaying = !IsPlaying;
        }

        public void PLAY_NEXT(object s ,doa e) { PLAY_NEXT(s , e, false); }
        public void PLAY_NEXT(object s ,doa e, bool force)
        {
            int idx = CurEpis.IndexOf(PlayingEpi); if (idx >= CurEpis.Count - 1) return;
            Epi nextEpi = CurEpis[idx + 1];
            CurEpi = nextEpi;
            if (PlayingEpi != null) PlayingEpi.IsPlaying = false; PlayingEpi = CurEpi;
            if (!CurEpi.IsDLed) { PLAY_NEXT(null, null); return; }
            PLAY(force);
        }

        public void PLAY_PREV(object s ,doa e) 
        {
            if (CurMP.Position.Seconds > 5) Skip_To(0);
            else
            {
                int idx = CurEpis.IndexOf(PlayingEpi); if (idx == 0) return;
                Epi prevEpi = CurEpis[idx - 1];
                CurEpi = prevEpi;
                if (PlayingEpi != null) PlayingEpi.IsPlaying = false; PlayingEpi = CurEpi;
                if (!CurEpi.IsDLed) { PLAY_PREV(null, null); return; }
                PLAY(false);
            }
        }


        public void SKIP_BACK(object s ,doa e) { CurMP.Position = CurMP.Position.Add(TimeSpan.FromSeconds(-5)); }

        public void SKIP_FORWARD(object s ,doa e) { CurMP.Position = CurMP.Position.Add(TimeSpan.FromSeconds(5)); }
        
        public void Skip_To(double d) { if(CurMP == null) return; CurMP.Position = new TimeSpan(0 , 0 , (int) (d*_duration)); }

        public void Change_Vol(double d) { CurVolume = d; if (CurMP == null) return; CurMP.Volume = CurVolume; }

        
        public void PlayAll(double startVol)
        {
            try
            {
                CurPod = PodTotal;
                CurEpi = PodTotal.Epis.First();
                if (PlayingEpi != null) PlayingEpi.IsPlaying = false;
                PLAY(true, startVol);
            }
            catch (Exception ex) { mt.Log(ex); PLAY(true, startVol); }
        }


        /// -------------------------------------------------- TICKER ----------------------------------------------
        BackgroundWorker worker; int tickWritePlayingEpi = 0;
        void MediaTicker() { while (true) { Thread.Sleep(1000); worker.ReportProgress(0); } }

        void MediaTickerProgressChanged(object s , ProgressChangedEventArgs a)
        {
            if (CurMP == null) return;

            PlayingEpi.CurPosi = CurPosi = (CurMP.Position.TotalSeconds / _duration) * 100;
            CurVolume = CurMP.Volume;
            PlayingEpi.Duration = _duration;

            PlayingPosi = CurMP.Position.ToString(@"h\:m\:ss").Replace("0:" , "");

            if (_playingDurTS == default(TimeSpan)) _playingDurTS = TimeSpan.FromSeconds(_duration);

            if (string.IsNullOrWhiteSpace(_playingDurStr)) _playingDurStr = _playingDurTS.ToString(@"h\:m\:ss").Replace("0:" , "");

            PlayingDur = (_playingDurTS - CurMP.Position).ToString(@"h\:m\:ss").Replace("0:" , "") + " / " + _playingDurStr;

            if (PlayingPodColors == null)
            {
                if (PlayingPod.Colors == null)
                {
                    PlayingPod.Colors = ImageUtilities.FindDominantColors(PlayingPod.ImgPath);
                }
                PlayingPodColors = PlayingPod.Colors;
            }

            if (tickWritePlayingEpi < 10) { tickWritePlayingEpi ++; return; }
            PlayingEpi.WriteThingsDown(Pods); tickWritePlayingEpi = 0;
        }


        


        /// -------------------------------------------------- DOWNLOAD ----------------------------------------------
        public void DOWNLOAD_EPI(object ss ,doa e) { AddToDownloadQueue(CurEpi); }

        void AddToDownloadQueue(Epi epi)
        {
            if (epi.IsDLed || epi.IsDLing) return;
            if (CurDLEpis == null) CurDLEpis = new List<Epi> { epi };
            else if (!CurDLEpis.Contains(epi)) { CurDLEpis.Add(epi); }
            else if (_dlWebClient != null && _dlWebClient.IsBusy) return;
            PodDLing.Title = "Downloading : " + CurDLEpis.Count;
            epi.InQueue = true;
            StartDownloadQueue();
        }

        void StartDownloadQueue()
        {
            if(_dlWebClient != null && _dlWebClient.IsBusy) { /*Log("_dlWebClient busy");*/ return; }
            if (CurDLEpis == null || CurDLEpis.Count == 0) return; 
            Log("--------------------------------------------- Create New DL ------------------------------------------------------------------------------------------------------------------");
            CreateNewDownLoad();
        }
        
        WebClient _dlWebClient;
        void CreateNewDownLoad()
        {
            Epi epi = null;

            foreach (Epi curDlEpi in CurDLEpis) { if (curDlEpi.IsDLing) continue; epi = curDlEpi; epi.IsDLing = true; break; }

            if (epi == null) { Log("cantFindEpiToDL:" , epi.LocalFileName); return; }

            _dlWebClient = new WebClient();
            Log("startDL:" , epi.LocalFileName);
            _dlWebClient.DownloadProgressChanged += delegate(object s , DownloadProgressChangedEventArgs a)
            {
                ((Epi) a.UserState).DLPerc = (uint) a.ProgressPercentage;
            };
            _dlWebClient.DownloadFileCompleted += delegate(object s , AsyncCompletedEventArgs a)
            {
                CurState = State.None; 
                DoneDownloadAnEpi((Epi) a.UserState);
            };
            
            try 
            {
                string path = vals.MediaDir; if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                path = Path.Combine(path , epi.PodSafeTitle); if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                path = Path.Combine(path , epi.LocalFileName);

                CurState = State.PullingFiles; 
                _dlWebClient.DownloadFileAsync(new Uri(epi.Url) , epi.GetLocalPath() + "_temp", epi);
            } 
            catch (Exception ex) { mt.Log(ex); _dlWebClient.Dispose(); CurState = State.None; }
        }

        void DoneDownloadAnEpi(Epi doneEpi)
        {
            doneEpi.DLPerc = 0; doneEpi.IsDLed = true; doneEpi.IsDLing = false;
            
            try
            {
                string path = vals.MediaDir; if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                path = Path.Combine(path , doneEpi.PodSafeTitle); if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                path = Path.Combine(path , doneEpi.LocalFileName);
                File.Move(path + "_temp" , path);
            }
            catch (Exception ex) { mt.Log(ex); }

            if (CurDLEpis == null) return;

            CurDLEpis.Remove(doneEpi); doneEpi.InQueue = false;


            try 
            {
                doneEpi.Duration = mt.FileDuration(doneEpi.GetLocalPath());
                doneEpi.WriteThingsDown(Pods);
                Log("DoneDL:" , doneEpi.LocalFileName);
            } 
            catch (Exception ex) { mt.Log(ex); doneEpi.HasError = true; }
            


            if (CurDLEpis.Count > 0) { CreateNewDownLoad(); PodDLing.Title = "Downloading : " + CurDLEpis.Count; }
            else { CurDLEpis = null; PodDLing.Title = "Downloading"; }
            
            
            OnDownloadedNewFile?.Invoke(this , new GenericEventArgs());


            if (CurDLEpis == null) Log(" --------------------------------------------- DONE ALL DOWNLOADING. ------------------------------------------------------------------------------------------------------------------");


            return;
            MediaPlayer mp = new MediaPlayer();
            mp.Open(new Uri(doneEpi.GetLocalPath()));
            mp.MediaOpened += delegate
            {
                try { doneEpi.Duration = mp.NaturalDuration.TimeSpan.TotalSeconds; } catch (Exception ex) { mt.Log(ex); }
                mp.Close();
                doneEpi.WriteThingsDown(Pods);
            };
        }




        
    }
}