using RpgMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class MapMgr
    {
        public static Dictionary<string, Map> mapDic = new();
        public static Dictionary<int, List<Map>> mapList = new();
        //private static int MapID = 0;
        public static Random random = new((int)DateTime.Now.Ticks);
        public static string show { get; set; } = "";

        public MapMgr() 
        {
            //SetTimer();
        }

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
        public static Map? CreateMap(int mapID, string mapName)
        {
            return CreateMap(mapID, mapName, 0);
        }
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

            mapList.TryGetValue(mapID, out List<Map> list);
            list ??= new();
            list.Add(map);
            mapList[mapID] = list;
            return map;
        }

        // 返回一个可用的分线
        public static int GetMapLine(int mapID)
        {
            List<Map> list = new();
            mapList.TryGetValue(mapID, out list);
            int line = 0;
            foreach(Map m in list)
                line = Math.Max(line, m.Line);
            return line + 1;
        }

        // 根据地图名返回一个地图对象
        public static Map? GetMap(string mapName)
        {
            if (mapDic.ContainsKey(mapName))
                return mapDic[mapName];
            return null;
        }
        // 返回一个可以进入的地图
        // 有可进入的则直接返回，没有则新创建一个地图
        public static Map GetMap(int mapID)
        {
            var config = MapReader.GetConfig(mapID);
            List<Map> list = new();
            mapList.TryGetValue(mapID, out list);
            foreach (Map map in list)
            {
                if (map.RoleNum < config.MaxNum)
                    return map;
            }
            return CreateMap(mapID);
        }

        public static void DelMap(Map map)
        {
            mapDic.Remove(map.MapName);
            List<Map> list = new();
            mapList.TryGetValue(map.MapID, out list);
            list.Remove(map);
            if (list.Count > 0)
                mapList[map.MapID] = list;
            else
                mapList.Remove(map.MapID);
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

        public static void ShowDict()
        {
            foreach (var map in mapDic.Values) 
            { 
                Log.R($"Map ID:{map.MapID},MapName:{map.MapName},CreateTime:{map.CreateTime}"); 
            }
        }
        
    }
   
}
