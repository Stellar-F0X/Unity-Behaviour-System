using System;
using System.Collections.Generic;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace TaskStreamer.Tool
{
    // Referenced: https://github.com/FBast/unity-graphtools-fsm/blob/main/Editor/Edges/TransitionEdge.cs
    // Copyright (c) 2021 Original Author
    // Licensed under the MIT License. See LICENSE file in the root for details.
    public class ArrowEdge : Edge
    {
        public ArrowEdge()
        {
            this.styleSheets.Add(TaskStreamerEditor.settings.EdgeStyle); //USS 추가
            this.generateVisualContent = this.DrawArrow;
            this.isGhostEdgeMode = true;

            this.RegisterCallback<MouseEnterEvent>(_ => _hover = true);
            this.RegisterCallback<MouseLeaveEvent>(_ => _hover = false);
        }

        public ArrowEdge(Transition transition) : this()
        {
            this.RefreshTransitionData(transition);
        }
        

        private const float _ARROW_WIDTH = 12f;

        private bool _hover;


        public event Action<GraphElement> onTransitionSelected;
        public event Action<GraphElement> onTransitionUnselected;

        private Transition _targetTransition;




        public Transition targetTransition
        {
            get { return _targetTransition; }

            internal set { this._targetTransition = value; }
        }

        internal List<object> fieldProperties
        {
            get;
            private set;
        }

        public bool isGhostEdgeMode
        {
            set { this.isGhostEdge = value; }
        }



        protected override EdgeControl CreateEdgeControl()
        {
            return base.CreateEdgeControl();
        }
        
        
        public void RefreshTransitionData(in Transition newTransition)
        {
            Type type = newTransition.GetType();
            
            this.targetTransition = newTransition;
            this.fieldProperties = TypeUtility.TryGetFieldProperties(type, newTransition);
        }


        public override void OnSelected()
        {
            onTransitionSelected?.Invoke(this);
        }


        public override void OnUnselected()
        {
            onTransitionUnselected?.Invoke(this);
        }


        /// Determines whether the specified local point resides within the boundaries
        /// of the edge, including the arrow section.
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


        /// Updates the edge control points and tangents for the edge based on the positions
        /// of the associated input and output nodes, applying curvature and styling adjustments
        /// if necessary. If conditions for updating are not met, it returns false.
        /// <returns>
        /// True if the edge control was successfully updated; otherwise, false.
        /// </returns>
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
                float distance = Vector2.Distance(from, to);
                Vector2 perpendicular = new Vector2(-dir.y, dir.x);
                float curveStrength = Mathf.Min(5f, distance * 0.3f);
                tangent = perpendicular * curveStrength;
            }

            PointsAndTangents[0] = from;
            PointsAndTangents[1] = from + tangent;
            PointsAndTangents[2] = to + tangent;
            PointsAndTangents[3] = to;

            this.MarkDirtyRepaint();
            return true;
        }


        /// <summary>
        /// Draws a visual arrow for the edge in a graph view using the provided context.
        /// </summary>
        /// <param name="context">The mesh generation context used for rendering the arrow.</param>
        private void DrawArrow(MeshGenerationContext context)
        {
            Vector2 start = PointsAndTangents[PointsAndTangents.Length / 2 - 1];
            Vector2 end = PointsAndTangents[PointsAndTangents.Length / 2];
            Vector2 mid = (start + end) * 0.5f;
            Vector2 direction = end - start;

            if (direction.sqrMagnitude < 0.01f)
            {
                return;
            }

            direction.Normalize();

            float width = _ARROW_WIDTH * (_hover ? 1.5f : 1f);
            float perpendicularLength = width * 0.5f;
            float distanceFromMid = width * 1.732050f * 0.25f;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x) * perpendicularLength;

            MeshWriteData mesh = context.Allocate(3, 3);
            Vertex[] vertices = new Vertex[3];
            ushort[] indices = new ushort[3];

            vertices[0].position = mid + direction * distanceFromMid;
            vertices[1].position = mid - direction * distanceFromMid + perpendicular;
            vertices[2].position = mid - direction * distanceFromMid - perpendicular;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].position += Vector3.forward * Vertex.nearZ;
                vertices[i].tint = this.GetColorByStatus();
                indices[i] = (ushort)i;
            }

            mesh.SetAllVertices(vertices);
            mesh.SetAllIndices(indices);
        }


        /// Calculates the edge points needed for rendering a connection between nodes.
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



        /// Returns a color based on the current status of the edge.
        /// The method determines the appropriate color for the edge based on its state.
        /// If the edge is in ghost mode, it returns the ghost color.
        /// If the edge is selected, it returns the selected color.
        /// If the edge has an input or output, it returns the corresponding input or output color.
        /// Otherwise, it returns the default edge color.
        /// <returns>The color corresponding to the current state of the edge.</returns>
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