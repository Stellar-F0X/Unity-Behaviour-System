using UnityEditor.Experimental.GraphView;

namespace TaskStreamer.Utility
{
#if UNITY_EDITOR
    public class EdgeDictionary : URDictionary<UGUID, Edge> { }
#endif
}