using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class MapCommon
    {
        public static MapActor? GetActor(Map map, (int, long) key)
        {
            if (map.ActorMap.ContainsKey(key))
                return map.ActorMap[key];
            return null;
        }
        public static bool ActorInMap(Map Map, (int, long) Key)
        {
            return Map.ActorMap.ContainsKey(Key);
        }
        public static bool ActorInMap(Map Map, int Type, long ID)
        {
            return ActorInMap(Map, (Type, ID));
        }

        // 返回受击目标
        public static Dictionary<(int, long), MapActor> FilterHurt(Map Map, MapSkill Skill, MapActor SrcActor)
        {
            Dictionary<(int, long), MapActor> HurtMap = new();
            MapPos SrcPos = SrcActor.GetPos();
            SkillConfig config = Skill.SkillConfig;
            switch (config.Type)
            {
                case 1:
                    HurtMap = FilterHurt2(Map, Skill, SrcPos, config.AttackDistance, Skill.TargetMap);
                    break;
                case 0:
                    MapPos TarPos = Skill.Pos;
                    List<(int, long)> List = Map.Aoi.GetAoi(TarPos.x, TarPos.y);
                    
                    List<(int, long)> DList = new();
                    foreach (var t in List)
                    {
                        (int ttype, long tID) = t;
                        if ((ttype != 1 && ttype != 2) || tID == SrcActor.ID)
                            DList.Add(t);
                    }
                    foreach (var t in DList)
                        List.Remove(t);
                    //if (Skill.SkillId == 2 && List.Count <= 0)
                    //{
                    //    foreach(var t in DList)
                    //        Log.P($"Filter Hurt remove key {t}");
                    //}
                    switch (config.DamageType)
                    {
                        case 1: // 1：单个目标
                            HurtMap = FilterHurt2(Map, Skill, SrcPos, config.AttackDistance, Skill.TargetMap);
                            break;
                        case 2: // 2：圆形范围
                            if (config.Ranges.Count < 1) // 配置错误
                                return HurtMap;
                            HurtMap = FilterHurt2(Map, Skill, TarPos, config.Ranges[0], List);
                            break;
                        case 3: // 3：扇形范围
                            if (config.Ranges.Count < 2)    // 配置错误
                                return HurtMap;
                            double r = config.Ranges[0];
                            double an = config.Ranges[1];
                            foreach (var hurt in List)
                            {
                                var TarActor = MapCommon.GetActor(Map, hurt);
                                if (TarActor != null && MapTool.InSector(TarPos, TarActor.GetPos(), an, r))
                                {
                                    HurtMap[hurt] = TarActor;
                                    if (HurtMap.Count >= config.TargetNum)
                                        return HurtMap;
                                    break;
                                }
                            };
                            break;
                        default: break;
                    };
                    break;
                default: break;
            }
            return HurtMap;
        }
        public static Dictionary<(int, long), MapActor> FilterHurt2(Map Map, MapSkill Skill, MapPos SrcPos, double Distance, List<(int, long)> List)
        {
            Dictionary<(int, long), MapActor> HurtMap = new();
            SkillConfig config = Skill.SkillConfig;
            //if (Skill.SkillId == 2)
            //    Log.P($"List count: {List.Count}");
            foreach (var Hurt in List)
            {
                var TarActor = MapCommon.GetActor(Map, Hurt);
                //if (Skill.SkillId == 2)
                //    Log.P($"distance:{Distance}, hurt:{Hurt} distance 2:{MapTool.GetDistance(SrcPos, TarActor.GetPos())}");
                if (TarActor != null && MapTool.CheckDistance(SrcPos, TarActor.GetPos(), Distance))
                {
                    HurtMap[Hurt] = TarActor;
                    if (HurtMap.Count >= config.TargetNum)
                        return HurtMap;
                }
            };
            return HurtMap;
        }

        public static List<(int,int)> GetGridePosList(Map map, double x, double y)
        {
            List<(int, long)> keys = map.Aoi.GetAoi(x, y);
            List<(int, int)> Poslist = new();
            foreach (var key in keys)
            {
                var actor = GetActor(map, key);
                if (actor == null)
                    continue;
                MapPos pos = actor.GetPos();
                (int,int) p = ((int)pos.x, (int)pos.y);
                if(!Poslist.Contains(p))
                    Poslist.Add(p);
            }
            return Poslist;
        }

        public static List<(int,int)> GetMapPosList(Map map)
        {
            List<(int, int)> PosList = new();
            foreach(var actor in map.ActorMap.Values)
            {
                MapPos pos= actor.GetPos();
                (int, int) p = ((int)pos.x, (int)pos.y);
                if (!PosList.Contains(p))
                    PosList.Add(p);
            }
            return PosList;
        }

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
