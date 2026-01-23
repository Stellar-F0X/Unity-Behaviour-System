using System;
using System.Collections.Generic;
using TaskStreamer.Runtime.Utility;
using Unity.Properties;
using UnityEditor;
using UnityEngine;

namespace TaskStreamer.Runtime
{
#if UNITY_EDITOR
    [Serializable]
    public class NodeGroup : ISerializationCallbackReceiver
    {
        public NodeGroup(string title, Vector2 position, Graph graph)
        {
            this.title = title;
            this.position = position;
            this._graph = graph;
            this._nodeGuidSet = new HashSet<UGUID>();
            this._nodeGuidList = new List<UGUID>();
        }

        [DontCreateProperty]
        public string title;
        
        [DontCreateProperty]
        public Vector2 position;

        [SerializeField, DontCreateProperty]
        private List<UGUID> _nodeGuidList;
        private HashSet<UGUID> _nodeGuidSet;

        [SerializeReference, DontCreateProperty, HideInInspector]
        private Graph _graph;


        public bool Contains(UGUID nodeGuid)
        {
            return _nodeGuidSet.Contains(nodeGuid);
        }


        public void AddNodeToGroup(UGUID guid, bool recordUndo = true)
        {
            if (guid.IsEmpty() || _nodeGuidSet.Contains(guid))
            {
                return;
            }

            if (recordUndo && Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(_graph.graphAsset, "Task Streamer (AddNodeGuidToGroup)");
            }

            _nodeGuidSet.Add(guid);
            
            if (recordUndo && Application.isPlaying == false && Undo.isProcessing == false)
            {
                EditorUtility.SetDirty(_graph.graphAsset);
            }
        }


        public void RemoveNodeFromGroup(UGUID guid,  bool recordUndo = true)
        {
            if (guid.IsEmpty() || _nodeGuidSet.Contains(guid) == false)
            {
                return;
            }
            
            if (recordUndo && Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(_graph.graphAsset, "Task Streamer (RemoveNodeGuidToGroup)");
            }

            _nodeGuidSet.Remove(guid);
            
            if (recordUndo && Application.isPlaying == false && Undo.isProcessing == false)
            {
                EditorUtility.SetDirty(_graph.graphAsset);
            }
        }


        public void ChangeNodeGroupPosition(Vector2 newPosition)
        {
            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(_graph.graphAsset, "Task Streamer (NodeGroupViewPositionChanged)");
            }

            this.position = newPosition;
            
            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                EditorUtility.SetDirty(_graph.graphAsset);
            }
        }


        public void ChangeNodeGroupTitle(string newTitle)
        {
            if (string.IsNullOrEmpty(newTitle) || string.CompareOrdinal(newTitle, this.title) == 0)
            {
                return;
            }

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(_graph.graphAsset, "Task Streamer (NodeGroupViewTitleChanged)");
            }
            
            this.title = newTitle;
            
            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                EditorUtility.SetDirty(_graph.graphAsset);
            }
        }
        
        
        public void OnBeforeSerialize()
        {
            _nodeGuidList?.Clear();
            _nodeGuidList?.AddRange(_nodeGuidSet);
        }


        public void OnAfterDeserialize()
        {
            _nodeGuidSet?.Clear();
            _nodeGuidSet ??= new HashSet<UGUID>(_nodeGuidList);
            _nodeGuidList.ForEach(e => _nodeGuidSet.Add(e));
        }
    }
#endif
}