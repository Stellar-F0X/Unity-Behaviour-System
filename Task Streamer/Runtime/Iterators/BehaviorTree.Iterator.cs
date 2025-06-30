using System.Collections;
using System.Collections.Generic;
using TaskStreamer.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace TaskStreamer.BT
{
    public partial class BehaviorTree
    {
        private struct Iterator : IGraphIterator
        {
            public Iterator(BehaviorTree tree)
            {
                this._tree = tree;
            }

            private BehaviorTree _tree;

            public IEnumerator<NodeBase> GetEnumerator()
            {
                List<TreeTraversal> queue = ListPool<TreeTraversal>.Get();

                if (Application.isPlaying)
                {
                    queue.Add(new TreeTraversal((BehaviorNodeBase)_tree._nodeLookup[_tree.entry.guid], 0, 0));
                }
                else
                {
                    queue.Add(new TreeTraversal((BehaviorNodeBase)_tree.entry, 0, 0));
                }

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

                    if (info.node is not IChildNodeProvider provider)
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