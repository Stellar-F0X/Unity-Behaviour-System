using System;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable, Readable]
    public class RigidBodyVariable : BlackboardVariable<Rigidbody> { }
    
    
    [Serializable, Readable]
    public class RigidBody2DVariable : BlackboardVariable<Rigidbody2D> { }
}