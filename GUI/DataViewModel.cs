using GraphSdk.Enums;
using GUI.Annotations;
using GUI.Helpers;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GUI
{
	public class DataViewModel : INotifyPropertyChanged
	{
		public DataViewModel()
		{
			Planarity = PlanarityType.Planar;
			Experiment = ExperimentType.Nodes;
			ExperimentCount = 1;
			VertexCount = 100;
			ConnectionPercent = 100;
			TryCount = 50;
			ProbabilityCount = 1000;

			IsRunning = false;

		}

		#region PlanarityRB

		public bool PlanarRadioButton
		{
			get => _planarRadioButton;
			set
			{
				if (value == _planarRadioButton) return;
				_planarRadioButton = value;
				OnPropertyChanged();
			}
		}

		public bool NonPlanarRadioButton
		{
			get => _nonPlanarRadioButton;
			set
			{
				if (value == _nonPlanarRadioButton) return;
				_nonPlanarRadioButton = value;
				OnPropertyChanged();
			}
		}

		#endregion PlanarityRB

		#region ExperimentRB

		public bool NodeRadioButton
		{
			get => _nodeRadioButton;
			set
			{
				if (value == _nodeRadioButton) return;
				_nodeRadioButton = value;
				OnPropertyChanged();
			}
		}

		public bool ConnectionRadioButton
		{
			get => _connectionRadioButton;
			set
			{
				if (value == _connectionRadioButton) return;
				_connectionRadioButton = value;
				OnPropertyChanged();
			}
		}

		#endregion ExperimentRB

		#region DataTextBoxs

		public int ExperimentCount
		{
			get => _experimentCount;
			set
			{
				if (value == _experimentCount) return;
				_experimentCount = value;
				OnPropertyChanged();
			}
		}

		public int VertexCount
		{
			get => _vertexCount;
			set
			{
				if (value == _vertexCount) return;
				_vertexCount = value;
				OnPropertyChanged();
			}
		}

		public int ConnectionPercent
		{
			get => _connectionPercent;
			set
			{
				if (value == _connectionPercent)
					return;
				if (value > 100)
				{
					_connectionPercent = 100;
					return;
				}
				if (value < 0)
				{
					_connectionPercent = 0;
					return;
				}
				_connectionPercent = value;
				OnPropertyChanged();
			}
		}

		public int NonPlanarConnectionPercent
		{
			get => _nonPlanarConnectionPercent;
			set
			{
				if (value == _nonPlanarConnectionPercent) return;
				_nonPlanarConnectionPercent = value;
				OnPropertyChanged();
			}
		}

		#endregion

		public bool ReplaceConnections
		{
			get => _replaceConnections;
			set
			{
				if (value == _replaceConnections) return;
				_replaceConnections = value;
				OnPropertyChanged();
			}
		}

		public double AverageLinkCount
		{
			get => _averageLinkCount;
			set
			{
				if (value.Equals(_averageLinkCount)) return;
				_averageLinkCount = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(PercolationThresholdPerAverageLinkCount));
			}
		}

		public double PercolationThreshold
		{
			get => _percolationThreshold;
			set
			{
				if (value.Equals(_percolationThreshold)) return;
				_percolationThreshold = value;

				OnPropertyChanged();
				OnPropertyChanged(nameof(PercolationThresholdPerAverageLinkCount));
			}
		}

		public decimal AverageShortestPath
		{
			get => _averageShortestPath;
			set
			{
				if (Math.Abs(value - _averageShortestPath) < (decimal)0.001) return;
				_averageShortestPath = value;
				OnPropertyChanged();
			}
		}

		public decimal ClusteringRatio
		{
			get => _clusteringRatio;
			set
			{
				if (value.Equals(_clusteringRatio)) return;
				_clusteringRatio = value;
				OnPropertyChanged();
			}
		}

		public double PercolationThresholdPerAverageLinkCount => Math.Round(PercolationThreshold * 100 / AverageLinkCount, 3);

		public PlanarityType Planarity
		{
			get => PlanarRadioButton ? PlanarityType.Planar : PlanarityType.NonPlanar;
			set
			{
				switch (value)
				{
					case PlanarityType.Planar:
						PlanarRadioButton = true;
						break;
					case PlanarityType.NonPlanar:
						NonPlanarRadioButton = true;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(value), value, null);
				}
			}
		}

		public ExperimentType Experiment
		{
			get => NodeRadioButton ? ExperimentType.Nodes : ExperimentType.Connections;
			set
			{
				switch (value)
				{
					case ExperimentType.Nodes:
						NodeRadioButton = true;
						break;
					case ExperimentType.Connections:
						ConnectionRadioButton = true;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(value), value, null);
				}
			}
		}

		public GraphType GraphType
		{
			get => _graphType;
			set
			{
				if (value == _graphType) return;
				_graphType = value;
				OnPropertyChanged();
			}
		}

		#region CommandsForBinding

		public ICommand GraphGenerateCommand => 
			_graphGenerateCommand ?? (_graphGenerateCommand = new CommandWrapper(OnGraphGenerate));

		public ICommand CountPercolationCommand => 
			_countPercolationCommand ?? (_countPercolationCommand = new CommandWrapper(OnPercolationCount));

		public ICommand CancelCommand =>
			_cancelCommand ?? (_cancelCommand = new CommandWrapper(Cancel));

		public ICommand LoadOsmCommand =>
			_loadOsmCommand ?? (_loadOsmCommand = new CommandWrapper(LoadOsm));

		#endregion

		public ObservableCollection<Shape> CanvasItemsSource
		{
			get => _canvasItemsSource;
			set
			{
				if (Equals(value, _canvasItemsSource)) return;
				_canvasItemsSource = value;
				OnPropertyChanged();
			}
		}

		public IList<DataPoint> PercolationPoints
		{
			get => _percolationPoints;
			protected set
			{
				if (Equals(value, _percolationPoints)) return;
				_percolationPoints = value;
				OnPropertyChanged();
			}
		}

		public IList<DataPoint> ThresholdPoints
		{
			get => _thresholdPoints;
			set
			{
				if (Equals(value, _thresholdPoints)) return;
				_thresholdPoints = value;
				OnPropertyChanged();
			}
		}

		public IList<DataPoint> TrueThresholdPoints
		{
			get => _trueThresholdPoints;
			set
			{
				if (Equals(value, _trueThresholdPoints)) return;
				_trueThresholdPoints = value;
				OnPropertyChanged();
			}
		}

		public bool TrueThresholdVisibility
		{
			get => _trueThresholdVisibility;
			set
			{
				if (value == _trueThresholdVisibility) return;
				_trueThresholdVisibility = value;
				OnPropertyChanged();
			}
		}

		public string Status
		{
			get => _status;
			set
			{
				if (value == _status) return;
				_status = value;
				OnPropertyChanged();
			}
		}
		public int ProbabilityCount
		{
			get => _probabilityCount;
			set
			{
				if (value.Equals(_probabilityCount)) return;
				_probabilityCount = value;
				OnPropertyChanged();
			}
		}

		public int TryCount
		{
			get => _tryCount;
			set
			{
				if (value == _tryCount) return;
				_tryCount = value;
				OnPropertyChanged();
			}
		}

		protected virtual void OnPercolationCount(object obj = null) { }
		protected virtual void OnGraphGenerate(object obj = null) { }
		protected virtual void Cancel(object obj = null) { }
		protected virtual void LoadOsm(object obj = null) { }

		#region Constants

		private const int VertexRadius = 1;
		private const int VertexDiameter = VertexRadius * 2;

		#endregion
		protected List<Point> Vertices { get; set; }
		protected List<(Point, Point)> BlueConnections { get; set; }
		protected List<(Point, Point)> GreenConnections { get; set; }


		protected void RedrawGraph()
		{
			CanvasItemsSource = new ObservableCollection<Shape>();

			if (BlueConnections != null)
				foreach (var item in BlueConnections.Select(_ => new Line()
				{
					X1 = _.Item1.X,
					Y1 = _.Item1.Y,
					X2 = _.Item2.X,
					Y2 = _.Item2.Y,
					Stroke = Brushes.Blue
				}))
					CanvasItemsSource.Add(item);

			if (GreenConnections != null)
				foreach (var item in GreenConnections.Select(_ => new Line()
				{
					X1 = _.Item1.X,
					Y1 = _.Item1.Y,
					X2 = _.Item2.X,
					Y2 = _.Item2.Y,
					Stroke = Brushes.Red
				}))
					CanvasItemsSource.Add(item);

			if (Vertices != null)
				foreach (var item in Vertices.Select(point => new Ellipse
				{
					Margin = new Thickness(point.X - VertexRadius, point.Y - VertexRadius, 0, 0),
					Width = VertexDiameter,
					Height = VertexDiameter,
					Fill = Brushes.IndianRed
				}))
					CanvasItemsSource.Add(item);
		}

		public bool IsNotRunning => !IsRunning;

		public bool IsRunning
		{
			get => _isRunning;
			set
			{
				if (value == _isRunning) return;
				_isRunning = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(IsNotRunning));
			}
		}

		#region Private fields
		private bool _planarRadioButton;
		private bool _nonPlanarRadioButton;
		private bool _nodeRadioButton;
		private bool _connectionRadioButton;
		private ICommand _graphGenerateCommand;
		private ICommand _countPercolationCommand;
		private ICommand _cancelCommand;
		private int _experimentCount;
		private int _vertexCount;
		private int _connectionPercent;
		private int _nonPlanarConnectionPercent;
		private int _probabilityCount;
		private int _tryCount;
		private double _averageLinkCount;
		private double _percolationThreshold;
		private decimal _averageShortestPath;
		private decimal _clusteringRatio;
		private ObservableCollection<Shape> _canvasItemsSource;
		private GraphType _graphType;
		private IList<DataPoint> _percolationPoints;
		private IList<DataPoint> _thresholdPoints;
		private IList<DataPoint> _trueThresholdPoints;
		private bool _trueThresholdVisibility;
		private bool _replaceConnections;
		private bool _isRunning;
		private string _status;
		private ICommand _loadOsmCommand;

		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}