using System;
using System.Collections.Generic;

namespace TaskStreamer.Utility
{
    [Serializable]
    internal class UGUIDDictionary : UDictionary<UGUID, List<UGUID>> { }
}