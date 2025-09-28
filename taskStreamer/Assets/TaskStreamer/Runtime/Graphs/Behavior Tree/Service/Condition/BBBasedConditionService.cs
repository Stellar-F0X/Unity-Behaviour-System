using System;
using TaskStreamer;
using TaskStreamer.BT;
using Unity.Properties;

namespace TaskStreamer
{
    [Serializable, GeneratePropertyBag, Readable]
    public partial class BBBasedConditionService : ServiceBase
    {
        public BlackboardBasedCondition conditions = new BlackboardBasedCondition();


        public override bool CanVisit()
        {
            if (this.conditions is null)
            {
                return false;
            }
            else
            {
                return this.conditions.Execute();
            }
        }
    }
}