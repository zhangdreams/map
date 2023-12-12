using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class Time
    {
        /// <summary>
        /// 返回当前时间戳 (秒）
        /// </summary>
        /// <returns></returns>
        public static long NowSec()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            long Seconds = now.ToUnixTimeSeconds();
            return Seconds;
        }

        /// <summary>
        /// 返回当前时间戳 (毫秒）
        /// </summary>
        /// <returns></returns>
        public static long NowMillSec()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            long millSeconds = now.ToUnixTimeMilliseconds();
            return millSeconds;
        }
    }
}
