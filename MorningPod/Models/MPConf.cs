using System;
using System.IO;
using MorningPod.Ultilities;
using MorningPod.ViewModels;

namespace MorningPod.Models
{
    public class MPConf : NotificationObject
    {
        private DateTime _lastUpdate; public DateTime LastUpdate { get { return _lastUpdate; } set { _lastUpdate = value; RPC("LastUpdate"); } }
        private DateTime _lastWake; public DateTime LastWake { get { return _lastWake; } set { _lastWake = value; RPC("LastWake"); } }

        public double NextUpdateInMins { get; set; }
        private DateTime _nextUpdate; public DateTime NextUpdate { get { return _nextUpdate; } set { _nextUpdate = value; RPC("NextUpdate"); } }

        public double NextWakeInMins { get; set; }
        private DateTime _nextWake; public DateTime NextWake { get { return _nextWake; } set { _nextWake = value; RPC("NextWake"); } }





        public void SetUpdateTime(double d)
        {
            NextUpdateInMins = d;
            NextUpdate = NextUpdateInMins.ConvertMinsToNextDateTime();
            WriteThingsDown();
        }
        public void SetWakeTime(double d)
        {
            NextWakeInMins = d;
            NextWake = NextWakeInMins.ConvertMinsToNextDateTime();
            WriteThingsDown();
        }


        public void JustUpdated() { DateTime now = vals.now; LastUpdate = now; NextUpdate = now.AddDays(1); WriteThingsDown(); }

        public void JustWaked() { DateTime now = vals.now; LastWake = now; NextWake = now.AddDays(1); WriteThingsDown(); }



        public static MPConf TryRead()
        {
            string path = Path.Combine(vals.BaseDir,"config");
            if(!File.Exists(path)) return new MPConf();

            MPConf conf = mt.DeSerializeObject<MPConf>(path);

            if(conf == null) return new MPConf();
            return conf;
        }

        public void WriteThingsDown()
        {
            string path = Path.Combine(vals.BaseDir,"config");
            mt.SerializeObject<MPConf>(this,path);
        }

    }
}
