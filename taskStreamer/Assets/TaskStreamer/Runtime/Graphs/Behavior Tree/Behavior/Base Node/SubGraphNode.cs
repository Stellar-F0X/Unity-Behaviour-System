using System;
using TaskStreamer.Runtime.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Runtime.BT
{
    [Serializable]
    internal abstract class SubGraphNode : BehaviorNodeBase, ISubGraphProvider
    {
        [DontCreateProperty]
        private Graph _subGraph;
        
        [SerializeField, DontCreateProperty]
        private UGUID _subGraphGuid;

        
        public override BehaviorNodeType nodeType
        {
            get { return BehaviorNodeType.SubGraph; }
        }

        public UGUID subGraphGuid
        {
            get { return _subGraphGuid;}
            set { _subGraphGuid = value; }
        }

        public abstract GraphType subGraphType
        {
            get;
        }
 

        public override void OnAwake()
        {
            _subGraph = streamer.graphAsset.GetGraph(subGraphGuid);

            Debug.Assert(_subGraph != null, "SubGraph not found");
        }

        
        protected override void OnEnter()
        {
            if (_subGraph is null)
            {
                Debug.LogError($"{name} {nameof(OnEnter)}: SubGraph is null");
            }
            else
            {
                _subGraph.ResetGraph();
            }
        }

        
        protected override Status OnUpdate()
        {
            if (_subGraph is not null)
            {
                return _subGraph.UpdateGraph();
            }

            Debug.LogError($"{name} {nameof(OnUpdate)}: SubGraph is null");
            return Status.Failure;
        }

        
        protected override void OnExit()
        {
            if (_subGraph is null)
            {
                Debug.LogError($"{name} {nameof(OnExit)}: SubGraph is null");
            }
            else
            {
                _subGraph.StopGraph();
            }
        }
    }
}