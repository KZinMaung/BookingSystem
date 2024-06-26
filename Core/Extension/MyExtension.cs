﻿using System;
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
            //DateTime utc = DateTime.UtcNow;
            //return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.FindSystemTimeZoneById("Myanmar Standard Time"));

            return DateTime.Now;
        }
       
        public static DateTime getMMTime()
        {
            DateTime utc = DateTime.UtcNow;
            return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.FindSystemTimeZoneById("Myanmar Standard Time"));

        }

        public static string getUniqueNumber()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            string uniquecode = BitConverter.ToUInt32(buffer, 12).ToString();
            return uniquecode;
        }


    }
}
