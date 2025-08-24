using System;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class BBVariableFactoryModule : FactoryModule<BlackboardVariable>
    {
        public BBVariableFactoryModule(Type targetType, string title, int layer = 1) : base(targetType, title, true, true, layer) { }

        protected override BlackboardVariable Create(Type type, Vector2 position)
        {
            BlackboardVariable variable = Utility.ObjectFactory.CreateBBVariable(type, StringUtility.ToNicifyName(type.Name));
            Debug.Assert(variable is not null, "Variable is null");
            return variable;
        }
    }
}