using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public class PortView : Port
    {
        public PortView(EGraphType graphType, Direction direction, Capacity capacity) : base(Orientation.Vertical, direction, capacity, typeof(bool))
        {
            switch (graphType)
            {
                case EGraphType.BT: base.m_EdgeConnector = new EdgeConnector<Edge>(new BTEdgeConnectorListener()); break;

                case EGraphType.FSM: base.m_EdgeConnector = new EdgeConnector<TransitionEdgeView>(new FSMEdgeConnectorListener()); break;
                
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