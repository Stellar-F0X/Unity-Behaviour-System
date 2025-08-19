using System;
using UnityEditor.Experimental.GraphView;

namespace TaskStreamer.Utility
{
#if UNITY_EDITOR
    internal class EdgeDictionary : UGUIDBasedDictionary<Edge> { }
#endif
}