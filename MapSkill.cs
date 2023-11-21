using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    public struct MapPos
    {
        public double x;
        public double y;
        public int dir;
        public MapPos(double x, double y)
        {
            this.x = x;
            this.y = y;
            this.dir = 0;
        }
        public MapPos(double x, double y, int dir)
        {
            this.x = x;
            this.y = y;
            this.dir = dir;
        }
    }
    internal class MapSkill
    {
        public static long SkillEnID = 1;
        public long ID { get; set; }    // 实例ID
        public int ActorType { get; set; }
        public long ActorID { get; set; } // 技能释放者
        public List<(int,long)> TargetMap { get; set; } // 技能目标ID(非指向性技能通常为空)
        public int SkillId { get; set; } // 技能配置ID
        public MapPos Pos { get; set; } = new(0, 0, 0); // 目标位置
        public int CurWave { get; set; } // 当前技能段数
        public long SkillTime { get; set; } // 上一段技能触发时间
        public SkillConfig SkillConfig { get; set; } = new(); // 技能配置
        public MapSkill(int actorType, long actorID, List<(int,long)> TargetMap, int skillId, MapPos pos)
        {
            ID = GetEnID();
            ActorType = actorType;
            ActorID = actorID;
            this.TargetMap = TargetMap;
            SkillId = skillId;
            Pos = pos;
            CurWave = 0;
            SkillTime = 0;
            SkillConfig = SkillReader.GetConfig(skillId) ?? new() ;
        }

        public static long GetEnID()
        {
            return SkillEnID++;
        }

        
    }

    // 地图内的技能信息
    internal class ActorSkill
    {
        public int SkillID { get; set; } // 技能配置ID
        public int Pos { get; set; } // 技能位
        public long UseTime { get; set; } // 上次技能释放时间

        public ActorSkill(int SkillID, int Pos)
        {
            this.SkillID = SkillID;
            this.Pos = Pos;
            UseTime = 0;
        }
        public ActorSkill(int skillId, int pos, long useTime)
        {
            SkillID = skillId;
            this.Pos = pos;
            UseTime = useTime;
        }

    }
}
