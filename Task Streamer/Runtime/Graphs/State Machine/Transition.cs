using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.FSM
{
    [Readable]
    public sealed class Transition : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField, DontCreateProperty]
        private string _description;
#endif
        [SerializeField, DontCreateProperty]
        private bool _conditional;

        [SerializeField, DontCreateProperty]
        private NodeBase _sourceNode;

        [SerializeField, DontCreateProperty]
        private NodeBase _destinationNode;

        [SerializeField, CreateProperty]
        private BlackboardBasedCondition _conditions;


        public string description
        {
            get { return this._description; }
        }

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



        internal void Setup(NodeBase sourceNode, NodeBase destinationNode, bool coditional = false)
        {
            this._conditional = coditional;
            this._sourceNode = sourceNode;
            this._destinationNode = destinationNode;
            this._conditions = new BlackboardBasedCondition();
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