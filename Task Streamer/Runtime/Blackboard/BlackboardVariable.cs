using System;
using Unity.Properties;
using UnityEditor;
using UnityEngine;

namespace TaskStreamer
{
    /// <summary> Variable wrapper class </summary>
    [Serializable]
    public abstract class BlackboardVariable : IEquatable<BlackboardVariable>
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
            get { return _variable.type; }

            set { _variable.type = value; }
        }


        public string name
        {
            get { return _variable.name; }

            set { this._variable.name = value; }
        }

        public int nameHash
        {
            get { return _variable.nameHash; }
        }

        /// <summary> True if the variable is local to this node; false if retrieved from the shared blackboard. </summary>
        internal bool isGlobal
        {
            get { return _isGlobal; }

            set { _isGlobal = value; }
        }


        public bool Equals(BlackboardVariable other)
        {
            if (other is null)
            {
                return false;
            }

            if (this._variable.nameHash != other.nameHash || this.type != other.type)
            {
                return false;
            }

            return object.ReferenceEquals(this, other);
        }


        public abstract BlackboardVariable Clone();
    }


    [Serializable, GeneratePropertyBag]
    public partial class BlackboardVariable<T> : BlackboardVariable
    {
        public BlackboardVariable()
        {
#if UNITY_EDITOR
            Type variableType = typeof(Variable<>).MakeGenericType(typeof(T));
            var collection = TypeCache.GetTypesDerivedFrom(variableType);
            this._variable = Variable.Create(collection[0]);
            this.type = variableType;
            this.value = default;
#endif
        }

        public T value
        {
            get { return ((Variable<T>)_variable).value; }

            set { ((Variable<T>)_variable).value = value; }
        }

        
        public override BlackboardVariable Clone()
        {
            return new BlackboardVariable<T>
            {
                _variable = this._variable,
                _isGlobal = this._isGlobal,
                value = this.value,
                name = this.name,
                type = this.type
            };
        }
    }
}