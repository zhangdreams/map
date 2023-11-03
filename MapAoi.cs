using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class MapAoi
    {
        private static readonly int GrideLength = 16;
        private static readonly int GrideWidth = 16;
        public int Length { get; set; } // 地图长
        public int Width { get; set; } // 地图宽

        public Dictionary<(int,int), List<(int, long)>> Grides = new(); // 用来保存每一个格子的对象
        public Dictionary<(int, int), List<(int, int)>> Nerbors = new(); // 保存某个格子的九宫格格子索引

        public MapAoi(int Length, int Width) 
        {
            this.Length = Length;
            this.Width = Width;
            int GrideLen = (int)Length / GrideLength;
            int GrideWid = (int)Width / GrideWidth;
            for (int i = 0; i < GrideLen; i++)
            {
                for (int j = 0; j < GrideWid; j++)
                {
                    List<(int, long)> values = new();
                    Grides[(i, j)] = values;
                }
            }
        }

        // 返回某一个格子内的对象
        private List<(int, long)> GetGride(double X, double Y)
        {
            int GrideX = (int)X / GrideLength;
            int GrideY = (int)Y / GrideWidth;
            return GetGride(GrideX, GrideY);
        }
        private List<(int, long)> GetGride(int GrideX, int GrideY)
        {
            var key = (GrideX, GrideY);
            return GetGride(key);
        }
        private List<(int, long)> GetGride((int,int) key)
        {
            if (!Grides.ContainsKey(key))
            {
                List<(int, long)> values = new();
                return values;
            }
            return Grides[key];
        }

        // 返回某一个坐标点(格子)的九宫格索引
        private List<(int, int)> GetNeighbors(double X, double Y)
        {
            int GrideX = (int)X / GrideLength;
            int GrideY = (int)Y / GrideWidth;
            return GetNeighbors(GrideX, GrideY);
        }
        private List<(int, int)> GetNeighbors(int GrideX, int GrideY)
        {
            var key = (GrideX, GrideY);
            if (Nerbors.ContainsKey(key))
            {
                return Nerbors[key];
            }
            List<(int, int)> values = new();
            for (int i = GrideX-1; i <= GrideX+1; i++)
            {
                for (int j = GrideY-1; j <= GrideY+1; j++)
                {
                    var k = (i, j);
                    if(Grides.ContainsKey(k))
                        values.Add(k);
                }
            }
            Nerbors[key] = values;
            return values;
        }

        // 返回一个坐标点(格子)的九宫格对象
        public List<(int, long)> GetAoi(double X, double Y)
        {
            int GrideX = (int)X / GrideLength;
            int GrideY = (int)Y / GrideWidth;
            return GetAoi(GrideX, GrideY);
        }
        public List<(int, long)> GetAoi(int GrideX, int GrideY)
        {
            List<(int, int)> Keys = GetNeighbors(GrideX, GrideY);
            List<(int, long)> values = new();
            foreach ((int, int) k in Keys)
            {
                List<(int, long)> v = GetGride(k);
                values.AddRange(v);
            }
            return values;
        }

        // 进入某一个格子
        private void EnterArea(int Type, long ID, int GrideX, int GrideY)
        {
            var key = (GrideX, GrideY);
            List<(int, long)> list = Grides[key];
            var v = (Type, ID);
            if(!list.Contains(v))
                list.Add(v);
        }

        // 离开某个格子
        private void ExitArea(int Type, long ID, int GrideX, int GrideY)
        {
            var key = (GrideX, GrideY);
            List<(int, long)> list = Grides[key];
            var v = (Type, ID);
            list.Remove(v);
        }

        // 移动检测是否改变了格子
        // X,Y 移动前坐标 
        // X2,Y2移动后坐标
        public void DoMove(int Type, long ID, double X, double Y, double X2, double Y2)
        {
            int GrideX = (int)X / GrideLength;
            int GrideY = (int)Y / GrideWidth;
            int GrideX2 = (int)X2 / GrideLength;
            int GrideY2 = (int)Y2 / GrideWidth;
            if (GrideX != GrideX2 && GrideY != GrideY2)
            {
                // 改变了格子
                ExitArea(Type, ID, GrideX, GrideY);
                EnterArea(Type, ID, GrideX2, GrideY2 );
                // todo 视野广播
            }
        }
    }
}
