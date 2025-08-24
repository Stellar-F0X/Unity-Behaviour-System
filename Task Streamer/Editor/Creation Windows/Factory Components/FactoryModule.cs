using System;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public abstract class FactoryModule
    {
        protected FactoryModule(Type targetType, string title, bool tagetIsSubClass, int layer = 1)
        {
            this.targetType = targetType;
            this.title = string.IsNullOrEmpty(title) ? targetType.Name : title;
            this.tagetIsSubClass = tagetIsSubClass;
            this.layer = layer;
        }

        public Action<Type, Vector2, Delegate> sendCreationSignal
        {
            get;
            protected set;
        }

        public Type targetType
        {
            get;
            private set;
        }

        public string title
        {
            get;
            private set;
        }

        public int layer
        {
            get;
            private set;
        }

        public bool tagetIsSubClass
        {
            get;
            private set;
        }
    }


    public abstract class FactoryModule<T> : FactoryModule
    {
        protected FactoryModule(Type targetType, string title, bool tagetIsSubClass, bool useCreationCallback, int layer = 1) : 
            base(targetType, title, tagetIsSubClass, layer)
        {
            base.sendCreationSignal = this.ExecuteCreateActions;
            this._useCreationCallback = useCreationCallback;
        }

        private readonly bool _useCreationCallback;

        
        private void ExecuteCreateActions(Type childType, Vector2 position, Delegate createAction)
        {
            this.BeforeCreate(childType, position);
            
            T creation = default;

            try
            {
                creation = this.Create(childType, position);
            }
            catch (Exception e)
            {
                Debug.LogAssertion(e);
                return;
            }

            if (this._useCreationCallback)
            {
                createAction?.DynamicInvoke(creation);
            }

            this.AfterCreate(creation);
        }


        protected virtual void BeforeCreate(Type childType, Vector2 position) { }

        protected virtual void AfterCreate(T creation) { }

        protected abstract T Create(Type type, Vector2 position);
    }
}