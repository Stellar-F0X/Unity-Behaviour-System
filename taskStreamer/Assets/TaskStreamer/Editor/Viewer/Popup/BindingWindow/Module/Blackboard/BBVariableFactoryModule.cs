using System;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.Tool
{
    public class BBVariableFactoryModule : FactoryModule<BlackboardVariable>
    {
        public BBVariableFactoryModule(string title, int layer = 1) : base(typeof(BlackboardVariable), title, true, layer) { }


        protected override BlackboardVariable Create(Type type, Vector2 position, string entryName)
        {
            Type genericType = typeof(BlackboardVariable<>).MakeGenericType(type);
            string name = StringUtility.ToNicifyName(type.Name);
            BlackboardVariable variable = TSObjectFactory.CreateBlackboardVariable(genericType, name);
            Assert.IsNotNull(variable, "Variable is null");
            return variable;
        }
    }
}