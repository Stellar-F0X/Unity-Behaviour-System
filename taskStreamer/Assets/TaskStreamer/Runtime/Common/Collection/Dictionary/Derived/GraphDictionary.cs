using System;

namespace TaskStreamer.Runtime.Utility
{
    [Serializable]
    internal class GraphDictionary : UGUIDBasedDictionary<URKeyValuePair<Graph>, Graph>, ISerializableCollection { }
}