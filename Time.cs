using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class Time
    {
        // 返回当前时间戳 (秒）
        public static long Now()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            long Seconds = now.ToUnixTimeSeconds();
            return Seconds;
        }

        // 返回当前时间戳 (毫秒）
        public static long Now2()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            long millSeconds = now.ToUnixTimeMilliseconds();
            return millSeconds;
        }
    }
}
