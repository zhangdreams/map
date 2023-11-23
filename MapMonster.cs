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
        public int MonsterID { get; set; } = 0; // 配置ID
        public string Name { get; set; } = ""; // 怪物名
        public int Level { get; set; } = 0; // 等级
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
        public long DeadTime { get; set; } = 0; // 复活时间
        public Map map;

        public MapMonster(long ID, int MonsterID, string Name, int Speed, int Camp, double BornX, double BornY)
        {
            this.ID = ID;
            this.MonsterID = MonsterID;
            this.Name = Name;
            this.Speed = Speed;
            this.Camp = Camp;
            this.BornX = BornX;
            this.BornY = BornY;
            this.PosX = BornX;
            this.PosY = BornY;
            map = null;
            // this.doing.Add(new Idle());
            // todo
        }

        public bool IsAlive()
        {
            return State == 1;
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
            this.TargetX = PosX;
            this.TargetY = PosY;
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
            // Console.WriteLine($"{ID} StopMove");
            this.IsMoving = false;
            this.TargetX = 0;
            this.TargetY = 0;
            this.Path.Clear();
            if ( this.doing.Count > 0 && this.doing[0] is MoveTo)
                this.doing.RemoveAt(0);
        }

        // 加血
        public long AddHp(int Add)
        {
            if (State > 0)
            {
                HP = Math.Min(MaxHp, HP + Add);
            }
            return HP;
        }

        // 扣血
        public long DecHp(int Dec, int SrcType, long SrcActorID)
        {
            if (State > 0)
            {
                HP = Math.Max(HP - Dec, 0);
                (int t, long i) = TarKey;
                if(t == 0 && i == 0)    // 如果有目标暂时不切换
                {
                    TarKey = (SrcType, SrcActorID);
                    doing.Insert(0, new Fight());
                }
            }
            // Console.WriteLine($"monser dec hp {HP}， dec:{Dec}");
            if (HP <= 0)
                OnDead();
            return HP;
        }

        // 复活
        public void Reborn()
        {
            State = 1;
            HP = MaxHp;
            DeadTime = 0;
            Console.WriteLine($"monster reborn {ID}");
            // 出生点复活
            map.Aoi.DoMove(2, ID, PosX, PosY, BornX, BornY);
            PosX = BornX;
            PosY = BornY;
        }

        // 死亡事件
        public void OnDead()
        {
            State = 0;
            DeadTime = Time.Now2();
            IsMoving = false;
            Path.Clear();
            if (doing.Count > 0)
                doing.RemoveAt(0);
            Console.WriteLine($"monster dead {ID}");
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

        public void LoopDead(long Now2, int rebornTime)
        {
            if(State == 0 && Now2 >= DeadTime + rebornTime * 1000)
                Reborn();
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
