using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class MapMonster
    {
        public long ID { get; set; } = 0;    // 唯一ID
        public string Name { get; set; } = ""; // 角色名
        public long HP { get; set; } = 0;   // 当前血量
        public long MaxHp { get; set; } = 0;    // 最大血量
        public int Speed { get; set; } = 0; // 移动速度
        public int Camp { get; set; } = 0;   // 阵营
        public int State { get; set; } = 1; // 状态 0死亡 1正常
        public double PosX { get; set; } = 0;   // 当前坐标
        public double PosY { get; set; } = 0;
        public int Dir { get; set; } = 0;   // 朝向
        public bool IsMoving { get; set; } = false; // 是否正在移动中
        public List<Node> Path { get; set; } = new();    // 寻路路径
        public double TargetX { get; set; } = 0;  // 移动的目标坐标点
        public double TargetY { get; set; } = 0;

        public MapMonster(long ID, string Name, int Speed, int Camp)
        {
            this.ID = ID;
            this.Name = Name;
            this.Speed = Speed;
            this.Camp = Camp;
            // todo
        }

        public MapPos GetPos()
        {
            return new MapPos(PosX, PosY, Dir);
        }

        // 加血
        public long AddHp(long Add)
        {
            if (State > 0)
            {
                HP = Math.Min(MaxHp, HP + Add);
            }
            return HP;
        }

        // 扣血
        public long DecHp(long Dec)
        {
            if (State > 0)
            {
                HP = Math.Max(HP - Dec, 0);
            }
            if (HP <= 0)
                onDead();
            return HP;
        }

        // 复活
        public void Reborn()
        {
            State = 1;
            HP = MaxHp;
            // 可能改变位置
        }

        // 死亡事件
        public void onDead()
        {
            State = 0;
            // todo
        }
    }
}
