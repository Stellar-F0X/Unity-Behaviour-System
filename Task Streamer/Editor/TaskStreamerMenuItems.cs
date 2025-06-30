using TaskStreamer.BT;
using TaskStreamer.FSM;
using UnityEditor;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public static class TaskStreamerMenuItems
    {
        [MenuItem("Assets/Create/Task Streamer/BT/Action Node", false, 1)]
        public static void CreateActionNodeMenuItem()
        {
            string path = EditorHelper.FindAssetPath("NewActionNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewActionNode.cs");
        }


        [MenuItem("Assets/Create/Task Streamer/BT/Composite Node", false, 1)]
        public static void CreateCompositeNodeMenuItem()
        {
            string path = EditorHelper.FindAssetPath("NewCompositeNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewCompositeNode.cs");
        }


        [MenuItem("Assets/Create/Task Streamer/BT/Decorator Node", false, 1)]
        public static void CreateDecoratorNodeMenuItem()
        {
            string path = EditorHelper.FindAssetPath("NewDecoratorNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewDecoratorNode.cs");
        }


        [MenuItem("Assets/Create/Task Streamer/FSM/State Node", false, 2)]
        public static void CreateStateNodeMenuItem()
        {
            string path = EditorHelper.FindAssetPath("NewStateNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewStateNode.cs");
        }


        [MenuItem("Assets/Create/Task Streamer/FSM/Transition", false, 1)]
        public static void CreateTransitionMenuItem()
        {
            string path = EditorHelper.FindAssetPath("NewTransition.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "Compare__Type__Variables.cs");
        }


        [MenuItem("Assets/Create/Task Streamer/Variable")]
        public static void CreateVariableMenuItem()
        {
            string path = EditorHelper.FindAssetPath("NewVariable.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "__Type__Variables.cs");
        }


        [MenuItem("Assets/Create/Task Streamer/FSM/Graph Asset", false, 0)]
        public static void CreateFSMGraphAsset()
        {
            GraphAsset asset = ScriptableObject.CreateInstance<GraphAsset>();
            asset.mainGraphType = EGraphType.FSM;
            ProjectWindowUtil.CreateAsset(asset, "New FSM Graph.asset");
        }


        [MenuItem("Assets/Create/Task Streamer/BT/Graph Asset", false, 0)]
        public static void CreateBTGraphAsset()
        {
            GraphAsset asset = ScriptableObject.CreateInstance<GraphAsset>();
            asset.mainGraphType = EGraphType.BT;
            ProjectWindowUtil.CreateAsset(asset, "New BT Graph.asset");
        }


        [MenuItem("Assets/Create/Task Streamer/Blackboard")]
        public static void CreateBlackboard()
        {
            Blackboard asset = ScriptableObject.CreateInstance<Blackboard>();
            ProjectWindowUtil.CreateAsset(asset, "New Blackboard Asset.asset");
        }
    }
}