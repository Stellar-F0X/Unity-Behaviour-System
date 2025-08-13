using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable, GeneratePropertyBag]
    public partial class BlackboardBasedCondition
    {
        [DontCreateProperty]
        public EvaluationPolicy evaluationPolicy;
        
        [SerializeReference]
        public List<ConditionModule> modules = new List<ConditionModule>();
        

        public bool Execute()
        {
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
                if (this.modules[index].Execute())
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
                if (this.modules[index].Execute() == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}