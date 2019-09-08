using GraphSdk;
using GraphSdk.DataModels;
using OsmSharp;
using OsmSharp.Streams;
using System.IO;
using System.Linq;
using System.Windows;


namespace OsmLoader
{
	public class OsmGraphLoader : GraphCreator
	{
		private readonly string _filePath;

		public OsmGraphLoader(string filePath)
		{
			_filePath = filePath;
		}


		public override Graph GenerateGraph(int vertexCount = 0, int edgePercent = 0, int size = 0)
		{
			var graph = new Graph();
			using (var fs = File.OpenRead(_filePath))
			{
				var source = new XmlOsmStreamSource(fs);
				var nodes = source
					.Where(_ => _.Type == OsmGeoType.Node)
					.Where(_ => _.Id.HasValue)
					.Select(_ => new Vertex(_.Id.Value, new Point(0, 0)))
					.ToList();
				graph.Vertices = nodes;


				var ways = source
					.Where(osmGeo => osmGeo.Type == OsmGeoType.Way)
					.Cast<Way>()
					.Where(_ => _.Tags == null 
					            || !_.Tags.Contains("highway", "footway") //Пешеходные дорожки, тротуары
					            && !_.Tags.Contains("highway", "bridleway") //Дорожки для верховой езды
					            && !_.Tags.Contains("highway", "steps") //Лестницы, лестничные пролёты.
					            && !_.Tags.Contains("highway", "path") //Тропа (чаще всего, стихийная) использующаяся пешеходами, либо транспортом, кроме четырехколесного 
					            && !_.Tags.Contains("highway", "cycleway") //Велодорожка
					            && !_.Tags.Contains("highway", "proposed") //Планируемая/проектируемая дорога
					            && !_.Tags.Contains("highway", "construction") //Строящаяся или ремонтируемая дорога
								)
					.Select(_ => _.Nodes)
					.ToList();
				foreach (var way in ways)
					for (var i = 0; i < way.Length - 1; i++)
						graph.AddEdge(way[i], way[i + 1]);

			}

			for (var i = 0; i < graph.Vertices.Count;)
			{
				if (!graph.Vertices[i].ConnectedVertices.Any())
					graph.Vertices.RemoveAt(i);
				else
					i++;
			}

			return graph;
		}
	}
}