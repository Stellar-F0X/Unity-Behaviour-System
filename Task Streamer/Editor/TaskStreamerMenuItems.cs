using UnityEditor;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public static class TaskStreamerMenuItems
    {
        [MenuItem("Assets/Create/Task Streamer/BT/Action Node", false, 1)]
        public static void CreateActionNodeMenuItem()
        {
            string path = EditorUtilities.FindAssetPath("NewActionNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewActionNode.cs");
        }


        [MenuItem("Assets/Create/Task Streamer/BT/Composite Node", false, 1)]
        public static void CreateCompositeNodeMenuItem()
        {
            string path = EditorUtilities.FindAssetPath("NewCompositeNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewCompositeNode.cs");
        }


        [MenuItem("Assets/Create/Task Streamer/BT/Decorator Node", false, 1)]
        public static void CreateDecoratorNodeMenuItem()
        {
            string path = EditorUtilities.FindAssetPath("NewDecoratorNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewDecoratorNode.cs");
        }


        [MenuItem("Assets/Create/Task Streamer/FSM/State Node", false, 2)]
        public static void CreateStateNodeMenuItem()
        {
            string path = EditorUtilities.FindAssetPath("NewStateNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewStateNode.cs");
        }


        [MenuItem("Assets/Create/Task Streamer/FSM/Transition", false, 1)]
        public static void CreateTransitionMenuItem()
        {
            string path = EditorUtilities.FindAssetPath("NewTransition.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "Compare__Type__Variables.cs");
        }


        [MenuItem("Assets/Create/Task Streamer/Variable")]
        public static void CreateVariableMenuItem()
        {
            string path = EditorUtilities.FindAssetPath("NewVariable.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "__Type__Variables.cs");
        }


        [MenuItem("Assets/Create/Task Streamer/FSM/Graph Asset", false, 0)]
        public static void CreateFSMGraphAsset()
        {
            GraphAsset asset = ScriptableObject.CreateInstance<GraphAsset>();
            asset.mainGraphType = GraphType.FSM;
            ProjectWindowUtil.CreateAsset(asset, "New FSM Graph.asset");
        }


        [MenuItem("Assets/Create/Task Streamer/BT/Graph Asset", false, 0)]
        public static void CreateBTGraphAsset()
        {
            GraphAsset asset = ScriptableObject.CreateInstance<GraphAsset>();
            asset.mainGraphType = GraphType.BT;
            ProjectWindowUtil.CreateAsset(asset, "New BT Graph.asset");
        }


        [MenuItem("Assets/Create/Task Streamer/Blackboard")]
        public static void CreateBlackboard()
        {
            BlackboardAsset asset = ScriptableObject.CreateInstance<BlackboardAsset>();
            ProjectWindowUtil.CreateAsset(asset, "New Blackboard Asset.asset");
        }
    }
}