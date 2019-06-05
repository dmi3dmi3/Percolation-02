using System;

namespace GraphSdk.Enums
{
	public enum GraphType
	{
		PlaneGraph = 0,
		CubicLattice = 1
	}

	public static class GraphTypeFormatter
	{
		public static string Format(GraphType graphType)
		{
			switch (graphType)
			{
				case GraphType.PlaneGraph:
					return "Планарный граф";
				case GraphType.CubicLattice:
					return "Квадратная решетка";
				default:
					throw new ArgumentOutOfRangeException(nameof(graphType), graphType, null);
			}

		}
	}
}