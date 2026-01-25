using System;
using System.Collections;
using System.Collections.Generic;

namespace TaskStreamer.Runtime
{
	[Serializable]
	public class ObservableList<T> : IList<T>, IList, IReadOnlyList<T>
	{
		public ObservableList() : this(new List<T>()) {}

		public ObservableList(List<T> referencedList)
		{
			_referencedList = referencedList;
		}

		public event Action<NotifyListChanged, T, int> onChanged;
		
		private readonly List<T> _referencedList;

		
		public T this[int index]
		{
			set { onChanged?.Invoke(NotifyListChanged.Change, (_referencedList[index] = value), index); }
			
			get { return _referencedList[index]; }
		}

		object IList.this[int index]
		{
			get { return _referencedList[index]; }
			
			set { this[index] = (T)value; }
		}

		public int Count
		{
			get { return _referencedList.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool IsFixedSize
		{
			get { return false; }
		}

		public bool IsSynchronized
		{
			get { return false; }
		}

		public object SyncRoot
		{
			get { return ((ICollection)_referencedList).SyncRoot; }
		}

		
		public void Add(T item)
		{
			_referencedList.Add(item);
			onChanged?.Invoke(NotifyListChanged.Add, item, _referencedList.Count - 1);
		}

		
		int IList.Add(object value)
		{
			this.Add((T)value);
			return _referencedList.Count - 1;
		}
		

		public bool Remove(T item)
		{
			int index = _referencedList.IndexOf(item);

			if (index == -1)
			{
				return false;
			}

			bool removed = _referencedList.Remove(item);
			
			if (removed)
			{
				onChanged?.Invoke(NotifyListChanged.Remove, item, index);
			}

			return removed;
		}

		
		void IList.Remove(object value)
		{
			this.Remove((T)value);
		}

		
		public void Clear()
		{
			_referencedList.Clear();
			this.onChanged?.Invoke(NotifyListChanged.Clear, default, -1);
		}

		
		public bool Contains(T item)
		{
			return _referencedList.Contains(item);
		}

		
		bool IList.Contains(object value)
		{
			return Contains((T)value);
		}
		

		public int IndexOf(T item)
		{
			return _referencedList.IndexOf(item);
		}
		

		int IList.IndexOf(object value)
		{
			return IndexOf((T)value);
		}

		
		public void Insert(int index, T item)
		{
			_referencedList.Insert(index, item);
			this.onChanged?.Invoke(NotifyListChanged.Add, item, index);
		}

		
		void IList.Insert(int index, object value)
		{
			Insert(index, (T)value);
		}

		
		public void RemoveAt(int index)
		{
			T item = _referencedList[index];
			_referencedList.RemoveAt(index);
			this.onChanged?.Invoke(NotifyListChanged.Remove, item, index);
		}
		

		public void CopyTo(T[] array, int arrayIndex)
		{
			_referencedList.CopyTo(array, arrayIndex);
		}

		
		public IEnumerator<T> GetEnumerator()
		{
			return _referencedList.GetEnumerator();
		}
		

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		
		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)_referencedList).CopyTo(array, index);
		}
	}
}