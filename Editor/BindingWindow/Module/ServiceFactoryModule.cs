using System;
using TaskStreamer.BT;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class ServiceFactoryModule : FactoryModule<ServiceBase>
    {
        public ServiceFactoryModule(string title, bool useCallback = false, int layer = 1) : base(typeof(ServiceBase), title, useCallback, layer) { }
        
        
        protected override ServiceBase Create(Type type, Vector2 position, string entryName)
        {
            ServiceBase service = ObjectFactory.CreateService(type);
            Debug.Assert(service is not null, "service is null");
            return service;
        }
    }
}