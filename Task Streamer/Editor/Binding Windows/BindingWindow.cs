using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public sealed class BindingWindow : ScriptableObject, ISearchWindowProvider
    {
        public string windowTitle;
        
        public bool useHeadEntry;
        
        public List<FactoryModule> modules;
        
        private Delegate _creationCallback = new Action(delegate { });

        
        
        public bool modulesIsEmpty
        {
            get { return modules is null || modules.Count == 0; }
        }


        public void OpenWindow(Vector2 mousePosition, float width = 200, float height = 240)
        {
            if (TaskStreamerEditor.canEditGraph == false)
            {
                Debug.LogError($"{nameof(BindingWindow)} Error : CanEditGraph is false");
                return;
            }
        
            Vector2 screenPoint = GUIUtility.GUIToScreenPoint(mousePosition);
            
            SearchWindowContext context = new SearchWindowContext(screenPoint, width, height);
            
            SearchWindow.Open(context, this);
        }
        
        
        public void RegisterCreationCallbackOnce(Delegate callback)
        {
            this._creationCallback = null;
            this._creationCallback = callback;
        }


        public void UnregisterCreationCallbackOnce()
        {
            this._creationCallback = null;
        }


        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeList = new List<SearchTreeEntry>();

            if (useHeadEntry)
            {
                searchTreeList.Add(new SearchTreeGroupEntry(new GUIContent(windowTitle), 0));
            }

            if (modules is null || modules.Count <= 0)
            {
                return searchTreeList;
            }

            foreach (FactoryModule module in modules)
            {
                searchTreeList.AddRange(module.categoryProvider.ProvideCategories(module));
            }

            return searchTreeList;
        }


        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            if (entry.userData is (Type createType, FactoryModule factoryModule))
            {
                Vector2 position = this.CalculateMousePosition(context);
                factoryModule.onTryCreate?.Invoke(createType, position, entry.name, _creationCallback);
                _creationCallback = null;
                return true;
            }

            Debug.LogError($"{nameof(BindingWindow)} Error : Entry is empty");
            return false;
        }


        private Vector2 CalculateMousePosition(SearchWindowContext context)
        {
            if (TaskStreamerEditor.canEditGraph == false)
            {
                Debug.LogError($"{nameof(BindingWindow)} Error : CanEditGraph is false");
                return Vector2.zero;
            }

            TaskStreamerEditor editor = TaskStreamerEditor.Instance;

            Vector2 targetVector = context.screenMousePosition - editor.position.position;
            Vector2 mousePosition = editor.rootVisualElement.ChangeCoordinatesTo(editor.rootVisualElement.parent, targetVector);
            return editor.taskGraphView.contentViewContainer.WorldToLocal(mousePosition);
        }
    }
}