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
                List<StateBase> queue = ListPool<StateBase>.Get();
                HashSet<UGUID> visited = HashSetPool<UGUID>.Get();
                
                visited.Add(_machine.entry.guid);
                queue.Add((StateBase)_machine.entry);

                int pointIndex = 0;

                while (pointIndex < queue.Count)
                {
                    StateBase state = queue[pointIndex++];

                    yield return state;

                    foreach (Transition transition in state.transitions)
                    {
                        UGUID nextNode = transition.toNodeGuid;
                        
                        if (visited.Contains(nextNode))
                        {
                            continue;
                        }

                        if (_machine._nodeLookup.TryGetValue(nextNode, out NodeBase next))
                        {
                            visited.Add(nextNode);
                            queue.Add(next as StateBase);
                        }
                    }
                }

                HashSetPool<UGUID>.Release(visited);
                ListPool<StateBase>.Release(queue);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}