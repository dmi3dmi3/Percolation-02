using GraphSdk.DataModels;
using System.Collections.Generic;
using System.Linq;

namespace PlaneGraph
{
	public static class GraphExtension
	{
		public static bool HasWayBetweenWithoutEdge(this Graph graph, Vertex a, Vertex b)
		{
			var wave = graph.Vertices.ToDictionary(vertex => vertex.Value.Id, vertex => 0);


			var buffer = new List<Vertex> {a};

			foreach (var near in a.ConnectedVertices)
			{
				if (Equals(near, b)) continue;
				buffer.Add(near);
			}

			wave[a.Id] = 1;

			var pos = 0;

			while (pos < buffer.Count)
			{
				var nextP = buffer[pos];
				if (wave[nextP.Id] == 0)
				{
					foreach (var near in nextP.ConnectedVertices)
					{
						if (Equals(near, b)) 
							return true;
						if (wave[near.Id] == 0) 
							buffer.Add(near);
					}

					wave[nextP.Id] = 1;
				}

				pos++;
			}

			return false;
		}
	}
}