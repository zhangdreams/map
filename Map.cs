using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
        public Dictionary<long, MapRole> Roles { get; set; } = new(); // 保存玩家实例
        public Dictionary<long, MapMonster> Monsters { get; set; } = new(); 
        public Dictionary<(int,long), MapActor> ActorMap { get; set; } = new();   // 保存地图Actor实例
        public List<long> BuffRoleIDList { get; set; } = new List<long>(); // 保存地图内有buff的玩家ID
        public List<MapSkill> SkillList { get; set; }   // 保存地图内的技能实例

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
                LoopSkills(Now2);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // 玩家进入地图
        public void DoRoleEnter(MapRole MapRole, MapActor MapActor)
        {

            var config = MapReader.GetConfig(MapID);
            Random r = new Random();
            int PosX = r.Next(Math.Max(config.BornX-3, 0), Math.Min(config.BornX+3, config.Width));
            int PosY = r.Next(Math.Max(config.BornY-3, 0), Math.Min(config.BornY+3, config.Height));
            MapRole.PosX = PosX;
            MapRole.PosY = PosY;
            Roles[MapRole.ID] = MapRole;
            RoleNum++;
            MapActor.Map = this;
            ActorMap[(MapActor.Type, MapActor.ID)] = MapActor;
            // todo
        }

        public void DoRoleExit(long RoleID)
        {
            BuffRoleIDList.Remove(RoleID);
            Roles.Remove(RoleID);
            ActorMap.Remove((1, RoleID));
            RoleNum--;
            // todo
        }

        public long DoAddHP(int ActorType, long ActorID, long Add)
        {
            long NewHp = 0;
            var Key = (ActorType, ActorID);
            if (ActorMap.ContainsKey(Key))
            {
                MapActor Actor = ActorMap[Key];
                NewHp = Actor.DoAddHP(Add);
            }
            return NewHp;
        }

        public long DoDecHP(int ActorType, long ActorID, long Dec)
        {
            long NewHp = 0;
            var Key = (ActorType, ActorID);
            if (ActorMap.ContainsKey(Key))
            {
                MapActor Actor = ActorMap[Key];
                NewHp = Actor.DoDecHP(Dec);
            }
            return NewHp;
        }

        // 轮询检查移动
        public void LoopMoving(long Now2)
        {
            try
            {
                LoopMoving2(Now2);
            }catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void LoopMoving2(long Now2)
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

        public void AddSkillEntity(MapSkill Skill)
        {
            SkillList.Add(Skill);
            // 视野同步

        }

        // 技能轮询
        public void LoopSkills(long Now2)
        {
            try
            { 
                LoopSkills2(Now2);
            }catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void LoopSkills2(long Now2)
        {
            foreach (var Skill in SkillList)
            {
                (int, long) SrcKey = (Skill.ActorType, Skill.ActorID);
                if (!ActorMap.ContainsKey(SrcKey))
                {
                    // 技能拥有者不在地图
                    SkillList.Remove(Skill);
                    continue;
                }
                MapActor SrcActor = ActorMap[SrcKey];

                SkillConfig config = Skill.SkillConfig;
                int wave = Skill.CurWave;
                if (config.Waves.Count >= wave+1)   // 技能结束
                {
                    
                    SkillList.Remove(Skill);
                    continue;
                }
                int interval = config.Waves[wave];
                // 是否能触发下一波次伤害
                if (Skill.SkillTime + interval <= Now2) 
                    continue;

                Dictionary<(int, long), MapActor> HurtMap = MapCommon.FilterHurt(this, Skill, SrcActor);
                List<MapEffect> EffectMaps = new();
                foreach(var TarActor in HurtMap.Values)
                {
                    List<MapEffect> Effects = MapFight.DoFight(this, SrcActor, TarActor, config);
                    EffectMaps.AddRange(Effects);
                }
                Skill.CurWave++;
                Skill.SkillTime = Now2;

                // todo 同步效果
            }
        }
    }
}
