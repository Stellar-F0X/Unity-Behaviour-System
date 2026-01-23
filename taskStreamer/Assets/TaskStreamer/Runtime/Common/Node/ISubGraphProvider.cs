using TaskStreamer.Runtime.Utility;

namespace TaskStreamer.Runtime
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