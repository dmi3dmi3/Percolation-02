using CubicLattice;
using GraphSdk;
using GraphSdk.DataModels;
using GraphSdk.Enums;
using PlaneGraph;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using OsmLoader;


namespace GUI
{
	public static class MainModel
	{
		private static Graph s_graph;
		private static List<(Point, Point)> s_nonPlanarEdges;
		public static event EventHandler<double> PercolationProcess;
		public static event EventHandler<double> AspProcess;

		public static void GenerateGraph(int vertexCount, int edgePercent, int size, bool isPlanar, int nonPlanarPercent, GraphType graphType, bool replaceConnections)
		{
			switch (graphType)
			{
				case GraphType.PlaneGraph:
					s_graph = new PlaneGraphCreator().GenerateGraph(vertexCount, edgePercent, size);
					break;
				case GraphType.CubicLattice:
					s_graph = new CubicLatticeCreator().GenerateGraph(vertexCount, edgePercent, size);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(graphType), graphType, null);
			}
			s_nonPlanarEdges = !isPlanar
				? NonPlanarTools.AddNonPlanarEdges(s_graph, nonPlanarPercent, replaceConnections).Select(edge => (edge.A.Position, edge.B.Position)).ToList()
				: null;
		}

		public static void LoadGraphFromOsm(string filePath)
		{
			s_graph = new OsmGraphLoader(filePath).GenerateGraph();
		}

		public static List<Point> GetVertices() =>
			s_graph.Vertices.Select(vertex => vertex.Position).ToList();

		public static List<(Point, Point)> GetPlanarEdges() =>
			s_graph.Edges.Select(edge => (edge.A.Position, edge.B.Position)).ToList();

		public static List<(Point, Point)> GetNonPlanarEdges() =>
			s_nonPlanarEdges;

		public static double GetAverageLinkCount() =>
			CalculatingTools.GetAverageLinkCount(s_graph);

		public static List<double> GetPercolationThreshold(ExperimentType experimentType, double probabilityStep = 0.001, int tryCount = 10)
		{
			var steps = new List<double>((int)Math.Round(1 / probabilityStep, 0));
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
						var t = CalculatingTools.GetVertexPercolation(s_graph, d, tryCount);
						anotherSteps.AddOrUpdate(d, t, (d1, d2) => d2);
					});
					break;
				case ExperimentType.Connections:
					for (double i = 0; i <= maxVal; i += probabilityStep)
						probabilities.Add(i);
					Parallel.ForEach(probabilities, d =>
					{
						PercolationProcess?.Invoke(null, Math.Round(d * 100, 1));
						var t = CalculatingTools.GetEdgePercolation(s_graph, d, tryCount);
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

		public static decimal GetAverageShortestPath()
		{
			var count = s_graph.Vertices.Count;
			var res = new long[count];
			var done = 0;
			Parallel.For(0, s_graph.Vertices.Count, i =>
			{
				long innerRes = 0;
				foreach (var l in s_graph.GetShortestPaths(s_graph.Vertices[i].Id).Values)
					innerRes += l;
				res[i] = innerRes;
				Interlocked.Increment(ref done);
				AspProcess?.Invoke(null, (double)done / count * 100);
			});
			return res.Select(l => l / (decimal)s_graph.Vertices.Count / s_graph.Vertices.Count).Sum();
		}

		public static decimal GetClusteringRatio()
		{
			var clustering = Enumerable.Range(0, s_graph.Vertices.Count).Select(i => (decimal)0).ToArray();
			Parallel.For(0, s_graph.Vertices.Count, i =>
			{
				var current = s_graph.Vertices[i];
				if (current.ConnectedVertices.Count <= 1)
					return;
				decimal connectedNears = 0;
				foreach (var connected in current.ConnectedVertices)
					connectedNears += connected.ConnectedVertices.Intersect(current.ConnectedVertices).Count();

				clustering[i] = connectedNears /
							  (current.ConnectedVertices.Count * (current.ConnectedVertices.Count - 1));

			});

			return clustering.Select(_ => _ / s_graph.Vertices.Count).Sum();
		}
	}
}