using System;
using System.Collections.Generic;

namespace TaskStreamer.Runtime.Utility
{
	public static class Utility
	{
		public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
		{
			foreach (T item in collection) { action.Invoke(item); }
		}
	}
}