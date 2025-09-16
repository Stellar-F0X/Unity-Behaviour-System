using System;
using System.Collections;
using System.Collections.Generic;
using TaskStreamer.Utility;
using UnityEngine.Pool;

namespace TaskStreamer.FSM
{
    public partial class StateMachine
    {
        private struct BFSIterator : IGraphIterator
        {
            public BFSIterator(StateMachine machine)
            {
                this._machine = machine;
            }

            private readonly StateMachine _machine;


            public IEnumerator<NodeBase> GetEnumerator()
            {
                throw new NotImplementedException("BreadthFirstSearch iterator is not implemented for StateMachine.");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}