using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    /// <summary>
    /// aoi
    /// </summary>
    internal class MapAoi
    {
        private static readonly int GrideLength = 16;
        private static readonly int GrideWidth = 16;
        private int Length { get;} // 地图长
        private int Width { get; } // 地图宽

        private Dictionary<(int,int), List<(int, long)>> Grides = new(); // 用来保存每一个格子的对象
        private Dictionary<(int, int), List<(int, int)>> Nerbors = new(); // 保存某个格子的九宫格格子索引

        public MapAoi(int length, int width) 
        {
            this.Length = length;
            this.Width = width;
            int GrideLen = (int)length / GrideLength;
            int GrideWid = (int)width / GrideWidth;
            for (int i = 0; i <= GrideLen; i++)
            {
                for (int j = 0; j <= GrideWid; j++)
                {
                    List<(int, long)> values = new();
                    // Log.R($"aoi gride key {(i,j)}");
                    Grides[(i, j)] = values;
                }
            }
        }

        /// <summary>
        /// 返回某一个格子内的对象
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public List<(int, long)> GetGride(double x, double y)
        {
            int grideX = (int)x / GrideLength;
            int grideY = (int)y / GrideWidth;
            return GetGride(grideX, grideY);
        }
        /// <summary>
        /// 返回某一个格子内的对象
        /// </summary>
        /// <param name="grideX">格子X坐标</param>
        /// <param name="grideY">格子Y坐标</param>
        /// <returns></returns>
        private List<(int, long)> GetGride(int grideX, int grideY)
        {
            var key = (grideX, grideY);
            return GetGride(key);
        }
        /// <summary>
        /// 返回某一个格子内的对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private List<(int, long)> GetGride((int,int) key)
        {
            if (Grides.TryGetValue(key, out var value))
                return value;
            return new List<(int, long)>();
        }

        /// <summary>
        /// 返回某一个坐标点(格子)的九宫格索引
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private List<(int, int)> GetNeighbors(double x, double y)
        {
            int grideX = (int)x / GrideLength;
            int grideY = (int)y / GrideWidth;
            return GetNeighbors(grideX, grideY);
        }
        /// <summary>
        /// 返回某一个坐标点(格子)的九宫格索引
        /// </summary>
        /// <param name="grideX">格子X坐标</param>
        /// <param name="grideY">格子Y坐标</param>
        /// <returns></returns>
        private List<(int, int)> GetNeighbors(int grideX, int grideY)
        {
            var key = (grideX, grideY);
            if(Nerbors.TryGetValue(key, out var neighbors))
                return neighbors;
            List<(int, int)> values = new();
            for (int i = grideX - 1; i <= grideX + 1; i++)
            {
                for (int j = grideY-1; j <= grideY+1; j++)
                {
                    var k = (i, j);
                    if(Grides.ContainsKey(k))
                        values.Add(k);
                }
            }
            Nerbors[key] = values;
            return values;
        }

        /// <summary>
        /// 返回一个坐标点(格子)的九宫格对象
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public List<(int, long)> GetAoi(double x, double y)
        {
            int grideX = (int)x / GrideLength;
            int grideY = (int)y / GrideWidth;
            return GetAoi(grideX, grideY);
        }
        /// <summary>
        /// 返回一个坐标点(格子)的九宫格对象
        /// </summary>
        /// <param name="grideX">格子X坐标</param>
        /// <param name="grideY">格子Y坐标</param>
        /// <returns></returns>
        public List<(int, long)> GetAoi(int grideX, int grideY)
        {
            List<(int, int)> keys = GetNeighbors(grideX, grideY);
            List<(int, long)> values = new();
            foreach ((int, int) k in keys)
            {
                List<(int, long)> v = GetGride(k);
                for(int i = 0; i < v.Count; i++ )
                    values.Add(v[i]);
            }
            return values;
        }

        /// <summary>
        /// 进入某一个格子
        /// </summary>
        /// <param name="type">actor类型</param>
        /// <param name="id">actor id</param>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        public void EnterArea(int type, long id, double x, double y)
        {
            int GrideX = (int)x / GrideLength;
            int GrideY = (int)y / GrideWidth;
            EnterArea(type, id, GrideX, GrideY);
        }
        private void EnterArea(int type, long id, int grideX, int grideY)
        {
            //var key = (GrideX, GrideY);
            List<(int, long)> list = GetGride(grideX, grideY);
            var v = (type, id);
            if(!list.Contains(v))
                list.Add(v);
        }

        /// <summary>
        /// 离开某个格子
        /// </summary>
        /// <param name="type">actor类型</param>
        /// <param name="id">actor id</param>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        public void ExitArea(int type, long id, double x, double y)
        {
            int GrideX = (int)x / GrideLength;
            int GrideY = (int)y / GrideWidth;
            ExitArea(type, id, GrideX, GrideY);
        }
        private void ExitArea(int type, long id, int grideX, int grideY)
        {
            List<(int, long)> list = GetGride(grideX, grideY);
            var v = (type, id);
            list.Remove(v);
        }

        /// <summary>
        /// 移动检测是否改变了格子
        /// </summary>
        /// <param name="type">actor类型</param>
        /// <param name="id">actor id</param>
        /// <param name="x">移动前x坐标</param>
        /// <param name="y">移动前y坐标</param>
        /// <param name="x2">移动后x坐标</param>
        /// <param name="y2">移动后y坐标</param>
        public void DoMove(int type, long id, double x, double y, double x2, double y2)
        {
            int grideX = (int)x / GrideLength;
            int grideY = (int)y / GrideWidth;
            int grideX2 = (int)x2 / GrideLength;
            int grideY2 = (int)y2 / GrideWidth;
            if (grideX != grideX2 && grideY != grideY2)
            {
                // 改变了格子
                ExitArea(type, id, grideX, grideY);
                EnterArea(type, id, grideX2, grideY2 );
                // todo 视野广播
            }
        }

        /// <summary>
        /// 返回地图内某一个坐标所在的九宫格内实例的坐标
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <returns></returns>
        public Dictionary<(int, int), MapActor> GetGridePosList(Map map, double x, double y)
        {
            List<(int, long)> keys = GetAoi(x, y);
            Dictionary<(int, int), MapActor> posDict = new();
            foreach (var key in keys)
            {
                var actor = MapCommon.GetActor(map, key);
                if (actor == null)
                    continue;
                MapPos pos = actor.GetPos();
                (int, int) p = ((int)pos.x, (int)pos.y);
                posDict[p] = actor;
            }
            return posDict;
        }
    }
}
