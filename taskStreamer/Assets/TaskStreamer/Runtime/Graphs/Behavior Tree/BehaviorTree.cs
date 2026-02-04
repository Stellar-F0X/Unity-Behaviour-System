using System;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TaskStreamer.Runtime.BT
{
    [Serializable]
    public partial class BehaviorTree : Graph
    {
        private BehaviorTree(string graphName, GraphAsset graphAsset) : base(graphName, graphAsset) { }


        internal TreeInterrupter interrupter
        {
            get;
            set;
        }

        public override GraphType graphType
        {
            get { return GraphType.BT; }
        }


#if UNITY_EDITOR
        public static BehaviorTree CreateGraph(string graphName, GraphAsset graphAsset)
        {
            BehaviorTree graph = new BehaviorTree(graphName, graphAsset);

            graph.entry = graph.CreateAndAddNodeToList("Root", typeof(RootNode), new Vector2Int(0, 0));

            return graph;
        }
#endif

        public override IGraphIterator GetIterator(GraphIteratorType iteratorType = GraphIteratorType.Default)
        {
            switch (iteratorType)
            {
                case GraphIteratorType.LS: return new Graph.CommonLSIterator(this);
                
                case GraphIteratorType.BFS: return new BehaviorTree.BFSIterator(this);
                
                case GraphIteratorType.Default: return new BehaviorTree.BFSIterator(this);
            }

            throw new ArgumentException("BreadthFirstSearch iterator is not implemented for BehaviorTree.");
        }


        internal override Status UpdateGraph()
        {
            if (entry is BehaviorNodeBase behaviourNode)
            {
                return behaviourNode.UpdateNode();
            }
            else
            {
                return Status.Failure;
            }
        }


        internal override void ResetGraph()
        {
            if (interrupter is not null)
            {
                interrupter.ClearCallStack();
            }
            else
            {
                Debug.LogError("TreeInterrupter is null. Unable to reset the behavior tree.");
            }
        }


        internal override void StopGraph()
        {
            if (interrupter is not null)
            {
                interrupter.AbortSubtree(((BehaviorNodeBase)entry).callStackID);
            }
            else
            {
                Debug.LogError("TreeInterrupter is null. Unable to reset the behavior tree.");
            }
        }


        internal override void InitializeOnEnterRuntime(TaskStreamer streamer)
        {
            int callStackSize = 0;

            foreach (BehaviorNodeBase node in this.GetIterator(GraphIteratorType.BFS))
            {
                callStackSize = Mathf.Max(callStackSize, node.callStackID);
                node.streamer = streamer;
                node.tree = this;
            }

            this.interrupter = new TreeInterrupter(this, callStackSize);
        }



#if UNITY_EDITOR
        internal override void OnRemoveGraph()
        {
            if (this._nodeLookup is null || this._nodeLookup.Values.Count == 0)
            {
                return;
            }

            //Foreach를 사용하는 도중에 컬렉션을 수정할 수 없으므로 ToList()를 사용하여 컬렉션을 복사한 후 원본 컬렉션을 수정.
            foreach (NodeBase node in this._nodeLookup.Values.ToList())
            {
                this.DeleteAndRemoveNodeFromList(node, false);
            }
        }

        
        public void ConnectNodes(BehaviorNodeBase parent, BehaviorNodeBase child)
        {
            if (Application.isPlaying == false)
            {
                Undo.RecordObject(this.graphAsset, "Behavior Tree (Connect Parent And Child)");
            }
            
            switch (parent.nodeType)
            {
                case BehaviorNodeType.Root:
                {
                    ((RootNode)parent).child = child;
                    child.parent = parent;
                    break;
                }

                case BehaviorNodeType.Decorator:
                {
                    ((DecoratorNode)parent).child = child;
                    child.parent = parent;
                    break;
                }

                case BehaviorNodeType.Composite:
                {
                    ((CompositeNode)parent).children.Add(child);
                    child.parent = parent;
                    break;
                }
            }

            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty(this.graphAsset);
            }
        }


        public void DisconnectNodes(BehaviorNodeBase parent, BehaviorNodeBase child)
        {
            if (Application.isPlaying == false)
            {
                Undo.RecordObject(this.graphAsset, "Behavior Tree (Disconnect Parent And Child)");
            }

            switch (parent.nodeType)
            {
                case BehaviorNodeType.Root:
                {
                    ((RootNode)parent).child = null;
                    child.parent = null;
                    break;
                }

                case BehaviorNodeType.Decorator:
                {
                    ((DecoratorNode)parent).child = null;
                    child.parent = null;
                    break;
                }

                case BehaviorNodeType.Composite:
                {
                    ((CompositeNode)parent).children.Remove(child);
                    child.parent = null;
                    break;
                }
            }
            
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty(this.graphAsset);
            }
        }
#endif
    }
}