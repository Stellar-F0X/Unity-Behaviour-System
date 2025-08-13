using System.Collections;
using System.Collections.Generic;
using TaskStreamer.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace TaskStreamer.BT
{
    public partial class BehaviorTree
    {
        /// <summary> Linear search iterator </summary>
        private struct LSIterator : IGraphIterator
        {
            public LSIterator(BehaviorTree tree)
            {
                this._tree = tree;
            }

            private BehaviorTree _tree;

            public IEnumerator<NodeBase> GetEnumerator()
            {
                return _tree._nodeLookup.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
        
        
        /// <summary> Breadth-first search iterator </summary>
        private struct BFSIterator : IGraphIterator
        {
            public BFSIterator(BehaviorTree tree)
            {
                this._tree = tree;
            }

            private BehaviorTree _tree;

            public IEnumerator<NodeBase> GetEnumerator()
            {
                List<TreeTraversal> queue = ListPool<TreeTraversal>.Get();
                
                //그래프가 생성될 때 항상 entry 노드부터 만들어지므로 항상 존재한다.
                queue.Add(new TreeTraversal((BehaviorNodeBase)_tree.entry, 0, 0));

                int pointIndex = 0;
                int callStackSize = 0;

                while (pointIndex < queue.Count)
                {
                    TreeTraversal info = queue[pointIndex++];

                    if (Application.isPlaying)
                    {
                        info.node.callStackID = info.stackID;
                        info.node.depth = info.depth;
                    }

                    yield return info.node;

                    if (info.node is not IChildProvider provider)
                    {
                        continue;
                    }

                    bool isParallel = info.node is ParallelNode;

                    for (int i = 0; i < provider.childCount; i++)
                    {
                        int newStackID = isParallel ? (++callStackSize) : info.node.callStackID;
                        queue.Add(new TreeTraversal(provider[i], info.node.depth, newStackID));
                    }
                }

                ListPool<TreeTraversal>.Release(queue);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}