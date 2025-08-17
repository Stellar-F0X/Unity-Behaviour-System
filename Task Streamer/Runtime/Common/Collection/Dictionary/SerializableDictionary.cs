using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Utility
{
    [Serializable, Readable]
    internal abstract class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        public abstract List<TKey> cachedKeys { get; }

        public abstract List<TValue> cachedValues { get; }


        public void OnBeforeSerialize()
        {
            cachedKeys.Clear();
            cachedValues.Clear();

            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                if (pair.Value.Equals(null))
                {
                    continue;
                }

                cachedKeys.Add(pair.Key);
                cachedValues.Add(pair.Value);
            }
        }


        public void OnAfterDeserialize()
        {
            this.Clear();
            
            for (int i = 0; i < cachedKeys.Count; i++)
            {
                if (cachedValues[i].Equals(null))
                {
                    cachedKeys.RemoveAt(i);
                    cachedValues.RemoveAt(i);
                    --i;
                }
                else
                {
                    this[cachedKeys[i]] = cachedValues[i];
                }
            }
        }
    }


    /// <summary> Unity Referenced Dictionary </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    internal class URDictionary<TKey, TValue> : SerializableDictionary<TKey, TValue> where TKey : struct where TValue : class
    {
        [SerializeField, DontCreateProperty]
        private List<TKey> _keys = new List<TKey>();

        [SerializeReference, DontCreateProperty]
        private List<TValue> _values = new List<TValue>();

        public override List<TKey> cachedKeys => _keys;

        public override List<TValue> cachedValues => _values;
    }


    /// <summary> Unity Dictionary </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    internal class UDictionary<TKey, TValue> : SerializableDictionary<TKey, TValue>
    {
        [SerializeField, DontCreateProperty]
        private List<TKey> _keys = new List<TKey>();

        [SerializeField, DontCreateProperty]
        private List<TValue> _values = new List<TValue>();

        public override List<TKey> cachedKeys => _keys;

        public override List<TValue> cachedValues => _values;
    }
}