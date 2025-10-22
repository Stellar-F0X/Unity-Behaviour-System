using System;
using TaskStreamer;
using TaskStreamer.BT;
using Unity.Properties;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable, GeneratePropertyBag, Readable]
public partial class ViewLogger : ServiceBase
{
    public BlackboardVariable<string> messageA;
    
    
    public BlackboardVariable<string> messageB;
    
    
    public BlackboardVariable<string> messageC;
    
    
    
    public LogView logView;
    

    public override void OnStart()
    {
        logView ??= Object.FindAnyObjectByType<LogView>();
        logView.AddLog(messageA.value);
    }

    public override void OnUpdate()
    {
        logView.AddLog(messageB.value);
    }

    public override void OnStop()
    {
        logView.AddLog(messageC.value);
    }
}