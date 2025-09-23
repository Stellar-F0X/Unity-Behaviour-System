using System;
using UnityEngine.AI;

namespace TaskStreamer
{
    [Serializable, Readable]
    public class NavMeshAgentVariable : BlackboardVariable<NavMeshAgent> { }
}