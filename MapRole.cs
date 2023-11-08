using System.Data;

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
        public List<Node> Path { get; set; } = new();    // 寻路路径
        public double TargetX { get; set; } = 0;  // 移动的目标坐标点
        public double TargetY { get; set; } = 0;

        public Prop Prop { get; set; } = new(); // 属性
        public Dictionary<int, MapBuff> Buffs { get; set; } = new();
        public Dictionary<int, MapSkill> Skills { get; set; } = new();

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
            long Now2 = Time.Now2();
            
            if (!Buffs.ContainsKey(buffID))
            {
                MapBuff buff = new(buffID, Now2 + durTime, value);
                Buffs[buffID] = buff;
                // 新增buff(触发buff效果)
            }
            else
            {
                // 替换、叠加
                // todo 叠加持续时间
                MapBuff oldbuff = Buffs[buffID];
                oldbuff.EndTime += durTime;
            }
        }

        // 玩家停下(主动或者被动)
        public void StopMoving()
        {
            this.IsMoving = false;
            this.TargetX = 0;
            this.TargetY = 0;
        }

        // 加血
        public long AddHp(long Add)
        {
            long HP = Prop.HP;
            if (State > 0)
            {
                HP = Math.Min(Prop.MaxHp, HP + Add);
                Prop.HP = HP;
            }
            return HP;
        }

        // 扣血
        public long DecHp(long Dec)
        {
            long HP = Prop.HP;
            if(State > 0)
            {
                HP = Math.Max(HP - Dec, 0);
                Prop.HP = HP;
            }
            if (HP <= 0)
                onDead();
            return HP;
        }

        // 复活
        public void Reborn()
        {
            State = 1;
            Prop.HP = Prop.MaxHp;
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
    }
}
