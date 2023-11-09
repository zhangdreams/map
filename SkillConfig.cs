using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class SkillConfig
    {
        public int SkillID { get; set; }
        public string Name { get; set; }
        public double AttackParam { get; set; } // 技能伤害参数
        public double AttackDistance { get; set; } // 攻击距离
        public int CD { get; set; } // 技能cd (ms)
        public int Type { get; set; }   // 0非指向技能 1指向性技能
        public int TotalWave { get; set; } // 技能总伤害波数
        public string WaveInterval { get; set; }
        public List<int> Waves { get; set; } = new(); // 波次间隔
        public int DamageType { get; set; } // 伤害范围 1：单个目标 2：圆形范围 3：扇形范围 4：矩形范围

        // 技能范围参数 
        // DamageType 
        //      1: 无效
        //      2: {半径}
        //      3: {半径， 扇形角度}
        //      3: {长， 宽}
        public string RangeParams { get; set; }
        public List<int> Ranges { get; set; } = new(); 
        public int TargetNum { get; set; } // 最大目标数量
        public string SelfBuffs { get; set; }
        public List<int> SBuffs { get; set; } = new(); // 给自己加的buff（通常是技能释放的时候）
        public string TargetBuffs { get; set; }

        public List<int> TBuffs { get;set; } = new(); // 给目标加的buff（通常是波次结束）
    }

    internal class SkillReader
    {
        public static Dictionary<int, SkillConfig> SkillConfigs = new();

        public static List<int> GetSkillIDs()
        {
            return SkillConfigs.Keys.ToList();
        }

        public static SkillConfig? GetConfig(int skillID)
        {
            if(SkillConfigs.ContainsKey(skillID))
                return SkillConfigs[skillID];
            return null;
        }

        public static void Read()
        {
            string json = File.ReadAllText("../../../config/skill.json");
            var configs = JsonSerializer.Deserialize<List<SkillConfig>>(json);
            foreach(var conf in configs)
            {
                conf.Waves = StrToList(conf.WaveInterval);
                conf.Ranges = StrToList(conf.RangeParams);
                conf.SBuffs = StrToList(conf.SelfBuffs);
                conf.TBuffs = StrToList(conf.TargetBuffs);
                SkillConfigs[conf.SkillID] = conf;
               
                //ShowConfig(conf.SkillID);
            }
        }

        public static List<int> StrToList(string str)
        {
            if (str == "")
                return new List<int>();
            string[] parts = str.Split(',');
            List<int> list = parts.Select(part => int.Parse(part.Trim())).ToList();
            return list;
        }

        public static void ShowConfig(int SkillID)
        {
            var config = GetConfig(SkillID);
            if(config != null)
            {
                Console.WriteLine($"skillID:{config.SkillID}");
                Console.WriteLine($"Name:{config.Name}");
                Console.WriteLine($"AttackParam:{config.AttackParam}");
                Console.WriteLine($"AttackDistance:{config.AttackDistance}");
                Console.WriteLine($"CD:{config.CD}");
                Console.WriteLine($"Type:{config.Type}");
                Console.WriteLine($"TotalWave:{config.TotalWave}");
                Console.WriteLine($"DamageType:{config.DamageType}");
                Console.WriteLine($"Waves:{string.Join(",", config.Waves)}");
                Console.WriteLine($"Ranges:{string.Join(",", config.Ranges)}");
                Console.WriteLine($"SBuffs:{string.Join(",", config.SBuffs)}");
                Console.WriteLine($"TBuffs:{string.Join(",", config.TBuffs)}");
                Console.WriteLine();
            }
        }
    }
}
