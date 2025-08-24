using TaskStreamer.Utility;

namespace TaskStreamer
{
    public interface IBlackboard
    {
        public BlackboardVariable FindVariable(string variableKey);

        public BlackboardVariable FindVariable(in UGUID key);

        public void AddVariable(BlackboardVariable variable);

        public void RemoveVariable(BlackboardVariable variable);

        public bool HasVariable(string key);
    }
}