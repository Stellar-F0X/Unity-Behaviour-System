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


        public abstract EBehaviorNodeType nodeType
        {
            get;
        }

        public EStatus status
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


        public EStatus UpdateNode()
        {
            this.callCount++;

            if (callState == ENodeCallState.BeforeEnter)
            {
                this.EnterNode();
            }

            if (this.callState == ENodeCallState.Updating)
            {
                this.status = this.OnUpdate();

                if (this.status == EStatus.Running)
                {
                    return EStatus.Running;
                }

                if (this.tree.interrupter.GetCurrentNode(callStackID) != this)
                {
                    this.tree.interrupter.AbortSubtreeFrom(callStackID, this);
                }

                this.callState = ENodeCallState.BeforeExit;
            }

            if (this.callState == ENodeCallState.BeforeExit)
            {
                this.ExitNode();
            }

            return this.status;
        }


        public override sealed void EnterNode()
        {
            this.tree.interrupter.PushInCallStack(callStackID, this);
            this.onNodeEnter?.Invoke();
            this.OnEnter();
            this.callState = ENodeCallState.Updating;
        }


        public override sealed void ExitNode()
        {
            this.tree.interrupter.PopInCallStack(callStackID);
            this.OnExit();
            this.onNodeExit?.Invoke();
            this.callState = ENodeCallState.BeforeEnter;

            // If a parent node fails during execution, this node's result is set to Failure.
            this.status = (this.status == EStatus.Running ? EStatus.Failure : this.status);
        }


        /// Core behavior update function that must be implemented by derived classes.
        /// Returns the execution result of the node's behavior.
        protected abstract EStatus OnUpdate();



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
                case EBehaviorNodeType.Root: ((RootNode)this).child = child; break;

                case EBehaviorNodeType.Decorator: ((DecoratorNode)this).child = child; break;

                case EBehaviorNodeType.Composite: ((CompositeNode)this).children.Add(child); break;
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
                case EBehaviorNodeType.Root:
                {
                    RootNode root = (RootNode)this;
                    Debug.Assert(root != null, $"{newChild.name} cannot be converted");
                    root.child = newChild;
                    break;
                }

                case EBehaviorNodeType.Decorator:
                {
                    DecoratorNode deco = (DecoratorNode)this;
                    Debug.Assert(deco != null, $"{newChild.name} cannot be converted");
                    deco.child = newChild;
                    break;
                }

                case EBehaviorNodeType.Composite:
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
                case EBehaviorNodeType.Root: ((RootNode)this).child = null; break;

                case EBehaviorNodeType.Decorator: ((DecoratorNode)this).child = null; break;

                case EBehaviorNodeType.Composite: ((CompositeNode)this).children.Remove(child); break;
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