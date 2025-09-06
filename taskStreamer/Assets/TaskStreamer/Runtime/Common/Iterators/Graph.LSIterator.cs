using System.Collections;
using System.Collections.Generic;

namespace TaskStreamer
{
    public abstract partial class Graph
    {
        /// <summary> Linear search iterator </summary>
        public class CommonLSIterator : IGraphIterator
        {
            public CommonLSIterator(Graph graph)
            {
                this._graph = graph;
            }
            
            private readonly Graph _graph;
            
            
            public IEnumerator<NodeBase> GetEnumerator()
            {
                return _graph._nodeLookup.Values.GetEnumerator();
            }

            
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}