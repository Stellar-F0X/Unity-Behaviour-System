using System;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.Utility;

namespace TaskStreamer.Runtime
{
    /// <summary>Condition 클래스의 확장 메서드를 포함합니다.</summary>
    public static class ConditionExtension
    {
#if UNITY_EDITOR
        /// <summary>EncapsulatedLeftVariable을 기반으로 VariableHandle 반환</summary>
        /// <param name="condition">VariableHandle 생성을 위한 조건 객체</param>
        /// <returns>Condition의 EncapsulatedLeftVariable을 기반으로 생성된 VariableHandle</returns>
        internal static VariableHandle GetLeftVariableHandle(this Condition condition)
        {
            //만약 condition의 left Variable이 null이거나, 이용할 수 없는 객체라면 재할당한다.
            if (TryReassignmentMissingBlackboardVariable(condition, condition.lVariable, out BlackboardVariable variable))
            {
                condition.lVariable = variable;
            }

            return VariableHandleBuilder.GetHandle(condition.lVariable.key, condition.lVariable, condition)
                                        .WithFieldType(condition.lVariable.genericVariableType)
                                        .WithGetter<Func<Condition, object>>(c => c.lVariable)
                                        .WithSetter<Action<Condition, object>>((c, v) => c.lVariable = (BlackboardVariable)v)
                                        .Build();
        }


        /// <summary>EncapsulatedRightVariable의 VariableHandle을 생성하여 반환합니다.</summary>
        /// <param name="condition">VariableHandle을 생성할 Condition 객체입니다.</param>
        /// <returns>생성된 VariableHandle.</returns>
        internal static VariableHandle GetRightVariableHandle(this Condition condition)
        {
            //만약 condition의 right Variable이 null이거나, 이용할 수 없는 객체라면 재할당한다.
            if (TryReassignmentMissingBlackboardVariable(condition, condition.rVariable, out BlackboardVariable variable))
            {
                condition.rVariable = variable;
            }

            return VariableHandleBuilder.GetHandle(condition.rVariable.key, condition.rVariable, condition)
                                        .WithFieldType(condition.rVariable.genericVariableType)
                                        .WithGetter<Func<Condition, object>>(c => c.rVariable)
                                        .WithSetter<Action<Condition, object>>((c, v) => c.rVariable = (BlackboardVariable)v)
                                        .Build();
        }


        /// <summary>Condition과 기존 블랙보드 변수를 기반으로 새 블랙보드 변수를 생성하거나 기존 변수를 반환</summary>
        /// <param name="condition">현재 Condition 객체</param>
        /// <param name="variable">기존 블랙보드 변수</param>
        /// <param name="newVariable">반환될 새 블랙보드 변수 또는 null</param>
        /// <returns>블랙보드 변수가 유효하지 않아 새로 생성된 경우 true, 그렇지 않으면 false</returns>
        private static bool TryReassignmentMissingBlackboardVariable(Condition condition, BlackboardVariable variable, out BlackboardVariable newVariable)
        {
            //ISharedBlackboardVariable을 상속받은 클래스들은 모두 구체 클래스라, 형변환에 실패할 수 없고, 만약에 variable 변수가 유효하면 Early Return.
            if (((ISharedBlackboardVariable)variable).isValid)
            {
                newVariable = null;
                return false;
            }

            Type variableType = typeof(BlackboardVariable<>).GetImplementedType(condition.valueType);
            
            if (variable.isShared)
            {
                //variable.boxedValue는 Property라서 getter/setter가 호출되는데 blackboard가 유효하지 않아서 호출하기 애매함.
                newVariable = ObjectFactory.CreateBlackboardVariable(variableType);
            }
            else
            {
                newVariable = ObjectFactory.CreateBlackboardVariable(variableType, defaultValue: variable.boxedValue);
            }
            
            newVariable.usage = VariableUsage.Condition;
            return true;
        }
#endif
    }
}