namespace GraphTools.Models
{
	public class Edge
	{
		public readonly Vertex A;
		public readonly Vertex B;

		public Edge(Vertex a, Vertex b)
		{
			A = a;
			B = b;
		}

		public override bool Equals(object obj) =>
			obj is Edge edge 
			&& (A.Equals(edge.A) && B.Equals(edge.B) 
			    || B.Equals(edge.A) && A.Equals(edge.B));

		public override int GetHashCode() => (A.GetHashCode() + B.GetHashCode()).GetHashCode();
	}
}