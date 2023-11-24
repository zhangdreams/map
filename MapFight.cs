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
        public static List<MapEffect> DoFight(MapActor SrcActor, MapActor TarActor, SkillConfig config)
        {
            List<MapEffect> Effects = new();
            Prop SrcProp = SrcActor.Prop;
            Prop TarProp = TarActor.Prop;
            //检查miss和无敌
            if (TarActor.HasBuff(BuffType.god))    
            {
                Effects.Add(new(TarActor.Type, TarActor.ID, 3, 0));
                return Effects;
            }
            if (IsMiss(SrcActor, SrcProp, TarActor, TarProp))
                return Effects;

            // todo 一系列计算,这里先简单计算
            Random r = MapMgr.random;
            int rate = r.Next(90, 110);
            int Damage = Math.Max((int)((SrcProp.Attack * config.AttackParam - TarProp.Defense) * rate / 100), 0);
            TarActor.DoDecHP(Damage, SrcActor.Type, SrcActor.ID);

            Log.R($"SrcActor {SrcActor.ID} fight TarActor {TarActor.ID} skill {config.SkillID} damage:{Damage}");
            Log.P();
            MapEffect Effect = new(TarActor.Type, TarActor.ID, 1, Damage);
            Effects.Add(Effect);

            return Effects;
        }

        public static bool IsMiss(MapActor SrcActor, Prop SrcProp, MapActor TarActor, Prop TarProp)
        {
            // todo 是否miss
            return false;
        }

        public static bool IsCrit(MapActor SrcActor, Prop SrcProp, MapActor TarActor, Prop TarProp)
        {
            // todo 是否暴击
            return false;
        }
    }
}
