using System;
using UnityEngine;

namespace TaskStreamer.Utility
{
    /// <summary> Unity Serializable Referenced Key Value Pair </summary>
    [Serializable]
    internal struct URKeyValuePair<TValue> : IKeyValuePair<UGUID, TValue>
    {
        [SerializeField]
        private UGUID _key;

        [SerializeReference]
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