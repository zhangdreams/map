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
            BuffReader.Read();
            PropReader.Read();
            MonsterReader.Read();

            // 地图创建
            //Console.WriteLine("start map mgr");
            //_ = new MapMgr();
            //var IDList = MapReader.GetMapIDs();
            //foreach (var MapId in IDList)
            //{
            //    string mapName = MapTool.GetMapName(MapId);
            //    Console.WriteLine($"start create map:{MapId},{mapName}");
            //    MapMgr.CreateMap(MapId, mapName);
            //}

            //MapMgr.ShowDict();
            
            // 寻路测试
            //Node Start = new(45, 1);
            //Node Goal = new(45, 90);
            //Console.WriteLine($"start find path");
            //Stopwatch stopwatch = Stopwatch.StartNew();
            //var Nodes = MapPath.FindPath(1, Start, Goal);
            //Nodes ??= new List<Node>();
            //stopwatch.Stop();
            //Console.WriteLine($"map find path Nodes, {stopwatch.ElapsedMilliseconds}");
            //foreach(var n in Nodes)
            //{
            //    Console.Write($" ({n.X},{n.Y})");
            //}
            Console.WriteLine();
            
            Prop prop = new();
            Console.WriteLine($"attack value :{Common.GetFieldValue(prop, "Attack")}");
            Common.SetFieldValue(prop, "Speed", 200);
            Console.WriteLine($"Prop Attack value:{prop.Attack}");
            Console.WriteLine($"Prop Attack value:{prop.Speed}");

            
            //Console.ReadLine();

        }

    }
}