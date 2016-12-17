using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MorningPod.Ultilities
{

    public static class TimeProvider
    {
        public static DateTime BeginOfTime = new DateTime();

        private static ulong LastUniqueID = (ulong)(DateTime.UtcNow - BeginOfTime).TotalMilliseconds;

        public static ulong UniqueID
        {
            get
            {
                    ulong result = LastUniqueID;
                    while (LastUniqueID == result)
                    {
                        Thread.Sleep(1);
                        result = (ulong)(vals.now - BeginOfTime).TotalMilliseconds;
                    }

                    LastUniqueID = result;
                    return result;
            }
        }
    }
}
