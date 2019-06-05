using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace GraphTools.Models
{
	[DebuggerDisplay("[{Id}] ({X};{Y})")]
	public class Vertex
	{
		public Dictionary<int, Vertex> ConnectedVertices { get; set; }
		public int Id { get; }
		public Point Position { get; set; }
		public double X => Position.X;
		public double Y => Position.Y;

		public bool HasEdge(Vertex v) => ConnectedVertices.ContainsKey(v.Id);

		public void AddConnected(Vertex v) => ConnectedVertices.Add(v.Id, v);

		public Vertex(int id, Point position, Dictionary<int, Vertex> connectedVertices = null)
		{
			Id = id;
			Position = position;
			ConnectedVertices = connectedVertices ?? new Dictionary<int, Vertex>();
		}

		public override bool Equals(object obj)
		{
			if (obj is Vertex vertex)
				return Id == vertex.Id;
			return false;
		}

		public override int GetHashCode() => Id.GetHashCode();
	}
}