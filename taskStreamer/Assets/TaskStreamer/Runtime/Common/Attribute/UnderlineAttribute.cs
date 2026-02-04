using System;
using UnityEngine;

namespace TaskStreamer.Runtime
{
	[AttributeUsage(AttributeTargets.Field)]
	public class UnderlineAttribute : PropertyAttribute
	{
		public UnderlineAttribute(string title = null, float r = 0.7f, float g = 0.7f, float b = 0.7f, float thickness = 1f, float spacing = 5f)
		{
			this.title = title;
			this.color = new Color(r, g, b, 1f);
			this.thickness = thickness;
			this.spacing = spacing;
		}

		public readonly string title;
		public readonly Color color;
		public readonly float thickness;
		public readonly float spacing;
	}
}