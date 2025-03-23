using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;

//路径系统中的脚本

namespace RDTS
{
    public static class Statistics
    {
        public static DateTime StatistikStart;
        
        public static string Timestamp(DateTime datetime)
        {
            BigInteger? time = null;
            var inutc = datetime.ToUniversalTime();
            DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timeSpan = inutc.Subtract(UnixEpoch);
            time = timeSpan.Ticks * 100;
            return time.ToString();
        }
        
        public static string Timestamp(float time)
        {
            var simtime = StatistikStart.AddSeconds(time);
            return Timestamp(simtime);
        }
    }

    public abstract class StatBase
    {
        public string experiment;
        public float time;
        public abstract string ToInflux();
    }
    
    public class StatValue : StatBase
    {
        public string group;
        public string type;
        public string name;
        public float value;
        
        public override string ToInflux()
        {
            return $"StatVariable,experiment={experiment},group={group},type={type},name={name} value={value} {Statistics.Timestamp(time)}";
        }
    }
}