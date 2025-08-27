using TaskStreamer.Utility;

namespace TaskStreamer
{
    public interface ISharedBlackboardVariable
    {
        internal bool isValid
        {
            get;
        }

        internal void SetBlackboardAndVariableReference(in BlackboardAsset blackboard, in UGUID variableGuid);
    }
}