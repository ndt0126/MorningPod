using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Xml.Serialization;
using MorningPod.Ultilities;
using MorningPod.ViewModels;

namespace MorningPod.Models
{
    public class Pod : NotificationObject
    {
        public ulong Id { get; set; }
        public Pod() { Id = TimeProvider.UniqueID; }

        public string URL { get; set; }
        public string ImgUrl { get; set; }

        private string _title; public string Title { get { return _title; } set { _title = value; RPC("Title"); } }


        public string Description { get; set; }

        private string _imgPath; [XmlIgnore]public string ImgPath { set { _imgPath = value; } get { if (_imgPath == null) { _imgPath = Path.Combine(vals.ObjDir , SafeTitle + ".thumb"); } return _imgPath; } }

        private string _mediaPath; [XmlIgnore]public string MediaPath { set { _mediaPath = value; } get { if (_mediaPath == null) { if (SafeTitle == null) { return null; } _mediaPath = Path.Combine(vals.MediaDir , SafeTitle); } return _mediaPath; } } 

        private List<Epi> _epis;     public List<Epi> Epis { get { return _epis; } set { _epis = value; RPC("Epi"); } }
        public string SafeTitle { get; set; }
        public DateTime LastUpdate { get; set; }

        public Color[] Colors;

        public override string ToString() { return Title; }
    }
}
