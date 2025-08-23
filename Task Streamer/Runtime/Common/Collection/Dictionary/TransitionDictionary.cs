using System;
using TaskStreamer.FSM;

namespace TaskStreamer.Utility
{
    [Serializable, Readable]
    internal class TransitionDictionary : UGUIDBasedDictionary<URKeyValuePair<Transition>, Transition> { }
}