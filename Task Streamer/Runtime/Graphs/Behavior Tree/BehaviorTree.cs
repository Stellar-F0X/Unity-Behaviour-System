using System;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TaskStreamer.BT
{
    [Serializable]
    public partial class BehaviorTree : Graph
    {
        private BehaviorTree(string graphName, GraphAsset graphAsset) : base(graphName, graphAsset) { }


        public TreeInterrupter interrupter
        {
            get;
            internal set;
        }

        public override GraphType graphType
        {
            get { return GraphType.BT; }
        }


#if UNITY_EDITOR
        public static BehaviorTree CreateGraph(string graphName, GraphAsset graphAsset)
        {
            BehaviorTree graph = new BehaviorTree(graphName, graphAsset);

            graph.entry = graph.CreateNode("Root", typeof(RootNode), new Vector2Int(0, 0));

            return graph;
        }
#endif

        public override IGraphIterator GetIterator(GraphIteratorType iteratorType)
        {
            switch (iteratorType)
            {
                case GraphIteratorType.LS: return new BehaviorTree.LSIterator(this);

                case GraphIteratorType.BFS: return new BehaviorTree.BFSIterator(this);
            }

            throw new NotImplementedException("BreadthFirstSearch iterator is not implemented for BehaviorTree.");
        }


        public override Status UpdateGraph()
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


        public override void ResetGraph()
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


        public override void StopGraph()
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



        internal override void OnRemoveGraph()
        {
            if (this._nodeLookup is null || this._nodeLookup.Values.Count == 0)
            {
                return;
            }

            //Foreach를 사용하는 도중에 컬렉션을 수정할 수 없으므로 ToList()를 사용하여 컬렉션을 복사한 후 원본 컬렉션을 수정.
            foreach (NodeBase node in this._nodeLookup.Values.ToList())
            {
                this.DeleteNode(node);
            }
        }


        public static void ConnectNodes(BehaviorNodeBase parent, BehaviorNodeBase child)
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                Undo.RecordObject(parent, "Behavior Tree (Connect Parent And Child)");
            }
#endif
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

#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty(parent);
                EditorUtility.SetDirty(child);
            }
#endif
        }


        public static void DisconnectNodes(BehaviorNodeBase parent, BehaviorNodeBase child)
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                Undo.RecordObject(parent, "Behavior Tree (Disconnect Parent And Child)");
            }
#endif

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

#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty(parent);
                EditorUtility.SetDirty(child);
            }
#endif
        }
    }
}