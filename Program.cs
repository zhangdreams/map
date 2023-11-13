using System;
using System.Collections.Generic;
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
            
            Node Start = new(45, 1);
            Node Goal = new(45, 90);
            Console.WriteLine($"start find path");
            Stopwatch stopwatch = Stopwatch.StartNew();
            var Nodes = MapPath.FindPath(1, Start, Goal);
            Nodes = Nodes == null ? new List<Node>() : Nodes;
            stopwatch.Stop();
            Console.WriteLine($"map find path Nodes, {stopwatch.ElapsedMilliseconds}");
            foreach(var n in Nodes)
            {
                Console.Write($" ({n.X},{n.Y})");
            }
            Console.WriteLine();

            
            //Console.ReadLine();

        }

    }
}