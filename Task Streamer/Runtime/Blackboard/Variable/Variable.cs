using System;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable]
    public abstract class Variable : ISerializationCallbackReceiver
    {
        [SerializeField]
        protected string _name;

        [SerializeField]
        protected int _nameHash;

        [SerializeField]
        protected string _typeName;

        protected Type _type;


        public Type type
        {
            get { return this._type; }

            set { this._type = value; }
        }

        public string name
        {
            get { return this._name; }

            set { this.TryChangeName(value); }
        }

        public int nameHash
        {
            get { return this._nameHash; }
        }


        private void TryChangeName(string newKey)
        {
            if (string.IsNullOrEmpty(newKey))
            {
                Debug.LogError($"{_name} key value is empty. Please enter a valid key.");
                return;
            }

            this._name = newKey;
            this._nameHash = Utilities.StringToHash(this._name);
        }


        public void OnBeforeSerialize()
        {
            Debug.Assert(_type is not null, "Failed to serialize a property.");
            this._typeName = _type.AssemblyQualifiedName;
        }


        public void OnAfterDeserialize()
        {
            Debug.Assert(string.IsNullOrEmpty(_typeName) == false, "Failed to deserialize a property.");
            this._type = Type.GetType(_typeName);
        }
    }


    [Serializable]
    public abstract class Variable<T> : Variable
    {
        public Variable() { }
        
        
        [SerializeField]
        protected T _value;


        public virtual T value
        {
            get { return _value; }

            set { _value = value; }
        }
    }
}