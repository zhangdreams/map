using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    public class MapBuff
    {
        public int BuffID {  get; set; }
        public long EndTime {  get; set; }
        public int Value { get; set; }
        public MapBuff(int BuffID, long EndTime, int Value) 
        {
            this.BuffID = BuffID;
            this.EndTime = EndTime;
            this.Value = Value;
        }
    }

    public enum BuffType
    {
        god = 2,    // 无敌
        shield = 3 // 护盾
    }
}
