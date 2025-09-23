using System;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable, Readable]
    public class ColliderVariable : BlackboardVariable<Collider> { }
}