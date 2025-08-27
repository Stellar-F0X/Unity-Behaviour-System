using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable, GeneratePropertyBag, Readable]
    public sealed partial class BlackboardBasedCondition
    {
        [DontCreateProperty]
        public EvaluationPolicy evaluationPolicy;
        
        [SerializeReference]
        public List<Condition> modules = new List<Condition>();
        

        public bool Execute()
        {
            if (evaluationPolicy == EvaluationPolicy.None)
            {
                return true;
            }
            
            if (modules is null)
            {
                Debug.LogWarning("Blackboard variables is not set for this condition.");
                return false;
            }

            if (modules.Count == 0)
            {
                return false;
            }

            switch (evaluationPolicy)
            {
                case EvaluationPolicy.Any: return EvaluateWithOrLogic(modules.Count);

                case EvaluationPolicy.All: return EvaluateWithAndLogic(modules.Count);
                
                default: return false;
            }
        }


        private bool EvaluateWithOrLogic(int count)
        {
            for (int index = 0; index < count; ++index)
            {
                Condition condition = this.modules[index];
                
                if (condition.Execute(condition.comparisonValue))
                {
                    return true;
                }
            }

            return false;
        }


        private bool EvaluateWithAndLogic(int count)
        {
            for (int index = 0; index < count; ++index)
            {
                Condition condition = this.modules[index];
                
                if (condition.Execute(condition.comparisonValue) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}