using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class Map
    {
        public int ID { get; set; } = 0;    // id
        public int MapID { get; set; } = 0; // 地图ID
        public string MapName { get; set; } = ""; // 地图名（常用来索引）
        public int MapType { get; set; } = 0; // 地图类型
        public long CreateTime { get; set; } = 0; // 地图创建时间

        private long LastTickTime { get; set; } = 0;

        public Thread thread;   // 地图的线程
        public MapAoi Aoi;  // 地图内Aoi对象

        public int RoleNum { get; set; } = 0; // 地图内玩家数量
        public Dictionary<long, MapRole> Roles { get; set; } = new(); // 保存玩家实例
        public int MonsterNum { get; set; } = 0;    // 地图内怪物数量
        public Dictionary<long, MapMonster> Monsters { get; set; } = new(); 
        public Dictionary<(int,long), MapActor> ActorMap { get; set; } = new();   // 保存地图Actor实例
        //public List<(int, long)> BuffRoleIDList { get; set; } = new(); // 保存地图内有buff的实例对象
        public List<MapSkill> SkillList { get; set; } = new();   // 保存地图内的技能实例
        public int LoopTick200 {  get; set; } = 1;  // 200ms轮询标记
        public int LoopTick3000 { get; set; } = 1;

        public static Dictionary<long, (double, double)> lastPos = new();

        public Map(int id, int mapID, string mapName) 
        {
            this.ID = id;
            this.MapID = mapID;
            this.MapName = mapName;
            CreateTime = Time.Now();
            MapType = 1;

            // 创建线程
            Thread thread = new (InitMap);
            thread.Start();
            this.thread = thread;

            var config = MapReader.GetConfig(mapID);
            // 初始化AOI
            this.Aoi = new(config.Width, config.Height);
        }

        public void InitMap()
        {
            LastTickTime = Time.Now2();
            // 增加一个轮询
            Timer cTimer = new (MapLoop, null, 0, 100);

        }

        private void MapLoop(object? state)
        {
            try
            {
                long Now2 = Time.Now2();
                // 100ms 一次轮询
                LoopMoving(Now2);
                LoopSkills(Now2);
                LoopBuff(Now2);

                //  200ms 轮询
                if (LoopTick200 == 2)
                {
                    LoopTick200 = 1;
                    LoopTick3000 += 1;
                    LoopMonsterAI(Now2);    
                }
                else
                {
                    LoopTick200 += 1;
                    // todo 200ms 轮询的可以放一些在这里处理，平衡一下压力
                }

                // 3s 轮询 这里用来输入一些数据
                if (LoopTick3000 >= 15)
                {
                    LoopTick3000 = 1;
                    ShowDict();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // 玩家进入地图
        public void DoRoleEnter(MapRole MapRole, MapActor MapActor)
        {

            var config = MapReader.GetConfig(MapID);
            Random r = new();
            int PosX = r.Next(Math.Max(config.BornX-3, 0), Math.Min(config.BornX+3, config.Width));
            int PosY = r.Next(Math.Max(config.BornY-3, 0), Math.Min(config.BornY+3, config.Height));
            MapRole.PosX = PosX;
            MapRole.PosY = PosY;
            Roles[MapRole.ID] = MapRole;
            RoleNum ++;
            ActorMap[(1, MapActor.ID)] = MapActor;
            // todo
        }

        public void DoRoleExit(long RoleID)
        {
            Roles.Remove(RoleID);
            ActorMap.Remove((1, RoleID));
            RoleNum --;
            // todo
        }

        public void DoMonsterEnter(MapMonster monster, MapActor MapActor)
        {
            Monsters[monster.ID] = monster;
            MonsterNum ++;
            ActorMap[(2, MapActor.ID)] = MapActor;
        }
        public void DoMonsterExit(long MonsterID)
        {
            Monsters.Remove(MonsterID);
            ActorMap.Remove((2,MonsterID));
            MonsterNum --;
        }

        public long DoAddHP(int ActorType, long ActorID, int Add)
        {
            long NewHp = 0;
            var Key = (ActorType, ActorID);
            if (ActorMap.ContainsKey(Key))
            {
                MapActor Actor = ActorMap[Key];
                NewHp = Actor.DoAddHP(Add);
            }
            return NewHp;
        }

        public long DoDecHP(int ActorType, long ActorID, int Dec, int SrcType, long SrcActorID)
        {
            long NewHp = 0;
            var Key = (ActorType, ActorID);
            if (ActorMap.ContainsKey(Key))
            {
                MapActor Actor = ActorMap[Key];
                NewHp = Actor.DoDecHP(Dec, SrcType, SrcActorID);
            }
            return NewHp;
        }

        // 轮询检查移动
        public void LoopMoving(long Now2)
        {
            try
            {
                LoopMoving2(Now2);
            }catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void LoopMoving2(long Now2)
        {
            int upTime = (int)((int)Now2 - LastTickTime);
            foreach (var Dic in ActorMap)
            {
                MapActor Actor = Dic.Value;
                if (Actor.MoveState())
                {
                    MapPos Pos = Actor.GetPos();
                    (double NewX, double NewY) = Actor.DoMoving(upTime);
                    Aoi.DoMove(Actor.Type, Actor.ID, Pos.x, Pos.y, NewX, NewY);
                }
            }
        }

        public void AddSkillEntity(MapSkill Skill)
        {
            SkillList.Add(Skill);
            // 视野同步
        }

        // 技能轮询
        public void LoopSkills(long Now2)
        {
            try
            { 
                LoopSkills2(Now2);
            }catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void LoopSkills2(long Now2)
        {
            List<MapSkill> preDel = new();
            foreach (var Skill in SkillList)
            {
                (int, long) SrcKey = (Skill.ActorType, Skill.ActorID);
                if (!ActorMap.ContainsKey(SrcKey))
                {
                    // 技能拥有者不在地图
                    preDel.Add(Skill);
                    continue;
                }
                MapActor SrcActor = ActorMap[SrcKey];

                SkillConfig config = Skill.SkillConfig;
                int wave = Skill.CurWave;
                if (wave + 1 > config.Waves.Count)   // 技能结束
                {
                    preDel.Add(Skill);
                    continue;
                }
                int interval = config.Waves[wave];
                // 是否达到触发下一波次伤害时间
                if (Skill.SkillTime + interval >= Now2) 
                    continue;

                Dictionary<(int, long), MapActor> HurtMap = MapCommon.FilterHurt(this, Skill, SrcActor);
                List<MapEffect> EffectMaps = new();
                //Console.WriteLine($"skill loop hurtmap {HurtMap.Count}");
                foreach(var TarActor in HurtMap.Values)
                {
                    List<MapEffect> Effects = MapFight.DoFight(SrcActor, TarActor, config);
                    if(config.TBuffs.Count > 0)
                    {
                        // 可能会对目标添加buff
                        foreach (var TarEffect in EffectMaps)
                        {
                            var TActor = MapCommon.GetActor(this, (TarEffect.ActorType, TarEffect.ActorID));
                            TActor.AddBuff(config.TBuffs);
                        }
                    }
                    EffectMaps.AddRange(Effects);
                }
                Skill.CurWave++;
                Skill.SkillTime = Now2;

                // todo 同步效果 sync EffectMaps
            }
            foreach(var skill in preDel)
                _ = SkillList.Remove(skill);
        }


        public void LoopMonsterAI(long Now2)
        {
            try
            {
                LoopMonsterAI2(Now2);
            }catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void LoopMonsterAI2(long Now2)
        {
            List<long> removeKeys = new();
            foreach(var monster in Monsters.Values)
            {
                var Actor = MapCommon.GetActor(this, (2, monster.ID));
                if (Actor == null)
                {
                    removeKeys.Add(monster.ID);
                    continue;
                }

                if (!Actor.IsAlive())
                    continue;

                var doingList = monster.doing;
                if (doingList.Count <= 0) // 添加一个巡逻节点
                {
                    MoveTo moveTo = MonsterAI.Patrol(monster);
                    DoMoveTo(Actor, moveTo, ref doingList);
                    continue;
                }
                var doing = doingList[0];
                if (doing is Idle)  // 添加一个巡逻节点
                {
                    MoveTo moveTo = MonsterAI.Patrol(monster);
                    DoMoveTo(Actor, moveTo, ref doingList);
                    continue;
                }
                else if (doing is MoveTo to)  // 继续移动
                {
                    (int t, long i) = monster.TarKey;
                    bool hasTarget = t > 0 && i > 0;
                    if (hasTarget)
                    {
                        var TActor = MapCommon.GetActor(this, monster.TarKey);
                        if (TActor == null)  // 追击目标不存在了
                        {
                            doingList.Remove(doing);
                            continue;
                        }
                        MapPos pos = TActor.GetPos();
                        // 超出追击距离或目标死亡
                        if (!MapTool.CheckDistance(monster.PosX, monster.PosY, pos.x, pos.y, monster.PursueDistance) || !TActor.IsAlive())
                        {
                            doingList.Remove(doing);
                            MoveTo moveto = MonsterAI.ReturnBorn(monster);  // 回出生点
                            DoMoveTo(Actor, moveto, ref doingList);
                        }
                        // 位置可能更新
                        if (Math.Round(pos.x) != Math.Round(to.X) && Math.Round(pos.y) != Math.Round(to.Y))
                        {
                            to.X = pos.x; 
                            to.Y = pos.y;
                            monster.TargetX = pos.x;
                            monster.TargetY = pos.y;
                        }

                        //if (Math.Round(monster.PosX) == Math.Round(to.X) && Math.Round(monster.PosY) == Math.Round(to.Y))
                        //{
                        //    doingList.Remove(doing);
                        //    doingList.Insert(0, new Fight());
                        //}
                    }
                    else if (Actor.IsArrival())
                        doingList.Remove(doing);
                    continue;
                }
                else if(doing is Patrol2)
                {
                    var Next = MonsterAI.Patrol2(this, monster);
                    if(Next is MoveTo moveto)
                        DoMoveTo(Actor, moveto, ref doingList);
                    else
                        doingList.Insert(0, Next);
                    continue;
                }
                else if(doing is Pursue pursue)    // 追击
                {
                    (int, long) key = pursue.Key;
                    var TActor = MapCommon.GetActor(this, key);
                    if (TActor == null)  // 追击目标不存在了
                    {
                        doingList.Remove(doing);
                        continue;
                    }

                    MapPos pos = TActor.GetPos();
                    if(TActor.IsAlive() && MapTool.CheckDistance(monster.PosX, monster.PosY, pos.x, pos.y, monster.AttackDistance))
                    {
                        Fight fight = new ();
                        doingList.Insert(0, fight);
                    }
                    else if (TActor.IsAlive() && MapTool.CheckDistance(monster.PosX, monster.PosY, pos.x, pos.y, monster.PursueDistance))
                    {
                        MoveTo moveto = new(pos);
                        DoMoveTo(Actor, moveto, ref doingList);
                    }
                    else
                    { 
                        // 超出追击距离或目标已死亡
                        doingList.Remove(doing);
                        MoveTo moveto = MonsterAI.ReturnBorn(monster);
                        DoMoveTo(Actor, moveto, ref doingList);
                    }
                    continue;
                }else if(doing is Fight)
                {
                    var doNext = MonsterAI.Fight(this, monster);
                    switch (doNext)
                    {
                        case 0:
                            removeKeys.Add(monster.ID);
                            continue;
                        case Pursue:
                            doingList.Insert(0, doNext);
                            continue;
                        case int SkillID:
                            if (SkillID > 0)
                            {
                                var TActor = MapCommon.GetActor(this, monster.TarKey); // Fight中已过滤掉目标死亡或不存在的情况
                                Actor.DoUseSkill(SkillID, TActor.GetPos(), new List<(int, long)> { monster.TarKey });
                            }
                            break;
                        default:
                            continue;
                    }
                }
            }
            foreach(long ID in removeKeys)
                Monsters.Remove(ID);
        }

        public void DoMoveTo(MapActor Actor, MoveTo moveto, ref ArrayList doingList)
        {
            if (Actor.DoStartMove(moveto.X, moveto.Y))
                doingList.Insert(0, moveto);
        }

        public void LoopBuff(long Now2)
        {
            try
            {
                foreach(var actor in ActorMap.Values)
                {
                    if(actor.Buffs.Count > 0)
                        actor.LoopBuff(Now2);
                }
            }catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void ShowDict()
        {
            // Console.WriteLine($"monster number : {MonsterNum}");
            int n = 1;
            foreach(var monster in Monsters.Values)
            {
                if(!monster.IsAlive())
                {
                    Console.WriteLine($"mosnter ({n}) has dead");
                    continue;
                }
                Console.WriteLine($"monster ({n++}) :");
                Console.WriteLine($"     ID:{monster.ID}, monsterID:{monster.MonsterID}; {monster.PosX},{monster.PosY}; curHp:{(float)monster.HP / monster.MaxHp * 100}%,");
                //Console.WriteLine($"doingList {monster.doing.Count} curDoing:{monster.doing[0]}");
                //var doing = monster.doing[0];
                //if (doing is MoveTo to)
                //{
                //    Console.WriteLine($"move to targetX:{monster.TargetX}, targetY:{monster.TargetY}");
                //    to.Show();
                //}
                (double, double) p = (0,0);
                lastPos.TryGetValue(monster.MonsterID, out p);
                (double X, double Y) = p;
                bool print = monster.doing[0] is MoveTo;
                if (X == monster.PosX && Y == monster.PosY && print)
                {
                    foreach (var doing in monster.doing)
                        Console.WriteLine($"monster state {doing}, {monster.GetMoveState()}, {monster.TargetX}, {monster.TargetY}");
                }
                lastPos[monster.MonsterID] = (monster.PosX, monster.PosY);
            }
        }
    }
}
