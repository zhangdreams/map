using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RpgMap
{
    /// <summary>
    /// 怪物属性配置
    /// </summary>
    internal class PropReader
    {
        public static Dictionary<int, Prop> PropList = new();

        public static Prop? GetConfig(int propID)
        {
            if(PropList.ContainsKey(propID))
                return PropList[propID];
            return null;
        }

        /// <summary>
        /// 读取配置
        /// </summary>
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

        /// <summary>
        /// 配置输出展示
        /// </summary>
        /// <param name="index"></param>
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
