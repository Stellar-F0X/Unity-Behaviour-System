using System;
using TaskStreamer.Utility;
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
            get { return _variable.type; }

            set { _variable.type = value; }
        }


        public string name
        {
            get { return _variable.key; }

            set { this._variable.key = value; }
        }

        public int nameHash
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


        public abstract BlackboardVariable Clone();

#if UNITY_EDITOR
        internal abstract void OnChangeAccessModifier();
#endif
    }


    [Serializable]
    public partial class BlackboardVariable<T> : BlackboardVariable
    {
        [SerializeField]
        private T _initializedValue;
        
        
        public T value
        {
            get { return ((Variable<T>)_variable).value; }

            set { ((Variable<T>)_variable).value = value; }
        }


        
        public override BlackboardVariable Clone()
        {
            BlackboardVariable<T> clone = new BlackboardVariable<T>();
            
            clone._variable = this._variable;
            clone._isGlobal = this._isGlobal;
            clone.value = this.value;
            clone.name = this.name;
            clone.type = this.type;
            
            return clone;
        }

        //TODO: 추후 BlackboardVariableDrawer를 Custom Property에서 UI Toolkit으로 대체하면 사용해서 기본 값을 반영하자.
        internal override sealed void OnChangeAccessModifier()
        {
            if (_variable is Variable<T> convertedVariable)
            {
                convertedVariable.value = this._initializedValue;
            }
            else
            {
                Debug.LogError("Failed to change access modifier of the variable.");
            }
        }


        public static implicit operator BlackboardVariable<T>(T value)
        {
            BlackboardVariable<T> variable = new BlackboardVariable<T>();
            Type type = TypeCollection.GetVariableType<T>();
            variable._variable = Utilities.CreateVariable(type);
            variable._initializedValue = value;
            variable.value = value;
            return variable;
        }
    }
}