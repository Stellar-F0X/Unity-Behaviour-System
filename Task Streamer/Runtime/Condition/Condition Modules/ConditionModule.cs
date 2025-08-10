using System;
using Unity.Properties;

namespace TaskStreamer
{
    [Serializable, GeneratePropertyBag]
    public abstract class ConditionModule
    {
        [DontCreateProperty]
        public ComparisonType comparison;
        
        public virtual string tooltip
        {
            get { return "The operation is always performed with the left side as the standard."; }
        }

        public virtual ComparisonType availableOperators
        {
            get { return ComparisonType.Equal | ComparisonType.NotEqual; }
        }
        
        public abstract bool Execute();
    }

    //TODO: Abstract로 변경해보기.
    [Serializable]
    public class ConditionModule<T> : ConditionModule
    {
        [CreateProperty]
        public BlackboardVariable<T> variableA;
        
        [CreateProperty]
        public BlackboardVariable<T> variableB;
        
        
        public override bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}