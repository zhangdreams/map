using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class MapActor
    {
        public int Type { get; set; }   // ActorType  1:玩家 2:怪物
        public long ID { get; set; }
        public Prop Prop { get; set; } = new(); // 属性
        public Dictionary<int, MapBuff> Buffs { get; set; } = new();
        public Dictionary<int, ActorSkill> Skills { get; set; } = new();
        public Map Map { get; set; }

        public MapActor(int Type, long ID) 
        { 
            this.Type = Type;
            this.ID = ID;
        }

        public MapPos GetPos()
        {
            switch (Type)
            {
                case 1:
                    MapRole role = Map.Roles[ID];
                    return role.GetPos();
                case 2:
                    MapMonster monster = Map.Monsters[ID];
                    return monster.GetPos();
                default:
                    return new MapPos(0,0);
            }
        }

        // 添加buff 
        // durTime buff持续时间
        // value buff效果值
        public void AddBuff(int buffID, int durTime, long value)
        {
            long Now2 = Time.Now2();

            if (!Buffs.ContainsKey(buffID))
            {
                MapBuff buff = new(buffID, Now2 + durTime, value);
                Buffs[buffID] = buff;
                // 新增buff(触发buff效果)
            }
            else
            {
                // 替换、叠加
                // todo 叠加持续时间
                MapBuff oldbuff = Buffs[buffID];
                oldbuff.EndTime += durTime;
            }
        }

        // 加血
        public long DoAddHP(long Add)
        {
            long NewHp = 0;
            switch(Type)
            {
                case 1:
                    MapRole role = Map.Roles[ID];
                    NewHp = role.AddHp(Add);
                    break;
                case 2:
                    MapMonster monster = Map.Monsters[ID];
                    NewHp = monster.AddHp(Add);
                    break;
                default: 
                    break;
            }
            return NewHp;
        }
        // 扣血
        public long DoDecHP(long Dec)
        {
            long NewHp = 0;
            switch (Type)
            {
                case 1:
                    MapRole role = Map.Roles[ID];
                    NewHp = role.DecHp(Dec);
                    break;
                case 2:
                    MapMonster monster = Map.Monsters[ID];
                    NewHp = monster.DecHp(Dec);
                    break;
                default:
                    break;
            }
            return NewHp;
        }

        // 使用技能
        public void DoUseSkill(int SkillID, MapPos pos, List<(int,long)> TargetMap)
        {
            try
            {
                if (!Skills.ContainsKey(SkillID))
                    throw new Exception($"unequip skill {SkillID}");
                ActorSkill Skill = Skills[SkillID];
                foreach (var Key in TargetMap)
                {
                    if(!MapCommon.ActorInMap(Map, Key))
                        TargetMap.Remove(Key);
                }
                SkillConfig config = SkillReader.GetConfig(SkillID) ?? throw new Exception($"can not find skill config {SkillID}");

                MapSkill SkillEntity = new(Type, ID, TargetMap, SkillID, pos);
                Map.AddSkillEntity(SkillEntity);
                if(config.SBuffs.Count > 0)
                {
                    foreach(var Buff in config.SBuffs)
                    {
                        // todo 可能会给自己添加buff
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
                    
        }
    }
}
