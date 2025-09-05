using System.Collections;
using System.Collections.Generic;
using TaskStreamer.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace TaskStreamer.BT
{
    public partial class BehaviorTree
    {
        /// <summary> Breadth-first search iterator </summary>
        private struct BFSIterator : IGraphIterator
        {
            public BFSIterator(BehaviorTree tree)
            {
                this._tree = tree;
            }

            
            private readonly BehaviorTree _tree;

            
            public IEnumerator<NodeBase> GetEnumerator()
            {
                if (Application.isPlaying)
                {
                    return this.RuntimeEnumerator();
                }
                else
                {
                    return this.NonRuntimeEnumerator();
                }
            }
            
            
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }


            private IEnumerator<NodeBase> RuntimeEnumerator()
            {
                List<TreeTraversal> queue = ListPool<TreeTraversal>.Get();
                queue.Add(new TreeTraversal((BehaviorNodeBase)_tree.entry, 0, 0));

                int pointIndex = 0;
                int callStackSize = 0;

                //TODO: 왜 Root 하나뿐이면 오류가 나는지 찾아야됨.
                while (pointIndex < queue.Count)
                {
                    TreeTraversal traversal = queue[pointIndex++];
                    traversal.node.callStackID = traversal.stackID;
                    traversal.node.depth = traversal.depth;

                    yield return traversal.node;

                    if (traversal.node is not IChildProvider provider)
                    {
                        continue;
                    }

                    if (traversal.node is ParallelNode)
                    {
                        for (int i = 0; i < provider.childCount; i++)
                        {
                            queue.Add(new TreeTraversal(provider[i], traversal.node.depth + 1, (++callStackSize)));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < provider.childCount; i++)
                        {
                            queue.Add(new TreeTraversal(provider[i], traversal.node.depth + 1, traversal.node.callStackID));
                        }
                    }
                }

                ListPool<TreeTraversal>.Release(queue);
            }
            

            private IEnumerator<NodeBase> NonRuntimeEnumerator()
            {
                List<TreeTraversal> queue = ListPool<TreeTraversal>.Get();
                queue.Add(new TreeTraversal((BehaviorNodeBase)_tree.entry));

                int pointIndex = 0;

                while (pointIndex < queue.Count)
                {
                    TreeTraversal info = queue[pointIndex++];

                    yield return info.node;

                    if (info.node is not IChildProvider provider)
                    {
                        continue;
                    }

                    for (int i = 0; i < provider.childCount; ++i)
                    {
                        queue.Add(new TreeTraversal(provider[i]));
                    }
                }

                ListPool<TreeTraversal>.Release(queue);
            }
        }
    }
}