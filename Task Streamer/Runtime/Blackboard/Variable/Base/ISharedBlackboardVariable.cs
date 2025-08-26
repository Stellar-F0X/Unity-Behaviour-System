using TaskStreamer.Utility;

namespace TaskStreamer
{
    public interface ISharedBlackboardVariable
    {
        internal void SetBlackboardAndVariableReference(in IBlackboard blackboard, in UGUID variableGuid);
    }
}