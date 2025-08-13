using System;
using System.Collections.Generic;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer
{
#if UNITY_EDITOR
    [Serializable]
    public class NodeGroupData : NodeGroup.Friend, ISerializationCallbackReceiver
    {
        public NodeGroupData(string title, Vector2 position, NodeGroup group)
        {
            this.title = title;
            this.position = position;
            this._containedGroup = group;
            this._nodeGuidSet = new HashSet<UGUID>();
            this._nodeGuidList = new List<UGUID>();
        }

        public string title;
        public Vector2 position;

        [SerializeField]
        private List<UGUID> _nodeGuidList;
        private HashSet<UGUID> _nodeGuidSet;

        [SerializeField, DontCreateProperty, HideInInspector]
        private NodeGroup _containedGroup;


        public int containedNodeCount
        {
            get { return _nodeGuidList.Count; }
        }


        public bool Contains(UGUID nodeGuid)
        {
            return _nodeGuidSet.Contains(nodeGuid);
        }


        public void AddNodeToGroup(UGUID guid)
        {
            if (guid.IsEmpty() || _nodeGuidSet.Contains(guid))
            {
                return;
            }

            base.AddNodeToGroup(_containedGroup, () => _nodeGuidSet.Add(guid));
        }


        public void RemoveNodeFromGroup(UGUID guid)
        {
            if (guid.IsEmpty() || _nodeGuidSet.Contains(guid) == false)
            {
                return;
            }

            base.RemoveNodeFromGroup(_containedGroup, () => _nodeGuidSet.Remove(guid));
        }


        public void ChangeNodeGroupPosition(Vector2 newPosition)
        {
            base.ChangeNodeGroupPosition(_containedGroup, () => this.position = newPosition);
        }


        public void ChangeNodeGroupTitle(string newTitle)
        {
            if (string.IsNullOrEmpty(newTitle) || string.CompareOrdinal(newTitle, this.title) == 0)
            {
                return;
            }

            base.ChangeNodeGroupTitle(_containedGroup, () => this.title = newTitle);
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