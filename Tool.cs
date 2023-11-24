using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class MapTool
    {
        // 返回地图名（暂未考虑分线情况）
        public static string GetNormalName(int mapID)
        {
            return $"normal_map_{mapID}";
        }
        public static string GetNormalName(int mapID, int roleID)
        {
            return $"single_map_{mapID}_{roleID}";
        }

        // 用于移动更新位置
        // speed 为m/s 
        // millSec 为毫秒
        public static (double, double) CalcMovingPos(double X, double Y, double X2, double Y2, int speed, int millSec)
        {
            double distance = Math.Sqrt(Math.Pow(X2 - X, 2) + Math.Pow(Y2 - Y, 2));
            double totalTime = distance / speed * 1000;   // 按ms计算
            if (totalTime <= 0)
                return (X2, Y2);
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

        // 返回两个坐标点的距离
        public static double GetDistance(double X, double Y, double X2, double Y2)
        {
            return Math.Sqrt(Math.Pow(X2 - X, 2) + Math.Pow(Y2 - Y, 2));
        }
        public static double GetDistance(MapPos Pos1, MapPos Pos2)
        {
            return GetDistance(Pos1.x, Pos1.y, Pos2.x, Pos2.y);
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

        // 返回一个出生点为中心，半径R内的一个巡逻点
        // 一般怪物的巡逻范围不会太大，这里不考虑寻路问题
        public static (double, double) GetPatrolPos(double BornX, double BornY, double r)
        {
            // 生成随机半径
            double randomRadius = r * Math.Sqrt(RandomDouble(0, 1));
            // 生成随机角度（弧度）
            double randomAngle = RandomDouble(0, 2 * Math.PI);
            double randomX = BornX + randomRadius * Math.Cos(randomAngle);
            double randomY = BornY + randomRadius * Math.Sin(randomAngle);
            return (randomX, randomY);
        }
        private static double RandomDouble(double minValue, double maxValue)
        {
            Random random = MapMgr.random;
            return minValue + (maxValue - minValue) * random.NextDouble();
        }
    }
}
