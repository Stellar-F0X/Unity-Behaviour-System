using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Utility
{
    [Serializable]
    internal class UGUIDBasedDictionary<TPair, TValue> : Dictionary<UGUID, TValue>, ISerializationCallbackReceiver where TPair : IKeyValuePair<UGUID, TValue>, new()
    {
        [SerializeField, DontCreateProperty]
        public List<TPair> cachedPair = new List<TPair>();


        public void OnBeforeSerialize()
        {
            this.cachedPair.Clear();

            foreach (KeyValuePair<UGUID, TValue> pair in this)
            {
                this.cachedPair.Add(new TPair { key = pair.Key, value = pair.Value });
            }
        }


        public void OnAfterDeserialize()
        {
            this.Clear();

            for (int i = 0; i < this.cachedPair.Count; i++)
            {
                this[cachedPair[i].key] = this.cachedPair[i].value;
            }
        }
    }
}