using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace RpgMap
{
    internal class MapTool
    {
        /// <summary>
        /// 返回地图名（暂未考虑分线情况）
        /// </summary>
        /// <param name="mapID">地图ID</param>
        /// <returns></returns>
        public static string GetNormalName(int mapID)
        {
            return $"normal_map_{mapID}_0";
        }
        public static string GetNormalName(int mapID, int line)
        {
            return $"single_map_{mapID}_{line}";
        }

        /// <summary>
        /// 用于移动更新位置
        /// </summary>
        /// <param name="x">初始坐标X</param>
        /// <param name="y">初始坐标Y</param>
        /// <param name="x2">目标坐标X</param>
        /// <param name="y2">目标坐标Y</param>
        /// <param name="speed">速度 m/s</param>
        /// <param name="millSec">更新时长 （毫秒）</param>
        /// <returns>新的坐标</returns>
        public static (double, double) CalcMovingPos(double x, double y, double x2, double y2, int speed, int millSec)
        {
            double distance = Math.Sqrt(Math.Pow(x2 - x, 2) + Math.Pow(y2 - y, 2));
            double totalTime = distance / speed * 1000;   // 按ms计算
            if (totalTime <= 0 || totalTime <= millSec)
                return (x2, y2);
            double X3 = x + (x2 - x) * (millSec / totalTime);
            double Y3 = y + (y2 - y) * (millSec / totalTime);
            return (X3, Y3);
        }

        /// <summary>
        /// 判断两个坐标点的距离是否小于distance
        /// 也可判断x,y是否在中心点为x2,y2半径为Distance的圆内
        /// </summary>
        /// <param name="x">A点坐标</param>
        /// <param name="y"></param>
        /// <param name="x2">B点坐标</param>
        /// <param name="y2"></param>
        /// <param name="distance">距离</param>
        /// <returns></returns>
        public static bool CheckDistance(double x, double y, double x2, double y2, double distance)
        {
            double dis = Math.Sqrt(Math.Pow(x2 - x, 2) + Math.Pow(y2 - y, 2));
            return dis <= distance;
        }
        /// <summary>
        /// 判断两个坐标点的距离是否小于distance
        /// 也可判断x,y是否在中心点为x2,y2半径为Distance的圆内
        /// </summary>
        /// <param name="pos1">A点坐标</param>
        /// <param name="pos2">B点坐标</param>
        /// <param name="distance">距离</param>
        /// <returns></returns>
        public static bool CheckDistance(MapPos pos1, MapPos pos2, double distance) 
        {
            return CheckDistance(pos1.x, pos1.y, pos2.x, pos2.y, distance);
        }

        /// <summary>
        /// 返回两个坐标点的距离
        /// </summary>
        /// <param name="x">A点坐标</param>
        /// <param name="y"></param>
        /// <param name="x2">B点坐标</param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public static double GetDistance(double x, double y, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x, 2) + Math.Pow(y2 - y, 2));
        }
        /// <summary>
        /// 返回两个坐标点的距离
        /// </summary>
        /// <param name="pos1">A点坐标</param>
        /// <param name="pos2">B点坐标</param>
        /// <returns></returns>
        public static double GetDistance(MapPos pos1, MapPos pos2)
        {
            return GetDistance(pos1.x, pos1.y, pos2.x, pos2.y);
        }

        /// <summary>
        /// 判断x,y是否处于中心点为x2,y2半径为r 朝向为Dir角度为an的扇形范围内
        /// </summary>
        /// <param name="x">A点坐标</param>
        /// <param name="y"></param>
        /// <param name="x2">B点坐标</param>
        /// <param name="y2"></param>
        /// <param name="dir">朝向，为扇形的中心线</param>
        /// <param name="an">扇形角度</param>
        /// <param name="r">半径</param>
        /// <returns></returns>
        public static bool InSector(double x, double y, double x2, double y2, double dir, double an, double r)
        {
            double dis = Math.Sqrt(Math.Pow(x2 - x, 2) + Math.Pow(y2 - y, 2));
            double angle = Math.Atan2(y - y2, x - x2);
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
        /// <summary>
        /// 判断B是否处于中心点为A半径为r 朝向为Dir角度为an的扇形范围内
        /// </summary>
        /// <param name="pos1">A点坐标</param>
        /// <param name="pos2">B点坐标</param>
        /// <param name="an">扇形角度</param>
        /// <param name="r">半径</param>
        /// <returns></returns>
        public static bool InSector(MapPos pos1, MapPos pos2, double an, double r)
        {
            return InSector(pos1.x, pos1.y, pos2.x, pos2.y, pos1.dir, an, r);
        }

        /// <summary>
        /// 返回一个出生点为中心，半径R内的一个巡逻点
        /// 一般怪物的巡逻范围不会太大，这里不考虑寻路问题
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="pos">出生点坐标</param>
        /// <param name="r">半径</param>
        /// <returns></returns>
        public static (double, double) GetPatrolPos(Map map, MapPos pos, double r)
        {
            return GetPatrolPos(map, pos.x, pos.y, r, 5);
        }
        public static (double, double) GetPatrolPos(Map map, double bornX, double bornY, double r)
        {
            return GetPatrolPos(map, bornX, bornY, r, 5);
        }
        /// <summary>
        /// 返回一个出生点为中心，半径R内的一个巡逻点
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="bornX">出生点</param>
        /// <param name="bornY"></param>
        /// <param name="r">半径</param>
        /// <param name="times">随机次数</param>
        /// <returns></returns>
        public static (double, double) GetPatrolPos(Map map, double bornX, double bornY, double r, int times)
        {
            if(times <= 0) 
                return(bornX, bornY);
            var config = MapReader.GetConfig(map.MapID);
            // 生成随机半径
            double randomRadius = r * Math.Sqrt(RandomDouble(0, 1));
            // 生成随机角度（弧度）
            double randomAngle = RandomDouble(0, 2 * Math.PI);
            double randomX = bornX + randomRadius * Math.Cos(randomAngle);
            double randomY = bornY + randomRadius * Math.Sin(randomAngle);

            randomX = Math.Max(0, Math.Min(randomX, config.Width));
            randomY = Math.Max(0, Math.Min(randomY, config.Height));
            if (!MapPath.IsObstacle(map, (int)randomX, (int)randomY))
                return (randomX, randomY);
            return GetPatrolPos(map, bornX, bornY, r, times - 1);
        }

        /// <summary>
        /// double类型随机
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        private static double RandomDouble(double minValue, double maxValue)
        {
            Random random = MapMgr.random;
            return minValue + (maxValue - minValue) * random.NextDouble();
        }
    }
}
