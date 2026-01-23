using System;
using UnityEngine;

namespace TaskStreamer.Runtime
{
    [Serializable, Readable]
    public class AnimatorVariable : BlackboardVariable<Animator> { }
}