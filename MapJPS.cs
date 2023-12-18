using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    public class PriorityQueue<T>
    {
        private SortedDictionary<int, Queue<T>> priorityQueue = new ();

        public int Count
        {
            get
            {
                int count = 0;
                foreach (var queue in priorityQueue.Values)
                    count += queue.Count;
                return count;
            }
        }

        public void Enqueue(T item, int priority)
        {
            if (!priorityQueue.TryGetValue(priority, out var queue))
            {
                queue = new Queue<T>();
                priorityQueue[priority] = queue;
            }

            queue.Enqueue(item);
        }

        public T Dequeue()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Queue is empty");
            }

            var queue = priorityQueue.First();
            var item = queue.Value.Dequeue();

            if (queue.Value.Count == 0)
            {
                priorityQueue.Remove(queue.Key);
            }

            return item;
        }
    }

    class MapJPS
    {
        public static List<Node>? JumpPointSearch(Map map, Node start, Node goal)
        {
            if ((start.X == goal.X || start.Y == goal.Y) && !MapPath.ContainsObstacleBetween(map, start, goal))    // 中间没有障碍物，直接可达
                return new() { goal };

            List<Node>? ret = MapPath.CheckIndirect(map, start, goal);
            if (ret != null)
                return ret;

            PriorityQueue<Node> openSet = new();
            Dictionary<Node, Node> cameFrom = new();
            Dictionary<Node, int> gScore = new();

            openSet.Enqueue(start, 0);
            gScore[start] = 0;

            while (openSet.Count > 0)
            {
                Node current = openSet.Dequeue();

                if (current.Arrival(goal))
                {
                    return ReconstructPath(cameFrom, current);
                }

                List<Node> successors = FindSuccessors(map, current);
                if (successors.Count <= 0)
                    return null;

                foreach (Node successor in successors)
                {
                    int tentativeGScore = gScore[current] + successor.G;

                    if (!gScore.ContainsKey(successor) || tentativeGScore < gScore[successor])
                    {
                        gScore[successor] = tentativeGScore;

                        int priority = tentativeGScore + MapPath.CalculateDistance(map, successor, goal);
                        openSet.Enqueue(successor, priority);

                        cameFrom[successor] = current;
                    }
                }
            }
            return null;
        }

        private static List<Node> ReconstructPath(Dictionary<Node, Node> cameFrom, Node current)
        {
            List<Node> path = new();
            while (cameFrom.ContainsKey(current))
            {
                path.Insert(0, current);
                current = cameFrom[current];
            }
            path.Insert(0, current); // 添加起点
            return path;
        }

        private static List<Node> FindSuccessors(Map map, Node node)
        {
            List<Node> successors = new();

            // 左上、上、右上、左、右、左下、下、右下
            int[,] directions = { { -1, -1 }, { 0, -1 }, { 1, -1 }, { -1, 0 }, { 1, 0 }, { -1, 1 }, { 0, 1 }, { 1, 1 } };

            for (int i = 0; i < directions.GetLength(0); i++)
            {
                int x = node.X + directions[i, 0];
                int y = node.Y + directions[i, 1];

                if (MapPath.IsValidCoordinate(map.MapID, x, y) && !MapPath.IsObstacle(map, x, y))
                    successors.Add(new Node(x, y));
            }
            return successors;
        }
    }
}
