using System.Windows;

namespace PlaneGraph.DataModels.Trises
{
	public class TrisEdge
	{
		private readonly double _idx;
		private readonly double _idy;
		public TrisVertex A;
		public TrisVertex B;
		public Tris tris;

		public TrisEdge(Tris tris, TrisVertex a, TrisVertex b)
		{
			this.tris = tris;
			A = a;
			B = b;

			_idx = b.Position.X - a.Position.X;
			_idy = b.Position.Y - a.Position.Y;
		}

		/// <summary>
		/// Проверяет меньше ли точка прямой, проходящей через ребро
		/// </summary>
		public bool EdgeVisibleFrom(Point point)
		{
			double dx = point.X - A.Position.X;
			double dy = point.Y - A.Position.Y;

			var crossProduct = dx * _idy - dy * _idx;
			return crossProduct > 0;
		}

	}
}