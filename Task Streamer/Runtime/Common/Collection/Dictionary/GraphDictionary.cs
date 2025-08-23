using System;

namespace TaskStreamer.Utility
{
    [Serializable, Readable]
    internal class GraphDictionary : UGUIDBasedDictionary<URKeyValuePair<Graph>, Graph> { }
}