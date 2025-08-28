using System;
using TaskStreamer.Injection;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer
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
            if (ConditionExtension.ReassignmentMissingBlackboardVariable(condition, condition.lVariable, out BlackboardVariable variable))
            {
                condition.lVariable = variable;
            }

            return VariableHandleBuilder.GetHandle(variable.key, variable, condition)
                                        .WithFieldType(variable.genericVariableType)
                                        .WithGetter<Func<Condition, object>>(c => c.lVariable)
                                        .WithSetter<Action<Condition, object>>((c, v) => c.lVariable = (BlackboardVariable)v)
                                        .Build();
        }


        /// <summary>EncapsulatedRightVariable의 VariableHandle을 생성하여 반환합니다.</summary>
        /// <param name="condition">VariableHandle을 생성할 Condition 객체입니다.</param>
        /// <returns>생성된 VariableHandle.</returns>
        internal static VariableHandle GetRightVariableHandle(this Condition condition)
        {
            if (ConditionExtension.ReassignmentMissingBlackboardVariable(condition, condition.rVariable, out BlackboardVariable variable))
            {
                condition.rVariable = variable;
            }

            return VariableHandleBuilder.GetHandle(variable.key, variable, condition)
                                        .WithFieldType(variable.genericVariableType)
                                        .WithGetter<Func<Condition, object>>(c => c.rVariable)
                                        .WithSetter<Action<Condition, object>>((c, v) => c.rVariable = (BlackboardVariable)v)
                                        .Build();
        }


        /// <summary>Condition과 기존 블랙보드 변수를 기반으로 새 블랙보드 변수를 생성하거나 기존 변수를 반환</summary>
        /// <param name="condition">현재 Condition 객체</param>
        /// <param name="variable">기존 블랙보드 변수</param>
        /// <param name="newVariable">반환될 새 블랙보드 변수 또는 null</param>
        /// <returns>블랙보드 변수가 유효하지 않아 새로 생성된 경우 true, 그렇지 않으면 false</returns>
        private static bool ReassignmentMissingBlackboardVariable(Condition condition, BlackboardVariable variable, out BlackboardVariable newVariable)
        {
            if (variable is not ISharedBlackboardVariable shared)
            {
                newVariable = null;
                return false;
            }

            if (shared.isValid)
            {
                newVariable = null;
                return false;
            }

            Type variableType = typeof(BlackboardVariable<>).GetImplementedType(condition.valueType);
            newVariable = ObjectFactory.CreateBlackboardVariable(variableType);
            newVariable.usage = VariableUsage.Condition;
            return true;
        }
#endif
    }
}