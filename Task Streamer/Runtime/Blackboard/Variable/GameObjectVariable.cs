using System;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable, Readable]
    public class GameObjectVariable : BlackboardVariable<GameObject> { }
}