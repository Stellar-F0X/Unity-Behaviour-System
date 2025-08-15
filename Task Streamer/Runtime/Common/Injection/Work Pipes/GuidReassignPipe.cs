using System.Collections.Generic;
using TaskStreamer.Utility;
using Unity.Properties;

namespace TaskStreamer.Injection
{
    internal class GuidReassignPipe : GraphWorkPipeBase, IVisitPropertyAdapter<NodeDictionary>
    {
        public GuidReassignPipe(GraphWorker graphWorker) : base(graphWorker) { }


        public override void Visit<TContainer>(in VisitContext<TContainer, GraphDictionary> context, ref TContainer container, ref GraphDictionary value)
        {
            GraphDictionary newDictionary = new GraphDictionary();

            foreach (KeyValuePair<UGUID, Graph> graphPair in value)
            {
                UGUID newGuid = UGUID.Create();
                UGUID baseGuid = graphPair.Value.baseGraphGuid;

                graphPair.Value.guid = newGuid;

                if (baseGuid.IsEmpty() == false)
                {
                    graphPair.Value.baseGraphGuid = value[baseGuid].guid;
                }

                newDictionary.Add(newGuid, graphPair.Value);
            }

            base.Visit(context, ref container, ref value);
            value = newDictionary;
        }


        public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
        {
            NodeDictionary newNodeDictionary = new NodeDictionary();

            foreach (KeyValuePair<UGUID, NodeBase> pair in value)
            {
                NodeBase node = value[pair.Key];
                node.guid = UGUID.Create();
                newNodeDictionary.Add(node.guid, node);

                if (node is ISubGraphProvider subGraphNode)
                {
                    UGUID guid = subGraphNode.subGraphGuid;
                    Graph graph = _worker.graphAsset.GetGraph(guid);
                    subGraphNode.subGraphGuid = graph.guid;
                }

                List<NodeGroup> groups = _worker.currentGraph.nodeGroup;
                NodeGroup group = groups.Find(e => e.Contains(pair.Key));

                if (group is not null)
                {
                    group.RemoveNodeFromGroup(pair.Key, false);
                    group.AddNodeToGroup(node.guid, false);
                }
            }

            value = newNodeDictionary;
        }
    }
}