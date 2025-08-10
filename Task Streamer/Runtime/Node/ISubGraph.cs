using TaskStreamer.Utility;

namespace TaskStreamer
{
    public interface ISubGraph
    {
        public UGUID subGraphGuid
        {
            get;
            set;
        }

        public GraphType subGraphType
        {
            get;
        }
    }
}