using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.BT
{
    public abstract class BehaviorNodeBase : NodeBase
    {
        private BehaviorTree _tree;

        [SerializeReference, DontCreateProperty, HideInInspector]
        private BehaviorNodeBase _parent;

        [SerializeReference, DontCreateProperty, HideInInspector]
        protected List<BehaviorNodeBase> _children = new List<BehaviorNodeBase>();


        public abstract BehaviorNodeType nodeType
        {
            get;
        }

        public Status status
        {
            get;
            private set;
        }

        public int callStackID
        {
            get;
            internal set;
        }

        public int depth
        {
            get;
            internal set;
        }

        public BehaviorNodeBase parent
        {
            get { return _parent; }

            internal set { _parent = value; }
        }

        public BehaviorTree tree
        {
            get { return _tree; }

            internal set { _tree = value; }
        }


        internal Status UpdateNode()
        {
            this.callCount++;

            if (callState == NodeCallState.BeforeEnter)
            {
                this.EnterNode();
            }

            if (this.callState == NodeCallState.Updating)
            {
                this.status = this.OnUpdate();

                if (this.status == Status.Running)
                {
                    return Status.Running;
                }

                if (this.tree.interrupter.GetCurrentNode(callStackID) != this)
                {
                    this.tree.interrupter.AbortSubtreeFrom(callStackID, this);
                }

                this.callState = NodeCallState.BeforeExit;
            }

            if (this.callState == NodeCallState.BeforeExit)
            {
                this.ExitNode();
            }

            return this.status;
        }


        internal override sealed void EnterNode()
        {
            this.tree.interrupter.PushInCallStack(callStackID, this);
            this.onNodeEnter?.Invoke();
            this.OnEnter();
            this.callState = NodeCallState.Updating;
        }


        internal override sealed void ExitNode()
        {
            this.tree.interrupter.PopInCallStack(callStackID);
            this.OnExit();
            this.onNodeExit?.Invoke();
            this.callState = NodeCallState.BeforeEnter;

            // If a parent node fails during execution, this node's result is set to Failure.
            this.status = (this.status == Status.Running ? Status.Failure : this.status);
        }


        /// Core behavior update function that must be implemented by derived classes.
        /// Returns the execution result of the node's behavior.
        protected abstract Status OnUpdate();



        internal void AddChild(BehaviorNodeBase child)
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                UnityEditor.Undo.RecordObject(this, "Behavior Tree (AddChild)");
            }
#endif
            switch (this.nodeType)
            {
                case BehaviorNodeType.Root: ((RootNode)this).child = child; break;

                case BehaviorNodeType.Decorator: ((DecoratorNode)this).child = child; break;

                case BehaviorNodeType.Composite: ((CompositeNode)this).children.Add(child); break;
            }

#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.EditorUtility.SetDirty(child);
            }
#endif
        }


        internal void ChangeChild(BehaviorNodeBase originalChild, BehaviorNodeBase newChild)
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                UnityEditor.Undo.RecordObject(this, "Behavior Tree (ChangeChild)");
            }
#endif
            
            switch (this.nodeType)
            {
                case BehaviorNodeType.Root:
                {
                    RootNode root = (RootNode)this;
                    Debug.Assert(root != null, $"{newChild.name} cannot be converted");
                    root.child = newChild;
                    break;
                }

                case BehaviorNodeType.Decorator:
                {
                    DecoratorNode deco = (DecoratorNode)this;
                    Debug.Assert(deco != null, $"{newChild.name} cannot be converted");
                    deco.child = newChild;
                    break;
                }

                case BehaviorNodeType.Composite:
                {
                    CompositeNode compo = (CompositeNode)this;
                    int index = compo.children.IndexOf(originalChild);
                    Debug.Assert(index != -1, $"{originalChild.name} not found");
                    compo.children[index] = newChild; //replace in runtime
                    break;
                }
            }
            
            newChild.parent = this;
            
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.EditorUtility.SetDirty(newChild);
            }
#endif
        }


        internal void RemoveChild(BehaviorNodeBase child)
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                UnityEditor.Undo.RecordObject(this, "Behavior Tree (RemoveChild)");
            }
#endif

            switch (this.nodeType)
            {
                case BehaviorNodeType.Root: ((RootNode)this).child = null; break;

                case BehaviorNodeType.Decorator: ((DecoratorNode)this).child = null; break;

                case BehaviorNodeType.Composite: ((CompositeNode)this).children.Remove(child); break;
            }

#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.EditorUtility.SetDirty(child);
            }
#endif
        }
    }
}