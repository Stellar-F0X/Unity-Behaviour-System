using System;

namespace TaskStreamer.Utility
{
    [Serializable, Readable]
    internal class NodeDictionary : UGUIDBasedDictionary<URKeyValuePair<NodeBase>, NodeBase> { }
}