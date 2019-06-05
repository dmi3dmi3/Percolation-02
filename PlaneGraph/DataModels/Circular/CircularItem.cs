namespace PlaneGraph.DataModels.Circular
{
	public class CircularItem<T>
	{
		public CircularItem<T> Next;
		public CircularItem<T> Previous;
		public T Value;

		public CircularItem(T value)
		{
			Value = value;
		}

		public CircularItem()
		{
		}

		public override string ToString() => Value.ToString();
	}
}