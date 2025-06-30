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
        public UGUID startStateGuid;
        
        [DontCreateProperty]
        public UGUID targetStateGuid;
        
        [DontCreateProperty]
        public bool conditional;
        
        [DontCreateProperty]
        public ECompleteType completeType;

        [CreateProperty]
        public BlackboardBasedCondition conditions;


        internal void Setup(UGUID startStateGuid, UGUID targetStateGuid)
        {
            this.conditional = false;
            this.startStateGuid = startStateGuid;
            this.targetStateGuid = targetStateGuid;
            this.conditions = new BlackboardBasedCondition();
        }


        public bool CheckConditions()
        {
            if (this.conditional)
            {
                return this.conditions.Execute(completeType);
            }
            else
            {
                return true;
            }
        }
    }
}