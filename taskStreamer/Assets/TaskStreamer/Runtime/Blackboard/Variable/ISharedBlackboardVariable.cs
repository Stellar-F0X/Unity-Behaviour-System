namespace TaskStreamer.Runtime
{
    public interface ISharedBlackboardVariable
    {
        internal bool isValid
        {
            get;
        }
        
        internal void SetBlackboardReference(in BlackboardAsset blackboard);
    }
}