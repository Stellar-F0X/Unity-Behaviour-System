using System.Collections;
using System.Collections.Generic;

namespace TaskStreamer.FSM
{
    public partial class StateMachine
    {
        private struct Iterator : IGraphIterator
        {
            public Iterator(StateMachine machine)
            {
                this._machine = machine;
            }

            private StateMachine _machine;
            
            public IEnumerator<NodeBase> GetEnumerator()
            {
                return _machine._nodeLookup.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}