using GraphSdk;
using GraphSdk.DataModels;
using OsmSharp;
using OsmSharp.Streams;
using System.IO;
using System.IO.Packaging;
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


		public override Graph GenerateGraph(int vertexCount = 0, int edgePercent = 0, int size = 500)
		{
			var graph = new Graph();
			using (var fs = File.OpenRead(_filePath))
			{
				var source = new XmlOsmStreamSource(fs);
				var nodes = source
					.Where(_ => _.Type == OsmGeoType.Node)
                    .Cast<Node>()
					.Where(_ => _.Id.HasValue && _.Longitude.HasValue && _.Latitude.HasValue)
					.Select(_ => new Vertex(_.Id.Value, new Point(_.Longitude.Value, _.Latitude.Value)))
					.ToList();
				graph.Vertices = nodes;


				var ways = source
					.Where(osmGeo => osmGeo.Type == OsmGeoType.Way)
					.Cast<Way>()
					.Where(_ => _.Tags == null 
					            || _.Tags.ContainsKey("highway")
                                && !_.Tags.Contains("highway", "footway") //Пешеходные дорожки, тротуары
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

            var minVec = new Vector(
                graph.Vertices.Select(_ => _.X).Min(), 
                graph.Vertices.Select(_ => _.Y).Min());
            graph.Vertices.ForEach(_ => _.Position = Point.Add(_.Position, -minVec));

            var maxX = graph.Vertices.Select(_ => _.X).Max();
            var maxY = graph.Vertices.Select(_ => _.Y).Max();

            var XKoef = size / maxX;
            var YKoef = size / maxY;

            graph.Vertices.ForEach(_ => _.Position = new Point(_.X * XKoef, _.Y * YKoef));


            return graph;
		}
	}
}