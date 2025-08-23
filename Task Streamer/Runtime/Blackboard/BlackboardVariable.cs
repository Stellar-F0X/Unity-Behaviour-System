using System;
using TaskStreamer.Utility;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer
{
    /// <summary> Variable wrapper class </summary>
    [Serializable, Readable]
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

        //TODO: 콘크리트 Variable 클래스가 아니라 BlackboardVariable<T> 타입이나, T를 갖고 있어야되는지 생각.
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
    public class BlackboardVariable<T> : BlackboardVariable
    {
        public T value
        {
            get
            {
                if (_variable is Variable<T> convertedVariable)
                {
                    return convertedVariable.value;
                }
                else
                {
                    Debug.LogError($"_variable type mismatch: {typeof(T).Name}.");
                    return default;
                }
            }

            set
            {
                if (_variable is Variable<T> convertedVariable)
                {
                    convertedVariable.value = value;
                }
                else
                {
                    Debug.LogError($"_variable type mismatch: {typeof(T).Name}.");
                }
            }
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