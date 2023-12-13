using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    /// <summary>
    /// 地图实例对象 
    /// </summary>
    internal class MapActor
    {
        public int Type { get; }   // ActorType  1:玩家 2:怪物
        public long ID { get; }
        private Prop Prop { get; set; } = new(); // 属性
        private Prop BaseProp { get; set; } = new(); // 刚进地图保存的属性
        private Dictionary<int, MapBuff> Buffs { get; set; } = new();
        private Dictionary<int, long> BuffProps { get; set; } = new();    // 用于保存buff修改的属性，方便buff删除时清除属性
        private Dictionary<int, ActorSkill> Skills { get; set; } = new();
        public Map Map { get; }

        public MapActor(int Type, long ID, Prop Prop, Map map) 
        { 
            this.Type = Type;
            this.ID = ID;
            this.Prop = Prop;
            this.BaseProp = Prop;
            this.Map = map;
        }

        /// <summary>
        /// 返回actor对象的属性
        /// </summary>
        /// <returns></returns>
        public Prop GetProp() 
        { 
            return Prop; 
        }

        /// <summary>
        /// 返回actor对象的基础属性
        /// </summary>
        /// <returns></returns>
        public Prop GetBaseProp()
        { 
            return BaseProp; 
        }

        /// <summary>
        /// 返回actor对象的属性
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, MapBuff> GetBuffs()
        {
            return Buffs;
        }

        /// <summary>
        /// 返回actor对象的技能
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, ActorSkill> GetSkills()
        {
            return Skills;
        }

        /// <summary>
        /// 设置actor对象的技能
        /// </summary>
        /// <param name="skills">技能</param>
        public void SetSkills(Dictionary<int, ActorSkill> skills)
        {
            Skills = skills;
        }

        /// <summary>
        /// 实例是否存活
        /// </summary>
        /// <returns></returns>
        public bool IsAlive()
        {
            switch (Type)
            {
                case 1:
                    MapRole role = Map.GetMapRole(ID);
                    return role.IsAlive();
                case 2:
                    MapMonster monster = Map.GetMapMonster(ID);
                    return monster.IsAlive();
                default:
                    return false;
            }
        }

        /// <summary>
        /// 设置实例对象某一个属性值
        /// </summary>
        /// <param name="key">属性key</param>
        /// <param name="newValue">属性值</param>
        public void SetActorProp(string key, object newValue)
        {
            switch (Type)
            {
                case 1:
                    MapRole role = Map.GetMapRole(ID);
                    role.SetProp(key, newValue);
                    break;
                case 2:
                    MapMonster monster = Map.GetMapMonster(ID);
                    monster.SetProp(key, newValue);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 返回实例的阵营
        /// </summary>
        /// <returns>阵营ID</returns>
        public int GetCamp()
        {
            switch (Type)
            {
                case 1:
                    MapRole role = Map.GetMapRole(ID);
                    return role.GetCamp();
                case 2:
                    MapMonster monster = Map.GetMapMonster(ID);
                    return monster.GetCamp();
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 返回当前实例的坐标
        /// </summary>
        /// <returns>坐标</returns>
        public MapPos GetPos()
        {
            switch (Type)
            {
                case 1:
                    MapRole role = Map.GetMapRole(ID);
                    return role.GetPos();
                case 2:
                    MapMonster monster = Map.GetMapMonster(ID);
                    return monster.GetPos();
                default:
                    return new MapPos(0,0);
            }
        }

        /// <summary>
        /// 返回实例的移动状态
        /// </summary>
        /// <returns>移动状态</returns>
        public bool MoveState()
        {
            switch (Type)
            {
                case 1:
                    MapRole role = Map.GetMapRole(ID);
                    return role.GetMoveState();
                case 2:
                    MapMonster monster = Map.GetMapMonster(ID);
                    return monster.GetMoveState();
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// 设置实例的移动状态
        /// </summary>
        /// <param name="state">移动状态</param>
        public void SetMoveState(bool state)
        {
            switch (Type)
            {
                case 1:
                    MapRole role = Map.GetMapRole(ID);
                    role.SetMoveState(state);
                    break;
                case 2:
                    MapMonster monster = Map.GetMapMonster(ID);
                    monster.SetMoveState(state);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 设置实例对象当前坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetTargetPos(double x, double y)
        {
            switch (Type)
            {
                case 1:
                    MapRole role = Map.GetMapRole(ID);
                    role.SetTargetPos(x, y);
                    break;
                case 2:
                    MapMonster monster = Map.GetMapMonster(ID);
                    monster.SetTargetPos(x, y);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 设置实例的寻路路径
        /// </summary>
        /// <param name="path"></param>
        public void SetPath(List<Node> path)
        {
            switch (Type)
            {
                // 玩家不需要设置寻路路点
                case 2:
                    MapMonster monster = Map.GetMapMonster(ID);
                    monster.SetPath(path);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 移动处理
        /// </summary>
        /// <param name="nowMillSec">当前时间毫秒级时间戳</param>
        /// <param name="upTime">两次更新的时间差（毫秒）</param>
        /// <returns>新的坐标点</returns>
        public (double, double) DoMoving(long nowMillSec, int upTime)
        {
            try
            {
                switch (Type)
                {
                    case 1:
                        MapRole role = Map.GetMapRole(ID);
                        var ret = role.Moving(upTime);
                        if (IsArrival())
                            DoStopMove();
                        return ret;
                    case 2:
                        MapMonster monster = Map.GetMapMonster(ID);
                        ret = monster.Moving(nowMillSec, upTime);
                        //if (MapMgr.ShowData == "m")
                        //    Log.W($" monster {ID} moving ret {ret}, upTime: {upTime}, path:{monster.GetPath().Count}");
                        if (IsArrival())
                        {
                            //if (MapMgr.ShowData == "m")
                            //    Log.W($"monster {ID} arrival {monster.PosX}, {monster.PosY}");
                            if (monster.GetPath().Count <= 0)
                                DoStopMove();
                            else
                                monster.MoveNext(); // 未到达终点
                        };
                        return ret;
                    default:
                        return (0, 0);
                }
            }catch(Exception e)
            {
                Log.E(e.ToString());
                return (0, 0);
            }
        }

        /// <summary>
        /// 判断到达当前目标点
        /// </summary>
        /// <returns></returns>
        public bool IsArrival()
        {
            // 允许位置出现一些误差
            switch (Type)
            {
                case 1:
                    MapRole role = Map.GetMapRole(ID);
                    return Math.Round(role.PosX) == Math.Round(role.TargetX) && Math.Round(role.PosY) == Math.Round(role.TargetY);
                case 2:
                    MapMonster monster = Map.GetMapMonster(ID);
                    return Math.Round(monster.PosX) == Math.Round(monster.TargetX) && Math.Round(monster.PosY) == Math.Round(monster.TargetY);
                default:
                    return false;
            }
        }

        /// <summary>
        /// 实例开始移动
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool DoStartMove(double x, double y)
        {
            try
            {
                switch (Type)
                {
                    case 1:
                        MapRole role = Map.GetMapRole(ID);
                        return role.DoStartMove(x, y);
                    case 2:
                        MapMonster monster = Map.GetMapMonster(ID);
                        return monster.DoStartMove(x, y);
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Log.E(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 对象停止移动
        /// </summary>
        public void DoStopMove()
        {
            switch (Type)
            {
                case 1:
                    MapRole role = Map.GetMapRole(ID);
                    role.StopMove();
                    break;
                case 2:
                    MapMonster monster = Map.GetMapMonster(ID);
                    monster.StopMove();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 来自技能的buff
        /// </summary>
        /// <param name="buffList"> buff id List</param>
        public void AddSkillBuff(List<int> buffList)
        {
            switch(buffList.Count)
            {
                case 1: 
                    int buffID = buffList[0];
                    AddBuff(buffID);
                    break;
                case 2: // 概率添加
                    buffID = buffList[0];
                    int rate = buffList[1];
                    if(MapMgr.random.Next(10000) <= rate)
                        AddBuff(buffID);
                    break;
                default:
                    Log.E($"unhandle buff param count {buffList.Count}");
                    break;
            }
        }

        /// <summary>
        /// 添加buff
        /// </summary>
        /// <param name="buffID">buff 配置id</param>
        public void AddBuff(int buffID)
        {
            var config = BuffReader.GetConfig(buffID);
            if (config == null)
                return;
            Log.R($"add buff ActorID {ID}, buff {buffID}, {config.Value}");
            AddBuff(buffID, config.DurTime, config.Value);
        }
        /// <summary>
        /// 添加buff
        /// </summary>
        /// <param name="buffID">buff 配置id</param>
        /// <param name="durTime">buff持续时间</param>
        public void AddBuff(int buffID, int durTime)
        {
            var config = BuffReader.GetConfig(buffID);
            if (config == null)
                return;
            AddBuff(buffID, durTime, config.Value);
        }
        /// <summary>
        /// 添加buff
        /// </summary>
        /// <param name="buffID">buff 配置id</param>
        /// <param name="durTime">buff持续时间</param>
        /// <param name="value">buff效果值</param>
        public void AddBuff(int buffID, int durTime, int value)
        {
            long nowMillSec = Time.NowMillSec();

            if (!Buffs.ContainsKey(buffID))
            {
                MapBuff buff = new(buffID, nowMillSec + durTime, value);
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

        /// <summary>
        /// 添加buff时，重复buff的处理
        /// </summary>
        /// <param name="buffID">buff id </param>
        /// <param name="durTime">buff持续时间</param>
        /// <param name="value">buff效果值</param>
        /// <param name="buff">buff实例对象</param>
        /// <returns></returns>
        public static MapBuff DoAddSameBuff(int buffID, int durTime, int value, MapBuff buff)
        {
            var config = BuffReader.GetConfig(buffID);
            switch (config.AddType)
            {
                case 1: // 叠加buff时长
                    buff.EndTime += durTime;
                    break;
                case 2: // 叠加buff值
                    buff.Value += value;
                    break;
                case 3: // 取buff值最大值或最小值
                    buff.Value = (buff.Value <0 && value < 0) ? Math.Min(value, buff.Value) : Math.Max(value, buff.Value);
                    break;
                default:
                    Log.E($"unhandle buff addtype: {config.AddType} buffid: {buffID}");
                    break;
            }
            return buff;
        }

        /// <summary>
        /// buff效果实现
        /// </summary>
        /// <param name="buff">buff实例</param>
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

        /// <summary>
        /// buff改变实例对象的百分比属性
        /// </summary>
        /// <param name="buff">buff实例 </param>
        /// <param name="config">buff配置 </param>
        public void AddBuffChangeRateProp(MapBuff buff, BuffConfig config)
        {
            if (config.Func == "")
                return;
            var oldValue = Common.GetFieldValue(Prop, config.Func) ?? 0;
            var baseValue = Common.GetFieldValue(BaseProp, config.Func) ?? 0;
            object? newValue;
            if (config.Func == "MaxHp" || config.Func == "HP")
                newValue = AddProp(buff, (long)oldValue, (long)baseValue);
            else
                newValue = AddProp(buff, (int)oldValue, (int)baseValue);
            if (newValue == null)
                return;
            Common.SetFieldValue(Prop, config.Func, newValue);
            //Log.R($"change rate prop {ID},oldvalue:{OldValue}, NewValue:{NewValue}");
            if (config.Func == "MaxHp" || config.Func == "Speed")
                SetActorProp(config.Func, newValue);
        }

        /// <summary>
        /// 属性改变
        /// </summary>
        /// <param name="buff">buff实例 </param>
        /// <param name="oldValue">老的值</param>
        /// <param name="baseValue">玩家属性基础值</param>
        /// <returns></returns>
        public object? AddProp(MapBuff buff, long oldValue, long baseValue)
        {
            BuffProps.TryGetValue(buff.BuffID, out long oldAdd);
            long newAdd = (long)((long)baseValue * ((double)buff.Value / 10000)); 
            if (oldAdd < 0 && buff.Value < 0)
            {
                if (newAdd >= oldAdd)
                    return null;
            }
            else
            {
                if (newAdd <= oldAdd)
                    return null;
            }
            long newValue = Math.Max(0, oldValue + (newAdd - oldAdd));
            BuffProps[buff.BuffID] = newAdd;
            return newValue;
        }

        /// <summary>
        /// 属性改变
        /// </summary>
        /// <param name="buff">buff实例 </param>
        /// <param name="oldValue">老的值</param>
        /// <param name="baseValue">玩家属性基础值</param>
        /// <returns></returns>
        public object? AddProp(MapBuff buff, int oldValue, int baseValue)
        {
            BuffProps.TryGetValue(buff.BuffID, out long oldAdd);
            int newAdd = (int)((int)baseValue * ((int)buff.Value / 10000));
            if (oldAdd < 0 && buff.Value < 0)
            {
                if (newAdd >= oldAdd)
                    return null;
            }
            else
            {
                if (newAdd <= oldAdd)
                    return null;
            }
            int newValue = Math.Max(0, oldValue + (newAdd - (int)oldAdd));
            BuffProps[buff.BuffID] = newAdd;
            return newValue;
        }

        /// <summary>
        /// buff删除
        /// </summary>
        /// <param name="buffID">buff id</param>
        public void DelBuff(int buffID)
        {
            Buffs.Remove(buffID);
            if(BuffProps.TryGetValue(buffID, out long add))
            {
                BuffProps.Remove(buffID);
                DelBuffChangeProp(buffID, add);
            }
            Log.R($"del buff ActorID {ID}, buff {buffID}");
            // todo 可能会有些效果要触发
        }

        /// <summary>
        /// 删除buff改变属性
        /// </summary>
        /// <param name="buffID">buff id </param>
        /// <param name="add">buff增加的值</param>
        public void DelBuffChangeProp(int buffID, long add)
        {
            var config = BuffReader.GetConfig(buffID);
            if (config.Func == "")
                return;
            var oldValue = Common.GetFieldValue(Prop, config.Func);
            oldValue ??= 0;
            object newValue;
            if (config.Func == "MaxHp" || config.Func == "HP")
                newValue = Math.Max(0, (long)oldValue - add);
            else
                newValue = Math.Max(0, (int)oldValue - add);
            Common.SetFieldValue(Prop, config.Func, newValue);
            if (config.Func == "MaxHp" || config.Func == "Speed")
                SetActorProp(config.Func, newValue);

        }

        /// <summary>
        /// 返回身上是否有指定类型的buff(比如判断无敌)
        /// </summary>
        /// <param name="buffType">buff类型</param>
        /// <returns></returns>
        public bool HasBuff(int buffType)
        {
            foreach(MapBuff buff in Buffs.Values)
            {
                var config = BuffReader.GetConfig(buff.BuffID);
                if (config == null)
                    continue;
                if(config.Type == buffType)
                    return true;
            }
            return false;
        }
        public bool HasBuff(BuffType type)
        {
            return HasBuff((int)type);
        }

        /// <summary>
        /// 返回一个指定buff类型的buff
        /// </summary>
        /// <param name="type">buff类型</param>
        /// <returns></returns>
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

        /// <summary>
        /// buff轮询
        /// </summary>
        /// <param name="nowMillSec">毫秒级时间戳</param>
        public void LoopBuff(long nowMillSec)
        {
            List<int> removeList = new();
            foreach(var buff in Buffs.Values)
            {
                if(nowMillSec >= buff.EndTime)
                    removeList.Add(buff.BuffID);
                // todo 或许会有一些效果
            }
            foreach(var ID in removeList)
                DelBuff(ID);
        }

        /// <summary>
        /// 加血
        /// </summary>
        /// <param name="add">增加的值</param>
        /// <returns></returns>
        public long DoAddHP(int add)
        {
            long newHp = 0;
            switch(Type)
            {
                case 1:
                    MapRole role = Map.GetMapRole(ID);
                    newHp = role.AddHp(add);
                    break;
                case 2:
                    MapMonster monster = Map.GetMapMonster(ID);
                    newHp = monster.AddHp(add);
                    break;
                default: 
                    break;
            }
            return newHp;
        }
        /// <summary>
        /// 扣血
        /// </summary>
        /// <param name="dec">扣血值 </param>
        /// <param name="srcType">来源实例类型</param>
        /// <param name="srcActorID">来源实例ID</param>
        /// <returns></returns>
        public long DoDecHP(int dec, int srcType, long srcActorID)
        {
            // 护盾承伤
            var buff = GetBuffByType(BuffType.shield);
            if (buff != null)
            {
                int decShield = Math.Min(dec, buff.Value);
                dec -= decShield;
                
                if(buff.Value - decShield > 0)
                    buff.Value -= decShield;
                else 
                    DelBuff(buff.BuffID);   // 护盾消失
            }
            long newHp = 0;
            switch (Type)
            {
                case 1:
                    MapRole role = Map.GetMapRole(ID);
                    newHp = role.DecHp(dec);
                    break;
                case 2:
                    MapMonster monster = Map.GetMapMonster(ID);
                    newHp = monster.DecHp(dec, srcType, srcActorID);
                    break;
                default:
                    break;
            }
            return newHp;
        }

        /// <summary>
        /// 使用技能
        /// </summary>
        /// <param name="skillID">技能ID</param>
        /// <param name="pos">技能目标位置实例</param>
        /// <param name="targetMap">目标实例key的List</param>
        public void DoUseSkill(int skillID, MapPos pos, List<(int,long)> targetMap)
        {
            try
            {
                if (!Skills.ContainsKey(skillID))
                    throw new Exception($"unequip skill {skillID}");
                ActorSkill skill = Skills[skillID];
                foreach (var Key in targetMap)
                {
                    if(!MapCommon.ActorInMap(Map, Key))
                        targetMap.Remove(Key);
                }
                SkillConfig config = SkillReader.GetConfig(skillID) ?? throw new Exception($"can not find skill config {skillID}");
                long nowMillSec = Time.NowMillSec();
                if (nowMillSec < config.CD + skill.UseTime)
                    throw new Exception($"skill in cd monsterID:{ID}, {skillID}");

                skill.UseTime = nowMillSec;
                MapSkill skillEntity = new(Type, ID, targetMap, skillID, pos);
                Map.AddSkillEntity(skillEntity);
                if(config.SBuffs.Count > 0)
                    AddSkillBuff(config.SBuffs);
                Log.W($"{Map.MapName} actor ({Type},{ID}) use skill: {skillID} enID:{skillEntity.ID} pos:{(int)pos.x},{(int)pos.y}");
            }
            catch (Exception e)
            {
                Log.E(e.ToString());
            }
                    
        }
    }
}
