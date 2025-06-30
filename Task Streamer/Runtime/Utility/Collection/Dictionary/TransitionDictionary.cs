using System;
using TaskStreamer.FSM;

namespace TaskStreamer.Utility
{
    [Serializable]
    public class TransitionDictionary : UDictionary<UGUID, Transition> { }
}