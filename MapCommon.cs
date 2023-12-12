using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class MapCommon
    {
        /// <summary>
        /// 返回地图内的实例对象
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="key"> 实例对象key</param>
        /// <returns></returns>
        public static MapActor? GetActor(Map map, (int, long) key)
        {
            if(map.GetMapActors().TryGetValue(key, out var actor))
                return actor;
            return null;
        }

        /// <summary>
        /// 返回某一个实例对象是否在地图中
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="key"> 实例对象key</param>
        /// <returns></returns>
        public static bool ActorInMap(Map map, (int, long) key)
        {
            return map.GetMapActors().ContainsKey(key);
        }
        public static bool ActorInMap(Map map, int type, long id)
        {
            return ActorInMap(map, (type, id));
        }

        /// <summary>
        /// 根据技能是否位置 返回可受击目标
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="skill">技能实例对象</param>
        /// <param name="srcActor">施法者actor对象</param>
        /// <returns></returns>
        public static Dictionary<(int, long), MapActor> FilterHurt(Map map, MapSkill skill, MapActor srcActor)
        {
            Dictionary<(int, long), MapActor> hurtMap = new();
            MapPos srcPos = srcActor.GetPos();
            SkillConfig config = skill.SkillConfig;
            switch (config.Type)
            {
                case 1:
                    hurtMap = FilterHurt2(map, skill, srcPos, config.AttackDistance, skill.TargetMap);
                    break;
                case 0:
                    MapPos tarPos = skill.Pos;
                    List<(int, long)> List = map.Aoi.GetAoi(tarPos.x, tarPos.y);
                    
                    List<(int, long)> dList = new();
                    foreach (var t in List)
                    {
                        (int ttype, long tID) = t;
                        if ((ttype != 1 && ttype != 2) || tID == srcActor.ID)
                            dList.Add(t);
                    }
                    foreach (var t in dList)
                        List.Remove(t);
                    switch (config.DamageType)
                    {
                        case 1: // 1：单个目标
                            hurtMap = FilterHurt2(map, skill, srcPos, config.AttackDistance, skill.TargetMap);
                            break;
                        case 2: // 2：圆形范围
                            if (config.Ranges.Count < 1) // 配置错误
                                return hurtMap;
                            hurtMap = FilterHurt2(map, skill, tarPos, config.Ranges[0], List);
                            break;
                        case 3: // 3：扇形范围
                            if (config.Ranges.Count < 2)    // 配置错误
                                return hurtMap;
                            double r = config.Ranges[0];
                            double an = config.Ranges[1];
                            foreach (var hurt in List)
                            {
                                var TarActor = MapCommon.GetActor(map, hurt);
                                if (TarActor != null && MapTool.InSector(tarPos, TarActor.GetPos(), an, r))
                                {
                                    hurtMap[hurt] = TarActor;
                                    if (hurtMap.Count >= config.TargetNum)
                                        return hurtMap;
                                    break;
                                }
                            };
                            break;
                        default: break;
                    };
                    break;
                default: break;
            }
            return hurtMap;
        }
        /// <summary>
        /// 对List内的对象进行筛选
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="skill">技能实例</param>
        /// <param name="srcPos">施法者位置坐标/技能坐标</param>
        /// <param name="distance">距离</param>
        /// <param name="list">筛选的列表</param>
        /// <returns>可受击的对象</returns>
        public static Dictionary<(int, long), MapActor> FilterHurt2(Map map, MapSkill skill, MapPos srcPos, double distance, List<(int, long)> list)
        {
            Dictionary<(int, long), MapActor> hurtMap = new();
            SkillConfig config = skill.SkillConfig;
            //if (Skill.SkillId == 2)
            //    Log.P($"List count: {List.Count}");
            foreach (var hurt in list)
            {
                var tarActor = MapCommon.GetActor(map, hurt);
                //if (Skill.SkillId == 2)
                //    Log.P($"distance:{Distance}, hurt:{Hurt} distance 2:{MapTool.GetDistance(SrcPos, TarActor.GetPos())}");
                if (tarActor != null && MapTool.CheckDistance(srcPos, tarActor.GetPos(), distance))
                {
                    hurtMap[hurt] = tarActor;
                    if (hurtMap.Count >= config.TargetNum)
                        return hurtMap;
                }
            };
            return hurtMap;
        }

        /// <summary>
        /// 返回地图内某一个坐标所在的九宫格内实例的坐标
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <returns></returns>
        public static Dictionary<(int,int), MapActor> GetGridePosList(Map map, double x, double y)
        {
            List<(int, long)> keys = map.Aoi.GetAoi(x, y);
            Dictionary<(int, int), MapActor> posDict = new();
            foreach (var key in keys)
            {
                var actor = GetActor(map, key);
                if (actor == null)
                    continue;
                MapPos pos = actor.GetPos();
                (int,int) p = ((int)pos.x, (int)pos.y);
                posDict[p] = actor;
            }
            return posDict;
        }

        /// <summary>
        /// 返回地图内所有实例的坐标
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static Dictionary<(int, int), MapActor> GetMapPosList(Map map)
        {
            Dictionary<(int, int), MapActor> posDict = new();
            foreach (var actor in map.GetMapActors().Values)
            {
                MapPos pos= actor.GetPos();
                (int, int) p = ((int)pos.x, (int)pos.y);
                posDict[p] = actor;
            }
            return posDict;
        }

        /// <summary>
        /// 返回地图内一个随机出生点
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="times">随机次数</param>
        /// <returns></returns>
        public static (int, int) RandomBornPos(Map map, int times)
        {
            if (times <= 0)
                return (0, 0);
            var mapConfig = MapReader.GetConfig(map.MapID);
            int x = MapMgr.random.Next(mapConfig.Width);
            int y = MapMgr.random.Next(mapConfig.Height);
            if (!MapPath.IsObstacle(map, x, y))
                return (x, y);
            return RandomBornPos(map, times - 1);
        }
    }
}
