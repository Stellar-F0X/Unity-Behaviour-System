using UnityEditor.Experimental.GraphView;

namespace TaskStreamer.Tool
{
    public class BTEdge : Edge
    {
        public BTEdge() => this.styleSheets.Add(TSUIElementSettings.instance.EdgeStyle); //USS 추가
    }
}