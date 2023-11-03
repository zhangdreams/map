using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class Map
    {
        public int ID { get; set; } = 0;    // id
        public int MapID { get; set; } = 0; // 地图ID
        public string MapName { get; set; } = ""; // 地图名（常用来索引）
        public int MapType { get; set; } = 0; // 地图类型
        public long CreateTime { get; set; } = 0; // 地图创建时间

        private long LastTickTime { get; set; } = 0;

        public Thread thread;   // 地图的线程
        public MapAoi Aoi;  // 地图内Aoi对象

        public int RoleNum { get; set; } = 0; // 地图内玩家数量

        public Dictionary<long, MapRole> Roles = new(); // 保存玩家实例
        public List<long> BuffRoleIDList { get; set; } = new List<long>(); // 保存地图内有buff的玩家ID

        public Map(int id, int mapID, string mapName) 
        {
            this.ID = id;
            this.MapID = mapID;
            this.MapName = mapName;
            CreateTime = Time.Now();
            MapType = 1;

            // 创建线程
            Thread thread = new (InitMap);
            thread.Start();
            this.thread = thread;

            // 初始化AOI
            MapAoi Aoi = new(100, 100);     // todo 先随便给个大小
            this.Aoi = Aoi;
        }
        public void InitMap()
        {
            LastTickTime = Time.Now2();
            // 增加一个轮询
            Timer cTimer = new (MapLoop, null, 0, 100);

        }

        private void MapLoop(object? state)
        {
            try
            {
                long Now2 = Time.Now2();
                // todo
                LoopMoving(Now2);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // 玩家进入地图
        public void DoRoleEnter(MapRole MapRole)
        {
            // todo 先随便给个位置
            MapRole.PosX = 10;
            MapRole.PosY = 10;
            Roles[MapRole.ID] = MapRole;
            // todo
            RoleNum++;
        }

        public void DoRoleExit(long RoleID)
        {
            // todo
            BuffRoleIDList.Remove(RoleID);
            Roles.Remove(RoleID);
            RoleNum--;
        }
        
        public void DoRoleDead(long RoleID)
        {
            if (Roles.ContainsKey(RoleID))
            {
                MapRole role = Roles[RoleID];
                role.State = 0;
            }
        }

        // 轮询检查移动
        public void LoopMoving(long Now2)
        {
            int upTime = (int)((int)Now2 - LastTickTime);
            foreach (var Dic in Roles)
            {
                MapRole role = Dic.Value;
                if (role.IsMoving)
                {
                    double X = role.PosX;
                    double Y = role.PosY;
                    (double NewX, double NewY) = MapTool.CalcMovingPos(X, Y, role.TargetX, role.TargetY, role.Speed, upTime);
                    role.PosX = NewX;
                    role.PosY = NewY;
                    Aoi.DoMove(1, role.ID, X, Y, NewX, NewY);
                }
            }
        }
    }
}
