using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetObjects.Core;
using OsmLoader;

namespace OsmAspGetter
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "map.osm");
            if (!File.Exists(path))
            {
                Console.WriteLine($"Файл не найден {path}");
                Console.WriteLine("Нажмите любую клавиу для выхода...");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Идет загрузка графа...");
            var graph = new OsmGraphLoader(path).GenerateGraph();
            Console.CursorLeft = 0;
            Console.WriteLine("Граф загружен.");

            Console.WriteLine("Идет форматирование графа...");
            var vertexDictionary = new Dictionary<long, int>(graph.Vertices.Count);
            var newId = 0;
            foreach (var vertex in graph.Vertices.Values)
                if (!vertexDictionary.ContainsKey(vertex.Id))
                    vertexDictionary.Add(vertex.Id, newId++);

            var edgeList = new List<(int, int)>();
            foreach (var edge in graph.Edges)
                edgeList.Add((vertexDictionary[edge.A.Id], vertexDictionary[edge.B.Id]));

            Console.CursorLeft = 0;
            Console.WriteLine("Граф отформатирован.");

            Console.WriteLine("Идет подсчет путей...");
            var result = new ConcurrentStack<double>();
            var count = 0;
            var total = graph.Vertices.Count;
            Parallel.For(0, graph.Vertices.Count, i =>
            {
                var dijkstra = new Dijkstra(edgeList, graph.Vertices.Count);
                result.Push((double) dijkstra.FindPaths(i).Sum(_ => _.path.Length) / graph.Vertices.Count);
                Interlocked.Increment(ref count);
                Console.WriteLine($"{count}/{total}");
            });
            Console.WriteLine($"Результат: {result.Sum() / graph.Vertices.Count}");
            Console.WriteLine("Нажмите любую клавиу для выхода...");
            Console.ReadKey();
        }
    }
}
