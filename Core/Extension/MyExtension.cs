using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Extension
{
    public static class MyExtension
    {

       
        public static DateTime getLocalTime()
        {
            DateTime utc = DateTime.UtcNow;
            return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.FindSystemTimeZoneById("Myanmar Standard Time"));
        }
       
        public static DateTime getMMTime()
        {
            DateTime utc = DateTime.UtcNow;
            return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.FindSystemTimeZoneById("Myanmar Standard Time"));

        }


    }
}
