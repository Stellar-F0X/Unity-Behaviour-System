namespace TaskStreamer.Runtime
{
    public interface IGraphIterable
    {
        public IGraphIterator GetIterator(GraphIteratorType iteratorType = GraphIteratorType.LS);
    }
}