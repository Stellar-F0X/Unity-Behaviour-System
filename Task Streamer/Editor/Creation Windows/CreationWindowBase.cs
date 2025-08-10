using System;
using System.Collections.Generic;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public abstract class CreationWindowBase : ScriptableObject, ISearchWindowProvider
    {
        protected TaskGraphView graphView
        {
            get { return TaskStreamerEditor.Instance.view; }
        }
        
        
        protected Vector2 CalculateMousePosition(SearchWindowContext context)
        {
            if (TaskStreamerEditor.canEditGraph == false)
            {
                Debug.LogError($"{nameof(TaskCreationWindowBase)} Error : CanEditGraph is false");
                return Vector2.zero;
            }

            TaskStreamerEditor editor = TaskStreamerEditor.Instance;

            Vector2 targetVector = context.screenMousePosition - editor.position.position;
            Vector2 mousePosition = editor.rootVisualElement.ChangeCoordinatesTo(editor.rootVisualElement.parent, targetVector);
            return editor.view.contentViewContainer.WorldToLocal(mousePosition);
        }

        
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (searchTreeEntry.userData is Action entryIAction)
            {
                entryIAction.Invoke();
                return true;
            }

            Debug.LogError($"{nameof(TaskCreationWindowBase)} Error : Entry is empty");
            return false;
        }
        
        
        protected SearchTreeEntry[] CreateSearchTreeEntry<TBase>(string title, Action<Type> invoke, int layerLevel = 1)
        {
            Type[] typeList = TypeCache.GetTypesDerivedFrom<TBase>().OrderByNameAndFilterAbstracts();
            SearchTreeEntry[] entries = new SearchTreeEntry[typeList.Length + 1];
            entries[0] = new SearchTreeGroupEntry(new GUIContent(title));
            entries[0].level = layerLevel;

            for (int i = 1; i < entries.Length; ++i)
            {
                Type targetType = typeList[i - 1];
                string typeName = TaskStreamerUtility.ApplySpacing(targetType.Name);

                entries[i] = new SearchTreeEntry(new GUIContent(typeName))
                {
                    userData = (Action)(() => invoke.Invoke(targetType)),
                    level = layerLevel + 1
                };
            }

            return entries;
        }
        
        
        public abstract List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context);
    }
}