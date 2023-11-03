using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class MapMgr
    {
        public Dictionary<string, Map> mapDic = new();
        private int MapID = 0;

        public Map? CreateMap(int mapID, string mapName)
        {
            if (mapDic.ContainsKey(mapName))
            {
                return null;
            }
            int ID = GetMapID();
            //var map = Map.NewMap(ID, MapID, mapName);
            Map map = new(ID, mapID, mapName);
            mapDic[mapName] = map;
            return map;
        }

        // 根据地图名返回一个地图对象
        public Map? GetMap(string mapName)
        {
            if (mapDic.ContainsKey(mapName))
                return mapDic[mapName];
            return null;
        }

        private int GetMapID()
        {
            if (MapID >= int.MaxValue)
                this.MapID = 1;
            return this.MapID++;
        }
       
    }
   
}
