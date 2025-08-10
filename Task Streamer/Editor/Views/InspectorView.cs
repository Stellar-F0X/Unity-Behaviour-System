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
        private IMGUIContainer _container;

        private Editor _editor;


        public void ClearInspectorView()
        {
            this.Clear();
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
            
            if (_container is null || _container.parent is null)
            {
                // Container가 없거나 부모에서 제거된 경우에만 새로 생성
                _container = new IMGUIContainer(this.DrawInspectorGUI);
                
                base.Add(_container);
            }
        }


        private void DrawInspectorGUI()
        {
            bool clearFlag = false;

            if (this._editor == null || this._editor.target == null)
            {
                clearFlag = true;
            }

            if (this._editor.serializedObject.targetObject == null)
            {
                clearFlag = true;
            }

            if (clearFlag)
            {
                this.ClearInspectorView();
            }
            else
            {
                this._editor.OnInspectorGUI();
            }
        }
    }
}