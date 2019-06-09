using GraphSdk.Enums;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GUI
{
	public class MainViewModel : DataViewModel
	{
		private readonly BackgroundWorker _backgroundWorker;

		public MainViewModel()
		{
			_backgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true, WorkerReportsProgress = true };
			_backgroundWorker.DoWork += (sender, args) => DoWork(args);
			_backgroundWorker.RunWorkerCompleted += (sender, args) => WorkCompleted(args);
			_backgroundWorker.ProgressChanged += (sender, args) => { Status = (string)args.UserState; };
			MainModel.PercolationProcess += (sender, d) =>
			{
				_backgroundWorker.ReportProgress(0, $"Идет {_currentExperiment + 1}/{ExperimentCount} " +
												   $"эксперимент: {Math.Round(_currentPercent++ / ProbabilityCount * 100, 1)}%");
			};
		}



		protected override void OnGraphGenerate(object obj = null)
		{
			IsRunning = true;
			_backgroundWorker.RunWorkerAsync(false);
		}

		protected override void OnPercolationCount(object obj = null)
		{
			IsRunning = true;
			_backgroundWorker.RunWorkerAsync(true);
		}

		protected override void Cancel(object obj = null)
		{
			_backgroundWorker.CancelAsync();
		}

		private int _currentExperiment;
		private double _currentPercent;

		private void DoWork(DoWorkEventArgs args)
		{
			var size = 650;

			_backgroundWorker.ReportProgress(0, "Идет генерация графа");
			MainModel.GenerateGraph(
				VertexCount, ConnectionPercent,
				size, Planarity == PlanarityType.Planar,
				NonPlanarConnectionPercent, GraphType,
				ReplaceConnections);
			Vertices = MainModel.GetVertices();
			BlueConnections = MainModel.GetPlanarEdges();
			GreenConnections = MainModel.GetNonPlanarEdges();
			_backgroundWorker.ReportProgress(0, "Граф построен");
			if (!(bool)args.Argument)
				return;
			var avl = new List<double>();
			var percolation = new List<double>();
			var asp = new List<double>();
			var clustering = new List<double>();
			for (_currentExperiment = 0; _currentExperiment < ExperimentCount; _currentExperiment++)
			{
				_currentPercent = 0;
				if (_backgroundWorker.CancellationPending)
				{
					args.Cancel = true;
					return;
				}
				_backgroundWorker.ReportProgress(0, $"Идет {_currentExperiment + 1}/{ExperimentCount} эксперимент");
				avl.Add(MainModel.GetAverageLinkCount());
				var t = MainModel.GetPercolationThreshold(Experiment, 1d / ProbabilityCount, TryCount);
				if (percolation.Any())
					for (var j = 0; j < t.Count; j++)
						percolation[j] += t[j];
				else
					percolation = t;
				asp.Add(MainModel.GetAverageShortestPath());
				clustering.Add(MainModel.GetClusteringRatio());
			}
			for (var i = 0; i < percolation.Count; i++)
				percolation[i] = (percolation[i] / ExperimentCount);


			_backgroundWorker.ReportProgress(0, $"Идет подсчет значений");
			double pt = 0;
			for (var i = 0; i < percolation.Count; i++)
				if (percolation[i] >= 0.5)
					pt = (double)i / percolation.Count;

			PercolationPoints = new List<DataPoint>(percolation.Select((d, i) => new DataPoint((double)i / percolation.Count, 1 - d)));
			ThresholdPoints = new List<DataPoint> { new DataPoint(pt, 0), new DataPoint(pt, 1) };

			// ReSharper disable once AssignmentInConditionalExpression
			if (TrueThresholdVisibility = (GraphType == GraphType.CubicLattice))
			{
				var trueValue = (Experiment == ExperimentType.Nodes ? 0.41 : 0.5);
				TrueThresholdPoints = new List<DataPoint> { new DataPoint(trueValue, 0), new DataPoint(trueValue, 1) };
			}

			AverageLinkCount = Math.Round(avl.Sum() / avl.Count, 3);
			PercolationThreshold = Math.Round(pt, 3);
			AverageShortestPath = Math.Round(asp.Sum() / asp.Count, 3);
			ClusteringRatio = Math.Round(clustering.Sum() / clustering.Count, 3);
		}

		private void WorkCompleted(RunWorkerCompletedEventArgs args)
		{
			Status = args.Cancelled ? "Отменено" : "Успешно завершено";
			if (!args.Cancelled)
				RedrawGraph();
			IsRunning = false;
		}
	}
}