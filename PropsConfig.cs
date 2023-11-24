using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class PropReader
    {
        public static Dictionary<int, Prop> PropList = new();

        public static Prop? GetConfig(int PropID)
        {
            if(PropList.ContainsKey(PropID))
                return PropList[PropID];
            return null;
        }

        public static void Read()
        {
            string json = File.ReadAllText("../../../config/props.json");
            var configs = JsonSerializer.Deserialize<List<Prop>>(json);
            foreach (var conf in configs)
            {
                conf.HP = conf.MaxHp;
                PropList[conf.Index] = conf;

                // Show(conf.Index);
            }
        }

        public static void Show(int index)
        {
            var config = GetConfig(index);
            if (config != null)
            {
                Log.R($"Index: {config.Index}");
                Log.R($"MaxHp: {config.MaxHp}");
                Log.R($"Attack: {config.Attack}");
                Log.R($"Defense: {config.Defense}");
                Log.R($"Speed: {config.Speed}");
            }
            else
                Log.E($"config {index} not found");
        }
    }
}
