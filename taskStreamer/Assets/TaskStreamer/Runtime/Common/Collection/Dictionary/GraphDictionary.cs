using System;

namespace TaskStreamer.Runtime.Utility
{
    [Serializable, Readable]
    internal class GraphDictionary : UGUIDBasedDictionary<URKeyValuePair<Graph>, Graph> { }
}