#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using TaskStreamer.Injection;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace TaskStreamer
{
    /// <summary>
    /// Represents a container for managing and interacting with a collection of Graph objects.
    /// This class supports operations like adding, removing, and linking subgraphs.
    /// </summary>
    [GeneratePropertyBag]
    public partial class GraphAsset : ScriptableObject, IEquatable<GraphAsset>
    {
        /// <summary>
        /// 그래프 에셋이 생성될때 사용되는 메인 그래프의 타입이다.
        /// 에셋 생성과 메인 그래프 생성을 동시에 할 수 없어서 사용된다.
        /// </summary>
        /// <value> BT or FSM </value>
        [DontCreateProperty]
        public GraphType mainGraphType;

        /// <summary>
        /// 현재 사용 중인 블랙보드이다.
        /// </summary>
        [SerializeField, DontCreateProperty]
        public Blackboard blackboard;

        /// <summary>
        /// 진입 그래프이자, 가장 최상위 그래프이다. 
        /// </summary>
        [SerializeReference, DontCreateProperty, HideInInspector]
        private Graph _main;

        /// <summary>
        /// 그래프의 부모-자식 관계를 정리하는 딕셔너리로 부모 그래프의 GUID를 Key, 자식 그래프 GUID List를 Value로 사용한다.
        /// 그래프를 삭제할때, 그 그래프를 부모로 하는 하위 그래프를 삭제하는 용도로 사용된다. 
        /// </summary>
        [SerializeField, DontCreateProperty]
        private UGUIDDictionary _graphTreeMap = new UGUIDDictionary();

        /// <summary>
        /// 해당 에셋의 메인 그래프를 비롯한 모든 그래프를 저장하는 그래프 컨테이너이다.
        /// 메인 그래프가 첫 번째로 들어가고, 이후 메인 그래프의 자식들이 들어간다.
        /// </summary>
        [SerializeField]
        private GraphDictionary _graphMap = new GraphDictionary();



        public Graph main
        {
            get { return _main; }

            set { _graphMap[value.guid] = (_main = value); }
        }

        public GraphDictionary.ValueCollection graphs
        {
            get { return _graphMap.Values; }
        }


        /// <summary> 그래프를 런타임용으로 복제한다. </summary>
        /// <param name="streamer"> 그래프가 런타임에 필요한 객체들을 위해 그래프를 실행시키는 TaskStreamer를 매개변수로 받는다. </param>
        /// <returns> 복제된 그래프를 반환한다. </returns>
        public GraphAsset Clone(TaskStreamer streamer)
        {
            GraphAsset instantiatedGraphAsset = null;
            Blackboard instantiatedBlackboard = null;
            GraphVisitor dataContainer = null;

            instantiatedGraphAsset = Object.Instantiate(this);

            if (this.blackboard != null)
            {
                instantiatedBlackboard = Object.Instantiate(this.blackboard);
                instantiatedGraphAsset.blackboard = instantiatedBlackboard;
            }

            if (PropertyBag.Exists<GraphAsset>() == false)
            {
                Debug.LogError("GraphAsset does not have a property bag.");
                return null;
            }

            dataContainer = new GraphVisitor(instantiatedBlackboard, instantiatedGraphAsset, streamer);

            dataContainer.AddAdapter(new RuntimeInitAdapter(dataContainer));

            IPropertyBag<GraphAsset> bag = PropertyBag.GetPropertyBag<GraphAsset>();
            bag.Accept(dataContainer, ref instantiatedGraphAsset);

            return instantiatedGraphAsset;
        }

        
        public List<Graph> GetSubGraphs(UGUID baseGraphGuid)
        {
            if (baseGraphGuid.IsEmpty() || _graphTreeMap.TryGetValue(baseGraphGuid, out UGUIDList subGraphGuids) == false)
            {
                return null;
            }

            List<Graph> resultContainer = new List<Graph>(subGraphGuids.Count);

            for (int index = subGraphGuids.Count - 1; index >= 0; --index)
            {
                if (_graphMap.TryGetValue(subGraphGuids[index], out Graph subGraph))
                {
                    resultContainer.Add(subGraph);
                }
            }

            return resultContainer;
        }


        public Graph GetGraph(UGUID graphGuid)
        {
            if (_graphMap.TryGetValue(graphGuid, out Graph graph))
            {
                return graph;
            }

            Debug.Log($"GetGraph: {graphGuid} is not found.");
            return null;
        }


        public bool Equals(GraphAsset other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other) && ReferenceEquals(this.main, other.main))
            {
                return true;
            }

            if (_graphMap.Count != other._graphMap.Count)
            {
                return false;
            }

            if (this.main.guid != other.main.guid)
            {
                return false;
            }

            return true;
        }


