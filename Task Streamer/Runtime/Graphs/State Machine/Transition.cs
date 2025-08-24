using System;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.FSM
{
    [Serializable, GeneratePropertyBag, Readable]
    public sealed class Transition : Task
    {
        internal Transition(NodeBase sourceNode, NodeBase destinationNode, bool coditional = false)
        {
            this._guid = UGUID.Create();
            this._conditional = coditional;
            this._sourceNode = sourceNode;
            this._destinationNode = destinationNode;
            this._conditions = new BlackboardBasedCondition();
            base.name = $"{sourceNode.name} To {destinationNode.name}";
            base.canEditName = false;
        }

        [SerializeField, DontCreateProperty]
        private bool _conditional;

        [SerializeField, CreateProperty]
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

        public bool conditional
        {
            get { return this._conditional; }

            internal set { this._conditional = value; }
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
            if (this.conditional)
            {
                return this.conditions.Execute();
            }
            else
            {
                return true;
            }
        }
    }
}