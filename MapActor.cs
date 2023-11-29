using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class MapActor
    {
        public int Type { get; set; }   // ActorType  1:玩家 2:怪物
        public long ID { get; set; }
        public Prop Prop { get; set; } = new(); // 属性
        public Prop BaseProp { get; set; } = new(); // 刚进地图保存的属性
        public Dictionary<int, MapBuff> Buffs { get; set; } = new();
        public Dictionary<int, long> BuffProps { get; set; } = new();    // 用于保存buff修改的属性，方便buff删除时清除属性
        public Dictionary<int, ActorSkill> Skills { get; set; } = new();
        public Map Map;

        public MapActor(int Type, long ID, Prop Prop, Map map) 
        { 
            this.Type = Type;
            this.ID = ID;
            this.Prop = Prop;
            this.BaseProp = Prop;
            this.Map = map;
        }

        public bool IsAlive()
        {
            switch(Type)
            {
                case 1:
                    MapRole role = Map.Roles[ID];
                    return role.IsAlive();
                case 2:
                    MapMonster monster = Map.Monsters[ID];
                    return monster.IsAlive();
                default:
                    return false;
            }
        }

        public void SetActorProp(string key, object NewValue)
        {
            switch (Type)
            {
                case 1:
                    MapRole role = Map.Roles[ID];
                    role.SetProp(key, NewValue);
                    break;
                case 2:
                    MapMonster monster = Map.Monsters[ID];
                    monster.SetProp(key, NewValue);
                    break;
                default:
                    break;
            }
        }

        public int GetCamp()
        {
            switch (Type)
            {
                case 1:
                    MapRole role = Map.Roles[ID];
                    return role.Camp;
                case 2:
                    MapMonster monster = Map.Monsters[ID];
                    return monster.Camp;
                default:
                    return 0;
            }
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
                    if (MapMgr.show == "m")
                    {
                        Log.W($" monster {ID} moving ret {ret}, upTime: {upTime}, path:{monster.Path.Count}");
                    }
                    if (IsArrival())
                    {
                        if (MapMgr.show == "m")
                            Log.W($"monster {ID} arrival {monster.PosX}, {monster.PosY}");
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

        public bool DoStartMove(double x, double y)
        {
            try
            {
                if(MapPath.IsObstacle(Map.MapID, (int)x, (int)y))
                {
                    throw new Exception($"pos can't move x={x},y={y}");
                }
                if (!MapPath.IsValidCoordinate(Map.MapID, (int)x, (int)y))
                {
                    throw new Exception($"pos is unvalid x={x},y={y}");
                }
                // todo 判断能否移动 一些状态的判断
                MapPos Pos = GetPos();
                Node Start = new((int)Pos.x, (int)Pos.y);
                Node Goal = new ((int)x, (int)y);
                var Path = MapPath.FindPath(Map.MapID, Start, Goal) ?? throw new Exception($"can not move to pos {(Map.MapName,ID)} x={x},y={y}, curpos {(Start.X, Start.Y)}");
                SetMoveState(true);
                SetTargetPos(x, y);
                SetPath(Path);
                return true;
                
            }catch (Exception ex)
            {
                Log.E(ex.ToString());
                return false;
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

        // 来自技能的buff
        public void AddSkillBuff(List<int> buffList)
        {
            switch(buffList.Count)
            {
                case 1: 
                    int BuffID = buffList[0];
                    AddBuff(BuffID);
                    break;
                case 2: // 概率添加
                    BuffID = buffList[0];
                    int rate = buffList[1];
                    if(MapMgr.random.Next(10000) <= rate)
                        AddBuff(BuffID);
                    break;
                default:
                    Log.E($"unhandle buff param count {buffList.Count}");
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
            Log.R($"add buff ActorID {ID}, buff {buffID}, {config.Value}");
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
                DoBuffEffect(buff);
            }
            else
            {
                MapBuff oldbuff = Buffs[buffID];
                oldbuff.EndTime += durTime;
                MapBuff Buff2 = DoAddSameBuff(buffID, durTime, value, oldbuff);
                Buffs[buffID] = Buff2;
                DoBuffEffect(Buff2);
            }
           
        }

        public static MapBuff DoAddSameBuff(int BuffID, int durTime, int value, MapBuff Buff)
        {
            var config = BuffReader.GetConfig(BuffID);
            switch (config.AddType)
            {
                case 1: // 叠加buff时长
                    Buff.EndTime += durTime;
                    break;
                case 2: // 叠加buff值
                    Buff.Value += value;
                    break;
                case 3: // 取buff值最大值或最小值
                    Buff.Value = (Buff.Value <0 && value < 0) ? Math.Min(value, Buff.Value) : Math.Max(value, Buff.Value);
                    break;
                default:
                    Log.E($"unhandle buff addtype: {config.AddType} buffid: {BuffID}");
                    break;
            }
            return Buff;
        }

        public void DoBuffEffect(MapBuff buff)
        {
            var config = BuffReader.GetConfig(buff.BuffID);
            switch(config.EffectType)
            {
                case 1: // 属性改变
                    AddBuffChangeRateProp(buff, config);
                    break;
                case 2: // 效果类
                    break;
                default:
                    Log.E($"unhandle buff effect Type :{config.EffectType}, buffid:{buff.BuffID}");
                    break;
            }
        }
        public void AddBuffChangeRateProp(MapBuff buff, BuffConfig config)
        {
            if (config.Func == "")
                return;
            var OldValue = Common.GetFieldValue(Prop, config.Func) ?? 0;
            var BaseValue = Common.GetFieldValue(BaseProp, config.Func) ?? 0;
            object? NewValue;
            if (config.Func == "MaxHp" || config.Func == "HP")
                NewValue = AddProp(buff, (long)OldValue, (long)BaseValue);
            else
                NewValue = AddProp(buff, (int)OldValue, (int)BaseValue);
            if (NewValue == null)
                return;
            Common.SetFieldValue(Prop, config.Func, NewValue);
            //Log.R($"change rate prop {ID},oldvalue:{OldValue}, NewValue:{NewValue}");
            if (config.Func == "MaxHp" || config.Func == "Speed")
                SetActorProp(config.Func, NewValue);
        }

        public object? AddProp(MapBuff buff, long OldValue, long BaseValue)
        {
            BuffProps.TryGetValue(buff.BuffID, out long OldAdd);
            long NewAdd = (long)((long)BaseValue * ((double)buff.Value / 10000)); 
            if (OldAdd < 0 && buff.Value < 0)
            {
                if (NewAdd >= OldAdd)
                    return null;
            }
            else
            {
                if (NewAdd <= OldAdd)
                    return null;
            }
            long NewValue = Math.Max(0, OldValue + (NewAdd - OldAdd));
            BuffProps[buff.BuffID] = NewAdd;
            return NewValue;
        }
        public object? AddProp(MapBuff buff, int OldValue, int BaseValue)
        {
            BuffProps.TryGetValue(buff.BuffID, out long OldAdd);
            int NewAdd = (int)((int)BaseValue * ((int)buff.Value / 10000));
            if (OldAdd < 0 && buff.Value < 0)
            {
                if (NewAdd >= OldAdd)
                    return null;
            }
            else
            {
                if (NewAdd <= OldAdd)
                    return null;
            }
            int NewValue = Math.Max(0, OldValue + (NewAdd - (int)OldAdd));
            BuffProps[buff.BuffID] = NewAdd;
            return NewValue;
        }

        public void DelBuff(int BuffID)
        {
            Buffs.Remove(BuffID);
            if(BuffProps.TryGetValue(BuffID, out long Add))
            {
                BuffProps.Remove(BuffID);
                DelBuffChangeProp(BuffID, Add);
            }
            Log.R($"del buff ActorID {ID}, buff {BuffID}");
            // todo 可能会有些效果要触发
        }

        public void DelBuffChangeProp(int BuffID, long Add)
        {
            var config = BuffReader.GetConfig(BuffID);
            if (config.Func == "")
                return;
            var OldValue = Common.GetFieldValue(Prop, config.Func);
            OldValue ??= 0;
            object NewValue;
            if (config.Func == "MaxHp" || config.Func == "HP")
                NewValue = Math.Max(0, (long)OldValue - Add);
            else
                NewValue = Math.Max(0, (int)OldValue - Add);
            Common.SetFieldValue(Prop, config.Func, NewValue);
            if (config.Func == "MaxHp" || config.Func == "Speed")
                SetActorProp(config.Func, NewValue);

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
        public bool HasBuff(BuffType type)
        {
            return HasBuff((int)type);
        }

        public MapBuff? GetBuffByType(BuffType type)
        {
            foreach (MapBuff buff in Buffs.Values)
            {
                var config = BuffReader.GetConfig(buff.BuffID);
                if (config == null)
                    continue;
                if (config.Type == (int)type)
                    return buff;
            }
            return null;
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
        public long DoAddHP(int Add)
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
        public long DoDecHP(int Dec, int SrcType, long SrcActorID)
        {
            // 护盾承伤
            var buff = GetBuffByType(BuffType.shield);
            if (buff != null)
            {
                int decShield = Math.Min(Dec, buff.Value);
                Dec -= decShield;
                
                if(buff.Value - decShield > 0)
                    buff.Value -= decShield;
                else 
                    DelBuff(buff.BuffID);   // 护盾消失
            }
            long NewHp = 0;
            switch (Type)
            {
                case 1:
                    MapRole role = Map.Roles[ID];
                    NewHp = role.DecHp(Dec);
                    break;
                case 2:
                    MapMonster monster = Map.Monsters[ID];
                    NewHp = monster.DecHp(Dec, SrcType, SrcActorID);
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
                    throw new Exception($"skill in cd monsterID:{ID}, {SkillID}");

                Skill.UseTime = Now2;
                MapSkill SkillEntity = new(Type, ID, TargetMap, SkillID, pos);
                Map.AddSkillEntity(SkillEntity);
                if(config.SBuffs.Count > 0)
                    AddSkillBuff(config.SBuffs);
                Log.W($"actor ({Type},{ID}) use skill: {SkillID} enID:{SkillEntity.ID} pos:{(int)pos.x},{(int)pos.y}");
            }
            catch (Exception e)
            {
                Log.E(e.ToString());
            }
                    
        }
    }
}
