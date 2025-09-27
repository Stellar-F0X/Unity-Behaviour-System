using System;
using TaskStreamer.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.Tool
{
    public class EnumBlackboardVariableFactoryModule : FactoryModule<BlackboardVariable<Enum>>
    {
        public EnumBlackboardVariableFactoryModule(string title, int layer) : base(typeof(Enum), title, true, layer) { }

        protected override BlackboardVariable<Enum> Create(Type type, Vector2 position, string entryName)
        {
            string nicifyName = StringUtility.ToNicifyName(type.Name);
            Type enumType = typeof(BlackboardVariable<>).GetImplementedType(typeof(Enum));

            BlackboardVariable variable = ObjectFactory.CreateBlackboardVariable(enumType, nicifyName);
            Assert.IsNotNull(variable, "Variable is null");

            BlackboardVariable<Enum> enumVariable = variable as BlackboardVariable<Enum>;
            Assert.IsNotNull(enumVariable, "Enum Variable is null");
            
            //enumBlackboardVariable.value로 해도 어차피 boxing이 일어나기 때문에 그냥 대입.
            enumVariable.boxedValue = Enum.GetValues(type).GetValue(0);
            return enumVariable;
        }
    }
}