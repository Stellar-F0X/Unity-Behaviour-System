using System;
using TaskStreamer.Utility;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer
{
    /// <summary> Variable wrapper class </summary>
    [Serializable, GeneratePropertyBag, Readable]
    public abstract class BlackboardVariable
    {
        [SerializeReference]
        protected Variable _variable;

        [SerializeField, DontCreateProperty]
        protected bool _isGlobal;


        public Variable variable
        {
            get { return _variable; }

            internal set { _variable = value; }
        }

        public Type type
        {
            get { return _variable?.type; }

            set { _variable.type = value; }
        }

        public string key
        {
            get { return _variable.key; }

            set { this._variable.key = value; }
        }

        public int keyHash
        {
            get { return _variable.keyHash; }
        }

        public UGUID guid
        {
            get { return _variable.guid; }
        }

        /// <summary> True if the variable is local to this node; false if retrieved from the shared blackboard. </summary>
        internal bool isGlobal
        {
            get { return _isGlobal; }

            set { _isGlobal = value; }
        }
        
        internal object boxedValue
        {
            get { return _variable.boxedValue; }

            set { _variable.boxedValue = value; }
        }
        

        public abstract BlackboardVariable Duplicate();
    }


    [Serializable]
    public partial class BlackboardVariable<T> : BlackboardVariable
    {
        public T value
        {
            get { return ((Variable<T>)_variable).value; }

            set { ((Variable<T>)_variable).value = value; }
        }


        public override BlackboardVariable Duplicate()
        {
            BlackboardVariable<T> clone = new BlackboardVariable<T>();

            clone._variable = this._variable;
            clone._isGlobal = this._isGlobal;
            clone.value = this.value;
            clone.key = this.key;
            clone.type = this.type;

            return clone;
        }
    }
}