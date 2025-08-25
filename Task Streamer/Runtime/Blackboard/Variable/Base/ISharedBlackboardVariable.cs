using TaskStreamer.Utility;

namespace TaskStreamer
{
    public interface ISharedBlackboardVariable
    {
        public void SetBlackboardAndVariableReference(in IBlackboard blackboard, in UGUID variableGuid);
    }
}