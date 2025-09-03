using UnityEditor;
using UnityEngine;

namespace TaskStreamer.Tool
{
    internal static class TaskStreamerMenuItems
    {
        [MenuItem("Assets/Create/Task Streamer/BT/C# Action Node", false, 11)]
        public static void CreateActionNodeMenuItem()
        {
            string path = EditorUtility.FindAssetPath("NewActionNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "New Action Node.cs");
        }


        [MenuItem("Assets/Create/Task Streamer/BT/C# Composite Node", false, 11)]
        public static void CreateCompositeNodeMenuItem()
        {
            string path = EditorUtility.FindAssetPath("NewCompositeNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "New Composite Node.cs");
        }


        [MenuItem("Assets/Create/Task Streamer/BT/C# Decorator Node", false, 11)]
        public static void CreateDecoratorNodeMenuItem()
        {
            string path = EditorUtility.FindAssetPath("NewDecoratorNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "New Decorator Node.cs");
        }


        [MenuItem("Assets/Create/Task Streamer/BT/C# Service", false, 11)]
        public static void CreateServiceMenuItem()
        {
            string path = EditorUtility.FindAssetPath("NewService.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "New Service.cs");
        }
        
        
        [MenuItem("Assets/Create/Task Streamer/FSM/C# State Node", false, 12)]
        public static void CreateStateNodeMenuItem()
        {
            string path = EditorUtility.FindAssetPath("NewActionState.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "New Action State.cs");
        }
        

        [MenuItem("Assets/Create/Task Streamer/C# Blackboard Variable Based Condition", false)]
        public static void CreateTransitionMenuItem()
        {
            string path = EditorUtility.FindAssetPath("NewCondition.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "New Condition.cs");
        }


        [MenuItem("Assets/Create/Task Streamer/C# Blackboard Variable")]
        public static void CreateVariableMenuItem()
        {
            string path = EditorUtility.FindAssetPath("NewBlackboardVariable.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "New Blackboard Variables.cs");
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
    }
}