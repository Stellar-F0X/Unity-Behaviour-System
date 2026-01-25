using System;
using TaskStreamer.Runtime.BT;
using TaskStreamer.Runtime.Utility;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    internal class ServiceView : VisualElement
    {
        public ServiceView(ServiceBase service)
        {
            TSEditor.serviceBlock.CloneTree(this);
            Type serviceType = service.GetType();
            base.userData = service;
            
            Label typeLabel = this.Q<Label>("type-label");
            typeLabel.text = StringUtility.ToNicifyName(serviceType.Name); 
        }
    }
}