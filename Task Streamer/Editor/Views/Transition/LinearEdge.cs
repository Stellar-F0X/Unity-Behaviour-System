using UnityEditor.Experimental.GraphView;

namespace TaskStreamer.Tool
{
    public class LinearEdge : Edge
    {
        public LinearEdge()
        {
            this.styleSheets.Add(TaskStreamerEditor.settings.EdgeStyle); //USS 추가
        }
    }
}