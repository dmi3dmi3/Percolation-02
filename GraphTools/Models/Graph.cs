using System.Collections.Generic;

namespace GraphTools.Models
{
	public class Graph
	{
		public List<Vertex> Vertices { get; set; }
		public List<Edge> Edges { get; set; }

		public Graph()
		{
			Vertices = new List<Vertex>();
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
	}
}