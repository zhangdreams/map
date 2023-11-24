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
        private static int MapID = 0;
        public static Random random = new((int)DateTime.Now.Ticks);
        public static string show { get; set; } = "";

        public MapMgr() 
        {
            //SetTimer();
        }

        public static Map? CreateMap(int mapID, string mapName)
        {
            if (mapDic.ContainsKey(mapName))
            {
                Log.E($"exist Map MapID:{mapID}, mapName:{mapName}");
                return null;
            }
            int ID = GetMapID();
            //var map = Map.NewMap(ID, MapID, mapName);
            Map map = new(ID, mapID, mapName);
            mapDic[mapName] = map;
            return map;
        }

        // 根据地图名返回一个地图对象
        public static Map? GetMap(string mapName)
        {
            if (mapDic.ContainsKey(mapName))
                return mapDic[mapName];
            return null;
        }

        private static int GetMapID()
        {
            if (MapID >= int.MaxValue)
                MapID = 1;
            return MapID++;
        }

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
                Log.R($"Map ID:{map.ID}, MapID:{map.MapID},MapName:{map.MapName},CreateTime:{map.CreateTime}"); 
            }
        }
        
    }
   
}
