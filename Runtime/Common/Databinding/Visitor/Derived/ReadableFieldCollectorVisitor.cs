using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Injection
{
    /// <summary> ReadableFieldCollectorVisitor 클래스는 DefaultVisitProcessor를 기반으로, 필드 정보를 수집하여 지정된 컨테이너에 저장하는 역할을 수행합니다. </summary>
    public class ReadableFieldCollectorVisitor : ReadableVisitorBase
    {
        /// <summary>주어진 컨테이너와 값에 기반하여 값을 가져오는 델리게이트입니다.</summary>
        private delegate TValue ValueGetter<TContainer, TValue>(ref TContainer container);


        /// <summary>읽을 수 있는 필드 정보를 수집 처리하는 방문자 프로세서입니다.</summary>
        private delegate void ValueSetter<TContainer, TValue>(ref TContainer container, TValue value);



        /// <summary> Unity Properties의 기본 방문 처리기 </summary>
        public ReadableFieldCollectorVisitor(List<object> propertiesContainer)
        {
            this._propertiesContainer = propertiesContainer;
        }


        /// <summary>읽기 가능한 필드 정보를 수집하는 프로세서를 나타냅니다.</summary>
        private readonly List<object> _propertiesContainer;


        /// <summary>특정 프로퍼티에 대한 방문 로직을 처리합니다.</summary>
        protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
        {
            if (value is null || container is null)
            {
                return;
            }

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

            if (handle.IsValid())
            {
                _propertiesContainer.Add(handle);
            }
            else
            {
                Debug.LogError("잘못된 VariableHandle 값, 고쳐라 인간.");
            }
        }
    }
}