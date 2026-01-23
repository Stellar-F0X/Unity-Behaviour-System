using System;
using UnityEngine;

namespace TaskStreamer.Runtime
{
    [Serializable, Readable]
    public class ColliderVariable : BlackboardVariable<Collider> { }
}