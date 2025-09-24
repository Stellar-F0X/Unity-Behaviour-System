using System;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable, Readable]
    internal class LayerMaskVariable : BlackboardVariable<LayerMask> { }
}