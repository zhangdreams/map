using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RpgMap
{
    /// <summary>
    /// 配置用坐标
    /// </summary>
    public struct ConfigPos
    {
        public int X { get; } = 0;
        public int Y { get; } = 0;
        public ConfigPos(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    /// <summary>
    /// 地图配置
    /// </summary>
    public class MapConfig
    {
        public int MapID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public int BornX { get; set; }
        public int BornY { get; set; }
        public int MaxNum { get; set; }
        public string UnWalk { get; set; } = string.Empty;
        public Dictionary<ConfigPos, int> UnWalkList { get; set; } = new();
    }

    internal class MapReader
    {
        public static Dictionary<int, MapConfig> mapConfigs = new();

        /// <summary>
        /// 返回所有的地图配置IDList
        /// </summary>
        /// <returns></returns>
        public static List<int> GetMapIDs()
        {
            return mapConfigs.Keys.ToList();
        }

        /// <summary>
        /// 返回指定id的地图配置
        /// </summary>
        /// <param name="mapID">地图id</param>
        /// <returns></returns>
        public static MapConfig? GetConfig(int mapID)
        {
            if (mapConfigs.ContainsKey(mapID))
                return mapConfigs[mapID];
            return null;
        }

        /// <summary>
        /// 从配置表读取配置
        /// </summary>
        public static void Read()
        {
            string json = File.ReadAllText("../../../config/maps.json");
            var configs = JsonSerializer.Deserialize<List<MapConfig>>(json);
            foreach (var conf in configs)
            {
                string pattern = @"\(\d+,\d+\)";
                MatchCollection matches = Regex.Matches(conf.UnWalk, pattern);
               
                //string[] coordinatePairs = conf.UnWalk.Split(',');
                Dictionary<ConfigPos, int> posdict = new();
                foreach (Match match in matches.Cast<Match>())
                {
                    var coordinatePair = match.Value;
                    string cleanedPair = coordinatePair.Replace("(", "").Replace(")", "").Trim();
                    string[] parts = cleanedPair.Split(',');
                    if (parts.Length == 2)
                    {
                        if (int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                        {
                            var pos = new ConfigPos(x,y);
                            posdict[pos] = 1;
                        }
                    }
                }
                Log.P($"posList:{posdict.Count}");
                conf.UnWalkList = posdict;
                mapConfigs[conf.MapID] = conf;
            }
            //foreach (var coordinatePair in mapConfigs)
            //{
            //    ShowConfig(coordinatePair.Key);
            //}
        }

        /// <summary>
        /// 输出地图配置
        /// </summary>
        /// <param name="mapID"></param>
        public static void ShowConfig(int mapID)
        {
            var c = GetConfig(mapID);
            if (c != null)
            {
                Log.R($"ID: {c.MapID}");
                Log.R($"Name: {c.Name}");
                Log.R($"Width: {c.Width}");
                Log.R($"Height: {c.Height}");
                Log.R($"BornX: {c.BornX}");
                Log.R($"BornY: {c.BornY}");
                Log.R($"MaxNum: {c.MaxNum}");
                Log.R($"UnwalkList:{c.UnWalkList.Count}");
                Log.R($"unwalk pos:");
                foreach (var v in c.UnWalkList.Keys)
                    Console.Write($"{v.X},{v.Y};");
            }
            else
                Log.E($"config {mapID} not found");
        }
    }
}
