using CubicLattice;
using GraphSdk;
using GraphSdk.DataModels;
using GraphSdk.Enums;
using PlaneGraph;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace GUI
{
	public static class MainModel
	{
		private static Graph _graph;
		private static List<(Point, Point)> _nonPlanarEdges;
		public static event EventHandler<double> PercolationProcess; 

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

		public static List<double> GetPercolationThreshold(ExperimentType experimentType, double probabilityStep = 0.001, int tryCount = 10)
		{
			var steps = new List<double>((int)Math.Round(1/probabilityStep, 0));
			var anotherSteps = new ConcurrentDictionary<double, double>();
			var maxVal = 1 + probabilityStep / 10;
			var probabilities = new List<double>();
			switch (experimentType)
			{
				case ExperimentType.Nodes:
					for (double i = 0; i <= maxVal; i += probabilityStep) 
						probabilities.Add(i);
					Parallel.ForEach(probabilities, d =>
					{
						PercolationProcess?.Invoke(null, Math.Round(d * 100, 1));
						var t = CalculatingTools.GetVertexPercolation(_graph, d, tryCount);
						anotherSteps.AddOrUpdate(d, t, (d1, d2) => d2);
					});
					break;
				case ExperimentType.Connections:
					for (double i = 0; i <= maxVal; i += probabilityStep)
						probabilities.Add(i);
					Parallel.ForEach(probabilities, d =>
					{
						PercolationProcess?.Invoke(null, Math.Round(d * 100, 1));
						var t = CalculatingTools.GetEdgePercolation(_graph, d, tryCount);
						anotherSteps.AddOrUpdate(d, t, (d1, d2) => d2);
					});
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(experimentType), experimentType, null);
			}
			var sortedSteps = new SortedDictionary<double, double>(anotherSteps);
			steps = sortedSteps.Values.ToList();

			var resMean = steps.Sum() / steps.Count;
			var flag = true;
			while (flag)
				for (var i = 1; i < steps.Count - 1; i++)
				{
					if (Math.Abs(steps[i] - resMean) / StandardDeviation(steps[i - 1], steps[i], steps[i + 1]) > 3)
						for (var j = 1; j < steps.Count - 1; j++)
							steps[j] = (steps[j - 1] + steps[j] + steps[j + 1]) / 3;

					flag = false;
				}

			return steps;
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
				if (current.ConnectedVertices.Count <= 1) 
					continue;
				var connectedNears = 0d;
				foreach (var connected in current.ConnectedVertices)
					connectedNears += connected.ConnectedVertices.Intersect(current.ConnectedVertices).Count();

				clustering += connectedNears / (current.ConnectedVertices.Count * (current.ConnectedVertices.Count - 1));
			}

			return clustering / _graph.Vertices.Count;
		}
	}
}