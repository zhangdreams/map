using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class MapEffect
    {
        public int ActorType { get; set; }
        public long ActorID { get; set; }
        public int EffectType { get; set; } // 战斗效果 1：伤害 2：回血 3：miss 4：暴击  ...
        public long Value { get; set; }
        public MapEffect(int ActorType, long ActorID, int EffectType, long Value)
        {
            this.ActorType = ActorType;
            this.ActorID = ActorID;
            this.EffectType = EffectType;
            this.Value = Value;
        }
    }

    internal class MapFight
    {
        public static List<MapEffect> DoFight(Map Map, MapActor SrcActor, MapActor TarActor, SkillConfig config)
        {
            List<MapEffect> Effects = new();
            Prop SrcProp = SrcActor.Prop;
            Prop TarProp = TarActor.Prop;
            if (IsMiss(SrcProp, TarProp) || TarActor.HasBuff(2))    //检查miss和无敌
                return Effects;
            // todo 一系列计算
            long Damage = (long)(SrcProp.Attack * config.AttackParam) - TarProp.Defense;
            TarActor.DoDecHP(Damage, SrcActor.Type, SrcActor.ID);

            MapEffect Effect = new(TarActor.Type, TarActor.ID, 1, Damage);
            Effects.Add(Effect);

            return Effects;
        }

        public static bool IsMiss(Prop SrcProp, Prop TarProp)
        {
            // todo 是否miss
            return false;
        }

        public static bool IsCrit(Prop SrcProp, Prop TarProp)
        {
            // todo 是否暴击
            return false;
        }
    }
}
