using System;
using System.Collections.Generic;
using System.Drawing;
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
            long millSeconds = now.ToUnixTimeSeconds();
            return millSeconds;
        }

        // 返回当前时间戳 (毫秒）
        public static long Now2()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            long Seconds = now.ToUnixTimeMilliseconds();
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
            int totalTime = (int)distance / speed * 1000;   // 按ms计算
            double X3 = X + (X2 - X) * (millSec / totalTime);
            double Y3 = Y + (Y2 - Y) * (millSec / totalTime);
            return (X3, Y3);
        }

        // 判断两个坐标点的距离是否小于distance
        // 也可判断x,y是否在中心点为x2,y2半径为Distance的圆内
        public static bool CheckDistance(double X, double Y, double X2, double Y2, double Distance)
        {
            double dis = Math.Sqrt(Math.Pow(X2 - X, 2) + Math.Pow(Y2 - Y, 2));
            return dis <= Distance;
        }
        public static bool CheckDistance(MapPos Pos1, MapPos Pos2, double Distance) 
        {
            return CheckDistance(Pos1.x, Pos1.y, Pos2.x, Pos2.y, Distance);
        }

        // 判断x,y是否处于中心点为x2,y2半径为r 朝向为Dir角度为an的扇形范围内
        // dir为一个朝向，为扇形的中心线
        public static bool InSector(double X, double Y, double X2, double Y2, double dir, double an, double r)
        {
            double dis = Math.Sqrt(Math.Pow(X2 - X, 2) + Math.Pow(Y2 - Y, 2));
            double angle = Math.Atan2(Y - Y2, X - X2);
            angle = angle * 180 / Math.PI; // 弧度转角度
            angle = angle < 0 ? angle + 360 : angle;

            // 扇形的起始和结束角度
            double sAngle = dir - an / 2;
            sAngle = sAngle >= 0 ? sAngle : sAngle + 360;
            double eAngle = dir + an / 2;
            eAngle = eAngle < 360 ? eAngle : eAngle - 360;

            bool InAngle;
            if (eAngle >= sAngle)
                InAngle = angle >= sAngle && angle <= eAngle;
            else
                InAngle = angle >= sAngle || angle <= eAngle;   // 起始和结束角度横跨了0°

            return InAngle && dis <= r;
        }
        public static bool InSector(MapPos Pos1, MapPos Pos2, double an, double r)
        {
            return InSector(Pos1.x, Pos1.y, Pos2.x, Pos2.y, Pos2.dir, an, r);
        }
    }
}
