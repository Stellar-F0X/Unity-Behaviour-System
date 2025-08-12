using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class NodeGroupView : Group
    {
        public NodeGroupView(NodeGroupData groupDataContainer, Color backgroundColor) : base()
        {
            this.title = groupDataContainer.title;
            this._groupData = groupDataContainer;
            this.style.backgroundColor = backgroundColor;
        }
        
        private readonly NodeGroupData _groupData;


        public NodeGroupData groupData
        {
            get { return _groupData; }
        }


        protected override void OnGroupRenamed(string oldName, string newName)
        {
            _groupData.ChangeNodeGroupTitle(newName);
        }

        
        //NodeView도 위치를 Record하는데, GroupView를 움직이면 NodeView도 움직이며 위치가 기록되어 Undo 기록이 중첩됨.
        //따라서 Group에 요소가 있는 상태로 움직인 후 GroupView가 정상적으로 동작하려면 여러번 Undo해야 되며
        //또한 NodeView를 기준으로 GroupView 위치가 정해지기 때문에 Group에 요소가 없는 상태일 때만 기록시킴.  
        protected override void SetScopePositionOnly(Rect newPos)
        {
            this._groupData.ChangeNodeGroupPosition(newPos.position);
            
            base.SetScopePositionOnly(newPos);
        }


        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            if (this.IsEditorAddDisabled())
            {
                return;
            }

            foreach (GraphElement element in elements)
            {
                if (element.selected && element is NodeViewBase view && view.targetNode != null)
                {
                    _groupData.AddNodeToGroup(view.targetNode.guid);
                }
            }
        }


        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            if (this.IsEditorAddDisabled())
            {
                return;
            }

            foreach (GraphElement element in elements)
            {
                if (element.selected && element is NodeViewBase view && view.targetNode != null)
                {
                    _groupData.RemoveNodeFromGroup(view.targetNode.guid);
                }
            }
        } 
        
        
        private bool IsEditorAddDisabled()
        {
            if (TaskStreamerEditor.Instance is null || _groupData is null)
            {
                return true;
            }

            if (TaskStreamerEditor.canEditGraph == false || TaskStreamerEditor.isLoadingTreeToView)
            {
                return true;
            }

            return false;
        }
    }
}