using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.FSM
{
    public abstract class SubGraphState : StateBase, ISubGraph
    {
        [DontCreateProperty]
        private Graph _subGraph;
        
        [SerializeField, HideInInspector]
        private UGUID _subGraphGuid;

        
        public override EStateNodeType nodeType
        {
            get { return EStateNodeType.SubGraph; }
        }

        public UGUID subGraphGuid
        {
            get { return _subGraphGuid;}
            set { _subGraphGuid = value; }
        }

        public abstract EGraphType subGraphType
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

        protected override void OnUpdate()
        {
            if (_subGraph is null)
            {
                Debug.LogError($"{name} {nameof(OnUpdate)}: SubGraph is null");
            }
            else
            {
                _subGraph.UpdateGraph();
            }
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