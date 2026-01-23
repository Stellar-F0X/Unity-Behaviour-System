using System;
using System.Collections.Generic;

namespace TaskStreamer.Runtime.Utility
{
    [Serializable, Readable]
    internal class UGUIDDictionary : UGUIDBasedDictionary<UKeyValuePair<List<UGUID>>, List<UGUID>> { }
}