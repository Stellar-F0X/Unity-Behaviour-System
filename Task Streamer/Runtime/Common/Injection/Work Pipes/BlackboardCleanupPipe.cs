using System.Collections.Generic;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;
using BBCondition = TaskStreamer.BlackboardBasedCondition;

namespace TaskStreamer.Injection
{
    /// <summary> Blackboard 교체될 때, 이미 등록되어 있는 BlackboardVariable을 해제하는 용도로 사용되는 객체. </summary>
    internal class BlackboardCleanupPipe : GraphPipe,
                                           IVisitPropertyAdapter<NodeDictionary>,
                                           IVisitPropertyAdapter<BBCondition>,
                                           IVisitContravariantPropertyAdapter<BlackboardVariable>
    {
        public BlackboardCleanupPipe(GraphTraveler traveler) : base(traveler) { }


        public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
        {
            IPropertyBag<Dictionary<UGUID, NodeBase>> propertyBag = PropertyBag.GetPropertyBag<Dictionary<UGUID, NodeBase>>();
            Dictionary<UGUID, NodeBase> dictionaryValue = (Dictionary<UGUID, NodeBase>)value;
            propertyBag.Accept(traveler, ref dictionaryValue);
        }
        

        public void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, BlackboardVariable value)
        {
            if (this.IsVariableValidInBlackboard(value) == false || value.isGlobal == false)
            {
                return;
            }
            
            //제대로 Variable이 유효하지 않은 경우 (BB가 변경됐거나, 참조 중인 BB의 Variable이 삭제됨)
            value.variable = null;
        }
        
        
        public void Visit<TContainer>(in VisitContext<TContainer, BBCondition> context, ref TContainer container, ref BBCondition value)
        {
            if (value.modules is null || value.modules.Count == 0)
            {
                return;
            }
            
            foreach (Condition condition in value.modules)
            {
                if (this.IsVariableValidInBlackboard(condition.encapsulatedLeftVariable))
                {
                    condition.encapsulatedLeftVariable.variable = null;
                }

                if (this.IsVariableValidInBlackboard(condition.encapsulatedRightVariable))
                {
                    condition.encapsulatedRightVariable.variable = null;
                }
            }
        }
        
        
        private bool IsVariableValidInBlackboard(BlackboardVariable variable)
        {
            if (traveler.blackboard == null || variable is null || variable.variable is null)
            {
                return false;
            }

            // 블랙보드에서 해당 Variable의 GUID로 검색
            return traveler.blackboard.variables.Exists(v => v != null && v.guid == variable.variable.guid);
        }
    }
}