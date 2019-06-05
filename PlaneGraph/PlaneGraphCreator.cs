using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GraphSdk;
using GraphSdk.DataModels;
using PlaneGraph.DataModels.Circular;
using PlaneGraph.DataModels.Trises;

namespace PlaneGraph
{
	public class PlaneGraphCreator : GraphCreator
	{
		public override Graph GenerateGraph(int vertexCount, int edgePercent, int size = 500)
		{
			var graph = new Graph();

			var random = new Random();
			graph.Vertices = Enumerable.Range(0, vertexCount)
				.Select(i => new Vertex(i, new Point(random.NextDouble() * size, random.NextDouble() * size)))
				.ToList();
			if (graph.Vertices.Count < 2)
				return graph;
			if (graph.Vertices.Count == 2)
			{
				graph.AddEdge(graph.Vertices[0], graph.Vertices[1]);
				return graph;
			}

			// соединяем точки гранями, строим триангуляцию Делоне
			// чтобы ускорить процесс, найдем самую левую вверхнюю точку, отсортируем все точки по расстоянию от нее
			// и соеденим с ближайшей к ней точке, оброзовав первую грань графа.

			var first = graph.Vertices[0];
			foreach (var v in graph.Vertices)
				if (v.X < first.X
					|| v.X == first.X && v.Y < first.Y)
					first = v;

			var range = new double[graph.Vertices.Count];
			for (var i = 0; i < graph.Vertices.Count; i++)
			{
				var x = graph.Vertices[i].X - first.X;
				var y = graph.Vertices[i].Y - first.Y;
				range[i] = x * x + y * y;
			}

			var sortedVertices = graph.Vertices.ToArray();
			Array.Sort(range, sortedVertices);

			var vertex = new TrisVertex[sortedVertices.Length];
			for (var i = 0; i < sortedVertices.Length; i++)
				vertex[i] = new TrisVertex(sortedVertices[i]);

			// создаем первую грань

			var tris = new Tris(vertex[0], vertex[1], vertex[2]);
			tris.MakeClockwise();

			var resultTriangles = new List<Tris>(sortedVertices.Length * 2);
			tris[0].Trises.Add(tris);
			tris[1].Trises.Add(tris);
			tris[2].Trises.Add(tris);
			resultTriangles.Add(tris);

			var hull = new CircularList<TrisEdge>();
			hull.AddItem(new TrisEdge(tris, tris[0], tris[1]));
			hull.AddItem(new TrisEdge(tris, tris[1], tris[2]));
			hull.AddItem(new TrisEdge(tris, tris[2], tris[0]));

			var seekStart = hull.Last;
			CircularItem<TrisEdge> visiblePoint = null;
			Tris returnLinkTris = null;

			for (var i = 3; i < vertex.Length; i++)
			{
				var nextPoint = vertex[i];

				if (seekStart.Value.EdgeVisibleFrom(nextPoint.Position))
					visiblePoint = seekStart;
				else
					visiblePoint = hull.FindNext(seekStart,
						t => t.Value.EdgeVisibleFrom(nextPoint.Position));

				var addNewPointInHull = false;

				if (visiblePoint != null)
				{
					var notVisibleLeft = hull.FindPrevious(visiblePoint,
						t => !t.Value.EdgeVisibleFrom(nextPoint.Position));

					var notVisibleRight = hull.FindNext(visiblePoint,
						t => !t.Value.EdgeVisibleFrom(nextPoint.Position));

					if (notVisibleLeft != null && notVisibleRight != null)
					{
						var visibleLeft = notVisibleLeft.Next;
						var edge = visibleLeft.Value;
						var hullItem = visibleLeft;

						Tris firstEdgeTris = null;
						Tris lastEdgeTris = null;

						while (hullItem != notVisibleRight)
						{
							var nextTris = new Tris(edge.A, nextPoint, edge.B);

							nextTris[0].Trises.Add(nextTris);
							nextTris[1].Trises.Add(nextTris);
							nextTris[2].Trises.Add(nextTris);
							resultTriangles.Add(nextTris);

							// разворачиваем треугольники, чтобы удовлетворяли критерию Делоне
							if (FlipIfNeeded(edge.tris, nextTris, nextPoint, out returnLinkTris))
								if (firstEdgeTris == null)
									firstEdgeTris = returnLinkTris;


							if (firstEdgeTris == null) firstEdgeTris = nextTris;

							lastEdgeTris = nextTris;

							hullItem = hullItem.Next;
							edge = hullItem.Value;
							addNewPointInHull = true;
						}

						// закрываем дырку
						hull.LinkTwoItem(notVisibleLeft, notVisibleRight);
						seekStart = hull.AddItemAfter(notVisibleLeft);
						seekStart.Value = new TrisEdge(firstEdgeTris, notVisibleLeft.Value.B, nextPoint);


						seekStart = hull.AddItemAfter(seekStart);
						seekStart.Value = new TrisEdge(lastEdgeTris, nextPoint, notVisibleRight.Value.A);
					}
				}

				if (!addNewPointInHull) visiblePoint = null;

			}

			// формируем связи на основе триангуляции
			var createEdges = 0;
			graph.Edges = new List<Edge>(resultTriangles.Count * 2);
			foreach (var tr in resultTriangles)
			{
				graph.AddEdge(tr[0].Original, tr[1].Original);
				graph.AddEdge(tr[1].Original, tr[2].Original);
				graph.AddEdge(tr[2].Original, tr[0].Original);

				createEdges++;
			}


			// пытаемся оставить только нужное кол-во связей, при этом проверяя что все узлы соеденены

			var needEdges = (int)(graph.Edges.Count / 100f * edgePercent);

			var rnd = new Random();

			var testEdges = new List<Edge>(graph.Edges);
			var beginEdgesCount = testEdges.Count;

			while (testEdges.Count > 0 && needEdges < graph.Edges.Count)
			{
				var removeIndex = rnd.Next(testEdges.Count);
				var testEdge = testEdges[removeIndex];

				if (graph.HasWayBetweenWithoutEdge(testEdge.A, testEdge.B))
					graph.RemoveEdge(testEdge.A, testEdge.B);

				testEdges.RemoveAt(removeIndex);
			}

			return graph;
		}

