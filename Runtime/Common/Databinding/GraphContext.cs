namespace TaskStreamer
{
    public class GraphContext
    {
        public GraphContext(GraphAsset graphAsset, BlackboardAsset blackboard = null, TaskStreamer taskStreamer = null)
        {
            this.blackboard = blackboard;
            this.graphAsset = graphAsset;
            this.taskStreamer = taskStreamer;
        }


        public TaskStreamer taskStreamer
        {
            private set;
            get;
        }

        public BlackboardAsset blackboard
        {
            private set;
            get;
        }

        public GraphAsset graphAsset
        {
            private set;
            get;
        }

        public Graph currentGraph
        {
            get;
            set;
        }
    }
}