using System.Collections.Generic;
using System.Linq;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;
using BBCondition = TaskStreamer.BlackboardBasedCondition;

namespace TaskStreamer.Injection
{
    /// <summary> Blackboard 교체될 때, 이미 등록되어 있는 BlackboardVariable을 해제하는 용도로 사용되는 객체. </summary>
    internal class BlackboardSyncVisitor : GraphVisitorBase,
                                           IVisitPropertyAdapter<NodeDictionary>,
                                           IVisitPropertyAdapter<BBCondition>,
                                           IVisitContravariantPropertyAdapter<BlackboardVariable>
    {
        public BlackboardSyncVisitor(GraphContext context) : base(context)
        {
            //내부적으로 Concurrent dictionary에서 PropertyBag을 가져오는 방식이라 평균 O(1)이지만, 역시 캐싱이 제일 빠르다. 
            _nodeDictionaryVisitBag = PropertyBag.GetPropertyBag<Dictionary<UGUID, NodeBase>>();
        }

        
        private static IPropertyBag<Dictionary<UGUID, NodeBase>> _nodeDictionaryVisitBag; 

        

        /// <summary> NodeDictionary 프로퍼티를 방문하여 값을 처리합니다. </summary>
        /// <param name="context"> 방문 시 제공되는 컨텍스트입니다. </param>
        /// <param name="container"> 방문 중인 콘테이너 객체입니다. </param>
        /// <param name="value"> 방문 대상 NodeDictionary 값입니다. </param>
        public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
        {
            Dictionary<UGUID, NodeBase> dictionaryValue = (Dictionary<UGUID, NodeBase>)value;
            _nodeDictionaryVisitBag.Accept(this, ref dictionaryValue);
        }


        /// <summary> NodeDictionary 타입의 프로퍼티를 방문할 때 실행되는 메서드입니다. </summary>
        /// <param name="context">프로퍼티 방문에 대한 컨텍스트를 나타냅니다.</param>
        /// <param name="container">프로퍼티가 속한 컨테이너 객체입니다.</param>
        /// <param name="value">방문 중인 NodeDictionary 타입의 값입니다.</param>
        public void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, BlackboardVariable bbVariable)
        {
            if (context.Property.IsReadOnly)
            {
                Debug.LogError($"'{typeof(TContainer)}.{context.Property.Name}' is read-only and cannot be modified.");
                return;
            }
            
            //CleanupProcess는 블랙보드가 없어졌거나, 다른 BB로 교체, 또는 BB내, Variable이 하나라도 제거됐을때, 동작한다.
            //따라서 매개변수로 전달된 variable이 블랙보드에 존재하면 Pass, 없다면 노드의 필드에 등록된 Variable을 제거한다.
            if (bbVariable.isShared == false || this.IsVariableValidInBlackboard(bbVariable))
            {
                return;
            }

            //제대로 Variable이 유효하지 않은 경우 (BB가 변경됐거나, 참조 중인 BB의 Variable이 삭제됨)
            BlackboardVariable newVariable = ObjectFactory.CreateBlackboardVariable(bbVariable.implementedType);
            context.Property.SetValue(ref container, newVariable);
        }


        /// <summary> NodeDictionary 속성을 방문하여 관련 작업을 수행합니다. </summary>
        /// <param name="context">방문 컨텍스트 정보입니다.</param>
        /// <param name="container">속성이 포함된 컨테이너 객체입니다.</param>
        /// <param name="value">방문 중인 NodeDictionary 값입니다.</param>
        public void Visit<TContainer>(in VisitContext<TContainer, BBCondition> context, ref TContainer container, ref BBCondition value)
        {
            if (value.modules is null || value.modules.Count == 0)
            {
                return;
            }

            // Blackboard에 등록된 BBVariable의 Variable만 새로 생성해서 교체해야 하므로
            // 해당 판단과 생성 로직을 함수로 추출하여 중복을 제거합니다.
            foreach (Condition condition in value.modules)
            {
                if (condition is null)
                {
                    continue;
                }

                condition.lVariable = this.ReplaceIfRegistered(condition.lVariable);

                condition.rVariable = this.ReplaceIfRegistered(condition.rVariable);
            }
        }


        /// <summary> 등록된 BlackboardVariable이 유효한지 확인하고 필요 시 새로 교체합니다. </summary>
        /// <param name="variable"> 검사 및 교체 대상 BlackboardVariable입니다. </param>
        /// <returns> 유효성에 따라 교체된 혹은 입력된 원래의 BlackboardVariable을 반환합니다. </returns>
        private BlackboardVariable ReplaceIfRegistered(BlackboardVariable variable)
        {
            //Condition의 l/rVariable은 항상 함께 만들어지기 때문에 하나라도 없으면 문제가 됨.
            Debug.Assert(variable is not null, "variable is null");

            if (variable.isShared == false || this.IsVariableValidInBlackboard(variable))
            {
                //Condition에서 사용 중인 l/r Variable이 Local Variable이라면 그대로 Return.
                return variable;
            }
            else
            {
                //Shared Variable이라면 BB에 변동이 생겨, 동작하는 것이므로 기존 Shared Variable 대신 쓸, Local Variable로 만들어 반환한다.
                return ObjectFactory.CreateBlackboardVariable(variable.implementedType);
            }
        }


        /// <summary> 전달된 BlackboardVariable이 해당 블랙보드에 유효한지 검사한다. </summary>
        /// <param name="variable">유효성을 검사할 BlackboardVariable.</param>
        /// <returns>변수가 블랙보드에 존재하면 true, 그렇지 않으면 false.</returns>
        private bool IsVariableValidInBlackboard(BlackboardVariable variable)
        {
            Debug.Assert(variable is not null, "variable is not null");

            //Blackboard가 없으면 무조건 새로운 BlackboardVariable을 할당해야되므로 Early Return을 하는 True가 아닌 False를 반환.
            if (_context.blackboard == null)
            {
                return false;
            }

            // 블랙보드에서 해당 Variable의 GUID로 검색
            return _context.blackboard.variables.FirstOrDefault(v => v != null && v.guid == variable.guid) is not null;
        }
    }
}