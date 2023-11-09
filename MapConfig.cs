using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RpgMap
{
    public struct ConfigPos
    {
        public int x { get; set; }
        public int y { get; set; }
        public ConfigPos(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class MapConfig
    {
        public int MapID { get; set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int BornX { get; set; }
        public int BornY { get; set; }
        public string UnWalk { get; set; }
        public List<ConfigPos> UnWalkList { get; set; }
    }

    internal class MapReader
    {
        public static Dictionary<int, MapConfig> mapConfigs = new();

        // 返回所有的地图配置IDList
        public static List<int> GetMapIDs()
        {
            return mapConfigs.Keys.ToList();
        }

        public static MapConfig? GetConfig(int mapID)
        {
            if (mapConfigs.ContainsKey(mapID))
                return mapConfigs[mapID];
            return null;
        }
        public static void Read()
        {
            string json = File.ReadAllText("../../../config/maps.json");
            var configs = JsonSerializer.Deserialize<List<MapConfig>>(json);
            foreach (var conf in configs)
            {
                string pattern = @"\(\d+,\d+\)";
                MatchCollection matches = Regex.Matches(conf.UnWalk, pattern);
               
                string[] coordinatePairs = conf.UnWalk.Split(',');
                List<ConfigPos> posList = new();
                foreach (Match match in matches)
                {
                    var coordinatePair = match.Value;
                    string cleanedPair = coordinatePair.Replace("(", "").Replace(")", "").Trim();
                    string[] parts = cleanedPair.Split(',');
                    if (parts.Length == 2)
                    {
                        if (int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                        {
                            var pos = new ConfigPos(x,y);
                            posList.Add(pos);
                        }
                    }
                }
                Console.WriteLine($"posList:{posList.Count}");
                conf.UnWalkList = posList;
                mapConfigs[conf.MapID] = conf;
            }
            //foreach (var coordinatePair in mapConfigs)
            //{
            //    ShowConfig(coordinatePair.Key);
            //}
        }

        public static void ShowConfig(int MapID)
        {
            var c = GetConfig(MapID);
            if (c != null)
            {
                Console.WriteLine($"ID: {c.MapID}");
                Console.WriteLine($"Name: {c.Name}");
                Console.WriteLine($"Width: {c.Width}");
                Console.WriteLine($"Height: {c.Height}");
                Console.WriteLine($"BornX: {c.BornX}");
                Console.WriteLine($"BornY: {c.BornY}");
                Console.WriteLine($"UnwalkList:{c.UnWalkList.Count}");
                Console.WriteLine($"unwalk pos:");
                foreach (var v in c.UnWalkList)
                    Console.Write($"{v.x},{v.y};");
            }
        }
    }
}
