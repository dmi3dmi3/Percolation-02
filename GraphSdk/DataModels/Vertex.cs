using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace GraphSdk.DataModels
{
	[DebuggerDisplay("[{Id}] ({X};{Y})")]
	public class Vertex
	{
		public List<Vertex> ConnectedVertices { get; set; }
		public int Id { get; }
		public Point Position { get; set; }
		public double X => Position.X;
		public double Y => Position.Y;

		public bool HasEdge(Vertex v) => ConnectedVertices.Exists(vertex => vertex.Id == v.Id);

		public void AddConnected(Vertex v) => ConnectedVertices.Add(v);
		public void RemoveConnected(Vertex v) => ConnectedVertices.Remove(v);

		public Vertex(int id, Point position, List<Vertex> connectedVertices = null)
		{
			Id = id;
			Position = position;
			ConnectedVertices = connectedVertices ?? new List<Vertex>();
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