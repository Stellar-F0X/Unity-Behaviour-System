using System;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.BT;
using Unity.Properties;

[Serializable, GeneratePropertyBag, Readable]
public partial class TestNode : ActionNode
{
    protected override void OnEnter() 
    {
    
    }

    protected override TaskStreamer.Runtime.Status OnUpdate()
    {
        return TaskStreamer.Runtime.Status.Failure;
    }

    protected override void OnExit()
    {
    
    }
}