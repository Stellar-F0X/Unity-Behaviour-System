using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TaskStreamer
{
#if UNITY_EDITOR
    public class NodeGroup
    {
        [SerializeField]
        private List<NodeGroupData> _groupDataList = new List<NodeGroupData>();


        public IReadOnlyList<NodeGroupData> dataList
        {
            get { return _groupDataList; }
        }
        
        
        public NodeGroupData CreateGroupData(string title, Vector2 position)
        {
            NodeGroupData newNodeGroupData = ScriptableObject.CreateInstance<NodeGroupData>();
            newNodeGroupData.hideFlags = HideFlags.HideInHierarchy;
            newNodeGroupData.title = title;
            newNodeGroupData.position =  position;

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(newNodeGroupData, "Task Streamer (CreateGroup)");
            }

            _groupDataList.Add(newNodeGroupData);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RegisterCreatedObjectUndo(newNodeGroupData, "Task Streamer (CreateGroup)");
                EditorUtility.SetDirty(newNodeGroupData);
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
                Undo.RecordObject(data, "Task Streamer (RemoveGroup)");
            }

            _groupDataList.Remove(data);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.DestroyObjectImmediate(data);
                EditorUtility.SetDirty(data);
            }
        }
    }
#endif
}