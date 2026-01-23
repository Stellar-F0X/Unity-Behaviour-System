using System;
using UnityEngine;

namespace TaskStreamer.Runtime
{
    [Serializable, Readable]
    public class GameObjectVariable : BlackboardVariable<GameObject> { }
}