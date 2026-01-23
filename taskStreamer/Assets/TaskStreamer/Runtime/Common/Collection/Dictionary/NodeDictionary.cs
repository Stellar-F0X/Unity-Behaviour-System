using System;

namespace TaskStreamer.Runtime.Utility
{
    [Serializable, Readable]
    internal class NodeDictionary : UGUIDBasedDictionary<URKeyValuePair<NodeBase>, NodeBase> { }
}