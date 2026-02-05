namespace TaskStreamer.Runtime
{
    //setDirty/Undo/Redo Command
    internal interface ICommand
    {
        public void Execute();
    }
}