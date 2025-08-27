using TaskStreamer.Utility;

namespace TaskStreamer
{
    public interface ISharedBlackboardVariable
    {
        internal void SetBlackboardAndVariableReference(in BlackboardData blackboard, in UGUID variableGuid);
    }
}