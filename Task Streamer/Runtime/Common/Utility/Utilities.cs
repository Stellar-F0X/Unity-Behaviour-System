using System;
using System.Text.RegularExpressions;
using TaskStreamer.BT;
using TaskStreamer.FSM;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TaskStreamer.Utility
{
    public static class Utilities
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
                throw new ArgumentException($"{typeof(Utilities)}: NodeName is null or empty");
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
                throw new ArgumentException($"{typeof(Utilities)}: NodeType is not NodeBase");
            }

            if ((ScriptableObject.CreateInstance(nodeType) as Object) is not NodeBase newNode)
            {
                throw new Exception($"{typeof(Utilities)}: Failed to create node of type {nodeType}");
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
                throw new ArgumentException($"{typeof(Utilities)}: Transition already exists.");
            }

            Transition newTransition = ScriptableObject.CreateInstance<Transition>();
            newTransition.hideFlags = HideFlags.HideInHierarchy;
            newTransition.name = $"{from.guid}.{to.guid}";
            newTransition.Setup(from, to);
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


        public static ConditionModule CreateConditionModule(Type type)
        {
            ConditionModule module = Activator.CreateInstance(type) as ConditionModule;
            Debug.Assert(module is not null, "Failed to create a condition module.");
            return module;
        }


        public static void CreateGraph(GraphAsset asset, GraphType graphType, ref Graph graph, string graphName)
        {
            if (asset == null)
            {
                Debug.LogError($"{typeof(Utilities)}: GraphAsset is null.");
                return;
            }
            
            switch (graphType)
            {
                case GraphType.FSM: graph = StateMachine.CreateGraph(graphName, asset); break;

                case GraphType.BT: graph = BehaviorTree.CreateGraph(graphName, asset); break;
            }
            
            Debug.Assert(graph != null, "Failed to create a graph.");
        }
    }
}