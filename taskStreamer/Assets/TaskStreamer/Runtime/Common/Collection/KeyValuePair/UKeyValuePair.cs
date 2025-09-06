using System;
using UnityEngine;

namespace TaskStreamer.Utility
{
    /// <summary> Unity Serializable Key Value Pair </summary>
    [Serializable]
    internal struct UKeyValuePair<TValue> : IKeyValuePair<UGUID, TValue>
    {
        [SerializeField]
        private UGUID _key;

        [SerializeField]
        private TValue _value;
        
        public UGUID key
        {
            get => _key;
            set => _key = value;
        }
        
        public TValue value
        {
            get => _value;
            set => _value = value;
        }
    }
}