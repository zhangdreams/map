using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class BuffConfig
    {
        public int BuffID { get; set; }     // buff ID
        public string Name { get; set; } = ""; // name
        public int Type {  get; set; }      // buff类型 2:无敌
        public int DurTime { get; set; }    // buff持续时长
        public int Level { get; set; }  // buff等级 相同Type 等级高的会替换掉等级低的
        public int AddType { get; set; }    // 叠加类型 根据相同Type 相同等级时： 1:叠加buff时长 2:叠加buff值 3:取buff值最大值或最小值
        public int EffectType { get; set; } //  效果类型 1:属性改变  2:效果类型
        public int Value { get; set; }  // buff效果值
        public string Func { get; set; } = ""; // 效果函数(属性类用来返回改变的属性索引)
    }

    internal class BuffReader
    {
        public static Dictionary<int, BuffConfig> BuffConfigs = new();

        public static List<int> GetBuffIDs()
        {
            return BuffConfigs.Keys.ToList();
        }

        public static BuffConfig? GetConfig(int BuffID)
        {
            if(BuffConfigs.ContainsKey(BuffID))
                return BuffConfigs[BuffID];
            return null;
        }

        public static void Read()
        {
            string json = File.ReadAllText("../../../config/buffs.json");
            var configs = JsonSerializer.Deserialize<List<BuffConfig>>(json);
            foreach(var conf in configs)
            {
                BuffConfigs[conf.BuffID] = conf;

                // Show(conf.BuffID);
            }
        }

        public static void Show(int BuffID)
        {
            var config = GetConfig(BuffID);
            if (config != null)
            {
                Log.P($"BuffID: {config.BuffID}");
                Log.R($"Name: {config.Name}");
                Log.R($"Type: {config.Type}");
                Log.R($"DurTime: {config.DurTime}");
                Log.R($"Level: {config.Level}");
                Log.R($"EffectType: {config.EffectType}");
                Log.R($"Value: {config.Value}");
                Log.R($"Func: {config.Func}");
            }   
        }
    }
}
