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

        public EGraphType subGraphType
        {
            get;
        }
    }
}