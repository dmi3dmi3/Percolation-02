using GraphSdk.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphSdk
{
	public static class NonPlanarTools
	{
		private static Random Random { get; }

		static NonPlanarTools()
		{
			Random = new Random();
		}

		public static List<Edge> AddNonPlanarEdges(Graph graph, int NonPlanarPercent, bool replaceConnections)
		{
			var nonPlanarEdges = new List<Edge>();

			var facets = new List<List<Vertex>>();
			//Создаем список смежности
			var adjacencyList = graph.Vertices.Values.ToDictionary(vertex => vertex, vertex => vertex.ConnectedVertices);
			foreach (var item in adjacencyList)
			{
				// где связанные вершины сотируем по поляному углу
				item.Value.Sort((v1, v2) =>
				{
					var t1 = Math.Atan2(v1.X - item.Key.X, v1.Y - item.Key.Y);
					var t2 = Math.Atan2(v2.X - item.Key.X, v2.Y - item.Key.Y);
					return t1 > t2 ? 1 : t2 > t1 ? -1 : 0;
				});
			}

			var colors = adjacencyList
				.ToDictionary(
					pair => pair.Key.Id,
					pair => pair.Value.ToDictionary(vertex => vertex.Id, vertex => false));

			//Обходим граф закрашивая список смежности
			//и каждый раз "самую левую" вершину из непосещенных
			foreach (var item in adjacencyList)
				foreach (var nearVertex in item.Value)
				{
					if (colors[item.Key.Id][nearVertex.Id])
						continue;
					colors[item.Key.Id][nearVertex.Id] = true;
					var v = nearVertex;
					var pv = item.Key;
					var facet = new List<Vertex>();
					for (; ; )
					{
						facet.Add(v);
						var index = adjacencyList[v].IndexOf(pv);
						if (++index == adjacencyList[v].Count)
							index = 0;
						if (colors[v.Id].Skip(index).First().Value)
							break;
						var key = colors[v.Id].Skip(index).First().Key;
						colors[v.Id][key] = true;
						pv = v;
						v = adjacencyList[v].Skip(index).First();
					}
					facets.Add(facet);
				}

			// строим граф поверхностей, где связи между поверхностями имающими общие точки
			var facetGraph = new Graph
			{
				Vertices = Enumerable
					.Range(0, facets.Count)
					.Select(i => new Vertex(i, facets[i][0].Position))
					.ToDictionary(_ => _.Id, _ => _)
			};

			for (var i = 0; i < facetGraph.Vertices.Count; i++)
				for (var j = i; j < facetGraph.Vertices.Count; j++)
				{
					if (i == j)
						continue;
					if (facets[i].Intersect(facets[j]).Any())
						facetGraph.AddEdge(i, j);

				}

			//Создаем матрицу кратчайших путей
			var alfa = 2;
			var probabilities = facetGraph.Vertices.Values
                .ToDictionary(
					vertex => vertex.Id,
					vertex => facetGraph
						.GetShortestPaths(vertex.Id)
						.ToDictionary(pair => pair.Key, pair => Math.Pow(pair.Value - 1, -alfa)));
			var addPerVertex = graph.Edges.Count * NonPlanarPercent / 100d / graph.Vertices.Count;
			foreach (var vertex in graph.Vertices.Values)
			{
				var toAdd = addPerVertex;
				do
				{
					var r = Random.NextDouble();
					if (toAdd >= 1 || r <= toAdd)
					{
						while (true)
						{
							var startFacet = facets.FindIndex(list => list.Exists(_ => _.Id == vertex.Id));
							var endVertex = graph.Vertices[Random.Next(graph.Vertices.Count)];
							var endFacet = facets.FindIndex(list => list.Exists(_ => _.Id == endVertex.Id));
							if (probabilities[startFacet][endFacet] >= Random.NextDouble())
							{
								if (replaceConnections && vertex.ConnectedVertices.Any())
									graph.RemoveEdge(vertex,
										vertex.ConnectedVertices[Random.Next(vertex.ConnectedVertices.Count)]);

								graph.AddEdge(vertex, endVertex);
								nonPlanarEdges.Add(new Edge(vertex, endVertex));
								break;
							}
						}
					}
					toAdd--;
				}
				while (toAdd > 0);

			}
			return nonPlanarEdges;
		}
	}
}