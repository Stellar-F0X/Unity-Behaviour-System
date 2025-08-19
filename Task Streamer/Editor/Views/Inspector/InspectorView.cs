using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaskStreamer.Injection;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    [UxmlElement]
    public partial class InspectorView : InspectorElement
    {
        public void ClearInspectorView()
        {
            this.Clear();
        }


        public void UpdateSelection(GraphElement visualElement)
        {
            this.ClearInspectorView();

            Debug.Assert(visualElement is not null, "visualElement is null");

            switch (visualElement)
            {
                case NodeViewBase view: this.Add(new TaskInspectorView(view.targetNode, view.onRenamingNode, view.fieldProperties)); break;
                
                case ArrowEdge edge: this.Add(new TaskInspectorView(edge.targetTransition, null, edge.fieldProperties)); break;
            }
        }
    }
}