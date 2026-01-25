namespace TaskStreamer.Tool
{
    //setDirty/Undo/Redo Command
    public interface ICommand
    {
        public void Execute();
    }
}