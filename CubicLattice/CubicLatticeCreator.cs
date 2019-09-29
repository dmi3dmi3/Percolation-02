using GraphSdk;
using GraphSdk.DataModels;
using System;
using System.Windows;

namespace CubicLattice
{
	public class CubicLatticeCreator : GraphCreator
	{
		public override Graph GenerateGraph(int vertexCount, int edgePercent, int size = 500)
		{
			var inRow = (int)Math.Truncate(Math.Sqrt(vertexCount));
			if (Math.Abs(inRow * inRow - vertexCount) > Math.Abs((inRow + 1) * (inRow + 1) - vertexCount))
				inRow++;
			var graph = new Graph();
			var distance = (double)size / inRow;
			var id = 0;
			var matrix = new Vertex[inRow, inRow];

			for (var i = 0; i < inRow; i++)
				for (var j = 0; j < inRow; j++)
					matrix[i, j] = new Vertex(id++, new Point(i * distance, distance * j));
			for (var i = 0; i < inRow; i++)
				for (var j = 0; j < inRow; j++)
				{
					if (j != 0)
						graph.AddEdge(matrix[i, j], matrix[i, j - 1]);

					if (i != 0)
						graph.AddEdge(matrix[i, j], matrix[i - 1, j]);
				}

			for (var i = 0; i < inRow; i++)
				for (var j = 0; j < inRow; j++)
					graph.Vertices.Add(matrix[i, j].Id, matrix[i, j]);

			return graph;
		}
	}
}