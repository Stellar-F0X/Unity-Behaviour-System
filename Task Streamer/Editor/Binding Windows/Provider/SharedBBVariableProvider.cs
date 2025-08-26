using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class SharedBBVariableProvider : ICategoryTreeProvider
    {
        public SearchTreeEntry[] ProvideCategories(FactoryModule module)
        {
            Debug.Assert(TaskStreamerEditor.canEditGraph == false, "Cannot edit graph");
            BlackboardAsset blackboard = TaskStreamerEditor.Instance.graphAsset.blackboard;
            Debug.Assert(blackboard != null, "");
            
            SearchTreeEntry[] entries = new SearchTreeEntry[blackboard.count + 1];
            entries[0] = new SearchTreeGroupEntry(new GUIContent(module.title));
            entries[0].level = module.layer;

            for (int i = 1; i < entries.Length; ++i)
            {
                BlackboardVariable bbVariable = blackboard.variables[i];
                entries[i] = new SearchTreeEntry(new GUIContent(bbVariable.key));
                entries[i].userData = (bbVariable.type, module);
                entries[i].level = module.layer + 1;
            }

            return entries;
        }
    }
}