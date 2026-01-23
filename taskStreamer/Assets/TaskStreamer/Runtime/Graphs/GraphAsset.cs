#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using TaskStreamer.Runtime.Utility;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace TaskStreamer.Runtime
{
    /// <summary>
    /// Represents a container for managing and interacting with a collection of Graph objects.
    /// This class supports operations like adding, removing, and linking subgraphs.
    /// </summary>
    [Readable, GeneratePropertyBag]
    public partial class GraphAsset : ScriptableObject, IEquatable<GraphAsset>
    {
        /// <summary> 그래프 에셋이 생성될 때 설정되는 메인 그래프의 타입. </summary>
        /// <value> BT, FSM, GOAP(미구현) </value>
        [DontCreateProperty]
        internal GraphType mainGraphType;


        /// <summary> 그래프 에셋에서 사용되는 블랙보드 데이터이다. </summary>
        [SerializeField, DontCreateProperty]
        internal BlackboardAsset blackboard;


        /// <summary> 그래프 에셋을 고유하게 식별하기 위한 GUID </summary>
        [SerializeField, DontCreateProperty]
        private UGUID _graphGuid;


        /// <summary> 진입 그래프이자, 가장 최상위 그래프를 나타낸다. </summary>
        [SerializeReference, DontCreateProperty, HideInInspector]
        private Graph _main;


        /// <summary>
        /// 그래프의 부모-자식 관계를 저장하는 딕셔너리로, 부모 그래프의 GUID를 Key로, 자식 그래프 GUID 리스트를 Value로 사용한다.
        /// 그래프 삭제 시, 관련된 하위 그래프들을 처리하는 데 활용된다.
        /// </summary>
        [SerializeField, DontCreateProperty]
        private UGUIDDictionary _graphTreeMap = new UGUIDDictionary();


        /// <summary> 메인 그래프와 해당 에셋의 모든 서브 그래프를 저장 및 관리하는 사전 컨테이너이다. </summary>
        [SerializeField]
        private GraphDictionary _graphMap = new GraphDictionary();



        /// <summary>
        /// 그래프의 고유 식별자로 사용되는 GUID이다.
        /// 그래프의 참조 및 관리를 위해 사용된다.
        /// </summary>
        public UGUID graphGuid
        {
            get { return _graphGuid; }

            internal set { _graphGuid = value; }
        }


        /// <summary> 메인 그래프를 나타내는 프로퍼티로, GraphAsset 내에서 주요 작업에 사용된다. </summary>
        /// <value>Graph</value>
        public Graph main
        {
            get { return _main; }

            set { _graphMap[value.guid] = (_main = value); }
        }


        /// <summary> 그래프 간의 관계 및 GUID 관리에 사용되는 딕셔너리이다. </summary>
        internal UGUIDDictionary graphMap
        {
            get { return _graphTreeMap; }

            set { _graphTreeMap = value; }
        }


        /// <summary> 그래프 자산에 포함된 모든 그래프의 컬렉션을 반환한다. </summary>
        /// <value> 포함된 그래프의 값을 나타내는 컬렉션. </value>
        public GraphDictionary.ValueCollection graphs
        {
            get { return _graphMap.Values; }
        }


        /// <summary> 그래프를 런타임용으로 복제한다. </summary>
        /// <param name="streamer"> 그래프가 런타임에 필요한 객체들을 위해 그래프를 실행시키는 TaskStreamer를 매개변수로 받는다. </param>
        /// <param name="runtimeData"> 런타임 시 사용할 블랙보드 데이터를 매개변수로 받는다. </param>
        /// <returns> 복제된 그래프를 반환한다. </returns>
        internal GraphAsset Clone(TaskStreamer streamer, BlackboardData runtimeData)
        {
            Debug.Assert(PropertyBag.Exists<GraphAsset>(), "GraphAsset does not have a property bag.");

            GraphAsset newGraphAsset = Object.Instantiate(this);
            BlackboardAsset newBlackboard = null;

            //블랙보드가 없어도 동작해야 되기 때문에.
            if (this.blackboard != null)
            {
                //이거 순서 잘 지켜야 된다. 복사 이후 대입임, 대입 이후 복사하면 모든 런타임용 블랙보드 값이 바뀌니 주의.
                newBlackboard = Object.Instantiate(this.blackboard);
                newBlackboard.ChangeBlackboardData(runtimeData); //대입 1
                newGraphAsset.blackboard = newBlackboard;        //대입 2
            }

            IPropertyBag<GraphAsset> bag = PropertyBag.GetPropertyBag<GraphAsset>();

            GraphContext context = new GraphContext(newGraphAsset, newBlackboard, streamer);
            bag.Accept(new GraphRuntimeInitializeVisitor(context), ref newGraphAsset);
            return newGraphAsset;
        }


        /// <summary> 지정된 GUID에 해당하는 서브 그래프 목록을 반환한다. </summary>
        /// <param name="baseGraphGuid"> 서브 그래프들을 식별할 기준이 되는 그래프의 GUID. </param>
        /// <returns> 기준 그래프의 서브 그래프 목록을 반환하거나, 기준 GUID가 비어있거나 데이터가 없을 경우 null을 반환한다. </returns>
        internal List<Graph> GetSubGraphs(UGUID baseGraphGuid)
        {
            if (baseGraphGuid.IsEmpty() || _graphTreeMap.TryGetValue(baseGraphGuid, out List<UGUID> subGraphGuids) == false)
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


        /// <summary> 지정된 UGUID에 해당하는 그래프를 반환한다. </summary>
        /// <param name="graphGuid"> 검색할 그래프의 UGUID. </param>
        /// <returns> UGUID에 해당하는 그래프를 반환하거나, 없으면 null을 반환한다. </returns>
        internal Graph GetGraph(UGUID graphGuid)
        {
            if (_graphMap.TryGetValue(graphGuid, out Graph graph))
            {
                return graph;
            }

            Debug.Log($"GetGraph: {graphGuid} is not found.");
            return null;
        }


        /// <summary> 두 GraphAsset 객체가 동일한지 비교한다. </summary>
        /// <param name="other"> 비교 대상이 되는 GraphAsset 객체. </param>
        /// <returns> 두 객체가 동일하다면 true, 그렇지 않으면 false를 반환한다. </returns>
        public bool Equals(GraphAsset other)
        {
            if (other is null)
            {
                return false;
            }

            if (_graphMap.Count != other._graphMap.Count)
            {
                return false;
            }

            if (this.main.guid != other.main.guid)
            {
                return false;
            }

            return ReferenceEquals(this, other) && ReferenceEquals(this.main, other.main);
        }


#if UNITY_EDITOR
        /// <summary> 모든 그래프 요소의 GUID를 새로 생성된 GUID로 재할당한다. </summary>
        internal void ReassignAllGraphElementGuids()
        {
            if (PropertyBag.Exists<GraphAsset>() == false)
            {
                Debug.LogError("GraphAsset does not have a property bag.");
                return;
            }

            GuidReassignmentVisitor visitor = new GuidReassignmentVisitor(new GraphContext(this));
            IPropertyBag<GraphAsset> bag = PropertyBag.GetPropertyBag<GraphAsset>();
            GraphAsset reference = this;
            bag.Accept(visitor, ref reference);
            this.graphGuid = UGUID.Create();
        }


        /// <summary> 그래프의 변수 중 현재 Blackboard에 없는 변수들을 정리한다. </summary>
        internal void TrySynchronizeVariablesOfNodes()
        {
            Debug.Assert(PropertyBag.Exists<GraphAsset>(), "GraphAsset does not have a property bag.");

            BlackboardSyncVisitor visitor = new BlackboardSyncVisitor(new GraphContext(this, blackboard));
            IPropertyBag<GraphAsset> bag = PropertyBag.GetPropertyBag<GraphAsset>();
            GraphAsset reference = this;
            bag.Accept(visitor, ref reference);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                EditorUtility.SetDirty(this);
            }
        }


        /// <summary> 서브 그래프를 추가한다. </summary>
        /// <param name="baseGuid"> 기준 그래프의 GUID. </param>
        /// <param name="graph"> 추가할 서브 그래프 객체. </param>
        internal void AddSubGraph(UGUID baseGuid, Graph graph)
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


        /// <summary> 그래프와 해당 그래프의 모든 하위 그래프를 삭제한다. </summary>
        /// <param name="baseGuid"> 제거 대상 그래프의 부모 그래프 GUID. </param>
        /// <param name="graph"> 제거할 그래프 객체. </param>
        internal void RemoveSubGraph(UGUID baseGuid, Graph graph)
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


        /// <summary> 그래프 간 종속성을 추가한다. </summary>
        /// <param name="from"> 종속성을 추가할 기준이 되는 그래프의 GUID. </param>
        /// <param name="to"> 기준 그래프에 종속될 대상 그래프의 GUID. </param>
        private void AddSubGraphGuid(UGUID from, UGUID to)
        {
            if (from.IsEmpty() || to.IsEmpty())
            {
                Debug.LogError("Cannot add dependency with empty GUIDs.");
                return;
            }

            if (_graphTreeMap.ContainsKey(from) == false)
            {
                _graphTreeMap.Add(from, new List<UGUID>());
            }

            if (_graphTreeMap[from].Contains(to) == false)
            {
                _graphTreeMap[from].Add(to);
            }
        }


        /// <summary> 그래프 간의 의존성을 제거한다. </summary>
        /// <param name="from"> 부모 그래프의 GUID. </param>
        /// <param name="to"> 제거할 자식 그래프의 GUID. </param>
        private void RemoveSubGraphGuid(UGUID from, UGUID to)
        {
            if (from.IsEmpty() || to.IsEmpty())
            {
                Debug.LogError("Cannot remove dependency with empty GUIDs.");
                return;
            }

            bool found = _graphTreeMap.TryGetValue(from, out List<UGUID> dependencies);

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