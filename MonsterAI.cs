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
    }
    // 追击
    internal struct Pursue
    {
        public (int, long) Key;
        public Pursue((int, long) Key)
        {
            this.Key = Key;
        }
    }

    internal struct Fight
    {
        public Fight() { }
    }

    internal class MonsterAI
    {
        // 空闲
        public static Idle Idle()
        {
            return new Idle();
        }

        // 巡逻
        public static MoveTo Patrol(MapMonster monster)
        {
            (double randomX, double randomY) = MapTool.GetPatrolPos(monster.BornX, monster.BornY, monster.PatrolDistance);   
            return new MoveTo(randomX, randomY);
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

                double dis = MapTool.GetDistance(monster.PosX, monster.PosY, pos.x, pos.y);
                if (dis == -1)
                {
                    TarKey = key;
                    Distance = dis;
                }
            }
            if (Distance == -1)
                return null;
            return new Pursue(TarKey);
        }

        public static object Fight(Map map, MapMonster monster)
        {
            var actor = MapCommon.GetActor(map, monster.TarKey);
            if (actor == null)
                return null;
            MapPos pos = actor.GetPos();
            if (!MapTool.CheckDistance(monster.PosX, monster.PosY, pos.x, pos.y, monster.AttackDistance))
                return new Pursue(monster.TarKey);  // todo 距离不够 追击
            // 选择一个技能释放

            return new Fight();
        }
    }
}
