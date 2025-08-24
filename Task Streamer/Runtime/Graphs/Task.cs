using System;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable, Readable]
    public abstract class Task
    {
        [DontCreateProperty]
        public string name;
        
        [DontCreateProperty]
        public string tag;
        
        [DontCreateProperty]
        public string description;

        [SerializeField, DontCreateProperty]
        protected UGUID _guid;

#if UNITY_EDITOR
        [SerializeField, DontCreateProperty]
        public bool canEditName = true;
#endif
        
        public UGUID guid
        {
            get { return _guid; }

            internal set { _guid = value; }
        }
    }
}