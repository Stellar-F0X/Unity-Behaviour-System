using System.Collections;
using System.Collections.Generic;
using TaskStreamer.Utility;
using UnityEngine.Pool;

namespace TaskStreamer.FSM
{
    public partial class StateMachine
    {
        private struct LSIterator : IGraphIterator
        {
            public LSIterator(StateMachine machine)
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
        
        
        private struct BFSIterator : IGraphIterator
        {
            public BFSIterator(StateMachine machine)
            {
                this._machine = machine;
            }

            private StateMachine _machine;

            public IEnumerator<NodeBase> GetEnumerator()
            {
                List<StateBase> queue = ListPool<StateBase>.Get();
                HashSet<UGUID> visited = HashSetPool<UGUID>.Get();

                // 그래프가 생성될 때 항상 entry 노드부터 만들어지므로 항상 존재한다.
                visited.Add(_machine.entry.guid);
                queue.Add((StateBase)_machine._nodeLookup[_machine.entry.guid]);

                int pointIndex = 0;

                while (pointIndex < queue.Count)
                {
                    StateBase state = queue[pointIndex++];

                    yield return state;

                    foreach (Transition transition in state.transitions)
                    {
                        if (visited.Contains(transition.toNodeGuid))
                        {
                            continue;
                        }
                        
                        if (_machine._nodeLookup.TryGetValue(transition.toNodeGuid, out NodeBase stateBase))
                        {
                            visited.Add(transition.toNodeGuid);
                            queue.Add(stateBase as StateBase);
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