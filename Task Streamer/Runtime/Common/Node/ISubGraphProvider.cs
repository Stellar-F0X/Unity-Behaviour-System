using TaskStreamer.Utility;

namespace TaskStreamer
{
    public interface ISubGraphProvider
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