using TaskStreamer.Runtime.FSM;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace TaskStreamer.Tool
{
    // Referenced: https://github.com/FBast/unity-graphtools-fsm/blob/main/Editor/Edges/TransitionEdge.cs
    // Copyright (c) 2021 Original Author
    // Licensed under the MIT License. See LICENSE file in the root for details.
    /// <summary> Edge class with arrow styling and transition management. </summary>
    internal class FSMEdge : Edge, ISelectableView
    {
        /// Represents a custom Unity edge with arrow functionality for graph-based tools.
        public FSMEdge()
        {
            this.styleSheets.Add(TSUIElementSettings.instance.EdgeStyle); //USS 추가
            this.generateVisualContent = this.DrawArrow;
            this.isGhostEdgeMode = true;

            this.RegisterCallback<MouseEnterEvent>(this.OnMouseEnterCallback);
            this.RegisterCallback<MouseLeaveEvent>(this.OnMouseLeaveCallback);
        }


        /// Represents a custom edge in a graph view with additional functionalities such as events and transition data.
        public FSMEdge(Transition transition) : this()
        {
            this.targetTransition = transition;
            this.viewDataKey = transition.guid.ToString();
        }

        
        /// <summary> 화살표의 너비를 나타내는 상수 값 </summary>
        private const float _ARROW_WIDTH = 12f;

        
        /// <summary> Indicates whether the hover state is currently active. </summary>
        private bool _isHoverActivated;

        
        /// <summary> Transition 대상 데이터를 저장하는 비공개 필드입니다. </summary>
        private Transition _targetTransition;




        /// <summary> 연결된 Transition 개체를 나타내며, 내부적으로 설정 가능합니다. </summary>
        public Transition targetTransition
        {
            get { return this._targetTransition; }

            internal set { this._targetTransition = value; }
        }
        

        /// <summary> Indicates whether the edge is in ghost mode. </summary>
        public bool isGhostEdgeMode
        {
            set { this.isGhostEdge = value; }
        }



        /// <summary>EdgeControl 객체를 생성하여 반환한다.</summary>
        /// <returns>생성된 EdgeControl 객체.</returns>
        protected override EdgeControl CreateEdgeControl()
        {
            return base.CreateEdgeControl();
        }


        /// <summary>Triggers the transition selection event if it exists.</summary>
        public override void OnSelected()
        {
            TSEditor.Instance.taskGraphView.CallSelectionEvent(this);
        }


        /// <summary>Called when the edge is unselected by the user.</summary>
        public override void OnUnselected()
        {
            TSEditor.Instance.taskGraphView.CallUnselectionEvent(this);
        }


        /// <summary>Handles the mouse leave event for the arrow edge.</summary>
        /// <param name="evt">The mouse leave event instance.</param>
        private void OnMouseLeaveCallback(MouseLeaveEvent evt)
        {
            this._isHoverActivated = false;
            this.MarkDirtyRepaint(); //call generateVisualContent
        }


        /// Handles the event triggered when the mouse pointer enters the edge.
        /// <param name="evt">The mouse enter event containing event-specific details.</param>
        private void OnMouseEnterCallback(MouseEnterEvent evt)
        {
            this._isHoverActivated = true;
            this.MarkDirtyRepaint(); //call generateVisualContent
        }


        /// Determines whether the specified local point resides within the boundaries of the edge, including the arrow section.
        /// <param name="localPoint">The local point to check, relative to the edge.</param>
        /// <returns>true if the local point resides within the boundary of the edge or near the arrow area; false otherwise.</returns>
        public override bool ContainsPoint(Vector2 localPoint)
        {
            if (base.ContainsPoint(localPoint))
            {
                return true;
            }

            Vector2 start = PointsAndTangents[PointsAndTangents.Length / 2 - 1];
            Vector2 end = PointsAndTangents[PointsAndTangents.Length / 2];
            Vector2 mid = (start + end) / 2;

            return (localPoint - mid).sqrMagnitude <= (_ARROW_WIDTH * _ARROW_WIDTH);
        }


        /// <summary>Updates the edge control points and tangents based on input and output node positions.</summary>
        /// <returns>True if the edge control was successfully updated; otherwise, false.</returns>
        public override bool UpdateEdgeControl()
        {
            base.UpdateEdgeControl();

            if (PointsAndTangents is null || PointsAndTangents.Length < 4)
            {
                return false;
            }

            if (this.CalculateEdgePoints(input?.node, output?.node, out Vector2 from, out Vector2 to) == false)
            {
                return false;
            }

            Vector2 tangent = Vector2.zero;

            if (isGhostEdge == false && output?.node is not null && input?.node is not null)
            {
                Vector2 dir = (to - from).normalized;
                Vector2 perpendicular = new Vector2(-dir.y, dir.x);
                float curveStrength = Mathf.Min(5f, Vector2.Distance(from, to) * 0.3f);
                tangent = perpendicular * curveStrength;
            }

            PointsAndTangents[0] = from;
            PointsAndTangents[1] = from + tangent;
            PointsAndTangents[2] = to + tangent;
            PointsAndTangents[3] = to;

            this.MarkDirtyRepaint();
            return true;
        }


        /// <summary>Draws a visual arrow for the edge in a graph view using the provided context.</summary>
        /// <param name="context">The mesh generation context used for rendering the arrow.</param>
        private void DrawArrow(MeshGenerationContext context)
        {
            if (this.CalculateArrowGeometry(out Vector2 mid, out Vector2 dir, out float disFromMid, out Vector2 perpendicular) == false)
            {
                return;
            }
            
            MeshWriteData mesh = context.Allocate(3, 3);
            Vertex[] vertices = new Vertex[3];
            ushort[] indices = new ushort[3];

            vertices[0].position = mid + dir * disFromMid;
            vertices[1].position = mid - dir * disFromMid + perpendicular;
            vertices[2].position = mid - dir * disFromMid - perpendicular;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].position += Vector3.forward * Vertex.nearZ;
                vertices[i].tint = this.GetColorByStatus();
                indices[i] = (ushort)i;
            }

            mesh.SetAllVertices(vertices);
            mesh.SetAllIndices(indices);
        }



        /// <summary>Calculates the geometry of the arrow for rendering.</summary>
        /// <param name="mid">The midpoint of the arrow geometry.</param>
        /// <param name="dis">The direction vector from start to end.</param>
        /// <param name="disFromMid">The distance from the midpoint to the arrow tip.</param>
        /// <param name="perpendicular">The perpendicular direction for defining the arrow's width.</param>
        /// <returns>true if the geometry is successfully calculated; otherwise, false.</returns>
        private bool CalculateArrowGeometry(out Vector2 mid, out Vector2 dis, out float disFromMid, out Vector2 perpendicular)
        {
            Vector2 start = PointsAndTangents[PointsAndTangents.Length / 2 - 1];
            Vector2 end = PointsAndTangents[PointsAndTangents.Length / 2];
            mid = (start + end) * 0.5f;
            dis = end - start;

            if (dis.sqrMagnitude < 0.01f)
            {
                perpendicular = Vector2.zero;
                disFromMid = 0;
                return false;
            }

            dis.Normalize();

            float width = _ARROW_WIDTH * (_isHoverActivated ? 1.5f : 1f);
            perpendicular = new Vector2(-dis.y, dis.x) * width * 0.5f;
            disFromMid = width * 1.732050f * 0.25f;
            return true;
        }



        /// <summary>Calculates the edge points needed for rendering a connection between nodes.</summary>
        /// <param name="inputNode">The input node of the edge. This can be null if only the output node is defined.</param>
        /// <param name="outputNode">The output node of the edge. This can be null if only the input node is defined.</param>
        /// <param name="from">The starting point of the edge. This is an output parameter that will contain the calculated position.</param>
        /// <param name="to">The ending point of the edge. This is an output parameter that will contain the calculated position.</param>
        /// <returns>Returns true if edge points are successfully calculated, otherwise false.</returns>
        private bool CalculateEdgePoints(Node inputNode, Node outputNode, out Vector2 from, out Vector2 to)
        {
            from = Vector2.zero;
            to = Vector2.zero;

            // output 또는 input 포트 중 하나라도 유효한 노드가 있어야 함.
            if (outputNode is null && inputNode is null)
            {
                return false;
            }

            if (this.isGhostEdge)
            {
                // 드래그 중 (고스트 엣지) 인 경우
                if (outputNode is not null)
                {
                    from = outputNode.GetPosition().center;
                    to = edgeControl.to;
                }
                else
                {
                    from = edgeControl.from;
                    to = inputNode.GetPosition().center;
                }

                return true;
            }

            // 완성된 엣지인 경우
            if (outputNode is null || inputNode is null)
            {
                return false;
            }

            from = outputNode.GetPosition().center;
            to = inputNode.GetPosition().center;
            return true;
        }



        /// <summary>Returns the color corresponding to the current edge status.</summary>
        /// <returns>The color based on the edge state such as ghost, selected, input, or output status.</returns>
        private Color GetColorByStatus()
        {
            if (base.isGhostEdge)
            {
                return base.ghostColor;
            }

            if (base.selected)
            {
                return base.selectedColor;
            }

            if (input is not null)
            {
                return edgeControl.inputColor;
            }

            if (output is not null)
            {
                return edgeControl.outputColor;
            }

            return base.defaultColor;
        }
    }
}