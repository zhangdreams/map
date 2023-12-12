using System.Data;

namespace RpgMap
{
    /// <summary>
    /// 玩家的地图实例
    /// </summary>
    internal class MapRole
    {
        public long ID { get; set; } = 0;    // 唯一ID
        public string Name { get; set; } = ""; // 角色名
        public int Level { get; set; } = 0; // 等级
        private long HP { get; set; } = 0;   // 当前血量
        private long MaxHp { get; set; } = 0;    // 最大血量
        private int Speed { get; set; } = 0; // 移动速度
        private int Camp { get; set; } = 0;   // 阵营
        private int State { get; set; } = 1; // 状态 0死亡 1正常
        public double PosX { get; set; } = 0;   // 当前坐标
        public double PosY { get; set; } = 0;
        public int Dir { get; set; } = 0;   // 朝向
        private bool IsMoving { get; set; } = false; // 是否正在移动中
        //public List<Node> Path { get; set; } = new();    // 寻路路径
        public double TargetX { get; set; } = 0;  // 移动的目标坐标点
        public double TargetY { get; set; } = 0;
        public Map map { get; }

        //public string Buffs;
        public MapRole(long ID, string Name, int Speed, int Camp, Map map)
        {
            this.ID = ID;
            this.Name = Name;
            this.Speed = Speed;
            this.Camp = Camp;
            this.map = map;
            // List<Node> path = AStar.FindPath(startNode, goalNode);
            // todo
        }

        /// <summary>
        /// 返回当前等级
        /// </summary>
        /// <returns></returns>
        public int GetLevel()
        {
            return Level;
        }

        /// <summary>
        /// 返回当前的血量
        /// </summary>
        /// <returns></returns>
        public long GetHP()
        {
            return HP;
        }

        /// <summary>
        /// 设置monster血量
        /// </summary>
        /// <param name="hp">血量</param>
        public void SetHP(long hp)
        {
            HP = hp;
        }

        /// <summary>
        /// 返回最大血量
        /// </summary>
        /// <returns></returns>
        public long GetMaxHP()
        {
            return MaxHp;
        }

        /// <summary>
        /// 设置monster最大血量
        /// </summary>
        /// <param name="hp"></param>
        public void SetMaxHP(long hp)
        {
            MaxHp = hp;
        }

        /// <summary>
        /// 返回阵营id
        /// </summary>
        /// <returns></returns>
        public int GetCamp()
        {
            return Camp;
        }

        /// <summary>
        /// 属性改变
        /// </summary>
        /// <param name="key">属性key</param>
        /// <param name="value">value</param>
        public void SetProp(string key, object value)
        {
            switch (key)
            {
                case "MaxHp":
                    int add = (int)((long)value - MaxHp);
                    MaxHp = (long)value;
                    AddHp(add);
                    break;
                case "Speed":
                    Speed = (int)value;
                    break;
                default:
                    Log.E($"MapMonster set prop unhandle key {key}");
                    break;
            }
        }

        /// <summary>
        /// 返回当前是否存活
        /// </summary>
        /// <returns></returns>
        public bool IsAlive()
        {
            return State == 1;
        }

        /// <summary>
        /// 返回移动状态
        /// </summary>
        /// <returns></returns>
        public bool GetMoveState()
        {
            return IsMoving;
        }

        /// <summary>
        /// 设置怪物移动状态
        /// </summary>
        /// <param name="state">移动状态</param>
        public void SetMoveState(bool State)
        {
            this.IsMoving = State;
        }

        /// <summary>
        /// 设置怪物目的地坐标
        /// </summary>
        /// <param name="posX">x坐标</param>
        /// <param name="posY">y坐标</param>
        public void SetTargetPos(double PosX, double PosY)
        {
            this.PosX = PosX;
            this.PosY = PosY;
        }

        /// <summary>
        /// 返回怪物当前坐标
        /// </summary>
        /// <returns></returns>
        public MapPos GetPos()
        {
            return new MapPos(PosX, PosY, Dir);
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        public void StopMove()
        {
            this.IsMoving = false;
            this.TargetX = 0;
            this.TargetY = 0;
        }

        /// <summary>
        /// 加血
        /// </summary>
        /// <param name="add">增加的血量</param>
        /// <returns>改变后的血量</returns>
        public long AddHp(int Add)
        {
            if (State > 0)
            {
                HP = Math.Min(MaxHp, HP + Add);
            }
            return HP;
        }

        /// <summary>
        /// 扣血
        /// </summary>
        /// <param name="dec">扣除的血量</param>
        /// <param name="srcType">伤害来源actor 类型</param>
        /// <param name="srcActorID">伤害来源actor id</param>
        /// <returns>改变后的血量</returns>
        public long DecHp(int Dec)
        {
            if(State > 0)
            {
                HP = Math.Max(HP - Dec, 0);
            }
            if (HP <= 0)
                OnDead();
            return HP;
        }

        /// <summary>
        /// 复活处理
        /// </summary>
        public void Reborn()
        {
            State = 1;
            HP = MaxHp;
           // 可能改变位置
        }

        /// <summary>
        /// 死亡事件
        /// </summary>
        public void OnDead()
        {
           State = 0;
           // todo
        }

        /// <summary>
        /// 位置更新
        /// </summary>
        /// <param name="nowMillSec">当前时间戳（毫秒）</param>
        /// <param name="upTime">更新的时间</param>
        /// <returns>新的坐标</returns>
        public (double, double) Moving(int upTime)
        {
            (double NewX, double NewY) = MapTool.CalcMovingPos(PosX, PosY, TargetX, TargetY, Speed, upTime);
            if (MapMgr.SphereRis > 0 && ((int)NewX != (int)PosX || (int)NewY != (int)PosY) && MapPath.IsObstacle(map, (int)NewX, (int)NewY))
            {
                StopMove();
                return (PosX, PosY);
            }
            PosX = NewX; 
            PosY = NewY;
            return (NewX, NewY);
        }

        /// <summary>
        /// 开始移动
        /// </summary>
        /// <param name="x">目标坐标点</param>
        /// <param name="y">目标坐标点</param>
        /// <returns>能否成功移动</returns>
        /// <exception cref="Exception">目标点不是一个合法的坐标</exception>
        public bool DoStartMove(double x, double y)
        {
            if (!MapPath.IsValidCoordinate(map.MapID, (int)x, (int)y))
            {
                throw new Exception($"pos is unvalid x={x},y={y}");
            }
            SetMoveState(true);
            SetTargetPos(x, y);
            return true;
        }
    }
}
