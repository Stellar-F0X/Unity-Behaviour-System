using System;
using TaskStreamer.Utility;

namespace TaskStreamer.BT
{
    [Serializable]
    public abstract class ServiceBase : Task, IEquatable<ServiceBase>
    {
        public ServiceBase()
        {
            this.guid = UGUID.Create();
            this.name = this.GetType().Name;
            this.canEditName = false;
        }


        public bool enable = true;

#if UNITY_EDITOR
        internal bool isExpanded = true;
#endif


        public virtual void OnStart() { }


        public virtual void OnUpdate() { }


        public virtual void OnStop() { }


        public bool Equals(ServiceBase other)
        {
            if (other is null)
            {
                return false;
            }

            if (this.guid != other.guid)
            {
                return false;
            }

            return ReferenceEquals(this, other);
        }
    }
}