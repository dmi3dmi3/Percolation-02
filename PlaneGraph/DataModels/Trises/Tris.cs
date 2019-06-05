namespace PlaneGraph.DataModels.Trises
{
	public class Tris
	{
		public TrisVertex[] Points = new TrisVertex[3];

		public Tris(TrisVertex a, TrisVertex b, TrisVertex c)
		{
			Points[0] = a;
			Points[1] = b;
			Points[2] = c;
		}

		public TrisVertex this[int i] => Points[i];

		public bool HasEdge(TrisVertex a, TrisVertex b)
		{
			return (a == Points[0] || a == Points[1] || a == Points[2]) &&
				   (b == Points[0] || b == Points[1] || b == Points[2]);
		}

		public int IndexOfPoint(TrisVertex a)
		{
			return Points[0] == a ? 0 : Points[1] == a ? 1 : 2;
		}

		public int IndexOfThirdPoint(TrisVertex a, TrisVertex b)
		{
			if (Points[0] != a && Points[0] != b) return 0;
			if (Points[1] != a && Points[1] != b) return 1;
			return 2;
		}


		/// <summary>
		///     Если ориентация не по часовой стрелке, то меняем b<->c.
		///     Для:
		///     _X
		///     |
		///     Y
		/// </summary>
		public void MakeClockwise()
		{
			var centroidX = (Points[0].X + Points[1].X + Points[2].X) / 3.0f;
			var centroidY = (Points[0].Y + Points[1].Y + Points[2].Y) / 3.0f;

			double dr0 = Points[0].X - centroidX,
				dc0 = Points[0].Y - centroidY;
			double dx01 = Points[1].X - Points[0].X,
				dy01 = Points[1].Y - Points[0].Y;

			var df = -dx01 * dc0 + dy01 * dr0;
			if (!(df < 0))
				return;
			var tmp = Points[2];
			Points[2] = Points[1];
			Points[1] = tmp;
		}

		/// <summary>
		///     Находим позиции и квадрат радиуса описаной окружности, через 3 точки
		/// </summary>
		public bool IsPointBrokeDelaunay(TrisVertex p)
		{
			// Используем относительные координаты до точки 'a'
			double xba = Points[1].X - Points[0].X;
			double yba = Points[1].Y - Points[0].Y;
			double xca = Points[2].X - Points[0].X;
			double yca = Points[2].Y - Points[0].Y;

			// Квадраты растояний граней принадлежащих 'a'
			var baLength = xba * xba + yba * yba;
			var caLength = xca * xca + yca * yca;

			// Считаем деноминатор формулы. 
			var d = xba * yca - yba * xca;

			if (d == 0) return false;

			var denominator = 0.5 / d;

			// Рассчитываем смещение (от 'pa') центра описанной окружности
			var xC = (yca * baLength - yba * caLength) * denominator;
			var yC = (xba * caLength - xca * baLength) * denominator;

			var radius2 = xC * xC + yC * yC;
			if (radius2 > 1e10 * baLength || radius2 > 1e10 * caLength) return false;

			var px = Points[0].X + xC - p.X;
			var py = Points[0].Y + yC - p.Y;
			var pointRadius2 = px * px + py * py;
			return pointRadius2 < radius2;
		}

	}
}