using System;
using UnityEngine;

namespace TaskStreamer.Runtime
{
    [Serializable, Readable]
    internal class LayerMaskVariable : BlackboardVariable<LayerMask> { }
}