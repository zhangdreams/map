using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class MapPos
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
        public long ID { get; set; }    // 实例ID
        public long RoleID { get; set; } // 技能释放者
        public long TargetID { get; set; } = 0; // 技能目标ID(非指向性技能通常为0)
        public int SkillId { get; set; } // 技能配置ID
        public MapPos Pos { get; set; } = new(0, 0, 0); // 目标位置
        public int CurWave { get; set; } // 当前技能段数
        public List<int> intervals = new(); // 技能段数之间的间隔
    }
}
