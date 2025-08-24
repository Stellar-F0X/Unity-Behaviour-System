using System;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer
{
    /// <summary> Variable wrapper class </summary>
    [Serializable]
    public abstract class BlackboardVariable
    {
        /// <summary> Encapsulates a flexible variable that supports serialization and type management. </summary>
        [SerializeReference]
        protected Variable _variable;

        /// <summary> Indicates whether the variable is shared across nodes (global) or local to a specific node. </summary>
        [SerializeField, DontCreateProperty]
        protected bool _isGlobal;


        /// <summary> Gets or sets the encapsulated variable for the blackboard. </summary>
        internal Variable variable
        {
            get { return _variable; }

            set { _variable = value; }
        }
        
        /// <summary> The type of the underlying variable encapsulated by this BlackboardVariable. </summary>
        internal Type type
        {
            get { return _variable?.type; }

            set { _variable.type = value; }
        }

        /// <summary> Gets or sets the unique identifier for the variable associated with this blackboard entry. </summary>
        internal string key
        {
            get { return _variable.key; }

            set { this._variable.key = value; }
        }

        /// <summary> Gets the hash value of the key associated with the variable. </summary>
        internal int keyHash
        {
            get { return _variable.keyHash; }
        }

        /// <summary> Unique identifier associated with the variable. </summary>
        internal UGUID guid
        {
            get { return _variable.guid; }
        }

        /// <summary> Indicates whether the variable is shared globally (true) or local to a specific node (false). </summary>
        internal bool isGlobal
        {
            get { return _isGlobal; }

            set { _isGlobal = value; }
        }

        /// <summary> Gets or sets the value of the variable in a generic object format. </summary>
        internal object boxedValue
        {
            get { return _variable.boxedValue; }

            set { _variable.boxedValue = value; }
        }


        /// <summary>
        /// Creates a duplicate of the current BlackboardVariable instance.
        /// </summary>
        /// <returns>A new instance of BlackboardVariable that is a copy of the current instance.</returns>
        internal abstract BlackboardVariable Duplicate();
    }


    /// <summary> Abstract base class representing a variable within the Blackboard system </summary>
    [Serializable, Readable]
    public class BlackboardVariable<T> : BlackboardVariable
    {
        /// <summary> Gets or sets the value associated with the blackboard variable, with type checking and validation. </summary>
        public T value
        {
            get
            {
                if (_variable is null)
                {
                    Debug.LogError($"variable is null.");
                    return default;
                }

                if (_variable is not Variable<T> convertedVariable)
                {
                    Debug.LogError($"variable type mismatch: {typeof(T).Name}.");
                    return default;
                }

                return convertedVariable.value;
            }

            set
            {
                if (_variable is null)
                {
                    Debug.LogError($"variable is null.");
                    return;
                }

                if (_variable is not Variable<T> convertedVariable)
                {
                    Debug.LogError($"variable type mismatch: {typeof(T).Name}.");
                    return;
                }

                convertedVariable.value = value;
            }
        }


        /// <summary>
        /// 현재 객체를 복제한 새로운 BlackboardVariable 인스턴스를 반환합니다.
        /// </summary>
        /// <returns>복제된 BlackboardVariable 객체</returns>
        internal override BlackboardVariable Duplicate()
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