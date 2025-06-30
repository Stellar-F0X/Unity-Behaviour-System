using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class NodeGroupView : Group
    {
        public NodeGroupView(NodeGroupData dataContainer) : base()
        {
            this._data = dataContainer;

            this.title = dataContainer.title;
            this.style.backgroundColor = TaskStreamerEditor.settings.nodeGroupColor;
        }
        
        private readonly NodeGroupData _data;


        public NodeGroupData data
        {
            get { return _data; }
        }


        protected override void OnGroupRenamed(string oldName, string newName)
        {
            Undo.RecordObject(_data, "Task Streamer (NodeGroupViewNameChanged)");
            base.OnGroupRenamed(oldName, newName);
            _data.title = newName;
            EditorUtility.SetDirty(_data);
        }


        protected override void SetScopePositionOnly(Rect newPos)
        {
            this._data.ChangeNodePosition(newPos.position);
            base.SetScopePositionOnly(newPos);
        }


        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            if (TaskStreamerEditor.Instance is null || _data is null)
            {
                return;
            }

            if (TaskStreamerEditor.canEditGraph && TaskStreamerEditor.isLoadingTreeToView == false)
            {
                foreach (var element in elements)
                {
                    if (element.selected && element is NodeView view && view.targetNode != null)
                    {
                        _data.AddNodeGuid(view.targetNode.guid);
                    }
                }
            }
        }


        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            if (TaskStreamerEditor.Instance is null || _data is null)
            {
                return;
            }

            if (TaskStreamerEditor.canEditGraph && TaskStreamerEditor.isLoadingTreeToView == false)
            {
                foreach (var element in elements)
                {
                    if (element.selected && element is NodeView view && view.targetNode != null)
                    {
                        _data.RemoveNodeGuid(view.targetNode.guid);
                    }
                }
            }
        } 
    }
}