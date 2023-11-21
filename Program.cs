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
            Console.WriteLine($"load config, {stopwatch.ElapsedMilliseconds}");

            // 地图创建
            Console.WriteLine("start map mgr");
            _ = new MapMgr();
            var IDList = MapReader.GetMapIDs();
            foreach (var MapId in IDList)
            {
                string mapName = MapTool.GetNormalName(MapId);
                Console.WriteLine($"start create map:{MapId},{mapName}");
                MapMgr.CreateMap(MapId, mapName);
            }
            MapMgr.ShowDict();
            BossEnterMap(1);

            //Prop prop = new();
            //Console.WriteLine($"attack value :{Common.GetFieldValue(prop, "Attack")}");
            //Common.SetFieldValue(prop, "Speed", 200);
            //Console.WriteLine($"Prop Attack value:{prop.Attack}");
            //Console.WriteLine($"Prop Attack value:{prop.Speed}");


            Console.ReadLine();
        }

        public static void BossEnterMap(int mapid)
        {
            string mapName = MapTool.GetNormalName(mapid);
            var map = MapMgr.GetMap(mapName);
            if (map == null)
            {
                Console.WriteLine($"cannot find the map mapid:{mapid} mapName:{mapName}");
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
                    Console.WriteLine($"cannot find the prop propid:{config.PropID}, monsterID:{ID}");
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