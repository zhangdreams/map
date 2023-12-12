using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    /// <summary>
    /// 战斗属性
    /// </summary>
    public class Prop
    {
        public int Index { get; set; } = 0; //配置ID用
        public long HP { get; set; } = 0;
        public long MaxHp { get; set; } = 0;
        public int Attack { get; set; } = 0;
        public int Defense {  get; set; } = 0;
        public int Speed { get; set; } = 0; // m/s
        public Prop() 
        {

        }

        /// <summary>
        /// 属性输出展示
        /// </summary>
        public void Show()
        {
            Log.R($"prop.HP : {HP}");
            Log.R($"prop.MaxHp : {MaxHp}");
            Log.R($"prop.Attack : {Attack}");
            Log.R($"prop.Defense : {Defense}");
            Log.R($"prop.Speed : {Speed}");
        }
    }
}