#if UNITY_EDITOR
        internal void ResetBoundVariables()
        {
            if (this.blackboard == null || blackboard.variables.Count == 0)
            {
                return;
            }

            if (PropertyBag.Exists<GraphAsset>() == false)
            {
                Debug.LogError("GraphAsset does not have a property bag.");
                return;
            }
            
            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(this, "Task Streamer (ResetVariables)");
            }

            GraphVisitor dataContainer = new GraphVisitor(blackboard, this, null);

            dataContainer.AddAdapter(new VariablesInitAdapter(dataContainer));

            IPropertyBag<GraphAsset> bag = PropertyBag.GetPropertyBag<GraphAsset>();
            GraphAsset reference = this;

            bag.Accept(dataContainer, ref reference);
            
            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                EditorUtility.SetDirty(this);
            }
        }
        
        
        public void AddSubGraph(UGUID baseGuid, Graph graph)
        {
            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(this, "Task Streamer (AddGraph)");
            }

            if (graph is null || graph.entry == null || graph.guid.IsEmpty())
            {
                Debug.LogError("Cannot add a null graph.");
                return;
            }

            if (_graphMap.ContainsKey(graph.guid))
            {
                Debug.LogWarning($"Graph with GUID {graph.guid} already exists in the collection.");
                return;
            }

            _graphMap.Add(graph.guid, graph);

            graph.baseGraphGuid = baseGuid;
            this.AddSubGraphGuid(baseGuid, graph.guid);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                EditorUtility.SetDirty(this);
            }
        }


        public void RemoveSubGraph(UGUID baseGuid, Graph graph)
        {
            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(this, "Task Streamer (RemoveGraph)");
            }

            List<Graph> foundSubGraphs = this.GetSubGraphs(graph.guid); //해당 그래프를 부모로 하는 그래프들을 구한다.

            if (foundSubGraphs == null)
            {
                this._graphMap.Remove(graph.guid); //만약 자식이 없다면 바로 삭제 후 Return.
                this.RemoveSubGraphGuid(graph.baseGraphGuid, graph.guid);
                return;
            }

            List<Graph> subGraphs = ListPool<Graph>.Get();
            subGraphs.AddRange(foundSubGraphs);

            for (int i = 0; i < subGraphs.Count; ++i)
            {
                foundSubGraphs = this.GetSubGraphs(subGraphs[i].guid); //찾은 그래프의 자식 그래프들을 찾는다.

                if (foundSubGraphs == null)
                {
                    continue;
                }

                subGraphs.AddRange(foundSubGraphs);
            }

            for (int index = subGraphs.Count - 1; index >= 0; index--)
            {
                this._graphMap.Remove(subGraphs[index].guid);
                this.RemoveSubGraphGuid(subGraphs[index].baseGraphGuid, subGraphs[index].guid);
            }

            this._graphMap.Remove(graph.guid);
            this.RemoveSubGraphGuid(baseGuid, graph.guid);

            ListPool<Graph>.Release(subGraphs);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                EditorUtility.SetDirty(this);
            }
        }


        private void AddSubGraphGuid(UGUID from, UGUID to)
        {
            if (from.IsEmpty() || to.IsEmpty())
            {
                Debug.LogError("Cannot add dependency with empty GUIDs.");
                return;
            }

            if (_graphTreeMap.ContainsKey(from) == false)
            {
                _graphTreeMap.Add(from, new UGUIDList());
            }

            if (_graphTreeMap[from].Contains(to) == false)
            {
                _graphTreeMap[from].Add(to);
            }
        }


        private void RemoveSubGraphGuid(UGUID from, UGUID to)
        {
            if (from.IsEmpty() || to.IsEmpty())
            {
                Debug.LogError("Cannot remove dependency with empty GUIDs.");
                return;
            }

            bool found = _graphTreeMap.TryGetValue(from, out UGUIDList dependencies);

            if (found == false)
            {
                return;
            }

            bool completeRemove = dependencies.Remove(to);

            if (completeRemove && dependencies.Count == 0)
            {
                _graphTreeMap.Remove(from);
            }
        }
#endif
    }
}