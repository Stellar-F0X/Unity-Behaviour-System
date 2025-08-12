using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TaskStreamer
{
#if UNITY_EDITOR
    [Serializable]
    public partial class NodeGroup
    {
        public NodeGroup(GraphAsset graphAsset)
        {
            this._graphAsset = graphAsset;
            this._groupDataList = new List<NodeGroupData>();
        }


        [SerializeField, DontCreateProperty]
        private GraphAsset _graphAsset;

        [SerializeReference]
        private List<NodeGroupData> _groupDataList;


        public List<NodeGroupData> dataList
        {
            get { return _groupDataList; }
        }

        public GraphAsset graphAsset
        {
            get { return _graphAsset; }
        }


        public NodeGroupData CreateGroupData(string title, Vector2 position)
        {
            NodeGroupData newNodeGroupData = new NodeGroupData(title, position, this);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(_graphAsset, "Task Streamer (CreateGroup)");
            }

            _groupDataList.Add(newNodeGroupData);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                EditorUtility.SetDirty(_graphAsset);
            }

            return newNodeGroupData;
        }


        public void DeleteGroupData(NodeGroupData data)
        {
            if (data is null)
            {
                return;
            }

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(_graphAsset, "Task Streamer (RemoveGroup)");
            }

            _groupDataList.Remove(data);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                EditorUtility.SetDirty(_graphAsset);
            }
        }


        private void AddNodeToGroup(Action addAction)
        {
            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(_graphAsset, "Task Streamer (AddNodeGuidToGroup)");
            }

            addAction.Invoke();

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                EditorUtility.SetDirty(_graphAsset);
            }
        }


        private void RemoveNodeFromGroup(Action removeAction)
        {
            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(_graphAsset, "Task Streamer (RemoveNodeGuidToGroup)");
            }

            removeAction.Invoke();

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                EditorUtility.SetDirty(_graphAsset);
            }
        }


        private void ChangeNodeGroupPosition(Action moveAction)
        {
            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(_graphAsset, "Task Streamer (NodeGroupViewPositionChanged)");
            }

            moveAction.Invoke();

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                EditorUtility.SetDirty(_graphAsset);
            }
        }


        private void ChangeNodeGroupTitle(Action renameAction)
        {
            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(_graphAsset, "Task Streamer (NodeGroupViewTitleChanged)");
            }

            renameAction.Invoke();

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                EditorUtility.SetDirty(_graphAsset);
            }
        }
    }
#endif
}