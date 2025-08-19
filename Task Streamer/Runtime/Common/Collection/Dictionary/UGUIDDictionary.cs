using System;
using System.Collections.Generic;

namespace TaskStreamer.Utility
{
    [Serializable]
    public class UGUIDList : List<UGUID> { }

    [Serializable]
    internal class UGUIDDictionary : UGUIDBasedDictionary<UGUIDList> { }
}