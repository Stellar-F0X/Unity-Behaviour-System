using System;
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

        public override EGraphType graphType
        {
            get { return EGraphType.BT; }
        }


#if UNITY_EDITOR
        public static BehaviorTree CreateGraph(string graphName, GraphAsset graphAsset)
        {
            BehaviorTree graph = new BehaviorTree(graphName, graphAsset);

            if (graph.entry == null)
            {
                graph.entry = graph.CreateNode("Root", typeof(RootNode), new Vector2Int(0, 0));
            }

            return graph;
        }
#endif
        
        public override IGraphIterator GetGraphIterator()
        {
            return new BehaviorTree.Iterator(this);
        }


        public override EStatus UpdateGraph()
        {
            if (entry is BehaviorNodeBase behaviourNode)
            {
                return behaviourNode.UpdateNode();
            }
            else
            {
                return EStatus.Failure;
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

            foreach (BehaviorNodeBase node in this.GetGraphIterator())
            {
                callStackSize = Mathf.Max(callStackSize, node.callStackID);
                node.streamer = streamer;
                node.tree = this;
            }

            this.interrupter = new TreeInterrupter(this, callStackSize);
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
                case EBehaviorNodeType.Root:
                {
                    ((RootNode)parent).child = child;
                    child.parent = parent;
                    break;
                }

                case EBehaviorNodeType.Decorator:
                {
                    ((DecoratorNode)parent).child = child;
                    child.parent = parent;
                    break;
                }

                case EBehaviorNodeType.Composite:
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
                case EBehaviorNodeType.Root:
                {
                    ((RootNode)parent).child = null;
                    child.parent = null;
                    break;
                }

                case EBehaviorNodeType.Decorator:
                {
                    ((DecoratorNode)parent).child = null;
                    child.parent = null;
                    break;
                }

                case EBehaviorNodeType.Composite:
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