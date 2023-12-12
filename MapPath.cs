using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RpgMap
{
    /// <summary>
    /// 节点对象
    /// </summary>
    class Node
    {
        public int X { get; } = 0;
        public int Y { get; } = 0;
        public int G { get; set; } = 0;  // 从起始节点到当前节点的代价
        public int H { get; set; } = 0;  // 启发式函数值
        public int F { get { return G + H; } }  // 评估函数值
        public Node Parent { get; set; }    // 父节点

        public Node(int x, int y)
        {
            X = x;
            Y = y;
            G = 0;
            H = 0;
            Parent = null;
        }

        /// <summary>
        /// 返回是否到达一个节点
        /// </summary>
        /// <param name="other">目标节点</param>
        /// <returns></returns>
        public bool Arrival(Node other)
        {
            return other.X == X && other.Y == Y;
        }

        /// <summary>
        /// 返回format后的节点坐标字符串
        /// </summary>
        /// <returns></returns>
        public string Show()
        {
            return $"({X},{Y})";
        }
    }

    class MapPath
    {
        /// <summary>
        /// 寻路
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="start">起点</param>
        /// <param name="goal">终点</param>
        /// <returns>路径List or null</returns>
        public static List<Node>? FindPath(Map map, Node start, Node goal)
        {
            if ((start.X == goal.X || start.Y == goal.Y) && !ContainsObstacleBetween(map, start, goal))    // 中间没有障碍物，直接可达
                return new() { goal };

            List<Node>? ret = CheckIndirect(map, start, goal);
            if (ret != null)
                return ret;

            Dictionary<Node, int> opendict = new();
            Dictionary<Node, int> closedict = new();
            Dictionary<(int,int), int> neighborMaps = new(); // 增加步长

            opendict[start] = 1;

            while (opendict.Count > 0)
            {
                Node current = opendict.First().Key;
                
                foreach(Node node in opendict.Keys)
                {
                    if(node.F < current.F || (node.F == current.F && node.H < current.H))
                        current = node;
                }
                opendict.Remove(current);
                closedict[current] = 1;

                if (current.Arrival(goal))
                    return ReconstructPath(current);

                if (!neighborMaps.TryGetValue((current.X, current.Y), out int step))
                {
                    step = 1;
                    neighborMaps[(current.X, current.Y)] = 1;
                }
                else
                    neighborMaps[(current.X, current.Y)] = step + 1;

                List<Node> neighbors = GetNeighbors(map, current, step);
                if (neighbors.Count <= 0)
                    return null;
                foreach (Node neighbor in neighbors)
                {
                    if (closedict.ContainsKey(neighbor))
                        continue;

                    int tentativeG = current.G + CalculateDistance(map, current, neighbor);
                    if (!opendict.ContainsKey(neighbor) || tentativeG < neighbor.G)
                    {
                        neighbor.Parent = current;
                        neighbor.G = tentativeG;
                        neighbor.H = CalculateDistance(map, neighbor, goal);
                        if (!opendict.ContainsKey(neighbor))
                            opendict[neighbor] = 1;
                    }
                }
            }

            return null; // No path found
        }

        /// <summary>
        /// 检查能否通过简单的中间点间接到达
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="start">起点</param>
        /// <param name="goal">终点</param>
        /// <returns></returns>
        private static List<Node>? CheckIndirect(Map map, Node start, Node goal)
        {
            Node mid = new(start.X, goal.Y);
            if(!ContainsObstacleBetween(map, start, mid)) 
            { 
                return new List<Node>() 
                { 
                    mid,
                    goal
                };
            }
            mid = new(goal.X, start.Y);
            if (!ContainsObstacleBetween(map, start, mid))
            {
                return new List<Node>()
                {
                    mid,
                    goal
                };
            }
            return null;
        }

        /// <summary>
        /// 通过node的parent返回一条路径
        /// </summary>
        /// <param name="node">路径点</param>
        /// <returns></returns>
        private static List<Node> ReconstructPath(Node node)
        {
            List<Node> path = new();
            while (node != null)
            {
                path.Insert(0, node);
                node = node.Parent;
            }
            return path;
        }

        /// <summary>
        /// 返回一个坐标点周围的坐标点 4向
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="node">坐标节点</param>
        /// <param name="step">步长</param>
        /// <returns></returns>
        public static List<Node> GetNeighbors(Map map, Node node, int step)
        {
            List<Node> neighbors = new ();

            int[] dx = { -step, step, 0, 0 }; // 横向移动
            int[] dy = { 0, 0, -step, step }; // 纵向移动

            for (int i = 0; i < 4; i++)
            {
                int newX = node.X + dx[i];
                int newY = node.Y + dy[i];

                Node newNode = new (newX, newY);
                // 检查新的坐标是否合法，不超出地图边界并且不是障碍物
                if (IsValidCoordinate(map.MapID, newX, newY) && !IsObstacle(map, newX, newY) && !ContainsObstacleBetween(map, node, newNode))
                    neighbors.Add(newNode);

            }
            // 效率较低
            //List<(int, int)> list = new();
            //for (int i = node.X-step; i <= node.X+step; i++)
            //{
            //    AddPoint((i, node.Y-step), ref list);
            //    AddPoint((i, node.Y+step), ref list);

            //}
            //for (int j = node.Y - step; j <= node.Y + step; j++)
            //{
            //    AddPoint((node.X - step, j), ref list);
            //    AddPoint((node.X + step, j), ref list);
            //}
            //foreach ((int newX, int newY) in list)
            //{
            //    //int newX = node.X + dx[i];
            //    //int newY = node.Y + dy[i];

            //    Node newNode = new Node(newX, newY);
            //    // 检查新的坐标是否合法，不超出地图边界并且不是障碍物
            //    if (IsValidCoordinate(MapID, newX, newY) && !IsObstacle(MapID, newX, newY) && !ContainsObstacleBetween(MapID, node, newNode))
            //    {
            //        neighbors.Add(newNode);
            //    }
            //}
            return neighbors;
        }

        /// <summary>
        /// 计算两个节点的移动代价
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="a">A点坐标</param>
        /// <param name="b">B点坐标</param>
        /// <returns></returns>
        private static int CalculateDistance(Map map, Node a, Node b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;

            int distance = (int)(Math.Sqrt(dx * dx + dy * dy) * 10);
            if(ContainsObstacleBetween(map, a, b))
                distance *= 5;

            return distance;
        }

        /// <summary>
        /// 返回两个节点直线路径是否包含障碍物
        /// </summary>
        /// <param name="map">地图对象</param>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <returns></returns>
        private static bool ContainsObstacleBetween(Map map, Node start, Node end)
        {
            // 检查两个节点之间的直线路径是否包含障碍物
            int dx = Math.Abs(end.X - start.X);
            int dy = Math.Abs(end.Y - start.Y);

            int xIncrement = (start.X < end.X) ? 1 : -1;
            int yIncrement = (start.Y < end.Y) ? 1 : -1;

            int x = start.X;
            int y = start.Y;
            int error = dx - dy;

            while (x != end.X || y != end.Y)
            {
                if (IsObstacle(map, x, y) && (x != start.X || y != start.Y))
                    return true;

                int doubleError = error * 2;

                if (doubleError > -dy)
                {
                    error -= dy;
                    x += xIncrement;
                }

                if (doubleError < dx)
                {
                    error += dx;
                    y += yIncrement;
                }
            }
            return false; // 没有发现障碍物
        }

        /// <summary>
        /// 返回一个坐标是否合法
        /// </summary>
        /// <param name="MapID">地图id</param>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <returns></returns>
        public static bool IsValidCoordinate(int MapID, int x, int y)
        {
            var config = MapReader.GetConfig(MapID);
            if (config == null)
                return false;
            int mapWidth = config.Width; 
            int mapHeight = config.Height;

            return x >= 0 && x <= mapWidth && y >= 0 && y <= mapHeight;
        }

        /// <summary>
        /// 返回一个坐标是否是固定阻挡 
        /// </summary>
        /// <param name="MapID">地图id</param>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <returns></returns>
        public static bool IsObstacle(int mapID, int x, int y)
        {
            var config = MapReader.GetConfig(mapID);
            if (config == null)
                return true;

            var pos = new ConfigPos(x, y);
            return config.UnWalkList.ContainsKey(pos);
        }

        /// <summary>
        /// 返回一个坐标是否是动态阻挡
        /// </summary>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <param name="posdict">动态阻挡坐标</param>
        /// <returns></returns>
        public static bool IsObstacle(int x, int y, Dictionary<(int,int), MapActor> posdict)
        {
            if(posdict.ContainsKey((x,y)))
                return true;
            foreach((int px, int py) in posdict.Keys)
            {
                if(MapTool.CheckDistance(x, y, px, py, MapMgr.SphereRis * 2))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 返回一个坐标是否是阻挡
        /// 相当于是上面两个的组合
        /// </summary>
        /// <param name="MapID">地图id</param>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <returns></returns>
        public static bool IsObstacle(Map map, int x, int y)
        {
            var config = MapReader.GetConfig(map.MapID);
            if (config == null)
                return true;
            var pos = new ConfigPos(x, y);
            if(config.UnWalkList.ContainsKey(pos))
                return true;

            // 玩家(动态障碍物)
            Dictionary<(int, int), MapActor> posDict = map.Aoi.GetGridePosList(map, x, y);
            if(posDict.ContainsKey((x,y)))
                return true;
            foreach ((int px, int py) in posDict.Keys)
            {
                if (MapTool.GetDistance(x, y, px, py) < MapMgr.SphereRis * 2)
                    return true;
            }
            return false;
        }
    }

}
