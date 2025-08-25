using System;
using System.Collections.Generic;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;
using BBCondition = TaskStreamer.BlackboardBasedCondition;

namespace TaskStreamer.Injection
{
    /// <summary> Blackboard 교체될 때, 이미 등록되어 있는 BlackboardVariable을 해제하는 용도로 사용되는 객체. </summary>
    internal class BlackboardCleanupProcess : GraphVisitProcess,
                                              IVisitPropertyAdapter<NodeDictionary>,
                                              IVisitPropertyAdapter<BBCondition>,
                                              IVisitContravariantPropertyAdapter<BlackboardVariable>
    {
        public BlackboardCleanupProcess(GraphVisitProcessor processor) : base(processor) { }


        public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
        {
            IPropertyBag<Dictionary<UGUID, NodeBase>> propertyBag = PropertyBag.GetPropertyBag<Dictionary<UGUID, NodeBase>>();
            Dictionary<UGUID, NodeBase> dictionaryValue = (Dictionary<UGUID, NodeBase>)value;
            propertyBag.Accept(processor, ref dictionaryValue);
        }


        public void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, BlackboardVariable bbVariable)
        {
            if (this.IsVariableValidInBlackboard(bbVariable) == false || bbVariable.isShared == false)
            {
                return;
            }
            
            //제대로 Variable이 유효하지 않은 경우 (BB가 변경됐거나, 참조 중인 BB의 Variable이 삭제됨)
            var newVariable = ObjectFactory.CreateBBVariable(bbVariable.type, BlackboardVariable.DEFAULT_VARIABLE_NAME);
            context.Property.SetValue(ref container, newVariable);
        }


        public void Visit<TContainer>(in VisitContext<TContainer, BBCondition> context, ref TContainer container, ref BBCondition value)
        {
            if (value.modules is null || value.modules.Count == 0)
            {
                return;
            }

            foreach (Condition condition in value.modules)
            {
                //Blackboard는 BBVariable이 아니라 Variable을 사용하기 때문에 BB에서 등록된 BBVariable의 Variable만 없애주면 됨. 
                if (this.IsVariableValidInBlackboard(condition.encapsulatedLeftVariable))
                {
                    Type type = condition.encapsulatedLeftVariable.type;
                    string name = BlackboardVariable.DEFAULT_VARIABLE_NAME;
                    condition.encapsulatedLeftVariable = ObjectFactory.CreateBBVariable(type, name);
                }

                if (this.IsVariableValidInBlackboard(condition.encapsulatedRightVariable))
                {
                    Type type = condition.encapsulatedLeftVariable.type;
                    string name = BlackboardVariable.DEFAULT_VARIABLE_NAME;
                    condition.encapsulatedRightVariable = ObjectFactory.CreateBBVariable(type, name);
                }
            }
        }


        private bool IsVariableValidInBlackboard(BlackboardVariable variable)
        {
            Debug.Assert(variable is not null, "variable is not null");
            
            if (processor.blackboard == null)
            {
                return false;
            }

            // 블랙보드에서 해당 Variable의 GUID로 검색
            return processor.blackboard.variables.Exists(v => v != null && v.guid == variable.guid);
        }
    }
}