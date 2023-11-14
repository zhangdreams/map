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
        public Map Map;

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

        public bool MoveState()
        {
            switch (Type)
            {
                case 1:
                    MapRole role = Map.Roles[ID];
                    return role.GetMoveState();
                case 2:
                    MapMonster monster = Map.Monsters[ID];
                    return monster.GetMoveState();
                default:
                    return false;
            }
        }
        public void SetMoveState(bool State)
        {
            switch (Type)
            {
                case 1:
                    MapRole role = Map.Roles[ID];
                    role.SetMoveState(State);
                    break;
                case 2:
                    MapMonster monster = Map.Monsters[ID];
                    monster.SetMoveState(State);
                    break;
                default:
                    break;
            }
        }

        public void SetTargetPos(double x, double y)
        {
            switch (Type)
            {
                case 1:
                    MapRole role = Map.Roles[ID];
                    role.SetTargetPos(x, y);
                    break;
                case 2:
                    MapMonster monster = Map.Monsters[ID];
                    monster.SetTargetPos(x, y);
                    break;
                default:
                    break;
            }
        }

        public void SetPath(List<Node> Path)
        {
            switch (Type)
            {
                // 玩家不需要设置寻路路点
                case 2:
                    MapMonster monster = Map.Monsters[ID];
                    monster.SetPath(Path);
                    break;
                default:
                    break;
            }
        }

        public (double, double) DoMoving(int upTime)
        {
            switch (Type)
            {
                case 1:
                    MapRole role = Map.Roles[ID];
                    var ret = role.Moving(upTime);
                    if(IsArrival())
                        DoStopMove();
                    return ret;
                case 2:
                    MapMonster monster = Map.Monsters[ID];
                    ret = monster.Moving(upTime);
                    if (IsArrival())
                    {
                        if (monster.Path.Count <= 0)
                            DoStopMove();
                        else
                            monster.MoveNext(); // 未到达终点
                    };
                    return ret;
                default:
                    return (0,0);
            }
        }

        // 判断到达
        public bool IsArrival()
        {
            // 允许位置出现一些误差
            switch (Type)
            {
                case 1:
                    MapRole role = Map.Roles[ID];
                    return Math.Round(role.PosX) == Math.Round(role.TargetX) && Math.Round(role.PosY) == Math.Round(role.TargetY);
                case 2:
                    MapMonster monster = Map.Monsters[ID];
                    return Math.Round(monster.PosX) == Math.Round(monster.TargetX) && Math.Round(monster.PosY) == Math.Round(monster.TargetY);
                default:
                    return false;
            }
        }

        public void DoStartMove(double x, double y)
        {
            try
            {
                if(MapPath.IsObstacle(Map.MapID, (int)x, (int)y))
                {
                    throw new Exception($"pos can't move x={x},y={y}");
                }
                if(!MapPath.IsValidCoordinate(Map.MapID, (int)x, (int)y))
                {
                    throw new Exception($"pos is unvalid x={x},y={y}");
                }
                // todo 判断能否移动 一些状态的判断

                MapPos Pos = GetPos();
                Node Start = new((int)Pos.x, (int)Pos.y);
                Node Goal = new ((int)x, (int)y);
                var Path = MapPath.FindPath(Map.MapID, Start, Goal) ?? throw new Exception($"can move to pos x={x},y={y}");
                SetMoveState(true);
                SetTargetPos(x, y);
                SetPath(Path);
                
            }catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void DoStopMove()
        {
            switch (Type)
            {
                case 1:
                    MapRole role = Map.Roles[ID];
                    role.StopMove();
                    break;
                case 2:
                    MapMonster monster = Map.Monsters[ID];
                    monster.StopMove();
                    break;
                default:
                    break;
            }
        }

        // 添加buff 
        // durTime buff持续时间
        // value buff效果值
        public void AddBuff(int buffID)
        {
            var config = BuffReader.GetConfig(buffID);
            if (config == null)
                return;
            AddBuff(buffID, config.DurTime, config.Value);
        }
        public void AddBuff(int buffID, int durTime)
        {
            var config = BuffReader.GetConfig(buffID);
            if (config == null)
                return;
            AddBuff(buffID, durTime, config.Value);
        }
        public void AddBuff(int buffID, int durTime, int value)
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

        public void DelBuff(int BuffID)
        {
            Buffs.Remove(BuffID);
            // todo 可能会有些效果要触发
        }

        // 身上是否有指定类型的buff(比如判断无敌)
        public bool HasBuff(int BuffType)
        {
            foreach(MapBuff buff in Buffs.Values)
            {
                var config = BuffReader.GetConfig(buff.BuffID);
                if (config == null)
                    continue;
                if(config.Type == BuffType)
                    return true;
            }
            return false;
        }

        public void LoopBuff(long Now2)
        {
            List<int> removeList = new();
            foreach(var buff in Buffs.Values)
            {
                if(Now2 >= buff.EndTime)
                    removeList.Add(buff.BuffID);
                // todo 或许会有一些效果
            }
            foreach(var ID in removeList)
                DelBuff(ID);
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
                long Now2 = Time.Now2();
                if (Now2 < config.CD + Skill.UseTime)
                    throw new Exception($"skill in cd {SkillID}");

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
