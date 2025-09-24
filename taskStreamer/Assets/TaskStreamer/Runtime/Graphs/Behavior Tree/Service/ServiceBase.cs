using System;
using TaskStreamer.Utility;
using UnityEngine;

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

        
        /// <summary>
        /// 현재 Service가 부착되어 있는 노드입니다.
        /// 런타임 진입시 초기화 과정에 할당됩니다.
        /// </summary>
        public BehaviorNodeBase node
        {
            get;
            internal set;
        }

        public Transform transform
        {
            get { return node.transform; }
        }

        public GameObject gameObject
        {
            get { return node.gameObject; }
        }

        public TaskStreamer streamer
        {
            get { return node.streamer; }
        }

        public float enteredTime
        {
            get { return node.enteredTime; }
        }
        
        public float elapsedTime
        {
            get { return node.elapsedTime; }
        }


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
        

        public virtual void OnStart() { }


        public virtual void OnUpdate() { }


        public virtual void OnStop() { }
    }
}