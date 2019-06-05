using CubicLattice;
using GraphSdk;
using GraphSdk.DataModels;
using GraphSdk.Enums;
using PlaneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;


namespace GUI
{
	public static class MainModel
	{
		private static Graph _graph;
		private static List<(Point, Point)> _nonPlanarEdges;

		public static void GenerateGraph(int vertexCount, int edgePercent, int size, bool isPlanar, int nonPlanarPercent, GraphType graphType, bool replaceConnections)
		{
			switch (graphType)
			{
				case GraphType.PlaneGraph:
					_graph = new PlaneGraphCreator().GenerateGraph(vertexCount, edgePercent, size);
					break;
				case GraphType.CubicLattice:
					_graph = new CubicLatticeCreator().GenerateGraph(vertexCount, edgePercent, size);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(graphType), graphType, null);
			}
			_nonPlanarEdges = !isPlanar
				? NonPlanarTools.AddNonPlanarEdges(_graph, nonPlanarPercent, replaceConnections).Select(edge => (edge.A.Position, edge.B.Position)).ToList()
				: null;
		}

		public static List<Point> GetVertices() =>
			_graph.Vertices.Select(vertex => vertex.Position).ToList();

		public static List<(Point, Point)> GetPlanarEdges() =>
			_graph.Edges.Select(edge => (edge.A.Position, edge.B.Position)).ToList();

		public static List<(Point, Point)> GetNonPlanarEdges() =>
			_nonPlanarEdges;

		public static double GetAverageLinkCount() =>
			CalculatingTools.GetAverageLinkCount(_graph);

		public static Dictionary<int, double> GetPercolationThreshold(ExperimentType experimentType)
		{
			var steps = new Dictionary<double, double>(101);
			switch (experimentType)
			{
				case ExperimentType.Nodes:
					for (double i = 0; i <= 1.001; i += 0.01)
						steps.Add(i, CalculatingTools.GetNewVertexPercolation(_graph, i));
					break;
				case ExperimentType.Connections:
					for (double i = 0; i <= 1.001; i += 0.01)
						steps.Add(i, CalculatingTools.GetNewEdgePercolation(_graph, i));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(experimentType), experimentType, null);
			}

			var result = new Dictionary<int, double>();
			foreach (var d in steps)
				result.Add((int)Math.Round(d.Key * 100, 0), d.Value);

			var resMean = result.Values.Sum() / result.Count;
			var flag = true;
			while (flag)
				for (var i = 1; i < result.Count - 1; i++)
				{
					if (Math.Abs(result[i] - resMean) / StandardDeviation(result[i - 1], result[i], result[i + 1]) > 3)
						for (var j = 1; j < result.Count - 1; j++)
							result[j] = (result[j - 1] + result[j] + result[j + 1]) / 3;

					flag = false;
				}

			return result;
		}

		private static double StandardDeviation(params double[] values)
		{
			var mean = values.Sum() / values.Length;
			var squares = values.Select(d => (d - mean) * (d - mean)).ToList();
			return Math.Sqrt(squares.Sum() / values.Length);
		}
		public static double GetAverageShortestPath()
		{
			var res = 0d;
			foreach (var vertex in _graph.Vertices)
				res += _graph.GetShortestPaths(vertex.Id).Values.Where(i => i != int.MaxValue).Sum();
			return res / _graph.Vertices.Count / _graph.Vertices.Count;
		}

		public static double GetClusteringRatio()
		{
			var clustering = 0d;
			foreach (var current in _graph.Vertices)
			{
				var connectedNears = 0d;
				foreach (var connected in current.ConnectedVertices)
					connectedNears += connected.ConnectedVertices.Intersect(current.ConnectedVertices).Count();
				clustering += connectedNears / (current.ConnectedVertices.Count * (current.ConnectedVertices.Count - 1));
			}

			return clustering / _graph.Vertices.Count;
		}
	}
}