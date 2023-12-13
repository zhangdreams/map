using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    /// <summary>
    /// 空闲
    /// </summary>
    internal struct Idle
    {
        public Idle() { }
        public static void Show() { }
    }
    /// <summary>
    /// 移动
    /// </summary>
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
        /// <summary>
        /// 输出展示
        /// </summary>
        public readonly void Show()
        {
            Log.R($"moveto X：{X}, Y:{Y}");
        }
    }
    /// <summary>
    /// 追击
    /// </summary>
    internal struct Pursue
    {
        public (int, long) Key;
        public Pursue((int, long) Key)
        {
            this.Key = Key;
        }

        /// <summary>
        /// 输出展示
        /// </summary>
        public readonly void Show()
        {
            Log.R($"pursue key : {Key}");
        }
    }

    /// <summary>
    /// 战斗 
    /// </summary>
    internal struct Fight
    {
        public Fight() { }
        /// <summary>
        /// 输出展示
        /// </summary>
        public static void Show() { }
    }

    /// <summary>
    /// 巡逻
    /// </summary>
    internal struct Patrol
    {
        public Patrol() { }
        /// <summary>
        /// 输出展示
        /// </summary>
        public static void Show() { }
    }

    /// <summary>
    /// 会主动搜索目标进行攻击的巡逻
    /// </summary>
    internal struct Patrol2
    {
        public Patrol2() { }
        /// <summary>
        /// 输出展示
        /// </summary>
        public static void Show() { }
    }

    internal class MonsterAI
    {
        /// <summary>
        /// 空闲
        /// </summary>
        /// <returns></returns>
        public static Idle Idle()
        {
            return new Idle();
        }

        /// <summary>
        /// 被动怪巡逻
        /// </summary>
        /// <param name="monster">monster 对象</param>
        /// <returns></returns>
        public static MoveTo Patrol(MapMonster monster)
        {
            (double randomX, double randomY) = MapTool.GetPatrolPos(monster.map, monster.BornX, monster.BornY, monster.GetPatrolDistance());   
            return new MoveTo(randomX, randomY);
        }

        /// <summary>
        /// 主动怪巡逻
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="monster">monster对象</param>
        /// <returns></returns>
        public static Pursue? Patrol2(Map map, MapMonster monster)
        {
            var pursue = SearchInArea(map, monster);
            return pursue;
        }

        /// <summary>
        /// 主动怪，主动搜索并返回巡逻范围内距离最近的敌人
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="monster">monster对象</param>
        /// <returns></returns>
        public static Pursue? SearchInArea(Map map, MapMonster monster)
        {
            List<(int,long)> list = map.Aoi.GetAoi(monster.BornX, monster.BornY);
            (int, long) tarKey = (0, 0);
            double distance = -1;
            foreach (var key in list)
            {
                var actor = MapCommon.GetActor(map, key);
                if (actor == null)
                    continue;
                MapPos pos = actor.GetPos();
                if (!MapTool.CheckDistance(monster.BornX, monster.BornY, pos.x, pos.y, monster.GetPatrolDistance()))
                    continue;
                if(monster.GetCamp() == actor.GetCamp())
                    continue;

                double dis = MapTool.GetDistance(monster.PosX, monster.PosY, pos.x, pos.y);
                if (distance == -1 || dis < distance)
                {
                    tarKey = key;
                    distance = dis;
                    monster.TarKey = key;
                }
            }
            if (distance == -1)
                return null;
            return new Pursue(tarKey);
        }

        /// <summary>
        /// 战斗AI 会筛选一个可用的技能
        /// 如果距离不够则追击
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="monster">monster对象</param>
        /// <returns></returns>
        public static object? Fight(Map map, MapMonster monster)
        {
            var tarActor = MapCommon.GetActor(map, monster.TarKey);
            if (tarActor == null)
            { 
                monster.doing.RemoveAt(0); 
                return null; 
            }
            if (!tarActor.IsAlive())
            {
                monster.TarKey = (0, 0);
                monster.doing.RemoveAt(0);
                if(monster.doing.Count > 0)
                    Log.W($"target dead remove fight doing:{monster.doing.Count}, {monster.doing[0]}");
                return null;
            }
            
            MapPos pos = tarActor.GetPos();
            if (!MapTool.CheckDistance(monster.PosX, monster.PosY, pos.x, pos.y, monster.GetAttackDistance()))
                return new Pursue(monster.TarKey);  // 距离不够 追击
            var actor = MapCommon.GetActor(map, (2,monster.ID));
            if (actor == null) // 可以从ai列表中删除
                return 0;
            // 选择一个技能释放
            int skillID = FilterSkillID(actor.GetSkills());
            if (skillID == 0)
                return null;
            return skillID;
        }

        /// <summary>
        /// 筛选一个可以释放的技能ID
        /// </summary>
        /// <param name="skillMap">技能map</param>
        /// <returns></returns>
        public static int FilterSkillID(Dictionary<int, ActorSkill> skillMap)
        {
            long nowMillSec = Time.NowMillSec();
            foreach (var skill in skillMap.Values)
            {
                var config = SkillReader.GetConfig(skill.SkillID);
                if (config == null) 
                    continue;
                if (nowMillSec >= skill.UseTime + config.CD)
                    return skill.SkillID;
            }
            return 0;
        }

        /// <summary>
        /// 返回出生点
        /// </summary>
        /// <param name="monster">monster对象</param>
        /// <returns></returns>
        public static MoveTo ReturnBorn(MapMonster monster)
        {
            return new MoveTo(monster.BornX, monster.BornY);
        }
    }
}
