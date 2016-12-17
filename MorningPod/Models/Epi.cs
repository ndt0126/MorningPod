using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Xml.Serialization;
using MorningPod.Ultilities;
using MorningPod.ViewModels;

namespace MorningPod.Models
{
    public class Epi : NotificationObject, IComparable
    {
        private bool _isPlaying; [XmlIgnore]public bool IsPlaying { get { return _isPlaying; } set { _isPlaying = value; RPC("IsPlaying"); } }
        private uint _dlPerc; [XmlIgnore]public uint DLPerc { get { return _dlPerc; } set { _dlPerc = value; RPC("DLPerc"); } }
        [XmlIgnore] public bool IsDLed { get { string locPath = GetLocalPath(); bool rs = File.Exists(locPath) && new FileInfo(locPath).Length > 0; return rs; } set { _isDLing = value; RPC("IsDLed"); } }
        
        private bool _isDLing; [XmlIgnore] public bool IsDLing { get { return _isDLing; } set { _isDLing = value; RPC("IsDLing"); RPC("IsDLingOrInQueue"); } }
        private bool _inQueue; [XmlIgnore]public bool InQueue { get { return _inQueue; } set { _inQueue = value; RPC("InQueue"); RPC("IsDLingOrInQueue"); } }
        [XmlIgnore] public bool IsDLingOrInQueue { get { return IsDLing || InQueue; } }

        private bool _hasError; public bool HasError { get { return _hasError; } set { _hasError = value; RPC("HasError"); } }

        public ulong PodId { get; set; }

        double _duration; public double Duration { get { return _duration; } set { _duration = value; RPC("SDuration"); } }
        public string SDuration { get { return TimeSpan.FromSeconds(Duration).ToString(@"h\:m\:ss").Replace("0:" , ""); } }

        public double CurPosi { get; set; }

        private int _pc; public int PC { get { return _pc; } set { _pc = value; RPC("PC"); } }

        public string Title { get; set; }
        public DateTime PubDate { get; set; }

        public string PodTitle { get; set; }
        public string PodSafeTitle { get; set; }

        [Browsable(false)]
        public string Description { get; set; }

        //[Browsable(false)]
        public string Url { get; set; }


        [XmlIgnore] public string PodImgPath { get; set; }

        [XmlIgnore] public Color[] PodColors { get; set; }


        public string GetLocalPath() { if (LocalFileName.IsBlank() || PodSafeTitle.IsBlank()) { return null; } return Path.Combine(Path.Combine(vals.MediaDir , PodSafeTitle) , LocalFileName); }

        public string LocalFileName { get; set; }
        


        public Epi() { }
        public Epi(Pod pod) { PodId = pod.Id; }



        public override string ToString() { return Title; }
        
        public static int SortByTitle(Epi x , Epi y) { return String.Compare(x.Title, y.Title, StringComparison.Ordinal); }
        public static int SortByTime(Epi x , Epi y) { return x.PubDate.CompareTo(y.PubDate) * -1; }

        public int CompareTo(object obj)
        {
            if (!(obj is Epi)) return 0;
            Epi otherEpi = (Epi) obj;
            return SortByTime(this , otherEpi);
        }


        private Pod pod = null;
        

        public void WriteThingsDown(List<Pod> Pods)
        {
            //if (this.pod == null)
            //{
            //    IEnumerable<Pod> findPod = Pods.Where(p => p.Id == PodId);
            //    this.pod = findPod.Count() > 0 ? findPod.First() : null;
            //}
            //if (pod != null) mt.SerializeObject(pod, Path.Combine(vals.ObjDir , pod.Title.PathSafe()));
            //else { }

            mt.SerializeObject(FindPod(Pods), Path.Combine(vals.ObjDir , pod.Title.PathSafe()));
        }

        public Pod FindPod(List<Pod> Pods)
        {
            if (this.pod == null)
            {
                IEnumerable<Pod> findPod = Pods.Where(p => p.Id == PodId);
                this.pod = findPod.Count() > 0 ? findPod.First() : null;
            }
            return this.pod;
        }
    }
}
