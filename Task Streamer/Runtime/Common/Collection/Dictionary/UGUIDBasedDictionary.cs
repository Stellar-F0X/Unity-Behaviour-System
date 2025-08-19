using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Utility
{
    [Serializable, Readable]
    internal class UGUIDBasedDictionary<TValue> : Dictionary<UGUID, TValue>, ISerializationCallbackReceiver
    {
        [Serializable]
        public struct KeyValuePair
        {
            [SerializeField]
            public UGUID key;

            [SerializeReference]
            public TValue value;
        }

        [DontCreateProperty]
        public List<KeyValuePair> cachedPair = new List<KeyValuePair>();


        public void OnBeforeSerialize()
        {
            this.cachedPair.Clear();

            foreach (KeyValuePair<UGUID, TValue> pair in this)
            {
                this.cachedPair.Add(new KeyValuePair { key = pair.Key, value = pair.Value });
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