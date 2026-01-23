using System;
using UnityEngine.AI;

namespace TaskStreamer.Runtime
{
    [Serializable, Readable]
    public class NavMeshAgentVariable : BlackboardVariable<NavMeshAgent> { }
}