using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public static class EditorExtension
    {
        public static void SetBorderColor(this IStyle elementStyle, Color color)
        {
            elementStyle.borderTopColor = color;
            elementStyle.borderBottomColor = color;
            elementStyle.borderLeftColor = color;
            elementStyle.borderRightColor = color;
        }
        
        
        public static void SetEdgeColor(this Edge edge, Color color)
        {
            edge.edgeControl.inputColor = color;
            edge.edgeControl.outputColor = color;
        }
    }
}