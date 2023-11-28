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
            Stopwatch stopwatch = Stopwatch.StartNew();
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
            var IDList = MapReader.GetMapIDs();
            foreach (var MapId in IDList)
            {
                string mapName = MapTool.GetNormalName(MapId);
                //Log.P($"start create map:{MapId},{mapName}");
                MapMgr.CreateMap(MapId, mapName);
            }
            MapMgr.ShowDict();
            //BossEnterMap(1);
            DoMonsterEnterMap(1, 99);

            //Prop prop = new();
            //Log.P($"attack value :{Common.GetFieldValue(prop, "Attack")}");
            //Common.SetFieldValue(prop, "Speed", 200);
            //Log.P($"Prop Attack value:{prop.Attack}");
            //Log.P($"Prop Attack value:{prop.Speed}");


            while (MapMgr.show != "c")
            {
                MapMgr.show = Console.ReadLine() ?? "n";
            }
        }

        public static void BossEnterMap(int mapid)
        {
            string mapName = MapTool.GetNormalName(mapid);
            var map = MapMgr.GetMap(mapName);
            if (map == null)
            {
                Log.E($"cannot find the map mapid:{mapid} mapName:{mapName}");
                return;
            }
            List<int> IDs = MonsterReader.GetMonsterIDs();
            foreach (var ID in IDs)
            {
                var config = MonsterReader.GetConfig(ID);
                var prop = PropReader.GetConfig(config.PropID);
                if (prop == null)
                {
                    Log.E($"cannot find the prop propid:{config.PropID}, monsterID:{ID}");
                    continue;
                }
                long MID = map.GetMaxMonsterID();
                int camp = map.getMaxCamp();
                MapMonster monster = new(MID, ID, config.Name, prop.Speed, camp, 10 * camp, 10 * camp)
                {
                    HP = prop.MaxHp,
                    MaxHp = prop.MaxHp,
                    PatrolDistance = config.PatrolDistance,
                    PursueDistance = config.PatrolDistance,
                    AttackDistance = config.AttackDistance,
                };
                monster.doing.Add(new Patrol2());
                monster.map = map;
                Dictionary<int, ActorSkill> skills = new();
                int skillPos = 1;
                foreach (var sid in config.Skills)
                {
                    ActorSkill skill = new(sid, skillPos);
                    skills[sid] = skill;
                }
                MapActor actor = new(2, MID, prop, map)
                {
                    Skills = skills,
                };

                map.DoMonsterEnter(monster, actor);
            }
            
        }

        public static void DoMonsterEnterMap(int mapid, int monsterNum)
        {
            var mapConfig = MapReader.GetConfig(mapid);
            List<int> IDs = MonsterReader.GetMonsterIDs();
            for (int i = 0;i < monsterNum;i++)
            {
                Map map = MapMgr.GetMap(mapid);
                int N = MapMgr.random.Next(IDs.Count);
                int ID = IDs[N];
                var config = MonsterReader.GetConfig(ID);
                var prop = PropReader.GetConfig(config.PropID);
                if (prop == null)
                {
                    Log.E($"cannot find the prop propid:{config.PropID}, monsterID:{ID}");
                    continue;
                }
                long MID = map.GetMaxMonsterID();
                int camp = map.getMaxCamp();
                int x = MapMgr.random.Next(mapConfig.Width);
                int y = MapMgr.random.Next(mapConfig.Height);
                MapMonster monster = new(MID, ID, config.Name, prop.Speed, camp, x, y)
                {
                    HP = prop.MaxHp,
                    MaxHp = prop.MaxHp,
                    PatrolDistance = config.PatrolDistance,
                    PursueDistance = config.PatrolDistance,
                    AttackDistance = config.AttackDistance,
                };
                monster.doing.Add(new Patrol2());
                monster.map = map;
                Dictionary<int, ActorSkill> skills = new();
                int skillPos = 1;
                foreach (var sid in config.Skills)
                {
                    ActorSkill skill = new(sid, skillPos);
                    skills[sid] = skill;
                }
                MapActor actor = new(2, MID, prop, map)
                {
                    Skills = skills,
                };
                map.DoMonsterEnter(monster, actor);
            }
        }
    }
}