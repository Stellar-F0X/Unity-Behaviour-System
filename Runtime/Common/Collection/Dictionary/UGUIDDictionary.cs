using System;
using System.Collections.Generic;

namespace TaskStreamer.Utility
{
    [Serializable, Readable]
    internal class UGUIDDictionary : UGUIDBasedDictionary<UKeyValuePair<List<UGUID>>, List<UGUID>> { }
}