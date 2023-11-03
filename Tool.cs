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
            long millSeconds = now.ToUnixTimeMilliseconds();
            return millSeconds;
        }

        // 返回当前时间戳 (毫秒）
        public static long Now2()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            long Seconds = now.ToUnixTimeSeconds();
            return Seconds;
        }

    }

    internal class MapTool
    {
        // 返回地图名（暂未考虑分线情况）
        public static string GetMapName(int mapID)
        {
            return $"normal_map_{mapID}";
        }
        public static string GetMapName(int mapID, int roleID)
        {
            return $"single_map_{mapID}_{roleID}";
        }

        // 用于移动更新位置
        // speed 为m/s 
        // millSec 为毫秒
        public static (double, double) CalcMovingPos(double X, double Y, double X2, double Y2, int speed, int millSec)
        {
            double distance = Math.Sqrt(Math.Pow(X2 - X, 2) + Math.Pow(Y2 - Y, 2));
            int totalTime = (int)distance / speed * 1000;
            double X3 = X + (X2 - X) * (millSec / totalTime);
            double Y3 = Y + (Y2 - Y) * (millSec / totalTime);
            return (X3, Y3);
        }
    }

}
