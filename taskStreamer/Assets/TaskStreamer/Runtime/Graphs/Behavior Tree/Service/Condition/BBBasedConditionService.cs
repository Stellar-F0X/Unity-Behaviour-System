using System;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.BT;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Runtime
{
    [Serializable, GeneratePropertyBag, Readable]
    public partial class BBBasedConditionService : ServiceBase
    {
        [SerializeField]
        internal BlackboardBasedCondition conditions = new BlackboardBasedCondition();


        public override bool CanVisit()
        {
            if (this.conditions is null)
            {
                return false;
            }

            return this.conditions.Execute(base.node);
        }
    }
}