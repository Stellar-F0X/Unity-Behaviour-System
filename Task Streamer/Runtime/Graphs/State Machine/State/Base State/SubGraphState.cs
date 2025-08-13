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


        public override StateNodeType nodeType
        {
            get { return StateNodeType.SubGraph; }
        }

        public UGUID subGraphGuid
        {
            get { return _subGraphGuid; }
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
                return;
            }

            _subGraph.ResetGraph();

            this.blockTransition = true;
        }

        protected override void OnUpdate()
        {
            if (_subGraph is null)
            {
                Debug.LogError($"{name} {nameof(OnUpdate)}: SubGraph is null");
                return;
            }

            if (_subGraph.UpdateGraph() != Status.Running)
            {
                this.blockTransition = false;
            }
        }

        protected override void OnExit()
        {
            if (_subGraph is null)
            {
                Debug.LogError($"{name} {nameof(OnExit)}: SubGraph is null");
                return;
            }

            _subGraph.StopGraph();
        }
    }
}