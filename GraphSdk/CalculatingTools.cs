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

		public static int GetVertexPercolationThreshold(Graph graph)
		{
			var inactiveCount = 0;
			var active = graph.Vertices.ToDictionary(vertex => vertex.Id, vertex => true);
			//do
			//{
			//	active[graph.Vertices[_random.Next(graph.Vertices.Count)].Id] = false;
			//} while (active.Count(pair => !pair.Value) < inactiveCount);

			//Выбираем два случайных не связанных узла
			var a = graph.Vertices[_random.Next(graph.Vertices.Count)];
			Vertex b;
			do
				b = graph.Vertices[_random.Next(graph.Vertices.Count)];
			while (a.ConnectedVertices.Exists(vertex => vertex.Id == b.Id));
			bool isPathFound;
			do
			{
				var used = graph.Vertices.ToDictionary(vertex => vertex.Id, vertex => false);
				isPathFound = false;
				DfsPathSearch(a);
				void DfsPathSearch(Vertex c)
				{
					if (!active[c.Id] || used[c.Id])
						return;
					used[c.Id] = true;
					if (c.ConnectedVertices.Exists(vertex => vertex.Id == b.Id))
					{
						isPathFound = true;
						return;
					}

					foreach (var connectedVertex in c.ConnectedVertices)
					{
						if (isPathFound)
							return;
						DfsPathSearch(connectedVertex);
					}
				}
			} while (isPathFound && Inactivate() < graph.Vertices.Count);

			return inactiveCount;


			int Inactivate()
			{
				//var toDeactivate = new List<Vertex>();
				//foreach (var vertex in graph.Vertices)
				//	if (active[vertex.Id] && vertex.ConnectedVertices.Any(v => !active[v.Id]))
				//		toDeactivate.Add(vertex);
				//foreach (var vertex in toDeactivate) active[vertex.Id] = false;
				//return inactiveCount += toDeactivate.Count;
				active[_random.Next(active.Count)] = false;
				return ++inactiveCount;
			}
		}

		public static int GetEdgePercolationThreshold(Graph graph)
		{
			var inactiveCount = 0;
			var active = graph.Edges.ToDictionary(edge => edge, edge => true);
			//do
			//{
			//	active[graph.Edges[_random.Next(graph.Edges.Count)]] = false;
			//} while (active.Count(pair => !pair.Value) < inactiveCount);

			var edgeConnectivity = graph.GetEdgeConnectivity();
			//Выбираем два случайных не связанных узла
			var a = graph.Vertices[_random.Next(graph.Vertices.Count)];
			Vertex b;
			do
				b = graph.Vertices[_random.Next(graph.Vertices.Count)];
			while (a.ConnectedVertices.Exists(vertex => vertex.Id == b.Id));
			bool isPathFound;
			var used = graph.Edges.ToDictionary(edge => edge, edge => false);
			do
			{
				foreach (var edge in graph.Edges) used[edge] = false;
				isPathFound = false;
				DfsPathSearch(a);

				void DfsPathSearch(Vertex c)
				{
					if (c.Equals(b))
					{
						isPathFound = true;
						return;
					}

					foreach (var connectedVertex in c.ConnectedVertices)
					{
						if (isPathFound)
							return;
						if ((!active[new Edge(c, connectedVertex)]) || used[new Edge(c, connectedVertex)])
							continue;
						used[new Edge(c, connectedVertex)] = true;
						DfsPathSearch(connectedVertex);
					}
				}



			} while (isPathFound && Inactivate() < graph.Edges.Count);

			return inactiveCount;
			int Inactivate()
			{
				//var toDeactivate = new List<Edge>();
				//foreach (var edge in graph.Edges)
				//	if (active[edge] && edgeConnectivity[edge].Any(_ => !active[_]))
				//		toDeactivate.Add(edge);
				//foreach (var edge in toDeactivate) active[edge] = false;
				//return inactiveCount += toDeactivate.Count;
				active[graph.Edges[_random.Next(graph.Edges.Count)]] = false;
				return ++inactiveCount;
			}
		}

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

		public static double GetNewVertexPercolation(Graph graph, double probability)
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

		public static double GetNewEdgePercolation(Graph graph, double probability)
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