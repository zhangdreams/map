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

        public MapMgr() { }

        public static Map? CreateMap(int mapID, string mapName)
        {
            if (mapDic.ContainsKey(mapName))
            {
                Console.WriteLine($"exist Map MapID:{mapID}, mapName:{mapName}");
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

        public static void ShowDict()
        {
            foreach (var map in mapDic.Values) 
            { 
                Console.WriteLine($"Map ID:{map.ID}, MapID:{map.MapID},MapName:{map.MapName},CreateTime:{map.CreateTime}"); 
            }
        }
        
    }
   
}
