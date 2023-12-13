using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RpgMap
{
    /// <summary>
    /// 地图内的怪物对象
    /// </summary>
    internal class MapMonster
    {
        public long ID { get; } = 0;    // 唯一ID
        public int MonsterID { get; } = 0; // 配置ID
        public string Name { get; } = ""; // 怪物名
        private int Level { get; set; } = 0; // 等级
        private long HP { get; set; } = 0;   // 当前血量
        private long MaxHp { get; set; } = 0;    // 最大血量
        private int Speed { get; set; } = 0; // 移动速度 m/s
        private int Camp { get; set; } = 0;   // 阵营
        private int State { get; set; } = 1; // 状态 0死亡 1正常
        public double PosX { get; set; } = 0;   // 当前坐标
        public double PosY { get; set; } = 0;
        public double BornX { get; } = 0;  // 出生点
        public double BornY { get; } = 0;
        public int Dir { get; set; } = 0;   // 朝向
        private bool IsMoving { get; set; } = false; // 是否正在移动中
        private List<Node> Path { get; set; } = new();    // 寻路路径
        public double TargetX { get; set; } = 0;  // 移动的目标坐标点
        public double TargetY { get; set; } = 0;
        public long AiTime { get; set; } = 0;   // 下次AI触发时间
        public ArrayList doing = new() ;    // AI Doing
        private double PatrolDistance { get; set; } = 3; // 巡逻半径
        private double PursueDistance { get; set; } = 3; // 追击半径
        private double AttackDistance { get; set; } = 1; // 攻击距离
        public (int, long) TarKey;  // 攻击目标
        private long DeadTime { get; set; } = 0; // 复活时间
        public Map map { get; }
        private long PauseTime { get; set; } = 0; // 碰到动态障碍物停止移动的时间

        public MapMonster(long ID, int MonsterID, string Name, int Speed, int Camp, double BornX, double BornY, Map map)
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
            this.AiTime = Time.NowMillSec() + MapMgr.random.Next(200);
            this.map = map;
            // this.doing.Add(new Idle());
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
        public void SetMoveState(bool state)
        {
            this.IsMoving = state;
        }
        
        /// <summary>
        /// 设置怪物目的地坐标
        /// </summary>
        /// <param name="posX">x坐标</param>
        /// <param name="posY">y坐标</param>
        public void SetTargetPos(double posX, double posY)
        {
            this.TargetX = posX;
            this.TargetY = posY;
        }

        /// <summary>
        /// 设置怪物移动路点
        /// </summary>
        /// <param name="path">路径点</param>
        public void SetPath(List<Node> path)
        {
            this.Path = path;
            //Log.W($"{ID} set path {this.Path.Count} pos:{(PosX, PosY)}");
        }

        /// <summary>
        /// 返回怪物移动路点
        /// </summary>
        /// <returns></returns>
        public List<Node> GetPath()
        { 
            return this.Path; 
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
        /// 返回怪物巡逻半径
        /// </summary>
        /// <returns></returns>
        public double GetPatrolDistance()
        {
            return PatrolDistance;
        }

        /// <summary>
        /// 设置怪物巡逻半径
        /// </summary>
        /// <param name="distance"></param>
        public void SetPatrolDistance(double distance)
        {
            PatrolDistance = distance;
        }

        /// <summary>
        /// 返回怪物追击半径
        /// </summary>
        /// <returns></returns>
        public double GetPursueDistance()
        {
            return PursueDistance;
        }

        /// <summary>
        /// 设置怪物追击半径
        /// </summary>
        /// <param name="distance"></param>
        public void SetPursueDistance(double distance)
        {
            PursueDistance = distance;
        }

        /// <summary>
        /// 返回怪物攻击半径
        /// </summary>
        /// <returns></returns>
        public double GetAttackDistance()
        {
            return AttackDistance;
        }

        /// <summary>
        /// 设置怪物攻击半径
        /// </summary>
        /// <param name="distance"></param>
        public void SetAttackDistance(double distance)
        {
            AttackDistance = distance;
        }
        /// <summary>
        /// 停止移动
        /// </summary>
        public void StopMove()
        {
            // Log.P($"{ID} StopMove");
            this.IsMoving = false;
            this.TargetX = 0;
            this.TargetY = 0;
            this.Path.Clear();
            //Log.W($"{ID} StopMove {this.Path.Count} pos:{(PosX,PosY)}");
            if ( this.doing.Count > 0 && this.doing[0] is MoveTo)
                this.doing.RemoveAt(0);
        }

        /// <summary>
        /// 加血
        /// </summary>
        /// <param name="add">增加的血量</param>
        /// <returns>改变后的血量</returns>
        public long AddHp(int add)
        {
            if (State > 0)
            {
                HP = Math.Min(MaxHp, HP + add);
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
        public long DecHp(int dec, int srcType, long srcActorID)
        {
            if (State > 0)
            {
                HP = Math.Max(HP - dec, 0);
                (int t, long i) = TarKey;
                if(t == 0 && i == 0)    // 如果有目标暂时不切换
                {
                    TarKey = (srcType, srcActorID);
                    doing.Insert(0, new Fight());
                }
            }
            // Log.P($"monser dec hp {HP}， dec:{Dec}");
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
            DeadTime = 0;
            Log.E($"monster reborn {ID}");
            // 出生点复活
            map.Aoi.DoMove(2, ID, PosX, PosY, BornX, BornY);
            PosX = BornX;
            PosY = BornY;
        }

        /// <summary>
        /// 死亡事件
        /// </summary>
        public void OnDead()
        {
            State = 0;
            DeadTime = Time.NowMillSec();
            IsMoving = false;
            Path.Clear();
            //Log.W($"{ID} OnDead {this.Path.Count} pos:{(PosX, PosY)}");
            if (doing.Count > 0)
                doing.RemoveAt(0);
            Log.E($"monster dead {ID}");
            // todo
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
            // todo 判断能否移动 一些状态的判断
            MapPos Pos = GetPos();
            Node Start = new((int)Pos.x, (int)Pos.y);
            Node Goal = new((int)x, (int)y);
            //var Path = MapPath.FindPath(Map, Start, Goal) ?? throw new Exception($"can not move to pos {(Map.MapName,ID)} x={x},y={y}, curpos {(Start.X, Start.Y)}");
            var Path = MapPath.FindPath(map, Start, Goal);
            if (Path == null)
                return false;
            SetMoveState(true);
            SetTargetPos(x, y);
            SetPath(Path);
            return true;
        }

        /// <summary>
        /// 位置更新
        /// </summary>
        /// <param name="nowMillSec">当前时间戳（毫秒）</param>
        /// <param name="upTime">更新的时间</param>
        /// <returns>新的坐标</returns>
        public (double, double) Moving(long nowMillSec, int upTime)
        {
            (double newX, double newY) = MapTool.CalcMovingPos(PosX, PosY, TargetX, TargetY, Speed, upTime);
            if (MapMgr.SphereRis > 0 && ((int)newX != (int)PosX || (int)newY != (int)PosY) && MapPath.IsObstacle(map, (int)newX, (int)newY))
            {
                if (PauseTime == 0)
                {
                    PauseTime = nowMillSec + 2000;    // 2s后仍被阻挡则重新寻路
                    return (PosX, PosY);
                }
                else if(nowMillSec >= PauseTime)
                {
                    PauseTime = 0;
                    Node start = new((int)PosX, (int)PosY);
                    Node goal;
                    if (Path.Count > 0)
                        goal = Path[0];
                    else 
                        goal = new((int)TargetX, (int)TargetY);

                    var path2 = MapPath.FindPath(map, start, goal);
                    Log.W($"monster {ID} pause reFindPath start:{start.Show()} goal:{goal.Show()}, target{((int)TargetX, (int)TargetY)}, NewPos {((int)newX, (int)newY)}, {Path.Count}");
                    if (path2 == null || path2.Count <= 0)
                    {
                        StopMove();
                        return (PosX, PosY);
                    }
                    path2.AddRange(Path);
                    Path = path2;
                    //Log.W($"{ID} moving {Path.Count}");
                    try
                    {
                        TargetX = Path[0].X;
                        TargetY = Path[0].Y;
                    }catch (Exception e)
                    {
                        Log.E($"moving path {e}");
                    }

                    return (PosX, PosY);
                }else
                    return (PosX, PosY);
            }else if(PauseTime > 0)
                PauseTime = 0;
            PosX = newX;
            PosY = newY;
            return (newX, newY);
        }

        /// <summary>
        /// 死亡轮询 
        /// 处理是否复活
        /// </summary>
        /// <param name="nowMillSec">当前毫秒级时间戳</param>
        /// <param name="rebornTime">复活时长</param>
        public void LoopDead(long nowMillSec, int rebornTime)
        {
            if(State == 0 && nowMillSec >= DeadTime + rebornTime * 1000)
                Reborn();
        }

        /// <summary>
        /// 移动到下一个路径点
        /// </summary>
        public void MoveNext()
        {
            if(Path.Count > 0)
            {
                Node next = Path[0];
                if(next != null)
                {
                    TargetX = next.X;
                    TargetY = next.Y;
                    //try
                    //{
                        Path.RemoveAt(0);
                    //}
                    //catch (Exception e)
                    //{
                    //    Log.E(e.ToString());
                    //    Log.E($"Move Next {Path.Count}");
                    //}
                }
                else
                {
                    //try
                    //{ 
                        Path.RemoveAt(0);
                    //}
                    //catch (Exception e)
                    //{
                    //    Log.E(e.ToString());
                    //    Log.E($"Move Next {Path.Count}");
                    //}
                    MoveNext();
                }
            }else
                StopMove();
        }
    }
}
