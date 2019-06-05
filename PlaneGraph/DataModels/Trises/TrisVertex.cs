using System.Collections.Generic;
using System.Windows;
using GraphSdk.DataModels;

namespace PlaneGraph.DataModels.Trises
{
	public class TrisVertex
	{
		public Vertex Original;
		public Point Position;
		public List<Tris> Trises;

		public TrisVertex(Vertex original)
		{
			Original = original;
			Position = original.Position;
			Trises = new List<Tris>(6);
		}

		public double X => Position.X;

		public double Y => Position.Y;

		public Tris FindAnotherTrisWith(Tris tris, TrisVertex a, TrisVertex b)
		{
			return Trises.Find(f => f != tris && f.HasEdge(a, b));
		}

	}
}