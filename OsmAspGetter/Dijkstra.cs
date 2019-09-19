using System;
using System.Collections.Generic;
using System.Linq;
using static System.Linq.Enumerable;
using EdgeList = System.Collections.Generic.List<(int node, double weight)>;

namespace NetObjects.Core
{
    public class Dijkstra
    {
        private readonly Graph _graph;

        public Dijkstra(IEnumerable<(int, int)> edges, int vertexCount)
        {
            _graph = new Graph(vertexCount);
            foreach (var (item1, item2) in edges)
                _graph.AddEdge(item1, item2, 1);
        }

        public int[] FindPath(int startNode, int endNode)
        {
            var path = _graph.FindPath(startNode);
            return double.IsPositiveInfinity(path[endNode].distance) 
                ? new int[0] 
                : Path(path, startNode, endNode).Select(p => p.node).ToArray();
        }

        public IEnumerable<(int destination, int[] path)> FindPaths(int startNode)
        {
            var path = _graph.FindPath(startNode);
            for (var i = 0; i < path.Length; i++)
                yield return (i, FindPath(startNode, i));

        }

        IEnumerable<(double distance, int node)> Path((double distance, int prev)[] path, int start, int destination)
        {
            yield return (path[destination].distance, destination);
            for (var i = destination; i != start; i = path[i].prev)
                yield return (path[path[i].prev].distance, path[i].prev);
        }

        sealed class Graph
        {
            private readonly List<EdgeList> adjacency;

            public Graph(int vertexCount) => 
                adjacency = Range(0, vertexCount)
                    .Select(v => new EdgeList())
                    .ToList();

            public int Count => adjacency.Count;
            private bool HasEdge(int s, int e) => adjacency[s].Any(p => p.node == e);
            public bool RemoveEdge(int s, int e) => adjacency[s].RemoveAll(p => p.node == e) > 0;

            public bool AddEdge(int s, int e, double weight)
            {
                if (HasEdge(s, e))
                    return false;
                adjacency[s].Add((e, weight));
                return true;
            }

            public (double distance, int prev)[] FindPath(int start)
            {
                var info = Range(0, adjacency.Count)
                    .Select(i => (distance: double.PositiveInfinity, prev: i))
                    .ToArray();
                info[start].distance = 0;
                var visited = new System.Collections.BitArray(adjacency.Count);

                var heap = new Heap<(int node, double distance)>((a, b) => a.distance.CompareTo(b.distance));
                heap.Push((start, 0));
                while (heap.Count > 0)
                {
                    var current = heap.Pop();
                    if (visited[current.node])
                        continue;
                    var edges = adjacency[current.node];
                    for (var n = 0; n < edges.Count; n++)
                    {
                        var v = edges[n].node;
                        if (visited[v])
                            continue;
                        var alt = info[current.node].distance + edges[n].weight;
                        if (!(alt < info[v].distance))
                            continue;
                        info[v] = (alt, current.node);
                        heap.Push((v, alt));
                    }

                    visited[current.node] = true;
                }

                return info;
            }
        }

        sealed class Heap<T>
        {
            private readonly IComparer<T> _comparer;
            private readonly List<T> _list = new List<T> { default };

            public Heap() : this(default(IComparer<T>)) { }

            public Heap(IComparer<T> comparer)
            {
                _comparer = comparer ?? Comparer<T>.Default;
            }

            public Heap(Comparison<T> comparison) : this(Comparer<T>.Create(comparison)) { }

            public int Count => _list.Count - 1;

            public void Push(T element)
            {
                _list.Add(element);
                SiftUp(_list.Count - 1);
            }

            public T Pop()
            {
                var result = _list[1];
                _list[1] = _list[_list.Count - 1];
                _list.RemoveAt(_list.Count - 1);
                SiftDown(1);
                return result;
            }

            private static int Parent(int i) => i / 2;
            private static int Left(int i) => i * 2;
            private static int Right(int i) => i * 2 + 1;

            private void SiftUp(int i)
            {
                while (i > 1)
                {
                    var parent = Parent(i);
                    if (_comparer.Compare(_list[i], _list[parent]) > 0)
                        return;
                    (_list[parent], _list[i]) = (_list[i], _list[parent]);
                    i = parent;
                }
            }

            private void SiftDown(int i)
            {
                for (var left = Left(i); left < _list.Count; left = Left(i))
                {
                    var smallest = _comparer.Compare(_list[left], _list[i]) <= 0 ? left : i;
                    var right = Right(i);
                    if (right < _list.Count && _comparer.Compare(_list[right], _list[smallest]) <= 0) smallest = right;
                    if (smallest == i)
                        return;
                    (_list[i], _list[smallest]) = (_list[smallest], _list[i]);
                    i = smallest;
                }
            }
        }
    }
}