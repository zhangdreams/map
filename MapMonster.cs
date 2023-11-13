using System;
using System.Collections;
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
        public double BornX { get; set; } = 0;  // 出生点
        public double BornY { get; set; } = 0;
        public int Dir { get; set; } = 0;   // 朝向
        public bool IsMoving { get; set; } = false; // 是否正在移动中
        public List<Node> Path { get; set; } = new();    // 寻路路径
        public double TargetX { get; set; } = 0;  // 移动的目标坐标点
        public double TargetY { get; set; } = 0;
        public ArrayList doing = new() ;    // AI Doing
        public double PatrolDistance { get; set; } = 3; // 巡逻半径
        public double PursueDistance { get; set; } = 3; // 追击半径
        public double AttackDistance { get; set; } = 1; // 攻击距离
        public (int, long) TarKey;  // 攻击目标

        public MapMonster(long ID, string Name, int Speed, int Camp, double BornX, double BornY)
        {
            this.ID = ID;
            this.Name = Name;
            this.Speed = Speed;
            this.Camp = Camp;
            this.BornX = BornX;
            this.BornY = BornY;
            // todo
        }

        public bool GetMoveState()
        {
            return IsMoving;
        }

        public void SetMoveState(bool State)
        {
            this.IsMoving = State;
        }
        
        public void SetTargetPos(double PosX, double PosY)
        {
            this.PosX = PosX;
            this.PosY = PosY;
        }

        public void SetPath(List<Node> Path)
        {
            this.Path = Path;
        }

        public MapPos GetPos()
        {
            return new MapPos(PosX, PosY, Dir);
        }

        public void StopMove()
        {
            this.IsMoving = false;
            this.TargetX = 0;
            this.TargetY = 0;
            this.Path.Clear();
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

        // 位置更新
        public (double, double) Moving(int upTime)
        {
            (double NewX, double NewY) = MapTool.CalcMovingPos(PosX, PosY, TargetX, TargetY, Speed, upTime);
            PosX = NewX;
            PosY = NewY;
            return (NewX, NewY);
        }

        // 移动到下一个路径点
        public void MoveNext()
        {
            if(Path.Count > 0)
            {
                Node next = Path[0];
                TargetX = next.X;
                TargetY = next.Y;
                Path.RemoveAt(0);
            }
        }
    }
}
