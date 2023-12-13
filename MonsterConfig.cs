using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RpgMap
{
    /// <summary>
    /// 怪物配置类
    /// </summary>
    internal class MonsterConfig
    {
        public int ID { get; set; }    // ID
        public string Name { get; set; } = ""; 
        public int Level { get; set; }
        public int PropID { get; set; } // 属性配置ID
        public int PatrolDistance { get; set; } // 巡逻距离
        public int PursueDistance { get; set; } // 追击距离
        public int AttackDistance { get; set; } // 攻击距离
        public string Skill { get; set; } = string.Empty;
        public List<int> Skills = new();
        public int RebornTime { get; set; } // 复活时间
    }

    internal class MonsterReader
    {
        public static Dictionary<int, MonsterConfig> MonsterList = new();

        /// <summary>
        /// 返回所有的怪物配置id
        /// </summary>
        /// <returns></returns>
        public static List<int> GetMonsterIDs()
        {
            return MonsterList.Keys.ToList();
        }

        /// <summary>
        /// 返回指定id的怪物配置
        /// </summary>
        /// <param name="monsterID"></param>
        /// <returns></returns>
        public static MonsterConfig? GetConfig(int monsterID)
        {
            if(MonsterList.ContainsKey(monsterID))
                return MonsterList[monsterID];
            return null;
        }

        /// <summary>
        /// 从配置读取
        /// </summary>
        public static void Read()
        {
            string json = File.ReadAllText("../../../config/monsters.json");
            var configs = JsonSerializer.Deserialize<List<MonsterConfig>>(json);
            foreach (var conf in configs)
            {
                conf.Skills = Common.StrToList(conf.Skill);
                MonsterList[conf.ID] = conf;

                //Show(conf.ID);
            }
        }

        /// <summary>
        /// 配置输出展示
        /// </summary>
        /// <param name="id"></param>
        public static void Show(int id)
        {
            var config = GetConfig(id);
            if (config != null)
            {
                Log.R($"ID: {config.ID}");
                Log.R($"Name: {config.Name}");
                Log.R($"Level: {config.Level}");
                Log.R($"PropID: {config.PropID}");
                Log.R($"Patrol Distance: {config.PatrolDistance}");
                Log.R($"Pursue Distance: {config.PursueDistance}");
                Log.R($"Attack Distance: {config.AttackDistance}");
                Console.Write($"Skills : ");
                foreach (var sid in config.Skills)
                    Console.Write($"{sid} ,");
                Log.P();
            }
            else
                Log.E($"config {id} not found");
        }
    }
}
