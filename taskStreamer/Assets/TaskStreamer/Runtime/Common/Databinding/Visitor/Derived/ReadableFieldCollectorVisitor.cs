using System;
using System.Collections.Generic;
using TaskStreamer.BT;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer
{
#if UNITY_EDITOR
    /// <summary> ReadableVisitorBase 기반으로, Task 객체의 필드 정보를 수집하여 지정된 컨테이너에 저장한다. </summary>
    public class ReadableFieldCollectorVisitor : ReadableVisitorBase
    {
        private delegate TValue ValueGetter<TContainer, TValue>(ref TContainer container);
        
        private delegate void ValueSetter<TContainer, TValue>(ref TContainer container, TValue value);



        /// <summary> Unity Properties의 기본 방문 처리기 </summary>
        public ReadableFieldCollectorVisitor(PriorityQueue<VariableHandle> propertiesContainer)
        {
            this._propertiesContainer = propertiesContainer;
        }


        /// <summary>읽기 가능한 필드 정보를 수집하는 프로세서를 나타냅니다.</summary>
        private readonly PriorityQueue<VariableHandle> _propertiesContainer;


        /// <summary>특정 프로퍼티에 대한 방문 로직을 처리합니다.</summary>
        protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
        {
            Assert.IsFalse(container is null, $"Container ({typeof(TContainer)}) is null");
            Assert.IsFalse(value is null, $"Value ({typeof(TValue)}) is null");

            if (property.IsReadOnly)
            {
                Debug.LogError($"'{typeof(TContainer)}.{property.Name}' is read-only and cannot be modified.");
                return;
            }

            VariableHandle handle = VariableHandleBuilder.GetHandle(property.Name, value, container)
                                                         .WithGetter<ValueGetter<TContainer, TValue>>(property.GetValue)
                                                         .WithSetter<ValueSetter<TContainer, TValue>>(property.SetValue)
                                                         .WithAttributes(property.GetAttributes())
                                                         .Build();

            PropertyOrderAttribute order = property.GetAttribute<PropertyOrderAttribute>();
            Assert.IsTrue(handle.IsValid(), "잘못된 VariableHandle 값, 고쳐라 인간.");
            this._propertiesContainer.Enqueue(handle, order is null ? int.MaxValue : order.priority);
        }



        protected override bool IsExcluded<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
        {
            Type type = property.DeclaredValueType();

            //에디터에선 인스펙터에 있어서 해당 타입의 객체가 필요하지 않으므로 수집에서 제외한다. 
            if (type == typeof(List<Transition>))
            {
                return true;
            }

            //마찬가지.
            if (type == typeof(List<ServiceBase>))
            {
                return true;
            }

            return base.IsExcluded(property, ref container, ref value);
        }
    }
#endif
}