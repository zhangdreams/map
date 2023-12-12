using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    /// <summary>
    /// 技能效果实例
    /// </summary>
    internal class MapEffect
    {
        public int ActorType { get; }
        public long ActorID { get; }
        public int EffectType { get; } // 战斗效果 1：伤害 2：回血 3：miss 4：暴击  ...
        public long Value { get; }
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
        /// <summary>
        /// 技能效果、伤害计算
        /// </summary>
        /// <param name="srcActor">施法者</param>
        /// <param name="tarActor">受击者</param>
        /// <param name="config">技能配置</param>
        /// <returns></returns>
        public static List<MapEffect> DoFight(MapActor srcActor, MapActor tarActor, SkillConfig config)
        {
            List<MapEffect> effects = new();
            Prop srcProp = srcActor.GetProp();
            Prop tarProp = tarActor.GetProp();
            //检查miss和无敌
            if (tarActor.HasBuff(BuffType.god))    
            {
                Log.R($"target has god {(tarActor.Type, tarActor.ID)}");
                effects.Add(new(tarActor.Type, tarActor.ID, 3, 0));
                return effects;
            }
            if (IsMiss(srcActor, srcProp, tarActor, tarProp))
                return effects;

            // todo 一系列计算,这里先简单计算
            Random r = MapMgr.random;
            int rate = r.Next(90, 110);
            int damage = Math.Max((int)((srcProp.Attack * config.AttackParam - tarProp.Defense) * rate / 100), 0);
            tarActor.DoDecHP(damage, srcActor.Type, srcActor.ID);
            if (damage > 1500)
                Log.W($"{srcActor.Map.MapName} do fight damage {damage}, from {srcProp.Attack},{config.AttackParam},{tarProp.Defense},{rate}");

            Log.R($"{srcActor.Map.MapName} SrcActor {srcActor.ID} fight TarActor {tarActor.ID} skill {config.SkillID} damage:{damage}");
            Log.P();
            MapEffect effect = new(tarActor.Type, tarActor.ID, 1, damage);
            effects.Add(effect);

            return effects;
        }

        /// <summary>
        /// 返回技能是否miss
        /// </summary>
        /// <param name="srcActor">施法者</param>
        /// <param name="srcProp">施法者属性</param>
        /// <param name="tarActor">受击者</param>
        /// <param name="tarProp">受击者属性</param>
        /// <returns></returns>
        public static bool IsMiss(MapActor srcActor, Prop srcProp, MapActor tarActor, Prop tarProp)
        {
            // todo 是否miss
            return false;
        }

        /// <summary>
        /// 返回技能是否暴击
        /// </summary>
        /// <param name="srcActor">施法者</param>
        /// <param name="srcProp">施法者属性</param>
        /// <param name="tarActor">受击者</param>
        /// <param name="tarProp">受击者属性</param>
        /// <returns></returns>
        public static bool IsCrit(MapActor srcActor, Prop srcProp, MapActor tarActor, Prop tarProp)
        {
            // todo 是否暴击
            return false;
        }
    }
}
