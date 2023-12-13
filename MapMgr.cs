using RpgMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    /// <summary>
    /// 地图管理类
    /// </summary>
    internal class MapMgr
    {
        public static Dictionary<string, Map> mapDic = new();
        public static Dictionary<int, List<Map>> mapList = new();
        //private static int MapID = 0;
        public static readonly double SphereRis = 0.5;  // 碰撞球半径(0表示无视碰撞)
        public static Random random = new();
        public static string ShowData { get; set; } = "";   // 调试用，无意义

        public MapMgr() {}

        /// <summary>
        /// 创建一个地图
        /// 如果有该id的地图则创建一个分线
        /// </summary>
        /// <param name="mapID">地图ID</param>
        /// <returns></returns>
        public static Map CreateMap(int mapID)
        {
            string mapName = MapTool.GetNormalName(mapID);
            int line = 0;
            if (mapDic.ContainsKey(mapName))
            {
                line = GetMapLine(mapID);
                mapName = MapTool.GetNormalName(mapID, line);
            }
            return CreateMap(mapID, mapName, line);
        }

        /// <summary>
        /// 根据地图ID和地图名创建地图
        /// 分线ID为0
        /// </summary>
        /// <param name="mapID">地图ID</param>
        /// <param name="mapName">地图名</param>
        /// <returns></returns>
        public static Map? CreateMap(int mapID, string mapName)
        {
            return CreateMap(mapID, mapName, 0);
        }

        /// <summary>
        /// 根据地图ID、地图名和分线创建地图
        /// </summary>
        /// <param name="mapID">地图ID</param>
        /// <param name="mapName">地图名</param>
        /// <param name="line">分线ID</param>
        /// <returns></returns>
        public static Map? CreateMap(int mapID, string mapName, int line)
        {
            if (mapDic.ContainsKey(mapName))
            {
                Log.E($"exist Map MapID:{mapID}, mapName:{mapName}");
                return null;
            }
            //int ID = GetMapID();
            Map map = new(mapID, mapName, line);
            mapDic[mapName] = map;

            mapList.TryGetValue(mapID, out List<Map>? list);
            list ??= new();
            list.Add(map);
            mapList[mapID] = list;
            return map;
        }

        /// <summary>
        /// 返回一个可用的分线
        /// </summary>
        /// <param name="mapID">地图ID</param>
        /// <returns></returns>
        public static int GetMapLine(int mapID)
        {
            int line = 0;
            if (mapList.TryGetValue(mapID, out List<Map>? list))
            {
                foreach (Map m in list)
                    line = Math.Max(line, m.Line);
            }
            return line + 1;
        }

        /// <summary>
        /// 根据地图名返回一个地图对象
        /// </summary>
        /// <param name="mapName">地图名</param>
        /// <returns></returns>
        public static Map? GetMap(string mapName)
        {
            if (mapDic.TryGetValue(mapName, out var map))
                return map;
            return null;
        }
        /// <summary>
        /// 返回一个可以进入的地图
        /// 有可进入的则直接返回，没有则新创建一个地图
        /// </summary>
        /// <param name="mapID">地图id</param>
        /// <returns></returns>
        public static Map GetMap(int mapID)
        {
            var config = MapReader.GetConfig(mapID);
            mapList.TryGetValue(mapID, out List<Map> list);
            list ??= new();
            foreach (Map map in list)
            {
                if (map.GetRoleNum() < config.MaxNum)
                    return map;
            }
            return CreateMap(mapID);
        }

        /// <summary>
        /// 删除地图 
        /// </summary>
        /// <param name="map">需要删除的地图对象</param>
        public static void DelMap(Map map)
        {
            mapDic.Remove(map.MapName);
            if(mapList.TryGetValue(map.MapID, out List<Map> list))
            {
                list.Remove(map);
                if (list.Count > 0)
                    mapList[map.MapID] = list;
                else
                    mapList.Remove(map.MapID);
            }
            Log.E($"{map.MapName} map has removed");
        }

        /// <summary>
        /// 关闭地图
        /// </summary>
        /// <param name="map">需要关闭的地图对象</param>
        public static void CloseMap(Map map)
        {
            if(map.GetRoleNum() <= 0)   // 地图内没人，直接删除
                DelMap(map);
            map.KickAllRole();  // 踢出地图内玩家后删除地图
        }
        public static void CloseAllMap()
        {
            foreach(Map map in mapDic.Values)
                CloseMap(map);
        }

        //private static int GetMapID()
        //{
        //    if (MapID >= int.MaxValue)
        //        MapID = 1;
        //    return MapID++;
        //}

        //public void SetTimer()
        //{
        //    Timer MTimer = new (MapMonitor, null, 0, 60000);
        //}

        //public static void MapMonitor(object? o)
        //{
        //    Log.P($"monitor check {mapDic.Count}");
        //    foreach(var map in mapDic.Values)
        //    {
        //        if (!map.thread.IsAlive)
        //            Log.E("map thread exit");
        //    }
        //}

        /// <summary>
        /// 输出地图信息
        /// </summary>
        public static void ShowMapDict()
        {
            foreach (var map in mapDic.Values) 
            { 
                Log.R($"Map ID:{map.MapID},MapName:{map.MapName},CreateTime:{map.CreateTime}"); 
            }
        }
        
    }
   
}
