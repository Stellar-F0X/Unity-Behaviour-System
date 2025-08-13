using System;
using System.Collections.Generic;

namespace TaskStreamer.Utility
{
    [Serializable]
    public class UGUIDList : List<UGUID> { }
    
    [Serializable]
    public class UGUIDDictionary : URDictionary<UGUID, UGUIDList> { }
    
}