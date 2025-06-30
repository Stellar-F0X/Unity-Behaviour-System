using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Injection
{
#if UNITY_EDITOR
    public class NodeGroupAdapter : IVisitPropertyAdapter<NodeGroup>
    {
        public NodeGroupAdapter(GraphVisitor visitor)
        {
            _visitor = visitor;
        }
        
        private readonly GraphVisitor _visitor;
        
        public void Visit<TContainer>(in VisitContext<TContainer, NodeGroup> context, ref TContainer container, ref NodeGroup value)
        {
            Debug.Log($"NodeGroup : {context.Property.Name}");
        }
    }
#endif
}