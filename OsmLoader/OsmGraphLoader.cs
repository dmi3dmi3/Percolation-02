using System.Collections.Generic;
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
					.ToDictionary(_ => _.Id, _ => _);
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

            var toDelete = new List<long>();
            foreach (var vertex in graph.Vertices)
                if (!vertex.Value.ConnectedVertices.Any())
                    toDelete.Add(vertex.Key);

            foreach (var l in toDelete)
                graph.Vertices.Remove(l);

            var minVec = new Vector(
                graph.Vertices.Values.Select(_ => _.X).Min(), 
                graph.Vertices.Values.Select(_ => _.Y).Min());
            foreach (var kvp in graph.Vertices)
                kvp.Value.Position = Point.Add(kvp.Value.Position, -minVec);

            var maxX = graph.Vertices.Values.Select(_ => _.X).Max();
            var maxY = graph.Vertices.Values.Select(_ => _.Y).Max();

            var XKoef = size / maxX;
            var YKoef = size / maxY;

            foreach (var vertex in graph.Vertices.Values)
                vertex.Position = new Point(vertex.X * XKoef, vertex.Y * YKoef);

            return graph;
		}
	}
}