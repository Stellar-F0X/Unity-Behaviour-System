using System;
using TaskStreamer.Runtime.Utility;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.Runtime.FSM
{
    [Serializable, GeneratePropertyBag, Readable]
    internal class Transition : Task
    {
        internal Transition(NodeBase sourceNode, NodeBase destinationNode) : base()
        {
            this._guid = UGUID.Create();
            this._sourceNode = sourceNode;
            this._destinationNode = destinationNode;
            this._conditions = new BlackboardBasedCondition();
            base.name = $"{sourceNode.name} To {destinationNode.name}";

#if UNITY_EDITOR
            base.canEditName = false;
#endif
        }
        
        [SerializeField]
        protected BlackboardVariable<bool> _blockTransition;

        [SerializeField]
        private BlackboardBasedCondition _conditions;

        [SerializeReference, DontCreateProperty]
        protected NodeBase _sourceNode;

        [SerializeReference, DontCreateProperty]
        protected NodeBase _destinationNode;


        public UGUID fromNodeGuid
        {
            get { return sourceNode.guid; }
        }

        public UGUID toNodeGuid
        {
            get { return destinationNode.guid; }
        }

        public BlackboardBasedCondition conditions
        {
            get { return this._conditions; }
        }

        public NodeBase sourceNode
        {
            get { return this._sourceNode; }

            internal set { this._sourceNode = value; }
        }

        public NodeBase destinationNode
        {
            get { return this._destinationNode; }

            internal set { this._destinationNode = value; }
        }


        public virtual bool CheckConditions()
        {
            Assert.IsNotNull(this._blockTransition, "Transition blockTransition is not set.");
            
            if (this._blockTransition.value)
            {
                return false;
            }
            
            if (conditions.modules.Count > 0)
            {
                return this.conditions.Execute(this.sourceNode);
            }
            else
            {
                return true;
            }
        }
    }
}