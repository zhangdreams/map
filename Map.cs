using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        //public int ID { get; set; } = 0;    // id
        public int MapID { get; } = 0; // 地图ID
        public string MapName { get; } = ""; // 地图名（常用来索引）
        public int Line { get; } = 0;  // 分线
        public int MapType { get; } = 0; // 地图类型
        public long CreateTime { get; } = 0; // 地图创建时间

        private long LastTickTime { get; set; } = 0;

        public Thread Thread { get; }   // 地图的线程
        public MapAoi Aoi { get; }  // 地图内Aoi对象

        private int RoleNum { get; set; } = 0; // 地图内玩家数量
        private Dictionary<long, MapRole> Roles { get; set; } = new(); // 保存玩家实例
        private int MonsterNum { get; set; } = 0;    // 地图内怪物数量
        private Dictionary<long, MapMonster> Monsters { get; set; } = new();
        private Dictionary<(int,long), MapActor> ActorMap { get; set; } = new();   // 保存地图Actor实例
        //private Dictionary<(int, long), (int, int)> ActorPosMap { get; set; } = new();
        //public List<(int, long)> BuffRoleIDList { get; set; } = new(); // 保存地图内有buff的实例对象
        private List<MapSkill> SkillList { get; set; } = new();   // 保存地图内的技能实例
        private long MaxMonsterID { get; set; } = 1;   // 地图内怪物的最大实例ID
        private int MaxCamp {  get; set; } = 0;  // 地图内的最大阵营索引
        private Timer Timer;
        private int LoopTick200 {  get; set; } = 1;  // 200ms轮询标记
        private int LoopTick5000 { get; set; } = 1;

        private Dictionary<long, (double, double)> lastPos = new(); 
        private List<string> MessageQueue = new();

        public Map(int mapID, string mapName, int line) 
        {
            //this.ID = id;
            this.MapID = mapID;
            this.MapName = mapName;
            this.Line = line;
            CreateTime = Time.NowSec();
            MapType = 1;

            // 创建线程
            Thread thread = new(InitMap);
            thread.Start();
            Thread = thread;

            var config = MapReader.GetConfig(mapID);
            // 初始化AOI
            this.Aoi = new(config.Width, config.Height);
            Log.P($"create new map {mapID}, mapname:{mapName}");
        }

        /// <summary>
        ///  初始化地图、设置一个轮询的timer
        /// </summary>
        public void InitMap()
        {
            LastTickTime = Time.NowMillSec();
            // 增加一个轮询
            Timer cTimer = new (MapLoop, null, 500, 100);
            Timer = cTimer;
        }

        /// <summary>
        /// 地图轮询回调
        /// </summary>
        /// <param name="state"></param>
        private void MapLoop(object? state)
        {
            try
            {
                long nowMillSec = Time.NowMillSec();
                // 100ms 一次轮询
                LoopQueue();
                LoopMoving(nowMillSec);
                LoopSkills(nowMillSec);
                LoopBuff(nowMillSec);
                LoopMonsterAI(nowMillSec);

                //  200ms 轮询
                if (LoopTick200 == 2)
                {
                    LoopTick200 = 1;
                    LoopTick5000 += 1;
                    LoopMonsterDead(nowMillSec);
                }
                else
                {
                    LoopTick200 += 1;
                    // todo 200ms 轮询的可以放一些在这里处理，平衡一下压力
                }

                // 3s 轮询 这里用来输出一些数据
                if (LoopTick5000 >= 25)
                {
                    LoopTick5000 = 1;
                    ShowDict();
                }
                LastTickTime = nowMillSec;
            }
            catch(Exception e)
            {
                Log.E(e.ToString());
            }
        }

        /// <summary>
        /// 返回一个monster实例ID
        /// 生成monster实例的时候使用
        /// </summary>
        /// <returns></returns>
        public long GetMaxMonsterID()
        {
            if (MaxMonsterID >= long.MaxValue)
                MaxMonsterID = 1;
            return MaxMonsterID++;
        }

        /// <summary>
        /// 返回一个阵营ID
        /// </summary>
        /// <returns></returns>
        public int GetMaxCamp()
        {
            if(MaxCamp >= int.MaxValue)
                MaxCamp = 1;
            return MaxCamp++;
        }

        /// <summary>
        /// 返回地图内actor
        /// </summary>
        /// <returns></returns>
        public Dictionary<(int, long), MapActor> GetMapActors()
        {
            return ActorMap;
        }

        /// <summary>
        /// 返回地图内的monster对象
        /// </summary>
        /// <returns></returns>
        public Dictionary<long, MapMonster> GetMapMonsters()
        {
            return Monsters;
        }

        /// <summary>
        /// 返回地图内的指定的monster对象
        /// </summary>
        /// <param name="id">monster id </param>
        /// <returns></returns>
        public MapMonster GetMapMonster(long id)
        {
            return Monsters[id];
        }

        /// <summary>
        /// 返回地图内MapRole对象
        /// </summary>
        /// <returns></returns>
        public Dictionary<long, MapRole> GetMapRoles()
        {
            return Roles;
        }

        /// <summary>
        /// 返回指定的MapRole
        /// </summary>
        /// <param name="id">role id</param>
        /// <returns></returns>
        public MapRole GetMapRole(long id)
        {
            return Roles[id];
        }

        /// <summary>
        /// 根据类型返回对象
        /// </summary>
        /// <param name="type">1:role 2:monster</param>
        /// <param name="id">id </param>
        /// <returns></returns>
        //public object? GetActor(int type, long id)
        //{
        //    switch (type)
        //    {
        //        case 1:
        //            return GetMapRole(id);
        //        case 2:
        //            return GetMapMonster(id);
        //        default: 
        //            return null;
        //    }
        //}

        /// <summary>
        /// 返回地图内玩家数量
        /// </summary>
        /// <returns></returns>
        public int GetRoleNum()
        {
            // return RoleNum;
            return MonsterNum;  // todo 暂时先返回怪物数量代替玩家数量
        }

        /// <summary>
        /// 玩家进入地图
        /// </summary>
        /// <param name="mapRole"> mapRole实例类 </param>
        /// <param name="mapActor"> mapActor 实例类 </param>
        public void DoRoleEnter(MapRole mapRole, MapActor mapActor)
        {

            var config = MapReader.GetConfig(MapID);
            Random r = MapMgr.random;
            int PosX = r.Next(Math.Max(config.BornX-3, 0), Math.Min(config.BornX+3, config.Width));
            int PosY = r.Next(Math.Max(config.BornY-3, 0), Math.Min(config.BornY+3, config.Height));
            mapRole.PosX = PosX;
            mapRole.PosY = PosY;
            Roles[mapRole.ID] = mapRole;
            RoleNum++;
            ActorMap[(1, mapActor.ID)] = mapActor;
            Aoi.EnterArea(1, mapRole.ID, PosX, PosY);
            //ActorPosMap[(1, MapActor.ID)] = ((int)PosX, (int)PosY);
            // todo
        }

        /// <summary>
        /// 玩家退出地图
        /// </summary>
        /// <param name="roleID"> 角色ID </param>
        public void DoRoleExit(long roleID)
        {
            var actor = MapCommon.GetActor(this, (1, roleID));
            if (actor == null)
                return;
            // todo
            MapPos pos = actor.GetPos();
            Aoi.ExitArea(1, roleID, pos.x, pos.y);
            Roles.Remove(roleID);
            ActorMap.Remove((1, roleID));
            RoleNum --;
            //ActorPosMap.Remove((1, roleID));
            if (RoleNum <= 0 && Line > 0)  
            {
                Timer.Change(Timeout.Infinite, Timeout.Infinite);
                Timer.Dispose();
                MapMgr.DelMap(this);
            }
        }

        /// <summary>
        /// 怪物进入地图处理
        /// </summary>
        /// <param name="monster"> mapMonster 实例类 </param>
        /// <param name="mapActor"> mapActor 实例类 </param>
        public void DoMonsterEnter(MapMonster monster, MapActor mapActor)
        {
            Monsters[monster.ID] = monster;
            MonsterNum ++;
            ActorMap[(2, mapActor.ID)] = mapActor;
            Aoi.EnterArea(2, monster.ID, monster.PosX, monster.PosY);
            //ActorPosMap[(1, MapActor.ID)] = ((int)monster.PosX, (int)monster.PosY);
        }

        /// <summary>
        /// 怪物退出地图
        /// </summary>
        /// <param name="monsterID"> 怪物实例ID</param>
        public void DoMonsterExit(long monsterID)
        {
            var actor = MapCommon.GetActor(this, (2, monsterID));
            if (actor == null)
                return;
            MapPos pos = actor.GetPos();
            Aoi.ExitArea(2, monsterID, pos.x, pos.y);
            Monsters.Remove(monsterID);
            ActorMap.Remove((2, monsterID));
            MonsterNum --;
            //ActorPosMap.Remove((2, MonsterID));
            /// 这里暂时统计怪物数量计算
            if (MonsterNum <= 0)
            {
                Timer.Change(Timeout.Infinite, Timeout.Infinite);
                Timer.Dispose();
                MapMgr.DelMap(this);
            }
        }

        /// <summary>
        /// 踢出所有玩家 
        /// </summary>
        public void KickAllRole()
        {
            MessageQueue.Add("KickAllRole");
        }

        /// <summary>
        /// 加血处理
        /// </summary>
        /// <param name="actorType"> actor 类型 </param>
        /// <param name="actorID"> actor id</param>
        /// <param name="add"> 增加血量 </param>
        /// <returns></returns>
        public long DoAddHP(int actorType, long actorID, int add)
        {
            long newHp = 0;
            var key = (actorType, actorID);
            if (ActorMap.ContainsKey(key))
            {
                MapActor Actor = ActorMap[key];
                newHp = Actor.DoAddHP(add);
            }
            return newHp;
        }

        /// <summary>
        /// 扣血处理
        /// </summary>
        /// <param name="actorType"> actor 类型 </param>
        /// <param name="actorID"> actor id</param>
        /// <param name="dec"> 扣血值 </param>
        /// <param name="srcType"> 来源actor类型 </param>
        /// <param name="srcActorID">来源actor ID </param>
        /// <returns></returns>
        public long DoDecHP(int actorType, long actorID, int dec, int srcType, long srcActorID)
        {
            long NewHp = 0;
            var Key = (actorType, actorID);
            if (ActorMap.ContainsKey(Key))
            {
                MapActor Actor = ActorMap[Key];
                NewHp = Actor.DoDecHP(dec, srcType, srcActorID);
            }
            return NewHp;
        }

        /// <summary>
        /// 处理消息队列
        /// </summary>
        private void LoopQueue()
        {
            try
            {
                LoopQueue2();
            }catch (Exception e)
            {
                Log.E($"loop queue: {e.ToString()}");
            }
        }
        private void LoopQueue2()
        {
            while (MessageQueue.Count > 0)
            {
                string msg = MessageQueue[0];
                MessageQueue.RemoveAt(0);
                switch (msg)
                {
                    case "KickAllRole":
                        foreach(var id in Roles.Keys)
                            DoRoleExit(id);
                        foreach(var id in Monsters.Keys)   // 这里是把monster当做玩家处理
                            DoMonsterExit(id);
                        continue;
                    default:
                        continue;
                }
            }
        }

        /// <summary>
        /// 轮询检查移动 更新位置
        /// </summary>
        /// <param name="nowMillSec"> 毫秒级时间戳 </param>
        public void LoopMoving(long nowMillSec)
        {
            try
            {
                LoopMoving2(nowMillSec);
            }catch (Exception e)
            {
                Log.E($"loop moving: {e.ToString()}");
            }
        }
        public void LoopMoving2(long nowMillSec)
        {
            int upTime = (int)(nowMillSec - LastTickTime);
            foreach (MapActor Actor in ActorMap.Values)
            {
                if (Actor.IsAlive() && Actor.MoveState())
                {
                    MapPos Pos = Actor.GetPos();
                    (double NewX, double NewY) = Actor.DoMoving(nowMillSec, upTime);
                    Aoi.DoMove(Actor.Type, Actor.ID, Pos.x, Pos.y, NewX, NewY);
                    //ActorPosMap[(Actor.Type, Actor.ID)] = ((int)NewX, (int)NewY);
                }
            }
        }

        /// <summary>
        /// 增加一个技能实例
        /// </summary>
        /// <param name="skill">技能实例 </param>
        public void AddSkillEntity(MapSkill skill)
        {
            SkillList.Add(skill);
            if (SkillList.Count % 100 == 0)
                Log.E($"to much skill entity {SkillList.Count}");
            // 视野同步
        }

        /// <summary>
        /// 技能轮询
        /// </summary>
        /// <param name="nowMillSec">毫秒级时间戳</param>
        public void LoopSkills(long nowMillSec)
        {
            try
            { 
                LoopSkills2(nowMillSec);
            }catch (Exception e)
            {
                Log.E(e.ToString());
            }
        }
        public void LoopSkills2(long nowMillSec)
        {
            List<MapSkill> preDel = new();
            for(int i = 0; i < SkillList.Count; i++)
            {
                if (SkillList[i] == null)
                    continue;
                var skill = SkillList[i];
                (int, long) srcKey = (skill.ActorType, skill.ActorID);
                if (!ActorMap.ContainsKey(srcKey))
                {
                    // 技能拥有者不在地图
                    preDel.Add(skill);
                    continue;
                }
                MapActor srcActor = ActorMap[srcKey];

                SkillConfig config = skill.SkillConfig;
                int wave = skill.CurWave;
                if (wave + 1 > config.Waves.Count)   // 技能结束
                {
                    preDel.Add(skill);
                    continue;
                }
                int interval = config.Waves[wave];
                // 是否达到触发下一波次伤害时间
                if (skill.SkillTime + interval >= nowMillSec) 
                    continue;

                Dictionary<(int, long), MapActor> hurtMap = MapCommon.FilterHurt(this, skill, srcActor);
                List<MapEffect> effectMaps = new();
                foreach(var tarActor in hurtMap.Values)
                {
                    List<MapEffect> effects = MapFight.DoFight(srcActor, tarActor, config);
                    if(config.TBuffs.Count > 0)
                    {
                        // 可能会对目标添加buff
                        foreach (var tarEffect in effectMaps)
                        {
                            var tActor = MapCommon.GetActor(this, (tarEffect.ActorType, tarEffect.ActorID));
                            tActor.AddSkillBuff(config.TBuffs);
                        }
                    }
                    effectMaps.AddRange(effects);
                }
                skill.CurWave++;
                skill.SkillTime = nowMillSec;

                // todo 同步效果 sync effectMaps
            }
            foreach (var skill in preDel)
                _ = SkillList.Remove(skill);
        }

        /// <summary>
        /// 怪物AI轮询
        /// </summary>
        /// <param name="nowMillSec">毫秒级时间戳</param>
        public void LoopMonsterAI(long nowMillSec)
        {
            try
            {
                LoopMonsterAI2(nowMillSec);
            }
            catch (Exception e)
            {
                Log.E(e.ToString());
            }
        }
        public void LoopMonsterAI2(long nowMillSec)
        {
            List<long> removeKeys = new();
            foreach(var monster in Monsters.Values)
            {
                var actor = MapCommon.GetActor(this, (2, monster.ID));
                if (actor == null)
                {
                    removeKeys.Add(monster.ID);
                    continue;
                }
                if(nowMillSec < monster.AiTime)
                    continue;   // 未到AI触发时间
                monster.AiTime = nowMillSec + 300;

                if (!actor.IsAlive())
                    continue;

                var doingList = monster.doing;
                if (doingList.Count <= 0) // 添加一个巡逻节点
                {
                    MoveTo moveTo = MonsterAI.Patrol(monster);
                    DoMoveTo(actor, moveTo, ref doingList);
                    continue;
                }
                var doing = doingList[0];
                if (doing is Idle)  // 添加一个巡逻节点
                {
                    MoveTo moveTo = MonsterAI.Patrol(monster);
                    DoMoveTo(actor, moveTo, ref doingList);
                    continue;
                }
                else if (doing is MoveTo)  // 继续移动
                {
                    (int t, long i) = monster.TarKey;
                    bool hasTarget = t > 0 && i > 0;
                    if (hasTarget)
                    {
                        var TActor = MapCommon.GetActor(this, monster.TarKey);
                        if (TActor == null)  // 追击目标不存在了
                        {
                            doingList.RemoveAt(0);
                            monster.TarKey = (0, 0);
                            continue;
                        }
                        MapPos pos = TActor.GetPos();
                        // 超出追击距离或目标死亡
                        if (!MapTool.CheckDistance(monster.PosX, monster.PosY, pos.x, pos.y, monster.GetPursueDistance()) || !TActor.IsAlive())
                        {
                            doingList.RemoveAt(0);
                            monster.TarKey = (0, 0);
                            MoveTo moveto = MonsterAI.ReturnBorn(monster);  // 回出生点
                            DoMoveTo(actor, moveto, ref doingList);
                        }

                        // 到达攻击距离
                        if (MapTool.CheckDistance(monster.PosX, monster.PosY, pos.x, pos.y, monster.GetAttackDistance()))
                            monster.StopMove();
                    }
                    else if (actor.IsArrival())
                        doingList.RemoveAt(0);
                    else if (!monster.GetMoveState())
                        doingList.RemoveAt(0);
                    continue;
                }
                else if(doing is Patrol)
                {
                    MoveTo moveto = MonsterAI.Patrol(monster);
                    DoMoveTo(actor, moveto, ref doingList);
                }
                else if(doing is Patrol2)
                {
                    var Next = MonsterAI.Patrol2(this, monster);
                    if(Next == null)
                    {
                        MoveTo moveto = MonsterAI.Patrol(monster);
                        DoMoveTo(actor, moveto, ref doingList);
                    }
                    else
                        doingList.Insert(0, Next);
                    continue;
                }
                else if(doing is Pursue pursue)    // 追击
                {
                    doingList.RemoveAt(0);
                    (int, long) key = pursue.Key;
                    var TActor = MapCommon.GetActor(this, key);
                    if (TActor == null)  // 追击目标不存在了
                    {
                        monster.TarKey = (0, 0);
                        continue;
                    }
                    if (!TActor.IsAlive())
                    {
                        monster.TarKey = (0, 0);
                        continue;
                    }

                    MapPos pos = TActor.GetPos();
                    if(TActor.IsAlive() && MapTool.CheckDistance(monster.PosX, monster.PosY, pos.x, pos.y, monster.GetAttackDistance()))
                    {
                        if(doingList.Count >= 2)
                        {
                            var nextDo = doingList[1];
                            if (nextDo is not Fight)
                            {
                                Fight fight = new();
                                doingList.Insert(0, fight);
                            }
                        }
                        else
                        {
                            Fight fight = new();
                            doingList.Insert(0, fight);
                        }
                    }
                    else if (TActor.IsAlive() && MapTool.CheckDistance(monster.PosX, monster.PosY, pos.x, pos.y, monster.GetPursueDistance()))
                    {
                        (double rx, double ry) = MapTool.GetPatrolPos(this, pos, monster.GetAttackDistance());
                        MoveTo moveto = new(rx, ry);
                        DoMoveTo(actor, moveto, ref doingList);
                    }
                    else
                    {
                        // 超出追击距离或目标已死亡
                        doingList.RemoveAt(0);
                        MoveTo moveto = MonsterAI.ReturnBorn(monster);
                        DoMoveTo(actor, moveto, ref doingList);
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
                                actor.DoUseSkill(SkillID, TActor.GetPos(), new List<(int, long)> { monster.TarKey });
                            }
                            continue;
                        default:
                            continue;
                    }
                }
            }
            foreach(long ID in removeKeys)
                Monsters.Remove(ID);
        }

        /// <summary>
        /// 怪物开始往目标点移动
        /// </summary>
        /// <param name="actor">怪物实例对象</param>
        /// <param name="moveto">目标坐标类</param>
        /// <param name="doingList">待做列表</param>
        public static void DoMoveTo(MapActor actor, MoveTo moveto, ref ArrayList doingList)
        {
            if (actor.DoStartMove(moveto.X, moveto.Y))
                doingList.Insert(0, moveto);
        }

        /// <summary>
        /// buff轮询
        /// </summary>
        /// <param name="nowMillSec">毫秒级时间戳</param>
        public void LoopBuff(long nowMillSec)
        {
            try
            {
                foreach(var actor in ActorMap.Values)
                {
                    if(actor.GetBuffs().Count > 0)
                        actor.LoopBuff(nowMillSec);
                }
            }catch (Exception e)
            {
                Log.E(e.ToString());
            }
        }

        /// <summary>
        /// 怪物死亡轮询
        /// </summary>
        /// <param name="nowMillSec">毫秒级时间戳</param>
        public void LoopMonsterDead(long nowMillSec)
        {
            try
            {
                foreach (var monster in Monsters.Values)
                {
                    var config = MonsterReader.GetConfig(monster.MonsterID);
                    if(!monster.IsAlive() && config.RebornTime > 0)
                        monster.LoopDead(nowMillSec, config.RebornTime);
                }    
            }
            catch (Exception e)
            {
                Log.E(e.ToString());
            }
        }

        /// <summary>
        /// 输出怪物当前状态
        /// </summary>
        public void ShowDict()
        {
            //string ShowModel = MapMgr.ShowData;
            //switch (ShowModel)
            //{
            //    case "1":
            //        ShowBoss(1001);
            //        return;
            //    case "2":
            //        ShowBoss(1002);
            //        return;
            //    case "n":
            //        break;
            //    case "r":
            //        Log.P($"loop show dict");
            //        return;
            //    default:
            //        return;
            //}
            // Log.P($"monster number : {MonsterNum}");
            int n = 1;
            foreach(var monster in Monsters.Values)
            {
                if(!monster.IsAlive())
                {
                    Log.E($"monster ({n}) has dead");
                    continue;
                }
                Log.P($"{MapName} monster ({n++}) :");
                Log.W($"  ID:{monster.ID}, monsterID:{monster.MonsterID}; {monster.PosX},{monster.PosY}; isAlive :{monster.IsAlive()} curHp:{(float)monster.GetHP() / monster.GetMaxHP() * 100}%,");
                Log.W($"  doingList {monster.doing.Count}; target:{monster.TarKey}");
                if (monster.doing.Count > 0 && monster.doing[0] != null)
                {
                    ShowDoing(monster.doing[0]);
                    lastPos.TryGetValue(monster.MonsterID, out (double, double) p);
                    (double X, double Y) = p;
                    bool print = monster.doing[0] is MoveTo;
                    if (X == monster.PosX && Y == monster.PosY && print)
                    {
                        Log.R($"monster move state {monster.GetMoveState()}, target pos: ({monster.TargetX}, {monster.TargetY})");
                        foreach (var doing in monster.doing)
                            Log.R($"monster doing {doing}");
                    }
                    lastPos[monster.MonsterID] = (monster.PosX, monster.PosY);
                }
            }
            Log.P();
        }

        /// <summary>
        ///  输出某一个boss的状态
        /// </summary>
        /// <param name="id">实例ID</param>
        public void ShowBoss(long id)
        {
            Monsters.TryGetValue(id, out var monster);
            if (monster == null)
            {
                Log.E($"could not find monster : {id}");
                return;
            }
            Log.P($"{MapName} monster Target Pos {(int)monster.TargetX},{(int)monster.TargetY}; isMoving:{monster.GetMoveState()}, target:{monster.TarKey}");
            foreach(var d in monster.doing)
            {
                Log.P($"doing : {d}");
                ShowDoing(d);
            }
        }

        /// <summary>
        /// 输出待做的内容
        /// </summary>
        /// <param name="doing">待做List</param>
        /// <returns></returns>
        public static object ShowDoing(object doing)
        {
            switch(doing)
            {
                case MoveTo moveto:
                    moveto.Show();
                    break;
                case Pursue pursue:
                    pursue.Show();
                    break;
                default: 
                    break;
            }
            return doing;
        }
    }
}
