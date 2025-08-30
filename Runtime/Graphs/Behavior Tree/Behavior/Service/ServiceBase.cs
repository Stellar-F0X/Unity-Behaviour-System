using System;

namespace TaskStreamer.BT
{
    [Serializable]
    public abstract class ServiceBase
    {
        public bool enable = true;
        
        #if UNITY_EDITOR
        internal bool isExpanded = true;
        #endif
        
        public virtual void OnStart() { }

        
        public virtual void OnUpdate() { }

        
        public virtual void OnStop() { }
    }
}