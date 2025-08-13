using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEditor;
using UnityEngine;

[assembly: InternalsVisibleTo("TaskStreamer.Tool"), GeneratePropertyBagsForAssembly]
namespace TaskStreamer
{
    [Serializable, GeneratePropertyBag]
    public abstract partial class Graph : IEquatable<Graph>, IGraphIterable
    {
        protected Graph(string graphName, GraphAsset graphAsset)
        {
            this.name = graphName;
            this.guid = UGUID.Create();
            this.graphAsset = graphAsset;
            this._nodeLookup = new NodeDictionary();
            this._nodeGroup = new List<NodeGroup>();
        }

#if UNITY_EDITOR
        [SerializeField]
        private List<NodeGroup> _nodeGroup; 
#endif 
        [SerializeField]
        protected NodeDictionary _nodeLookup; 
        
        /// <summary> Entry Node Guid </summary>
        [SerializeField, DontCreateProperty]
        private UGUID _entryGuid; 

        [SerializeField, DontCreateProperty]
        protected GraphAsset _graphAsset; 

        
        public GraphAsset graphAsset
        {
            get { return _graphAsset; }

            private set { _graphAsset = value; }
        }

#if UNITY_EDITOR
        public List<NodeGroup> nodeGroup
        {
            get { return _nodeGroup; }
        }
#endif

        public NodeBase entry 
        {
            get
            {
                Debug.Assert(! _entryGuid.IsEmpty(), "Entry guid is empty");
                return _nodeLookup[_entryGuid];
            }

            internal set
            {
                _entryGuid = value.guid;
                _nodeLookup[_entryGuid] = value;
            }
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

        public abstract GraphType graphType
        {
            get; 
        }


        public bool TryGetNodeByGuid(UGUID nodeGuid, out NodeBase node)
        {
            if (nodeGuid.IsEmpty())
            {
                Debug.LogError("GUID is empty");
                node = null;
                return false;
            }

            return _nodeLookup.TryGetValue(nodeGuid, out node);
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

        
        public bool Equals(Graph other)
        {
            if (other is null)
            {
                return false;
            }
            
            if (_entryGuid != other._entryGuid)
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


        public abstract IGraphIterator GetIterator(GraphIteratorType iteratorType);


        internal abstract Status UpdateGraph(); 


        internal abstract void ResetGraph(); 


        internal abstract void StopGraph(); 

        
#if UNITY_EDITOR

#region Group Data
        internal NodeGroup CreateGroupData(string title, Vector2 position)
        {
            NodeGroup newNodeGroupData = new NodeGroup(title, position, this);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(_graphAsset, "Task Streamer (CreateGroup)");
            }

            _nodeGroup.Add(newNodeGroupData);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                EditorUtility.SetDirty(_graphAsset);
            }

            return newNodeGroupData;
        }


        internal void DeleteGroupData(NodeGroup data)
        {
            if (data is null)
            {
                return;
            }

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(_graphAsset, "Task Streamer (RemoveGroup)");
            }

            _nodeGroup.Remove(data);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                EditorUtility.SetDirty(_graphAsset);
            }
        }
        
        
        /// <summary> Regenerates unique GUIDs for all nodes within the graph. </summary>
        /// <exception cref="Exception"> Thrown when GUID regeneration fails for any node. </exception>
        internal void RegenerateAllNodeGuids()
        {
            List<UGUID> keyList = _nodeLookup.Keys.ToList();

            foreach (UGUID keyGuid in keyList)
            {
                NodeBase node = _nodeLookup[keyGuid];

                if (node is null)
                {
                    throw new Exception($"Node with GUID {keyGuid} is null");
                }

                UGUID newGuid = UGUID.Create();

                NodeGroup group = _nodeGroup.Find(e => e.Contains(keyGuid));

                if (group is not null)
                {
                    group.RemoveNodeFromGroup(keyGuid, false);
                    group.AddNodeToGroup(newGuid, false);
                }
                
                _nodeLookup.Remove(keyGuid);
                node.guid = newGuid;
                _nodeLookup.Add(newGuid, node);
            }
        }
#endregion


#region Sub Graph
        internal void AddSubGraph(Graph subGraph) 
        {
            Undo.RecordObject(this.graphAsset, "Task Streamer (AddSubGraph)"); 

            this.graphAsset.AddSubGraph(this.guid, subGraph); 
        } 


        internal void RemoveSelfAndSubGraphs()
        {
            Undo.RecordObject(this.graphAsset, "Task Streamer (RemoveSubGraph)");
            
            this.graphAsset.RemoveSubGraph(this.baseGraphGuid, this);
            
            this.OnRemoveGraph();

            EditorUtility.SetDirty(this.graphAsset);
            AssetDatabase.SaveAssets();
        }
#endregion


#region Graph
        internal abstract void OnRemoveGraph(); 
        
        
        /// <summary> Create And Insert To Node List </summary>
        /// <param name="nodeName">생성할 노드의 이름</param>
        /// <param name="nodeType">생성할 노드의 타입</param>
        /// <param name="position">노드의 위치 (기본값: default)</param>
        /// <returns>생성된 NodeBase 객체</returns>
        /// <exception cref="Exception">노드 생성 실패 시 발생</exception>
        internal NodeBase CreateNode(string nodeName, Type nodeType, Vector2Int position = default)
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


        internal void DeleteNode(NodeBase node, bool record = true)
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
#endregion
        
#endif
    }
}