using System.Collections.Generic;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Injection
{
    internal class GuidRebindingProcess : GraphVisitProcess, IVisitPropertyAdapter<NodeDictionary>
    {
        public GuidRebindingProcess(GraphVisitProcessor processor) : base(processor) { }



        public override void Visit<TContainer>(in VisitContext<TContainer, GraphDictionary> context, ref TContainer container, ref GraphDictionary value)
        {
            value = this.ProcessGraphGuidReassignment(value);
            Debug.Assert(value is not null, "value is not null");
            base.Visit(context, ref container, ref value);
        }


        public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
        {
            value = this.ProcessNodeGuidReassignment(value);
            Debug.Assert(value is not null, "value is not null");
        }


        private GraphDictionary ProcessGraphGuidReassignment(GraphDictionary originalDictionary)
        {
            GraphDictionary newDictionary = new GraphDictionary();
            UGUIDDictionary guidMapping = new UGUIDDictionary(); // 이전 GUID -> 새 GUID 매핑

            foreach (KeyValuePair<UGUID, Graph> graphPair in originalDictionary)
            {
                UGUID originalBaseGuid = graphPair.Value.baseGraphGuid;
                UGUID newGuid = UGUID.Create();

                graphPair.Value.guid = newGuid;
                bool isRootGraph = originalBaseGuid.IsEmpty();

                if (isRootGraph)
                {
                    newDictionary.Add(graphPair.Value.guid, graphPair.Value);
                    continue;
                }

                UGUID changedBaseGuid = originalDictionary[originalBaseGuid].guid;
                graphPair.Value.baseGraphGuid = changedBaseGuid;

                if (guidMapping.TryGetValue(changedBaseGuid, out List<UGUID> list))
                {
                    list.Add(graphPair.Value.guid);
                }
                else
                {
                    guidMapping.Add(changedBaseGuid, new List<UGUID>());
                    guidMapping[changedBaseGuid].Add(graphPair.Value.guid);
                }

                newDictionary.Add(graphPair.Value.guid, graphPair.Value);
            }

            processor.graphAsset.graphMap = guidMapping;
            return newDictionary;
        }


        private NodeDictionary ProcessNodeGuidReassignment(NodeDictionary originalDictionary)
        {
            NodeDictionary newNodeDictionary = new NodeDictionary();

            foreach (KeyValuePair<UGUID, NodeBase> pair in originalDictionary)
            {
                NodeBase node = originalDictionary[pair.Key];
                UGUID oldGuid = pair.Key;

                node.guid = UGUID.Create();

                newNodeDictionary.Add(node.guid, node);

                if (node is ISubGraphProvider subGraphNode)
                {
                    Graph graph = processor.graphAsset.GetGraph(subGraphNode.subGraphGuid);
                    subGraphNode.subGraphGuid = graph.guid;
                }

                List<NodeGroup> groups = processor.currentGraph.nodeGroup;
                NodeGroup group = groups.Find(e => e.Contains(oldGuid));

                if (group is null)
                {
                    continue;
                }

                group.RemoveNodeFromGroup(oldGuid, false);
                group.AddNodeToGroup(node.guid, false);
            }

            return newNodeDictionary;
        }
    }
}