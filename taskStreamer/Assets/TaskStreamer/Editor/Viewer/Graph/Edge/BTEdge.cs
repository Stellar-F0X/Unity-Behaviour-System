using UnityEditor.Experimental.GraphView;

namespace TaskStreamer.Tool
{
    public class BTEdge : Edge
    {
        public BTEdge() => this.styleSheets.Add(TSEditor.edgeStyle); //USS 추가
    }
}