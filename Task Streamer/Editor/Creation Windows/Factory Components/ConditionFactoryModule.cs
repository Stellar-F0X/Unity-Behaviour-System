using System;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class ConditionFactoryModule : FactoryModule<Condition>
    {
        public ConditionFactoryModule(Type targetType, string title, int layer = 1) : base(targetType, title, true, true, layer) { }

        
        protected override Condition Create(Type type, Vector2 position)
        {
            Condition module = this.CreateConditionModule(type);
            Debug.Assert(module is not null, "ConditionModule is null");
            return module;
        }


        private Condition CreateConditionModule(Type conditionType)
        {
            if (conditionType is null)
            {
                throw new ArgumentException($"{typeof(ObjectFactory)}: Wrong condition type");
            }

            Condition module = Activator.CreateInstance(conditionType) as Condition;

            if (module is null)
            {
                throw new ArgumentException($"{typeof(ObjectFactory)}: Failed to create a condition module.");
            }

            string variableName = BlackboardVariable.DEFAULT_VARIABLE_NAME;
            Type bbType = typeof(BlackboardVariable<>).GetImplementedType(conditionType!.BaseType!.GenericTypeArguments[0]);
            module.encapsulatedLeftVariable =  ObjectFactory.CreateBBVariable(bbType, variableName);
            module.encapsulatedRightVariable = ObjectFactory.CreateBBVariable(bbType, variableName);

            ComparableAttribute comparable = conditionType.GetAttribute<ComparableAttribute>();
            module.configuredComparisonType = comparable is null ? Condition.DEFAULT_COMPARISON : comparable.comparison;
            return module;
        }
    }
}