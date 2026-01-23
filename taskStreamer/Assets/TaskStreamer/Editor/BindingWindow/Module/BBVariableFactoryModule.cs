using System;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.Tool
{
    public class BlackboardVariableFactoryModule : FactoryModule<BlackboardVariable>
    {
        public BlackboardVariableFactoryModule(string title, int layer = 1) : base(typeof(BlackboardVariable), title, true, layer) { }


        protected override BlackboardVariable Create(Type type, Vector2 position, string entryName)
        {
            BlackboardVariable variable = ObjectFactory.CreateBlackboardVariable(type, StringUtility.ToNicifyName(type.Name));
            Assert.IsNotNull(variable, "Variable is null");
            return variable;
        }
    }
}