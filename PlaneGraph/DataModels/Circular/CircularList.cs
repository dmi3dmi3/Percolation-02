using System;

namespace PlaneGraph.DataModels.Circular
{
	public class CircularList<T>
	{
		private CircularItem<T> _removed;
		public CircularItem<T> Last;

		public CircularList()
		{
			Last = null;
			_removed = null;
		}

		private int ItemsCount
		{
			get
			{
				if (Last == null) return 0;

				var n = 1;
				var cursor = Last.Next;
				while (cursor != Last)
				{
					n++;
					cursor = cursor.Next;
				}

				return n;
			}
		}

		public CircularItem<T> AddItem(T value)
		{
			CircularItem<T> item;
			if (_removed != null)
			{
				item = _removed;
				item.Value = value;
				_removed = _removed.Next;
			}
			else
			{
				item = new CircularItem<T>(value);
			}

			if (Last != null)
			{
				item.Next = Last.Next;
				item.Previous = Last;
				Last.Next.Previous = item;
				Last.Next = item;
			}
			else
			{
				item.Next = item;
				item.Previous = item;
			}

			Last = item;
			return Last;
		}

		public void LinkTwoItem(CircularItem<T> left, CircularItem<T> right)
		{
			if (left.Next != right)
			{
				// есть кого удалять в кольце
				right.Previous.Next = _removed;
				_removed = left.Next;
				_removed.Previous = null;
			}

			left.Next = right;
			right.Previous = left;
			Last = right;
		}

		public CircularItem<T> AddItemAfter(CircularItem<T> circItem, T value)
		{
			if (circItem == null)
				return AddItem(value);

			CircularItem<T> item;
			if (_removed != null)
			{
				item = _removed;
				item.Value = value;
				_removed = _removed.Next;
			}
			else
			{
				item = new CircularItem<T>(value);
			}

			item.Next = circItem.Next;
			item.Previous = circItem;
			circItem.Next.Previous = item;
			circItem.Next = item;

			Last = item;
			return Last;
		}

		public CircularItem<T> AddItem()
		{
			CircularItem<T> item;
			if (_removed != null)
			{
				item = _removed;
				_removed = _removed.Next;
			}
			else
			{
				item = new CircularItem<T>();
			}

			if (Last != null)
			{
				item.Next = Last.Next;
				item.Previous = Last;
				Last.Next.Previous = item;
				Last.Next = item;
			}
			else
			{
				item.Next = item;
				item.Previous = item;
			}

			Last = item;
			return Last;
		}

		public CircularItem<T> AddItemAfter(CircularItem<T> circItem)
		{
			if (circItem == null)
				return AddItem();

			CircularItem<T> item;
			if (_removed != null)
			{
				item = _removed;
				_removed = _removed.Next;
			}
			else
			{
				item = new CircularItem<T>();
			}

			item.Next = circItem.Next;
			item.Previous = circItem;
			circItem.Next.Previous = item;
			circItem.Next = item;

			Last = item;
			return Last;
		}

		public CircularItem<T> FindNext(CircularItem<T> from, Predicate<CircularItem<T>> match)
		{
			var result = from.Next;

			while (result != from)
			{
				if (match(result))
					return result;
				result = result.Next;
			}

			return null;
		}

		public CircularItem<T> FindPrevious(CircularItem<T> from, Predicate<CircularItem<T>> match)
		{
			var result = from.Previous;

			while (result != from)
			{
				if (match(result))
					return result;
				result = result.Previous;
			}

			return null;
		}

	}
}