using System;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable, Readable]
    public class QuaternionVariable : BlackboardVariable<Quaternion> { }
}