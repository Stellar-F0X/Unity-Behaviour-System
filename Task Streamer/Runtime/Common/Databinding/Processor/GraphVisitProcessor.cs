namespace TaskStreamer.Injection
{
    public class GraphVisitProcessor : DefaultVisitProcessor
    {
        public GraphVisitProcessor(BlackboardAsset blackboard, GraphAsset graphAsset, TaskStreamer taskStreamer)
        {
            this._blackboard = blackboard;
            this._graphAsset = graphAsset;
            this._taskStreamer = taskStreamer;
        }
        
        private readonly TaskStreamer _taskStreamer;
        private readonly BlackboardAsset _blackboard;
        private readonly GraphAsset _graphAsset;


        public TaskStreamer taskStreamer
        {
            get { return _taskStreamer; }
        }

        public BlackboardAsset blackboard
        {
            get { return _blackboard; }
        }

        public GraphAsset graphAsset
        {
            get { return _graphAsset; }
        }

        public Graph currentGraph
        {
            get;
            set;
        }
    }
}