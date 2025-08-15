using Unity.Properties;

namespace TaskStreamer.Injection
{
    public class GraphWorker : PropertyVisitor
    {
        public GraphWorker(Blackboard blackboard, GraphAsset graphAsset, TaskStreamer taskStreamer)
        {
            this._blackboard = blackboard;
            this._graphAsset = graphAsset;
            this._taskStreamer = taskStreamer;
        }

        private readonly TaskStreamer _taskStreamer;
        private readonly Blackboard _blackboard;
        private readonly GraphAsset _graphAsset;

        public TaskStreamer taskStreamer
        {
            get { return _taskStreamer; }
        }

        public Blackboard blackboard
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


        protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
        {
            base.VisitProperty(property, ref container, ref value);
        }
    }
}