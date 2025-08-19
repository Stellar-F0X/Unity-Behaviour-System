using System;
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

#if UNITY_EDITOR
        [SerializeField, DontCreateProperty]
        public bool canEditName = true;
#endif
    }
}