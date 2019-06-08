using GraphSdk.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphSdk
{
	public static class CalculatingTools
	{
		private static Random _random;

		static CalculatingTools()
		{
			_random = new Random();
		}
		public static double GetAverageLinkCount(Graph graph) =>
			(double)graph.Edges.Count / graph.Vertices.Count * 2;

		private static bool CheckPath(this Graph graph, Vertex a, Vertex b, Dictionary<Edge, bool> active)
		{
			var distance = graph.Vertices.ToDictionary(vertex => vertex.Id, vertex => int.MaxValue);
			var used = graph.Vertices.ToDictionary(vertex => vertex.Id, vertex => false);
			distance[a.Id] = 0;
			foreach (var i in graph.Vertices)
			{
				Vertex v = null;
				foreach (var j in graph.Vertices)
					if (!used[j.Id] && (v == null || distance[j.Id] < distance[v.Id]))
						v = j;
				if (distance[v.Id] == int.MaxValue)
					break;
				used[v.Id] = true;
				foreach (var connected in v.ConnectedVertices)
					if (active[new Edge(v, connected)])
						if (distance[v.Id] + 1 < distance[connected.Id])
							distance[connected.Id] = distance[v.Id] + 1;
			}

			return distance[b.Id] != int.MaxValue;
		}

		private static bool CheckPath(this Graph graph, Vertex a, Vertex b, Dictionary<int, bool> active)
		{
			var distance = graph.Vertices.ToDictionary(vertex => vertex.Id, vertex => int.MaxValue);
			var used = graph.Vertices.ToDictionary(vertex => vertex.Id, vertex => false);
			distance[a.Id] = 0;
			foreach (var i in graph.Vertices)
			{
				Vertex v = null;
				foreach (var j in graph.Vertices)
					if (!used[j.Id] && (v == null || distance[j.Id] < distance[v.Id]))
						v = j;
				if (distance[v.Id] == int.MaxValue)
					break;
				used[v.Id] = true;
				foreach (var connected in v.ConnectedVertices)
					if (active[connected.Id])
						if (distance[v.Id] + 1 < distance[connected.Id])
							distance[connected.Id] = distance[v.Id] + 1;
			}

			return distance[b.Id] != int.MaxValue;

		}

		public static double GetVertexPercolation(Graph graph, double probability)
		{
			var active = graph.Vertices.ToDictionary(vertex => vertex.Id, vertex => _random.NextDouble() >= probability);
			var list = graph.Vertices.Where(vertex => active[vertex.Id]).ToList();
			var counter = 0;
			var tryCount = 10;
			for (int i = 0; i < tryCount; i++)
			{
				if (!list.Any())
					continue;
				var a = list[_random.Next(list.Count)];
				list.Remove(a);

				if (!list.Any())
					continue;
				var b = list[_random.Next(list.Count)];

				if (graph.CheckPath(a, b, active))
					counter++;
			}

			return (double)counter / tryCount;
		}

		public static double GetEdgePercolation(Graph graph, double probability)
		{
			var counter = 0;
			var tryCount = 10;

			for (int i = 0; i < tryCount; i++)
			{
				var active = graph.Edges.ToDictionary(edge => edge, vertex => _random.NextDouble() >= probability);

				var a = graph.Vertices[_random.Next(graph.Vertices.Count)];
				var b = graph.Vertices[_random.Next(graph.Vertices.Count)];

				if (graph.CheckPath(a, b, active))
					counter++;
			}

			return (double)counter / tryCount;
		}
	}
}