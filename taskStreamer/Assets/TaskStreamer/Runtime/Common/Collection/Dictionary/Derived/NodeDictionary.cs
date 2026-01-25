using System;

namespace TaskStreamer.Runtime.Utility
{
    [Serializable]
    internal class NodeDictionary : UGUIDBasedDictionary<URKeyValuePair<NodeBase>, NodeBase>, ISerializableCollection { }
}