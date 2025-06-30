using System;
using System.Runtime.CompilerServices;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEditor;
using UnityEngine;

[assembly: InternalsVisibleTo("TaskStreamer.Tool"), GeneratePropertyBagsForAssembly]
namespace TaskStreamer
{
    [Serializable, GeneratePropertyBag]
    public abstract class Graph : IEquatable<Graph>, IGraphIterable
    {
        protected Graph(string graphName, GraphAsset graphAsset)
        {
            this.name = graphName;
            this.guid = UGUID.Create();
            this.graphAsset = graphAsset;
            this._nodeLookup = new NodeDictionary();
        }

#if UNITY_EDITOR
        [SerializeField]
        private NodeGroup _nodeGroup;
#endif

        [SerializeField, DontCreateProperty]
        private NodeBase _entry;

        [SerializeField]
        protected NodeDictionary _nodeLookup;

        [SerializeField, DontCreateProperty]
        protected GraphAsset _graphAsset;


#region Properties

        public GraphAsset graphAsset
        {
            get { return _graphAsset; }

            private set { _graphAsset = value; }
        }

#if UNITY_EDITOR
        public NodeGroup nodeGroup
        {
            get { return _nodeGroup; }

            internal set { _nodeGroup = value; }
        }
#endif

        public NodeBase entry //TODO: 그냥 GUID로 nodeDictionary로 가져오는 방법은 안되는지 확인.
        {
            get { return _entry; }

            internal set { _entry = value; }
        }

        public int count
        {
            get { return _nodeLookup.Count; }
        }

        [field: SerializeField]
        public UGUID guid
        {
            get;
            private set;
        }

        [field: SerializeField]
        public UGUID baseGraphGuid
        {
            get;
            set;
        }

        [field: SerializeField]
        public string name
        {
            get;
            set;
        }

        public abstract EGraphType graphType
        {
            get;
        }
#endregion


        public bool TryGetNodeByGuid(UGUID guid, out NodeBase node)
        {
            if (guid.IsEmpty())
            {
                Debug.LogError("GUID is empty");
                node = null;
                return false;
            }

            return _nodeLookup.TryGetValue(guid, out node);
        }


        public NodeBase GetNodeByGuid(UGUID nodeGuid)
        {
            if (this.TryGetNodeByGuid(nodeGuid, out NodeBase node))
            {
                return node;
            }

            Debug.LogError("GUID is empty");
            return null;
        }


#if UNITY_EDITOR
        internal void AddSubGraph(Graph subGraph)
        {
            this.graphAsset.AddSubGraph(this.guid, subGraph);
        }


        internal void RemoveSelfAndSubGraphs()
        {
            this.graphAsset.RemoveSubGraph(this.baseGraphGuid, this);

            foreach (NodeBase node in this._nodeLookup.Values) //TODO: 문제가 생길 수도 있나?
            {
                this.DeleteNode(node);
            }
            
            EditorUtility.SetDirty(this.graphAsset);
            AssetDatabase.SaveAssets(); 
        }
#endif


        public bool Equals(Graph other)
        {
            if (other is null)
            {
                return false;
            }

            if (this._nodeLookup.Values != other._nodeLookup.Values)
            {
                return false;
            }

            return ReferenceEquals(this, other);
        }


        internal abstract void InitializeOnEnterRuntime(TaskStreamer streamer);


        public abstract IGraphIterator GetGraphIterator();


        public abstract EStatus UpdateGraph();


        public abstract void ResetGraph();


        public abstract void StopGraph();


#if UNITY_EDITOR
        /// <summary> Create And Insert To Node List </summary>
        /// <param name="nodeName">생성할 노드의 이름</param>
        /// <param name="nodeType">생성할 노드의 타입</param>
        /// <param name="position">노드의 위치 (기본값: default)</param>
        /// <returns>생성된 NodeBase 객체</returns>
        /// <exception cref="Exception">노드 생성 실패 시 발생</exception>
        public NodeBase CreateNode(string nodeName, Type nodeType, Vector2Int position = default)
        {
            // GraphGroup에 변경사항 기록
            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(this.graphAsset, "Task Streamer (CreateNode)");
            }

            NodeBase node = TaskStreamerUtility.CreateNode(nodeType, position);

            if (node is null)
            {
                throw new Exception("Node is null");
            }

            node.name = TaskStreamerUtility.ApplySpacing(nodeName);
            _nodeLookup.Add(node.guid, node);
            _nodeLookup.OnBeforeSerialize();

            // Node를 GraphAsset의 sub-asset으로 추가
            if (Application.isPlaying == false && AssetDatabase.Contains(this.graphAsset))
            {
                AssetDatabase.AddObjectToAsset(node, this.graphAsset);
            }

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RegisterCreatedObjectUndo(node, "Task Streamer (CreateNode)");
                EditorUtility.SetDirty(this.graphAsset);
                AssetDatabase.SaveAssets(); // 중요: 즉시 저장
            }

            return node;
        }


        public void DeleteNode(NodeBase node, bool record = true)
        {
            if (Application.isPlaying == false && Undo.isProcessing == false && record)
            {
                Undo.RecordObject(this.graphAsset, "Task Streamer (DeleteNode)");
            }

            _nodeLookup.Remove(node.guid);

            // Sub-asset에서도 제거
            if (Application.isPlaying == false && AssetDatabase.Contains(node))
            {
                AssetDatabase.RemoveObjectFromAsset(node);
            }

            if (Application.isPlaying == false && Undo.isProcessing == false && record)
            {
                Undo.DestroyObjectImmediate(node);
                EditorUtility.SetDirty(this.graphAsset);
                AssetDatabase.SaveAssets(); // 중요: 즉시 저장
            }
        }
#endif
    }
}