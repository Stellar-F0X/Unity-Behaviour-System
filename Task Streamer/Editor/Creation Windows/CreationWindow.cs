using System;
using System.Collections.Generic;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public sealed class CreationWindow : ScriptableObject, ISearchWindowProvider, ICreationWindow
    {
        private readonly static List<CreationWindow> _Windows = new List<CreationWindow>();

        private string _windowTitle;
        private bool _needMainEntryGroup;
        
        private List<FactoryModule> _modules;
        private Delegate _createActionDelegate;
        
        public bool modulesIsEmpty
        {
            get { return _modules is null || _modules.Count == 0; }
        }


        public static ICreationWindow GetCreationWindow(string title, bool needMainEntryGroup = true)
        {
            if (string.IsNullOrEmpty(title))
            {
                Debug.LogError($"{nameof(CreationWindow)} Error : Title is empty");
                return null;
            }
            
            CreationWindow window = _Windows.Find(window => string.CompareOrdinal(title, window._windowTitle) == 0);

            if (window == null)
            {
                window = ScriptableObject.CreateInstance<CreationWindow>();
                window._modules = new List<FactoryModule>();
                window._needMainEntryGroup = needMainEntryGroup;
                window._windowTitle = title;
                _Windows.Add(window);
            }
            
            return window;
        }


        public void OpenWindow(Vector2 mousePosition, float width = 200, float height = 240)
        {
            if (TaskStreamerEditor.canEditGraph == false)
            {
                Debug.LogError($"{nameof(CreationWindow)} Error : CanEditGraph is false");
                return;
            }
        
            Vector2 screenPoint = GUIUtility.GUIToScreenPoint(mousePosition);
            
            SearchWindowContext context = new SearchWindowContext(screenPoint, width, height);
            
            SearchWindow.Open(context, this);
        }


        public ICreationWindow AddFactoryModule(FactoryModule module)
        {
            if (_modules is null)
            {
                _modules = new List<FactoryModule>();
            }

            if (_modules.Contains(module))
            {
                Debug.LogWarning($"{nameof(CreationWindow)} Warning : Component {module.title} already exists.");
                return null;
            }
            else
            {
                _modules.Add(module);
            }

            return this;
        }


        public ICreationWindow RemoveFactoryModule(FactoryModule module)
        {
            if (_modules is not null && _modules.Contains(module))
            {
                _modules.Remove(module);
            }
            else
            {
                Debug.LogWarning($"{nameof(CreationWindow)} Warning : Component {module.title} does not exist.");
                return null;
            }
            
            return this;
        }
        
        
        public void RegisterCreationCallbackOnce(Delegate callback)
        {
            this._createActionDelegate = null;
            this._createActionDelegate = callback;
        }


        public void UnregisterCreationCallbackOnce()
        {
            this._createActionDelegate = null;
        }


        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeList = new List<SearchTreeEntry>();

            if (_needMainEntryGroup)
            {
                searchTreeList.Add(new SearchTreeGroupEntry(new GUIContent(_windowTitle), 0));
            }

            if (_modules is not null && _modules.Count > 0)
            {
                foreach (FactoryModule component in _modules)
                {
                    searchTreeList.AddRange(this.CreateSearchTreeEntry(component));
                }
            }

            return searchTreeList;
        }


        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (searchTreeEntry.userData is (Type createType, FactoryModule factoryModule))
            {
                Vector2 objectPosition = this.CalculateMousePosition(context);
                factoryModule.sendCreationSignal?.Invoke(createType, objectPosition, _createActionDelegate);
                _createActionDelegate = null;
                return true;
            }

            Debug.LogError($"{nameof(CreationWindow)} Error : Entry is empty");
            return false;
        }


        private SearchTreeEntry[] CreateSearchTreeEntry(FactoryModule module)
        {
            Type[] typeList = null;
            
            if (module.tagetIsSubClass)
            {
                typeList = TypeCache.GetTypesDerivedFrom(module.targetType).OrderByNameAndFilterAbstracts();
            }
            else
            {
                typeList = new Type[1] { module.targetType };
            }
            
            SearchTreeEntry[] entries = new SearchTreeEntry[typeList.Length + 1];
            entries[0] = new SearchTreeGroupEntry(new GUIContent(module.title));
            entries[0].level = module.layer;

            for (int i = 1; i < entries.Length; ++i)
            {
                string typeName = Utilities.ApplySpacing(typeList[i - 1].Name);
                entries[i] = new SearchTreeEntry(new GUIContent(typeName))
                {
                    userData = (typeList[i - 1], module),
                    level = module.layer + 1
                };
            }

            return entries;
        }


        private Vector2 CalculateMousePosition(SearchWindowContext context)
        {
            if (TaskStreamerEditor.canEditGraph == false)
            {
                Debug.LogError($"{nameof(CreationWindow)} Error : CanEditGraph is false");
                return Vector2.zero;
            }

            TaskStreamerEditor editor = TaskStreamerEditor.Instance;

            Vector2 targetVector = context.screenMousePosition - editor.position.position;
            Vector2 mousePosition = editor.rootVisualElement.ChangeCoordinatesTo(editor.rootVisualElement.parent, targetVector);
            return editor.view.contentViewContainer.WorldToLocal(mousePosition);
        }
    }
}