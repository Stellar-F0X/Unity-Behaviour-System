using System;
using Unity.Properties;
using TaskStreamer.Runtime;

    
[Serializable, GeneratePropertyBag, TaskDescription]
public class TestCondition : Condition
{
    public override bool Execute(NodeBase calledNode)
    {
        return false;
    }
}
