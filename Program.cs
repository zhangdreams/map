using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace RpgMap
{
    public class Program
    {
        public static void Main()
        {
            _ = new Log();
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            MapReader.Read();
            SkillReader.Read();
            BuffReader.Read();
            PropReader.Read();
            MonsterReader.Read();
            stopwatch.Stop();
            Log.P($"load config, {stopwatch.ElapsedMilliseconds}");

            // 地图创建
            Log.P("start map mgr");
            _ = new MapMgr();
            var iDList = MapReader.GetMapIDs();
            foreach (var mapId in iDList)
            {
                string mapName = MapTool.GetNormalName(mapId);
                //Log.P($"start create map:{MapId},{mapName}");
                MapMgr.CreateMap(mapId, mapName);
            }
            MapMgr.ShowMapDict();
            //BossEnterMap(1);
            DoMonsterEnterMap(1, 5000, new Patrol2());

            while (MapMgr.ShowData != "c")
            {
                MapMgr.ShowData = Console.ReadLine() ?? "n";
            }
            MapMgr.CloseAllMap();
            Thread.Sleep(5000);
            Log.SetWrite(false);
        }

        /// <summary>
        /// 处理怪物进入指定的地图
        /// 
        /// </summary>
        /// <param name="mapid">地图ID</param>
        public static void BossEnterMap(int mapid)
        {
            string mapName = MapTool.GetNormalName(mapid);
            var map = MapMgr.GetMap(mapName);
            if (map == null)
            {
                Log.E($"cannot find the map mapid:{mapid} mapName:{mapName}");
                return;
            }
            List<int> ids = MonsterReader.GetMonsterIDs();
            foreach (var id in ids)
            {
                var config = MonsterReader.GetConfig(id);
                var prop = PropReader.GetConfig(config.PropID);
                if (prop == null)
                {
                    Log.E($"cannot find the prop propid:{config.PropID}, monsterID:{id}");
                    continue;
                }
                long mid = map.GetMaxMonsterID();
                int camp = map.GetMaxCamp();
                MapMonster monster = new(mid, id, config.Name, prop.Speed, camp, 10 * camp, 10 * camp, map);
                monster.SetHP(prop.MaxHp);
                monster.SetMaxHP(prop.MaxHp);
                monster.SetPatrolDistance(config.PatrolDistance);
                monster.SetPursueDistance(config.PursueDistance);
                monster.SetAttackDistance(config.AttackDistance);
                monster.doing.Add(new Patrol2());
                Dictionary<int, ActorSkill> skills = new();
                int skillPos = 1;
                foreach (var sid in config.Skills)
                {
                    ActorSkill skill = new(sid, skillPos);
                    skills[sid] = skill;
                }
                MapActor actor = new(2, mid, prop, map);
                actor.SetSkills(skills);

                map.DoMonsterEnter(monster, actor);
            }
            
        }

        /// <summary>
        /// 指定的地图加入指定数量的怪物
        /// </summary>
        /// <param name="mapid">地图ID</param>
        /// <param name="monsterNum">数量</param>
        /// <param name="doing">初始的AI</param>
        public static void DoMonsterEnterMap(int mapid, int monsterNum, object doing)
        {
            List<int> ids = MonsterReader.GetMonsterIDs();
            for (int i = 0;i < monsterNum;i++)
            {
                Map map = MapMgr.GetMap(mapid);
                int n = MapMgr.random.Next(ids.Count);
                int id = ids[n];
                var config = MonsterReader.GetConfig(id);
                var prop = PropReader.GetConfig(config.PropID);
                if (prop == null)
                {
                    Log.E($"cannot find the prop propid:{config.PropID}, monsterID:{id}");
                    continue;
                }
                long mid = map.GetMaxMonsterID();
                int camp = map.GetMaxCamp();
                (int x, int y) = MapCommon.RandomBornPos(map, 5);
                MapMonster monster = new(mid, id, config.Name, prop.Speed, camp, x, y, map);
                monster.SetHP(prop.MaxHp);
                monster.SetMaxHP(prop.MaxHp);
                monster.SetPatrolDistance(config.PatrolDistance);
                monster.SetPursueDistance(config.PursueDistance);
                monster.SetAttackDistance(config.AttackDistance);
                monster.doing.Add(doing);
                Dictionary<int, ActorSkill> skills = new();
                int skillPos = 1;
                foreach (var sid in config.Skills)
                {
                    ActorSkill skill = new(sid, skillPos);
                    skills[sid] = skill;
                }
                MapActor actor = new(2, mid, prop, map);
                actor.SetSkills(skills);
                map.DoMonsterEnter(monster, actor);
            }
        }
    }
}