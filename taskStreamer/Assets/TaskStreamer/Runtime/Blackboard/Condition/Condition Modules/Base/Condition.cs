using System;
using TaskStreamer.Runtime.Utility;

namespace TaskStreamer.Runtime
{
    /// <summary>Condition 클래스는 다양한 비교 조건을 정의하는 기본 추상 클래스입니다.</summary>
    [Serializable]
    public abstract class Condition : Task
    {
        public Condition()
        {
            this.guid = UGUID.Create();
            this.name = this.GetType().Name;
        }

        
        public bool enable = true;

#if UNITY_EDITOR
        internal bool isExpanded = true;
#endif
        
        /// <summary>주어진 비교 조건을 기반으로 실행 결과를 반환합니다.</summary>
        /// <return>비교 결과에 따라 true 또는 false를 반환합니다.</return>
        public abstract bool Execute(NodeBase calledNode);
    }
}