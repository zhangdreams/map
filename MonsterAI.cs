using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    // 空闲
    internal struct Idle
    {
        public Idle() { }
        public static void Show() { }
    }
    // 移动
    internal struct MoveTo
    {
        public double X;
        public double Y;
        public MoveTo(double x, double y)
        {
            X = x; 
            Y = y;
        }
        public MoveTo(MapPos pos)
        {
            X = pos.x;
            Y = pos.y;
        }
        public readonly void Show()
        {
            Log.R($"moveto X：{X}, Y:{Y}");
        }
    }
    // 追击
    internal struct Pursue
    {
        public (int, long) Key;
        public Pursue((int, long) Key)
        {
            this.Key = Key;
        }

        public readonly void Show()
        {
            Log.R($"pursue key : {Key}");
        }
    }

    internal struct Fight
    {
        public Fight() { }
        public static void Show() { }
    }

    internal struct Patrol2
    {
        public Patrol2() { }
        public static void Show() { }
    }

    internal class MonsterAI
    {
        // 空闲
        public static Idle Idle()
        {
            return new Idle();
        }

        // 被动怪巡逻
        public static MoveTo Patrol(MapMonster monster)
        {
            (double randomX, double randomY) = MapTool.GetPatrolPos(monster.BornX, monster.BornY, monster.PatrolDistance);   
            return new MoveTo(randomX, randomY);
        }

        // 主动怪巡逻
        public static object Patrol2(Map map, MapMonster monster)
        {
            var Pursue = SearchInArea(map, monster);
            if (Pursue == null)
                return Patrol(monster);

            return Pursue;
        }

        // 主动怪，主动搜索并返回巡逻范围内距离最近的敌人
        public static Pursue? SearchInArea(Map map, MapMonster monster)
        {
            List<(int,long)> List = map.Aoi.GetAoi(monster.BornX, monster.BornY);
            (int, long) TarKey = (0, 0);
            double Distance = -1;
            foreach (var key in List)
            {
                var actor = MapCommon.GetActor(map, key);
                if (actor == null)
                    continue;
                MapPos pos = actor.GetPos();
                if (!MapTool.CheckDistance(monster.BornX, monster.BornY, pos.x, pos.y, monster.PatrolDistance))
                    continue;
                if(monster.Camp == actor.GetCamp())
                    continue;

                double dis = MapTool.GetDistance(monster.PosX, monster.PosY, pos.x, pos.y);
                if (Distance == -1 || dis < Distance)
                {
                    TarKey = key;
                    Distance = dis;
                    monster.TarKey = key;
                }
            }
            if (Distance == -1)
                return null;
            return new Pursue(TarKey);
        }

        public static object? Fight(Map map, MapMonster monster)
        {
            var actor = MapCommon.GetActor(map, monster.TarKey);
            if (actor == null)
            { 
                monster.doing.RemoveAt(0); 
                return null; 
            }
            if (!actor.IsAlive())
            {
                monster.TarKey = (0, 0);
                monster.doing.RemoveAt(0);
                Log.W($"target dead remove fight doing:{monster.doing.Count}, {monster.doing[0]}");
                return null;
            }
            
            MapPos pos = actor.GetPos();
            if (!MapTool.CheckDistance(monster.PosX, monster.PosY, pos.x, pos.y, monster.AttackDistance))
                return new Pursue(monster.TarKey);  // 距离不够 追击
            var Actor = MapCommon.GetActor(map, (2,monster.ID));
            if (Actor == null) // 可以从ai列表中删除
                return 0;
            // 选择一个技能释放
            int SkillID = FilterSkillID(Actor.Skills);
            if (SkillID == 0)
                return null;
            //Actor.DoUseSkill(SkillID, pos, new List<(int, long)> { monster.TarKey });
            return SkillID;
        }

        // 筛选一个可以释放的技能ID
        public static int FilterSkillID(Dictionary<int, ActorSkill> skillMap)
        {
            long Now2 = Time.Now2();
            foreach (var skill in skillMap.Values)
            {
                var config = SkillReader.GetConfig(skill.SkillID);
                if (config == null) 
                    continue;
                if (Now2 >= skill.UseTime + config.CD)
                    return skill.SkillID;
            }
            return 0;
        }

        public static MoveTo ReturnBorn(MapMonster monster)
        {
            return new MoveTo(monster.BornX, monster.BornY);
        }
    }
}
