using System;
using System.Collections.Generic;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.FSM
{
    [Serializable]
    public abstract class StateBase : NodeBase
    {
        private bool _blockTransition;
        private StateMachine _machine;

        [SerializeReference, HideInInspector]
        private List<Transition> _transitions = new List<Transition>();


        public StateMachine machine
        {
            get { return _machine; }

            internal set { _machine = value; }
        }


        internal IReadOnlyList<Transition> transitions
        {
            get { return _transitions; }
        }


        public float enteredTime
        {
            get;
            private set;
        }


        public float elapsedTime
        {
            get { return Time.time - enteredTime; }
        }


        public bool blockTransition
        {
            get { return _blockTransition; }

            set { _blockTransition = value; }
        }


        public abstract StateNodeType nodeType
        {
            get;
        }



        internal void UpdateNode()
        {
            this.callCount++;
            this.OnUpdate();

            if (this.CanTransition(out NodeBase nextState))
            {
                Assert.IsNotNull(this._machine);
                this._machine.ChangeState(nextState);
            }
        }



        internal bool TryGetTransition(UGUID nextStateNodeGuid, out Transition resultTransition)
        {
            foreach (Transition transition in this._transitions)
            {
                if (transition.toNodeGuid == nextStateNodeGuid)
                {
                    resultTransition = transition;
                    return true;
                }
            }

            resultTransition = null;
            return false;
        }



        internal override sealed void EnterNode()
        {
            this.enteredTime = Time.time;
            this.OnEnter();
            this.onNodeEnter?.Invoke(this);
            this.callState = NodeCallState.Updating;
        }



        internal override sealed void ExitNode()
        {
            this.OnExit();
            this.onNodeExit?.Invoke(this);
            this.callState = NodeCallState.BeforeEnter;
            this.enteredTime = 0;
        }



        protected virtual bool CanTransition(out NodeBase nextState)
        {
            if (this._blockTransition || this._transitions.Count == 0)
            {
                nextState = null;
                return false;
            }

            foreach (Transition transition in this._transitions)
            {
                if (transition.CheckConditions())
                {
                    nextState = transition.destinationNode;
                    return true;
                }
            }

            nextState = null;
            return false;
        }



        protected abstract void OnUpdate();



#if UNITY_EDITOR
        internal void AddTransition(in Transition transition)
        {
            if (transition == null)
            {
                Debug.LogError("Cannot add a null transition.");
                return;
            }

            if (this._transitions.Contains(transition))
            {
                Debug.LogWarning("Transition already exists in the state node.");
                return;
            }

            this._transitions.Add(transition);
        }


        internal void RemoveTransition(in Transition transition)
        {
            if (transition == null)
            {
                Debug.LogError("Cannot remove a null transition.");
                return;
            }

            if (this._transitions.Remove(transition) == false)
            {
                Debug.LogError("Transition not found in the state node.");
            }
        }
#endif
    }
}