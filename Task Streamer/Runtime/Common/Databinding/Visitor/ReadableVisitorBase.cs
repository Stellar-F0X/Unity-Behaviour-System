using System;
using System.Collections.Generic;
using System.Reflection;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using Unity.Properties;

namespace TaskStreamer.Injection
{
    /// <summary> ReadableAttribute가 수식된 클래스들 대상으로만 방문한다. </summary>
    public abstract class ReadableVisitorBase : PropertyVisitor
    {
        /// <summary> ReadableAttribute가 수식된 클래스들 대상으로만 방문한다. </summary>
        protected ReadableVisitorBase()
        {
            //만약 현 객체가 IPropertyVisitorAdapter 인터페이스를 상속받아, 구현된게 있다면 어뎁터에 추가. 
            if (this is IPropertyVisitorAdapter visitorAdapter)
            {
                this.AddAdapter(visitorAdapter);
            }
        }


        /// <summary> ReadableVisitorBase에서 방문 가능한 대상 타입들을 저장하는 정적 컬렉션입니다. </summary>
        private readonly static HashSet<ICustomAttributeProvider> _VisitAvailable = new HashSet<ICustomAttributeProvider>()
        {
            typeof(List<NodeGroup>),
            typeof(List<Transition>),
            typeof(List<Condition>),
            typeof(KeyValuePair<UGUID, Graph>),
            typeof(KeyValuePair<UGUID, NodeBase>)
        };


        /// <summary> 지정된 프로퍼티가 제외 대상인지 확인합니다. </summary>
        /// <param name="property"> 검사할 프로퍼티입니다. </param>
        /// <param name="container"> 컨테이너의 참조입니다. </param>
        /// <param name="value"> 프로퍼티의 값입니다. </param>
        /// <returns> 제외 대상인 경우 true, 그렇지 않으면 false를 반환합니다. </returns>
        protected override bool IsExcluded<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
        {
            Type type = property.DeclaredValueType();

            if (_VisitAvailable.Contains(type))
            {
                return false; //Ignore filtering
            }

            if (type.HasAttribute<ReadableAttribute>())
            {
                _VisitAvailable.Add(type);
                return false; //Ignore filtering
            }
            else
            {
                return true; //Filtering this TValue type
            }
        }
    }
}