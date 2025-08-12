namespace TaskStreamer
{
    public interface IGraphIterable
    {
        public IGraphIterator GetIterator(GraphIteratorType iteratorType = GraphIteratorType.LS);
    }
}