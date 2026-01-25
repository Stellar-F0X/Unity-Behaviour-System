using System;

namespace TaskStreamer.Tool
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	internal class SearchTreeEntryNameAttribute : Attribute
	{
		public SearchTreeEntryNameAttribute(string displayName)
		{
			this.displayName = displayName;
		}
		
		public string displayName;
	}
}