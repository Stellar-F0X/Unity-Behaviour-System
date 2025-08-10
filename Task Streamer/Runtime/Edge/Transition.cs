using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.FSM
{
    //조건적 Transition과 일반 트랜지션을 분리.
    public class Transition : ScriptableObject
    {
#if UNITY_EDITOR
        [DontCreateProperty]
        public string description;
#endif
        [DontCreateProperty]
        public UGUID fromNodeGuid;
        
        [DontCreateProperty]
        public UGUID toStateGuid;
        
        [DontCreateProperty]
        public bool conditional;

        [CreateProperty]
        public BlackboardBasedCondition conditions;


        internal void Setup(UGUID sourceNodeGuid, UGUID destinationNodeGuid)
        {
            this.conditional = false;
            this.fromNodeGuid = sourceNodeGuid;
            this.toStateGuid = destinationNodeGuid;
            this.conditions = new BlackboardBasedCondition();
        }


        public bool CheckConditions()
        {
            if (this.conditional)
            {
                return this.conditions.Execute();
            }
            else
            {
                return true;
            }
        }
    }
}