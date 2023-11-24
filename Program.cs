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
                Log.P($"start create map:{MapId},{mapName}");
                MapMgr.CreateMap(MapId, mapName);
            }
            MapMgr.ShowDict();
            BossEnterMap(1);

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
            long MID = 1001;
            int camp = 1;
            foreach (var ID in IDs)
            {
                var config = MonsterReader.GetConfig(ID);
                var prop = PropReader.GetConfig(config.PropID);
                if (prop == null)
                {
                    Log.E($"cannot find the prop propid:{config.PropID}, monsterID:{ID}");
                    continue;
                }
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
                MID++;
                camp++;

                map.DoMonsterEnter(monster, actor);
            }
            
        }

    }
}