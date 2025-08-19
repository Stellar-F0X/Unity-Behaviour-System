using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public class PortView : Port
    {
        public PortView(GraphType graphType, Direction direction, Capacity capacity) : base(Orientation.Vertical, direction, capacity, typeof(bool))
        {
            switch (graphType)
            {
                case GraphType.BT: base.m_EdgeConnector = new EdgeConnector<LinearEdge>(new BTEdgeConnectorListener()); break;

                case GraphType.FSM: base.m_EdgeConnector = new EdgeConnector<ArrowEdge>(new FSMEdgeConnectorListener()); break;
                
                default: throw new ArgumentOutOfRangeException(nameof(graphType), graphType, null);
            }
            
            this.AddManipulator(base.m_EdgeConnector);
        }


        public override bool ContainsPoint(Vector2 localPoint)
        {
            Rect rect = new Rect(0, 0, layout.width, layout.height);
            
            return rect.Contains(localPoint);
        }
    }
}