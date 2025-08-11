namespace TaskStreamer
{
    public interface IGraphIterable
    {
        public IGraphIterator GetGraphIterator(GraphIteratorType iteratorType = GraphIteratorType.LS);
    }
}