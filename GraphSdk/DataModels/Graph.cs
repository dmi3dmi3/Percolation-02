using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphSdk.DataModels
{
	public class Graph
	{
		public Dictionary<long, Vertex> Vertices { get; set; }
		public List<Edge> Edges { get; set; }

		public Graph()
		{
			Vertices = new Dictionary<long, Vertex>();
			Edges = new List<Edge>();
		}

		public void AddEdge(Vertex a, Vertex b)
		{
			if (a.HasEdge(b))
				return;
			a.AddConnected(b);
			b.AddConnected(a);
			Edges.Add(new Edge(a, b));
		}

		public void AddEdge(long idA, long idB)
		{
			var a = Vertices[idA];
			var b = Vertices[idB];
			AddEdge(a, b);
		}

		public void RemoveEdge(Vertex a, Vertex b)
		{
			var edge = Edges.FirstOrDefault(_ => _.A.Equals(a) && _.B.Equals(b) || _.A.Equals(b) && _.B.Equals(a));
			if (edge == null)
				return;
			a.RemoveConnected(b);
			b.RemoveConnected(a);
			Edges.Remove(edge);
		}

		public Dictionary<long, long> GetShortestPaths(long start)
		{
			const long max = long.MaxValue - 1;
			var distance = Vertices.Values.ToDictionary(vertex => vertex.Id, vertex => max);
			var used = Vertices.Values.ToDictionary(vertex => vertex.Id, vertex => false);
			distance[start] = 0;
			foreach (var i in Vertices)
			{
				Vertex v = null;
				foreach (var j in Vertices.Values)
					if (!used[j.Id] && (v == null || distance[j.Id] < distance[v.Id]))
						v = j;
				if (distance[v.Id] == max)
					break;
				used[v.Id] = true;
				foreach (var connected in v.ConnectedVertices)
					if (distance[v.Id] + 1 < distance[connected.Id])
						distance[connected.Id] = distance[v.Id] + 1;
			}

			distance = distance
				.Where(_ => _.Value != max)
				.ToDictionary(_ => _.Key, _ => _.Value);

			return distance;
		}

		public Dictionary<Edge, List<Edge>> GetEdgeConnectivity()
		{
			var result = Edges.ToDictionary(edge => edge, edge => new List<Edge>());
			foreach (var edge in Edges)
				foreach (var anotherEdge in Edges)
					if (anotherEdge.Contains(edge.A) || anotherEdge.Contains(edge.B))
						result[edge].Add(anotherEdge);
			return result;
		}
	}
}