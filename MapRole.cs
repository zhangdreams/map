using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class MapRole
    {
        public long ID { get; set; } = 0;    // 唯一ID
        public string Name { get; set; } = ""; // 角色名
        public int Speed { get; set; } = 0; // 移动速度
        public int Camp { get; set; } = 0;   // 阵营
        public int State { get; set; } = 1; // 状态 0死亡 1正常
        public double PosX { get; set; } = 0;   // 当前坐标
        public double PosY { get; set; } = 0;
        public bool IsMoving { get; set; } = false; // 是否正在移动中
        public List<Node> Path { get; set; }    // 寻路路径
        public double TargetX { get; set; } = 0;  // 移动的目标坐标点
        public double TargetY { get; set; } = 0;
        
        public Prop Prop { get; set; } // 属性
        public Dictionary<int, MapBuff> Buffs = new();

        //public string Buffs;
        public MapRole(long ID, string Name, int Speed, int Camp)
        {
            this.ID = ID;
            this.Name = Name;
            this.Speed = Speed;
            this.Camp = Camp;
            // List<Node> path = AStar.FindPath(startNode, goalNode);
            // todo
        }

        // 添加buff 
        // durTime buff持续时间
        // value buff效果值
        public void AddBuff(int buffID, int durTime, long value)
        {
            if(Buffs.ContainsKey(buffID))
            {
                // 新增buff
            }
            else
            {
                // 替换、叠加
            }
        }

        // 玩家停下(主动或者被动)
        public void StopMoving()
        {
            this.IsMoving = false;
            this.TargetX = 0;
            this.TargetY = 0;
        }
    }
}
