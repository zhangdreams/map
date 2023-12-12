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

    /// <summary>
    /// buff配置读取
    /// </summary>
    internal class BuffReader
    {
        public static Dictionary<int, BuffConfig> BuffConfigs = new();

        public static List<int> GetBuffIDs()
        {
            return BuffConfigs.Keys.ToList();
        }

        /// <summary>
        /// 返回指定buff对应的配置
        /// </summary>
        /// <param name="buffId"></param>
        /// <returns></returns>
        public static BuffConfig? GetConfig(int buffId)
        {
            if(BuffConfigs.ContainsKey(buffId))
                return BuffConfigs[buffId];
            return null;
        }

        /// <summary>
        /// 读取配置
        /// </summary>
        public static void Read()
        {
            string json = File.ReadAllText("../../../config/buffs.json");
            var configs = JsonSerializer.Deserialize<List<BuffConfig>>(json);
            foreach(var conf in configs)
            {
                BuffConfigs[conf.BuffID] = conf;

                // ShowConfig(conf.BuffID);
            }
        }

        /// <summary>
        /// 输出配置
        /// </summary>
        /// <param name="buffId">buffid </param>
        public static void ShowConfig(int buffId)
        {
            var config = GetConfig(buffId);
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
