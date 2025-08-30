namespace TaskStreamer.Tool
{
    public interface IRefreshablePanel
    {
        public void RefreshPanel();
    }
    
    public interface IRefreshableField
    {
        public void RefreshVariableFieldPanel(VariableHandle handle);
    }
}