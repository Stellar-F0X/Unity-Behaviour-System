using TaskStreamer.Runtime;

namespace TaskStreamer.Tool
{
    internal interface IRefreshableField
    {
        public void RefreshVariableFieldPanel(VariableHandle handle);
    }
}