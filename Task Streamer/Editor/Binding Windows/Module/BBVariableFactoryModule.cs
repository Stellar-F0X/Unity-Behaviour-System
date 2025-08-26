using System;
using TaskStreamer.Utility;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class BBVariableFactoryModule : FactoryModule<BlackboardVariable>
    {
        public BBVariableFactoryModule(string title, int layer = 1) : base(typeof(BlackboardVariable), title, true, layer) { }


        protected override BlackboardVariable Create(Type type, Vector2 position, string entryName)
        {
            BlackboardVariable variable = ObjectFactory.CreateBlackboardVariable(type, StringUtility.ToNicifyName(type.Name));
            Debug.Assert(variable is not null, "Variable is null");
            return variable;
        }
    }
}