namespace TaskStreamer.Utility
{
    internal interface IKeyValuePair<TKey, TValue>
    {
        public TKey key { get; set; }
        
        public TValue value { get; set; }
    }
}