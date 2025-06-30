using TaskStreamer.BT;
using TaskStreamer.FSM;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    //Referenced: https://github.com/thekiwicoder0/UnityBehaviourTreeEditor/blob/main/Editor/DoubleClickNode.cs
    public class DoubleClick : MouseManipulator
    {
        public DoubleClick(float doubleClickDuration)
        {
            _doubleClickDuration = doubleClickDuration;
        }

        private double _measurementStartTime = EditorApplication.timeSinceStartup;

        private double _doubleClickDuration;


        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }


        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }


        private void OnMouseDown(MouseDownEvent evt)
        {
            if (base.CanStopManipulation(evt) == false)
            {
                return;
            }

            NodeView clickedElement = evt.target as NodeView;

            if (clickedElement is null)
            {
                VisualElement element = evt.target as VisualElement;

                clickedElement = element.GetFirstAncestorOfType<NodeView>();
            }

            if (clickedElement is not null)
            {
                double duration = EditorApplication.timeSinceStartup - _measurementStartTime;

                if (duration < _doubleClickDuration)
                {
                    this.OnDoubleClick(evt, clickedElement);
                    evt.StopImmediatePropagation();
                }

                _measurementStartTime = EditorApplication.timeSinceStartup;
            }
        }


        private void OnDoubleClick(MouseDownEvent evt, NodeView clickedElement)
        {
            if (clickedElement.targetNode is ISubGraph subGraph)
            {
                Graph graph = TaskStreamerEditor.Instance.graphAsset.GetGraph(subGraph.subGraphGuid);

                if (graph != null)
                {
                    TaskStreamerEditor.Instance.ChangeGraph(graph, true);
                }
                else
                {
                    Debug.LogError($"{nameof(DoubleClick)}: Graph is null");
                }
            }
        }
    }
}