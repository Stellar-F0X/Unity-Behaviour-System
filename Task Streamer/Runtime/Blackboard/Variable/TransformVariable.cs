using System;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable, Readable]
    public class TransformVariable : BlackboardVariable<Transform> { }
}