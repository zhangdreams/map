using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RpgMap
{
    class Node
    {
        public int X { get; }
        public int Y { get; }
        public int G { get; set; }  // 从起始节点到当前节点的代价
        public int H { get; set; }  // 启发式函数值
        public int F { get { return G + H; } }  // 评估函数值
        public Node Parent { get; set; }

        public Node(int x, int y)
        {
            X = x;
            Y = y;
            G = 0;
            H = 0;
            Parent = null;
        }
    }

    class AStar
    {
        public static List<Node> FindPath(Node start, Node goal)
        {
            List<Node> openSet = new();
            List<Node> closedSet = new();

            openSet.Add(start);

            while (openSet.Count > 0)
            {
                Node current = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].F < current.F || (openSet[i].F == current.F && openSet[i].H < current.H))
                    {
                        current = openSet[i];
                    }
                }

                openSet.Remove(current);
                closedSet.Add(current);

                if (current == goal)
                {
                    return ReconstructPath(current);
                }

                foreach (Node neighbor in GetNeighbors(current))
                {
                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    int tentativeG = current.G + CalculateDistance(current, neighbor);

                    if (!openSet.Contains(neighbor) || tentativeG < neighbor.G)
                    {
                        neighbor.Parent = current;
                        neighbor.G = tentativeG;
                        neighbor.H = CalculateDistance(neighbor, goal);
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
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

        private static List<Node> GetNeighbors(Node node)
        {
            List<Node> neighbors = new ();

            int[] dx = { -1, 1, 0, 0 }; // 横向移动
            int[] dy = { 0, 0, -1, 1 }; // 纵向移动

            for (int i = 0; i < 4; i++)
            {
                int newX = node.X + dx[i];
                int newY = node.Y + dy[i];

                // 检查新的坐标是否合法，不超出地图边界并且不是障碍物
                if (IsValidCoordinate(newX, newY) && !IsObstacle(newX, newY))
                {
                    neighbors.Add(new Node(newX, newY));
                }
            }

            return neighbors;
        }

        private static int CalculateDistance(Node a, Node b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private static bool IsValidCoordinate(int x, int y)
        {
            int mapWidth = 100; // todo 先随便给个大小
            int mapHeight = 100;

            return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
        }

        private static bool IsObstacle(int x, int y)
        {
            //// 假设地图数据存储在二维数组中，其中障碍物用特定的值表示
            //int[,] mapData = /* 获取地图数据 */;

            //// 检查坐标是否包含障碍物值
            //return mapData[x, y] == /* 障碍物值 */;

            return false;
        }

    }

}
