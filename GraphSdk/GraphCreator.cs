using GraphSdk.DataModels;

namespace GraphSdk
{
	public abstract class GraphCreator
	{
		public abstract Graph GenerateGraph(int vertexCount, int edgePercent, int size);
	}
}