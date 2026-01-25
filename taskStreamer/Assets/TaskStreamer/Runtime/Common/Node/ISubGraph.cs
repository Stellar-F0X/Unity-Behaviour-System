using TaskStreamer.Runtime.Utility;

namespace TaskStreamer.Runtime
{
    public interface ISubGraph
    {
        public UGUID subGraphGuid { get; set; }

        public GraphType subGraphType { get; }
    }
}