		private bool FlipIfNeeded(Tris trisA, Tris trisB, TrisVertex newPoint, out Tris rt)
		{
			var indexB = trisB.IndexOfPoint(newPoint);
			var indexBNext = indexB < 2 ? indexB + 1 : 0;
			var indexBPrev = indexB > 0 ? indexB - 1 : 2;

			var indexA = trisA.IndexOfThirdPoint(trisB[indexBNext], trisB[indexBPrev]);
			var downPoint = trisA[indexA];

			var indexANext = indexA < 2 ? indexA + 1 : 0;
			var indexAPrev = indexA > 0 ? indexA - 1 : 2;

			var needAnotherCut = trisA.IsPointBrokeDelaunay(newPoint);

			if (!needAnotherCut)
				needAnotherCut = trisB.IsPointBrokeDelaunay(downPoint);

			if (needAnotherCut)
			{
				// запоминаем треугольники, "ниже"
				Tris trA = null;
				Tris trB = null;

				trA = downPoint.FindAnotherTrisWith(trisA, trisA[indexANext], downPoint);
				trB = downPoint.FindAnotherTrisWith(trisA, trisA[indexAPrev], downPoint);

				// рубим по другому образуемый этими треугольниками четырехугольник.           
				trisA[indexAPrev].Trises.Remove(trisA);
				trisB[indexBPrev].Trises.Remove(trisB);

				trisA.Points[indexAPrev] = newPoint;
				trisB.Points[indexBPrev] = downPoint;

				newPoint.Trises.Add(trisA);
				downPoint.Trises.Add(trisB);

				// вызываем рекурсивно для внутренних
				rt = trisA;
				Tris tmp;

				if (trA != null)
					if (FlipIfNeeded(trA, trisA, newPoint, out tmp))
						rt = trisA = tmp;

				if (trB != null) FlipIfNeeded(trB, trisB, newPoint, out tmp);
				return true;
			}

			rt = null;
			return false;
		}

	}
}