using System;
using System.Text.RegularExpressions;
using TaskStreamer.BT;
using TaskStreamer.FSM;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TaskStreamer.Utility
{
    public static class TaskStreamerUtility
    {
        public static int StringToHash(in string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Blackboard key cannot be null or empty.");
                return -1;
            }

            return Animator.StringToHash(key);
        }


        public static string ApplySpacing(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName))
            {
                throw new ArgumentException($"{typeof(TaskStreamerUtility)}: NodeName is null or empty");
            }

            if (nodeName.EndsWith("Node"))
            {
                nodeName = nodeName.Replace("Node", string.Empty);
            }
            
            if (nodeName.EndsWith("State"))
            {
                nodeName = nodeName.Replace("State", string.Empty);
            }

            return Regex.Replace(nodeName, "(?<=[a-z0-9])(?=[A-Z])", " ");
        }


        public static NodeBase CreateNode(Type nodeType, Vector2Int position = default)
        {
            if (typeof(NodeBase).IsAssignableFrom(nodeType) == false)
            {
                throw new ArgumentException($"{typeof(TaskStreamerUtility)}: NodeType is not NodeBase");
            }

            if ((ScriptableObject.CreateInstance(nodeType) as Object) is not NodeBase newNode)
            {
                throw new Exception($"{typeof(TaskStreamerUtility)}: Failed to create node of type {nodeType}");
            }

            newNode.guid = UGUID.Create();
            newNode.hideFlags = HideFlags.HideInHierarchy;
            newNode.name = ApplySpacing(nodeType.Name);
            newNode.position = position;
            return newNode;
        }
        
        
        public static Transition CreateTransition(StateBase from, StateBase to)
        {
            if (from.TryGetTransition(to.guid, out _))
            {
                throw new ArgumentException($"{typeof(TaskStreamerUtility)}: Transition already exists.");
            }

            Transition newTransition = ScriptableObject.CreateInstance<Transition>();
            newTransition.hideFlags = HideFlags.HideInHierarchy;
            newTransition.name = $"{from.guid}.{to.guid}";
            newTransition.Setup(from.guid, to.guid);
            return newTransition;
        }


        public static Variable CreateVariable(Type type, bool isLocal = false)
        {
            Debug.Assert(type is not null, "Failed to create a variable.");
            Variable newVariable = Activator.CreateInstance(type) as Variable;
            Debug.Assert(newVariable is not null, "Failed to create a variable.");

            newVariable.name = isLocal ? "#Constant Variable#" : $"New {type.Name}";
            newVariable.type = type;
            return newVariable;
        }
        
        
        public static void SetMainGraph(GraphAsset asset)
        {
            switch (asset.mainGraphType)
            {
                case GraphType.FSM: asset.main = StateMachine.CreateGraph("Main", asset);  break;
                
                case GraphType.BT:  asset.main = BehaviorTree.CreateGraph("Main", asset); break;
            }

#if UNITY_EDITOR
            if (asset.main != null)
            {
                UnityEditor.EditorUtility.SetDirty(asset);
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif
        }
    }
}