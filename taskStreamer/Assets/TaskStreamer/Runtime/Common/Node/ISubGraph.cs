using TaskStreamer.Runtime.Utility;

namespace TaskStreamer.Runtime
{
    internal interface ISubGraph
    {
        public UGUID subGraphGuid { get; set; }

        public GraphType subGraphType { get; }
    }
}