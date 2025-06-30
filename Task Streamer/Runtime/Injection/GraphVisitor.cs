using Unity.Properties;

namespace TaskStreamer.Injection
{
    public class GraphVisitor : PropertyVisitor
    {
        public GraphVisitor(Blackboard blackboard, GraphAsset graphAsset, TaskStreamer taskStreamer)
        {
            this.blackboard = blackboard;
            this.graphAsset = graphAsset;
            this.taskStreamer = taskStreamer;
        }
        
        public readonly TaskStreamer taskStreamer;
        public readonly Blackboard blackboard;
        public readonly GraphAsset graphAsset;
        
        public Graph currentGraph;
        public bool debug;
    }
}