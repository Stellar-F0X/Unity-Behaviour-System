using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TaskStreamer.Tool
{
    [UxmlElement]
    public partial class InspectorView : InspectorElement
    {
        private Editor _editor;


        public void ClearInspectorView()
        {
            base.Clear();
            Object.DestroyImmediate(_editor);
        }


        public void UpdateSelection(GraphElement visualElement)
        {
            this.ClearInspectorView();

            if (visualElement is null)
            {
                return;
            }

            Object drawTarget = null;

            switch (visualElement)
            {
                case NodeView view: drawTarget = view.targetNode; break;

                case TransitionEdgeView edge: drawTarget = edge.targetTransition; break;
            }

            if (drawTarget == null)
            {
                return;
            }

            this._editor = Editor.CreateEditor(drawTarget);
            base.Add(new IMGUIContainer(this.DrawInspectorGUI));
        }


        public void BorrowInspectorGUI(VisualElement element)
        {
            this.ClearInspectorView();
            base.Add(element);
        }


        private void DrawInspectorGUI()
        {
            if (this._editor?.target is null || this._editor.serializedObject.targetObject is null)
            {
                this.ClearInspectorView();
                return;
            }

            this._editor.OnInspectorGUI();
        }
    }
}