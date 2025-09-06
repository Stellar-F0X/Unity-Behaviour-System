using UnityEditor.Experimental.GraphView;

namespace TaskStreamer.Tool
{
    public class LinearEdge : Edge
    {
        public LinearEdge()
        {
            this.styleSheets.Add(TaskStreamerResourceLoader.edgeStyle); //USS 추가
        }
    }
}