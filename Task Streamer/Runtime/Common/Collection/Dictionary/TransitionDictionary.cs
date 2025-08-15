using System;
using TaskStreamer.FSM;

namespace TaskStreamer.Utility
{
    [Serializable]
    internal class TransitionDictionary : UDictionary<UGUID, Transition> { }
}