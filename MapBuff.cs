using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    /// <summary>
    /// 地图内实例身上的buff对象
    /// </summary>
    public class MapBuff
    {
        public int BuffID {  get; }
        public long EndTime {  get; set; }
        public int Value { get; set; }
        public MapBuff(int BuffID, long EndTime, int Value) 
        {
            this.BuffID = BuffID;
            this.EndTime = EndTime;
            this.Value = Value;
        }
    }

    /// <summary>
    /// buff类型枚举
    /// </summary>
    public enum BuffType
    {
        god = 2,    // 无敌
        shield = 3 // 护盾
    }
}
