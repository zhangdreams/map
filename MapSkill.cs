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
        public long RoleID { get; set; } // 技能释放者
        public long TargetID { get; set; } = 0; // 技能目标ID(非指向性技能通常为0)
        public int SkillId { get; set; } // 技能配置ID
        public MapPos Pos { get; set; } = new(0, 0, 0); // 目标位置
        public int CurWave { get; set; } // 当前技能段数
        public List<int> intervals = new(); // 技能段数之间的间隔
        public long SkillTime { get; set; } // 上一段技能触发时间
        public MapSkill(long roleID, long targetID, int skillId, MapPos pos)
        {
            ID = GetEnID();
            RoleID = roleID;
            TargetID = targetID;
            SkillId = skillId;
            Pos = pos;
            CurWave = 0;
            //this.intervals = ;
            SkillTime = 0;
        }

        public static long GetEnID()
        {
            return SkillEnID++;
        }

        // todo  根据养成对技能配置影响判断下要不要缓存一下技能配置？
    }
}
