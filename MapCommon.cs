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

        public static Dictionary<(int, long), MapActor> FilterHurt(Map Map, MapSkill Skill, MapActor SrcActor)
        {
            Dictionary<(int, long), MapActor> HurtMap = new();
            MapPos SrcPos = SrcActor.GetPos();
            SkillConfig config = Skill.SkillConfig;
            switch (config.Type)
            {
                case 1:
                    HurtMap = FilterHurt2(Map, Skill, SrcPos, config.AttackDistance);
                    break;
                case 2:
                    MapPos TarPos = Skill.Pos;
                    var List = Map.Aoi.GetAoi(TarPos.x, TarPos.y);
                    foreach (var t in List)
                    {
                        (int ttype, _) = t;
                        if (ttype != 1 && ttype != 2)
                            List.Remove(t);

                    }
                    switch (config.DamageType)
                    {
                        case 1: // 1：单个目标
                            HurtMap = FilterHurt2(Map, Skill, SrcPos, config.AttackDistance);
                            break;
                        case 2: // 2：圆形范围
                            if (config.Ranges.Count < 1) // 配置错误
                                return HurtMap;
                            HurtMap = FilterHurt2(Map, Skill, SrcPos, config.Ranges[0]);
                            break;
                        case 3: // 3：扇形范围
                            if (config.Ranges.Count < 2)    // 配置错误
                                return HurtMap;
                            double r = config.Ranges[0];
                            double an = config.Ranges[1];
                            foreach (var hurt in List)
                            {
                                var TarActor = MapCommon.GetActor(Map, hurt);
                                if (TarActor != null && MapTool.InSector(SrcPos, TarActor.GetPos(), an, r))
                                {
                                    HurtMap[hurt] = TarActor;
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
        public static Dictionary<(int, long), MapActor> FilterHurt2(Map Map, MapSkill Skill, MapPos SrcPos, double Distance)
        {
            Dictionary<(int, long), MapActor> HurtMap = new();
            SkillConfig config = Skill.SkillConfig;
            foreach (var Hurt in Skill.TargetMap)
            {
                var TarActor = MapCommon.GetActor(Map, Hurt);
                if (TarActor != null && MapTool.CheckDistance(SrcPos, TarActor.GetPos(), Distance))
                {
                    HurtMap[Hurt] = TarActor;
                }
            };
            return HurtMap;
        }
    }
}
