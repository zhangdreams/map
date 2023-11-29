using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RpgMap
{
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

        public bool Arrival(Node other)
        {
            return other.X == X && other.Y == Y;
        }
    }

    class MapPath
    {
        public static List<Node>? FindPath(int MapID, Node start, Node goal)
        {
            if (!ContainsObstacleBetween(MapID, start, goal))    // 中间没有障碍物，直接可达
                return new() { goal };

            List<Node> openSet = new();
            List<Node> closedSet = new();
            Dictionary<(int,int), int> neighborMaps = new(); // 增加步长

            openSet.Add(start);

            while (openSet.Count > 0)
            {
                Node current = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].F < current.F || (openSet[i].F == current.F && openSet[i].H < current.H))
                        current = openSet[i];
                }

                openSet.Remove(current);
                closedSet.Add(current);

                if (current.Arrival(goal))
                    return ReconstructPath(current);

                if (!neighborMaps.TryGetValue((current.X, current.Y), out int step))
                {
                    step = 1;
                    neighborMaps[(current.X, current.Y)] = 1;
                }
                else
                    neighborMaps[(current.X, current.Y)] = step + 1;

                //int step = 1;
                //if (!neighborMaps.ContainsKey((current.X,current.Y)))
                //    neighborMaps[(current.X, current.Y)] = 1;
                //else
                //{
                //    step = neighborMaps[(current.X, current.Y)];
                //    neighborMaps[(current.X, current.Y)] = step + 1;
                //}
                List<Node> Neighbors = GetNeighbors(MapID, current, step);
                if (Neighbors.Count <= 0)
                    return null;
                foreach (Node neighbor in Neighbors)
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    int tentativeG = current.G + CalculateDistance(MapID, current, neighbor);
                    if (!openSet.Contains(neighbor) || tentativeG < neighbor.G)
                    {
                        neighbor.Parent = current;
                        neighbor.G = tentativeG;
                        neighbor.H = CalculateDistance(MapID, neighbor, goal);
                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return null; // No path found
        }

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

        public static List<Node> GetNeighbors(int MapID, Node node, int step)
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
                if (IsValidCoordinate(MapID, newX, newY) && !IsObstacle(MapID, newX, newY) && !ContainsObstacleBetween(MapID, node, newNode))
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

        public static void AddPoint((int, int) P, ref List<(int, int)> list)
        {
            if(!list.Contains(P))
                list.Add(P);
        }

        private static int CalculateDistance(int MapID, Node a, Node b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;

            int distance = (int)(Math.Sqrt(dx * dx + dy * dy) * 10);
            if(ContainsObstacleBetween(MapID, a, b))
                distance *= 5;

            return distance;
        }

        private static bool ContainsObstacleBetween(int MapID, Node start, Node end)
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
                if (IsObstacle(MapID, x, y) && x != start.X && y != start.Y)
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

        public static bool IsValidCoordinate(int MapID, int x, int y)
        {
            var config = MapReader.GetConfig(MapID);
            if (config == null)
                return false;
            int mapWidth = config.Width; 
            int mapHeight = config.Height;

            return x >= 0 && x <= mapWidth && y >= 0 && y <= mapHeight;
        }

        public static bool IsObstacle(int MapID, int x, int y)
        {
            var config = MapReader.GetConfig(MapID);
            if (config == null)
                return true;

            var pos = new ConfigPos(x, y);
            return config.UnWalkList.Contains(pos);
        }

    }

}
