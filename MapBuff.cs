using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class MapBuff
    {
        public int BuffID {  get; set; }
        public long EndTime {  get; set; }
        public long Value { get; set; }
        public MapBuff(int BuffID, long EndTime, long Value) 
        {
            this.BuffID = BuffID;
            this.EndTime = EndTime;
            this.Value = Value;
        }
    }
}
