using System;
using System.Diagnostics;

namespace RpgMap
{
    public class Program
    {
        public static void Main()
        {
            MapReader.Read();
            SkillReader.Read();

            Console.WriteLine("start map mgr");
            //_ = new MapMgr();
            //var IDList = MapReader.GetMapIDs();
            //foreach (var MapId in IDList)
            //{
            //    string mapName = MapTool.GetMapName(MapId);
            //    Console.WriteLine($"start create map:{MapId},{mapName}");
            //    MapMgr.CreateMap(MapId, mapName);
            //}

            //MapMgr.ShowDict();

            
            Node Start = new(45, 19);
            Node Goal = new(47, 19);
            Console.WriteLine($"start find path");
            Stopwatch stopwatch = Stopwatch.StartNew();
            var Nodes = MapPath.FindPath(1, Start, Goal);
            stopwatch.Stop();
            Console.WriteLine($"map find path Nodes, {stopwatch.ElapsedMilliseconds}");
            foreach(var n in Nodes)
            {
                Console.Write($" ({n.X},{n.Y},{n.F})");
            }
            Console.WriteLine();
            //Console.ReadLine();

        }

    }
}