using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RpgMap
{
    /// <summary>
    /// 技能配置类
    /// </summary>
    internal class SkillConfig
    {
        public int SkillID { get; set; }
        public string Name { get; set; } = "";
        public double AttackParam { get; set; } // 技能伤害参数
        public double AttackDistance { get; set; } // 攻击距离
        public int CD { get; set; } // 技能cd (ms)
        public int Type { get; set; }   // 0非指向技能 1指向性技能
        public int TotalWave { get; set; } // 技能总伤害波数
        public string WaveInterval { get; set; } = "";
        public List<int> Waves { get; set; } = new(); // 波次间隔
        public int DamageType { get; set; } // 伤害范围 1：单个目标 2：圆形范围 3：扇形范围 4：矩形范围

        /// <summary>
        /// 技能范围参数 
        /// DamageType 
        ///      1: 无效
        ///      2: {半径}
        ///      3: {半径， 扇形角度}
        ///      3: {长， 宽}
        /// </summary>
        public string RangeParams { get; set; } = "";
        public List<int> Ranges { get; set; } = new(); 
        public int TargetNum { get; set; } // 最大目标数量
        public string SelfBuffs { get; set; } = "";
        public List<int> SBuffs { get; set; } = new(); // 给自己加的buff（通常是技能释放的时候）
        public string TargetBuffs { get; set; } = "";

        public List<int> TBuffs { get;set; } = new(); // 给目标加的buff（通常是波次结束）
    }

    internal class SkillReader
    {
        public static Dictionary<int, SkillConfig> SkillConfigs = new();

        /// <summary>
        /// 返回所有技能id
        /// </summary>
        /// <returns></returns>
        public static List<int> GetSkillIDs()
        {
            return SkillConfigs.Keys.ToList();
        }

        /// <summary>
        /// 返回指定技能id的技能配置
        /// </summary>
        /// <param name="skillID">技能ID</param>
        /// <returns></returns>
        public static SkillConfig? GetConfig(int skillID)
        {
            if(SkillConfigs.ContainsKey(skillID))
                return SkillConfigs[skillID];
            return null;
        }

        /// <summary>
        /// 读取配置
        /// </summary>
        public static void Read()
        {
            string json = File.ReadAllText("../../../config/skill.json");
            var configs = JsonSerializer.Deserialize<List<SkillConfig>>(json);
            foreach(var conf in configs)
            {
                conf.Waves = Common.StrToList(conf.WaveInterval);
                conf.Ranges = Common.StrToList(conf.RangeParams);
                conf.SBuffs = Common.StrToList(conf.SelfBuffs);
                conf.TBuffs = Common.StrToList(conf.TargetBuffs);
                SkillConfigs[conf.SkillID] = conf;
               
                //ShowConfig(conf.SkillID);
            }
        }

        /// <summary>
        /// 技能配置展示
        /// </summary>
        /// <param name="skillID">技能ID</param>
        public static void ShowConfig(int skillID)
        {
            var config = GetConfig(skillID);
            if(config != null)
            {
                Log.R($"skillID:{config.SkillID}");
                Log.R($"Name:{config.Name}");
                Log.R($"AttackParam:{config.AttackParam}");
                Log.R($"AttackDistance:{config.AttackDistance}");
                Log.R($"CD:{config.CD}");
                Log.R($"Type:{config.Type}");
                Log.R($"TotalWave:{config.TotalWave}");
                Log.R($"DamageType:{config.DamageType}");
                Log.R($"Waves:{string.Join(",", config.Waves)}");
                Log.R($"Ranges:{string.Join(",", config.Ranges)}");
                Log.R($"SBuffs:{string.Join(",", config.SBuffs)}");
                Log.R($"TBuffs:{string.Join(",", config.TBuffs)}");
                Log.P();
            }
            else
                Log.R($"config {skillID} not found");
        }
    }
}
