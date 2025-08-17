using System;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable]
    public abstract class Variable : ISerializationCallbackReceiver
    {
        protected Variable()
        {
            _guid = UGUID.Create();
            _keyHash = -1;
        }
        
        [SerializeField]
        private string _key;

        [SerializeField]
        private int _keyHash;
        
        [SerializeField]
        private UGUID _guid;

        [SerializeField]
        private string _typeName;


        public Type type
        {
            get;
            set;
        }

        public UGUID guid
        {
            get { return _guid; }
        }

        public string key
        {
            get { return this._key; }
            
            set { _keyHash = Utilities.StringToHash(_key = value); }
        }

        public int keyHash
        {
            get { return this._keyHash; }
        }


        public Variable Clone()
        {
            Variable clone = Activator.CreateInstance(type) as Variable;
            clone._typeName = this._typeName;
            clone.type = this.type;
            clone._keyHash = this._keyHash;
            clone._key = this._key;
            clone._guid = this._guid;
            return clone;
        }


        public void OnBeforeSerialize()
        {
            Debug.Assert(type is not null, "Failed to serialize a property.");
            this._typeName = type.AssemblyQualifiedName;
        }


        public void OnAfterDeserialize()
        {
            Debug.Assert(_typeName.IsNotNullOrEmpty(), "Failed to deserialize a property.");
            this.type = Type.GetType(_typeName);
        }
    }


    [Serializable]
    public abstract class Variable<T> : Variable
    {
        [SerializeField]
        protected T _value;


        public virtual T value
        {
            get { return _value; }
            set { _value = value; }
        }
    }
}