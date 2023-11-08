using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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

        public Dictionary<(int,long), MapRole> ActorMap = new();
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

            var config = MapReader.GetConfig(mapID);
            // 初始化AOI
            this.Aoi = new(config.Width, config.Height);
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

            var config = MapReader.GetConfig(MapID);
            Random r = new Random();
            int PosX = r.Next(Math.Max(config.BornX-3, 0), Math.Min(config.BornX+3, config.Width));
            int PosY = r.Next(Math.Max(config.BornY-3, 0), Math.Min(config.BornY+3, config.Height));
            MapRole.PosX = PosX;
            MapRole.PosY = PosY;
            Roles[MapRole.ID] = MapRole;
            RoleNum++;
            // todo
        }

        public void DoRoleExit(long RoleID)
        {
            BuffRoleIDList.Remove(RoleID);
            Roles.Remove(RoleID);
            RoleNum--;
            // todo
        }



        public long DoAddHP(long RoleID, long Add)
        {
            long NewHp = 0;
            if (Roles.ContainsKey(RoleID))
            {
                MapRole role = Roles[RoleID];
                NewHp = role.AddHp(Add);
            }
            return NewHp;
        }

        public long DoDecHP(long RoleID, long Dec)
        {
            long NewHp = 0;
            if(Roles.ContainsKey(RoleID))
            {
                MapRole role = Roles[RoleID];
                NewHp = role.DecHp(Dec);
            }
            return NewHp;
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
                    (double NewX, double NewY) = role.Moving(upTime);
                    Aoi.DoMove(1, role.ID, X, Y, NewX, NewY);
                }
            }
        }

    }
}
