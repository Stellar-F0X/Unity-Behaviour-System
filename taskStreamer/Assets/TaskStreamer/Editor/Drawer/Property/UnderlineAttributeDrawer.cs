using TaskStreamer.Runtime;
using UnityEditor;
using UnityEngine;

namespace TaskStreamer.Tool
{
	[CustomPropertyDrawer(typeof(UnderlineAttribute), true)]
	public class UnderlineAttributeDrawer : PropertyDrawer
	{
		private const float TitleHeight = 18f;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			UnderlineAttribute attr = (UnderlineAttribute)base.attribute;
			float height = EditorGUI.GetPropertyHeight(property, label, true);
			height += attr.thickness;

			if (string.IsNullOrEmpty(attr.title) == false)
			{
				height += TitleHeight + attr.spacing;
			}

			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			UnderlineAttribute attr = (UnderlineAttribute)base.attribute;
			float currentY = position.y;
			GUIContent fieldLabel = new GUIContent(label);

			if (string.IsNullOrEmpty(attr.title) == false)
			{
				Rect titleRect = new Rect(position.x, currentY, position.width, TitleHeight);
				EditorGUI.LabelField(titleRect, attr.title, EditorStyles.boldLabel);
				currentY += TitleHeight + attr.spacing * 0.5f;
			}

			Rect lineRect = new Rect(position.x, currentY, position.width, attr.thickness);
			EditorGUI.DrawRect(lineRect, attr.color);
			currentY += attr.thickness + attr.spacing * 0.5f;

			float propertyHeight = EditorGUI.GetPropertyHeight(property, fieldLabel, true);
			Rect propertyRect = new Rect(position.x, currentY, position.width, propertyHeight);
			EditorGUI.PropertyField(propertyRect, property, fieldLabel, true);
		}
	}
}
