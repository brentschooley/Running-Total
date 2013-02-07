using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunningTotal.Model
{
    public static class Extensions
    {
        public static double ToRadian(this double d)
        {
            return d * (Math.PI / 180);
        }

        public static string ToMinuteString(this double d)
        {
            double dec = d - Math.Floor(d);
            double seconds = Math.Round((60 * dec));
            if (seconds == 60)
            {
                return Math.Floor(d) + 1 + ":00";
            }
            return Math.Floor(d) + ":" + seconds;
        }
    }
}
