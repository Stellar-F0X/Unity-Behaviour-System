using System;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.FSM
{
    [Serializable, GeneratePropertyBag, Readable]
    public sealed class Transition : Task
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
        private BlackboardVariable<bool> _blockTransition;

        [SerializeField]
        private BlackboardBasedCondition _conditions;

        [SerializeReference, DontCreateProperty]
        private NodeBase _sourceNode;

        [SerializeReference, DontCreateProperty]
        private NodeBase _destinationNode;


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


        public bool CheckConditions()
        {
            Assert.IsNotNull(this._blockTransition, "Transition blockTransition is not set.");
            
            if (this._blockTransition.value)
            {
                return false;
            }
            else
            {
                return this.conditions.Execute();
            }
        }
    }
}