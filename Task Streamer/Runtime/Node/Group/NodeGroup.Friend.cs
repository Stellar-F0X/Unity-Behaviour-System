using System;

namespace TaskStreamer
{
    public partial class NodeGroup
    {
        public class Friend
        {
            protected void AddNodeToGroup(NodeGroup group, Action addAction) => group.AddNodeToGroup(addAction);
            
            protected void RemoveNodeFromGroup(NodeGroup group, Action removeAction) => group.RemoveNodeFromGroup(removeAction);

            protected void ChangeNodeGroupPosition(NodeGroup group, Action moveAction) => group.ChangeNodeGroupPosition(moveAction);

            protected void ChangeNodeGroupTitle(NodeGroup group, Action renameAction) => group.ChangeNodeGroupTitle(renameAction);
        }
    }
}