using GraphSdk.Enums;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GUI
{
	public class MainViewModel : DataViewModel
	{
		protected override void OnGraphGenerate(object obj = null)
		{
			IsRunning = true;
			var size = 650;

			MainModel.GenerateGraph(
				VertexCount, ConnectionPercent,
				size, Planarity == PlanarityType.Planar,
				NonPlanarConnectionPercent, GraphType,
				ReplaceConnections);
			Vertices = MainModel.GetVertices();
			BlueConnections = MainModel.GetPlanarEdges();
			GreenConnections = MainModel.GetNonPlanarEdges();
			RedrawGraph();
			IsRunning = false;
		}

		protected override void OnPercolationCount(object obj = null)
		{
			OnGraphGenerate(obj);
			IsRunning = true;
			var avl = new List<double>();
			var percolation = new Dictionary<int, double>();
			var asp = new List<double>();
			var clustering = new List<double>();
			for (int i = 0; i < ExperimentCount; i++)
			{
				avl.Add(MainModel.GetAverageLinkCount());
				var t = MainModel.GetPercolationThreshold(Experiment);
				foreach (var pair in t)
				{
					if (percolation.ContainsKey(pair.Key))
						percolation[pair.Key] += pair.Value;
					else
						percolation.Add(pair.Key, pair.Value);
				}
				asp.Add(MainModel.GetAverageShortestPath());
				clustering.Add(MainModel.GetClusteringRatio());
			}


			var pt = percolation.First(pair => pair.Value <= 0.5).Key / 100d;
			PercolationPoints = new List<DataPoint>(percolation.Select(pair => new DataPoint(pair.Key , 1-pair.Value)));
			ThresholdPoints = new List<DataPoint> { new DataPoint(pt * 100, 0), new DataPoint(pt * 100, 1) };

			// ReSharper disable once AssignmentInConditionalExpression
			if (TrueThresholdVisibility = (GraphType == GraphType.CubicLattice))
			{
				var trueValue = Experiment == ExperimentType.Nodes ? 41 : 50;
				TrueThresholdPoints = new List<DataPoint> { new DataPoint(trueValue, 0), new DataPoint(trueValue , 1) };
			}

			AverageLinkCount = Math.Round(avl.Sum() / avl.Count, 3);
			PercolationThreshold = Math.Round(pt, 3) ;
			AverageShortestPath = Math.Round(asp.Sum() / asp.Count, 3);
			ClusteringRatio = Math.Round(clustering.Sum() / clustering.Count, 3);
			IsRunning = false;
		}


	}
}