using TaskStreamer.Utility;

namespace TaskStreamer
{
    public interface IBlackboard
    {
        public Variable FindVariable(string variableKey);

        public Variable FindVariable(in UGUID key);

        public void AddVariable(Variable variable);

        public void RemoveVariable(Variable variable);

        public bool HasVariable(string key);
    }
}