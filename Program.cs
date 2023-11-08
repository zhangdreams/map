using System;

namespace RpgMap
{
    public class Program
    {
        public static void Main()
        {
            MapReader.Read();
            SkillReader.Read();

            Console.WriteLine("start map mgr");
            _ = new MapMgr();
            var IDList = MapReader.GetMapIDs();
            foreach (var MapId in IDList)
            {
                string mapName = MapTool.GetMapName(MapId);
                Console.WriteLine($"start create map:{MapId},{mapName}");
                MapMgr.CreateMap(MapId, mapName);
            }

            MapMgr.ShowDict();

            //Console.ReadLine();

        }

    }
}