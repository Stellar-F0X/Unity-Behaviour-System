using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public class TaskStreamerEditor : EditorWindow
    {
        private static EditorSettings _settings;

        private GraphAsset _graphAsset;

        private Graph _focusedGraph;

        private MiniMapView _miniMapView;

        private InspectorView _inspectorView;

        private TaskGraphView _graphView;

        //private NodeSearchFieldView _nodeSearchField;

        private BlackboardView _blackboardView;

        private GraphBreadcrumbs _graphBreadcrumbs;


        /// <summary>Behaviour Tree 에디터 설정 정보를 가져옵니다.</summary>
        public static EditorSettings settings
        {
            get { return _settings ??= TaskStreamerEditor.FindSettings(); }
        }


        /// <summary>현재 활성화된 Behaviour Tree 에디터 인스턴스를 가져옵니다.</summary>
        public static TaskStreamerEditor Instance
        {
            get;
            private set;
        }


        /// <summary>현재 트리를 편집할 수 있는지 여부를 나타냅니다.</summary>
        public static bool canEditGraph
        {
            get;
            private set;
        }


        /// <summary>Behaviour Tree 에셋을 Editor View에 로딩 중인지 여부를 나타냅니다.</summary>
        public static bool isLoadingTreeToView
        {
            get;
            private set;
        }


        /// <summary>현재 Behaviour Tree 뷰를 가져옵니다.</summary>
        public TaskGraphView view
        {
            get { return _graphView; }
        }


        /// <summary>현재 편집 중인 Behaviour Tree를 가져옵니다.</summary>
        public GraphAsset graphAsset
        {
            get { return _graphAsset; }
        }

        public Graph currentGraph
        {
            get { return _focusedGraph; }
            set { _focusedGraph = value; }
        }


        /// <summary>Unity 메뉴에서 Behaviour Tree 에디터 창을 엽니다.</summary>
        [MenuItem("Tools/Task Streamer")]
        public static void OpenWindow()
        {
            TaskStreamerEditor wnd = GetWindow<TaskStreamerEditor>();
            wnd.titleContent = new GUIContent("Task Streamer");
            Instance = wnd;
        }
        
        
        /// <summary>Behaviour Tree 에셋을 더블클릭했을 때 에디터를 엽니다.</summary>
        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            if (Selection.activeObject is GraphAsset graphAsset)
            {
                TaskStreamerEditor.OpenWindow(graphAsset);
                return true;
            }

            return false;
        }


        public static void OpenWindow(GraphAsset graphAsset)
        {
            if (Instance != null)
            {
                Instance.Focus();
            }
            
            //Call CreateGUI
            TaskStreamerEditor.OpenWindow();

            if (Instance._graphAsset != graphAsset)
            {
                Instance._graphAsset = graphAsset;
                Instance.ChangeGraph(graphAsset.main);
            }
        }



        private static EditorSettings FindSettings()
        {
            EditorSettings foundSettings = EditorUtilities.FindAssetByName<EditorSettings>($"t:{nameof(EditorSettings)}");

            Debug.Assert(foundSettings is not null, "EditorSettings asset not found.");

            return foundSettings;
        }



        /// <summary>에디터를 초기 상태로 초기화합니다.</summary>
        private void Initialize()
        {
            isLoadingTreeToView = false;
            canEditGraph = false;

            this._focusedGraph = null;
            this._graphAsset = null;
            
            this._inspectorView?.Clear();
            this._graphView?.ClearEditorView();
            this._blackboardView?.ClearBlackboardView();
        }


        /// <summary>에디터 UI를 생성하고 초기화합니다.</summary>
        private void CreateGUI()
        {
            Instance = this;

            Debug.Assert(rootVisualElement is not null, "Root Visual Element is null.");
            settings.editorXml.CloneTree(rootVisualElement);

            Debug.Assert(settings.editorXml is not null, "EditorXml is null.");
            rootVisualElement.styleSheets.Add(settings.editorStyle);

            _graphView = rootVisualElement.Q<TaskGraphView>();
            _miniMapView = rootVisualElement.Q<MiniMapView>();
            _inspectorView = rootVisualElement.Q<InspectorView>();
            //_nodeSearchField = rootVisualElement.Q<NodeSearchFieldView>();
            _blackboardView = rootVisualElement.Q<BlackboardView>();
            _graphBreadcrumbs = rootVisualElement.Q<GraphBreadcrumbs>();

            var elementAddButton = rootVisualElement.Q<Button>("element-add-button");
            var minimapActivateButton = rootVisualElement.Q<ToolbarToggle>("active-minimap");
            var blackboardBindField = rootVisualElement.Q<ObjectField>("blackboard-field");

            _blackboardView.Setup(elementAddButton, blackboardBindField);
            _miniMapView.Setup(minimapActivateButton, _graphView);
            //_nodeSearchField.Setup(_inspectorView, _graphView);

            _graphView.onElementSelected -= _inspectorView.UpdateSelection;
            _graphView.onElementSelected += _inspectorView.UpdateSelection;

            _graphView.onElementUnselected = null;
            _graphView.onElementUnselected += _ => _inspectorView.ClearInspectorView();

            this.OnSelectionChange();
        }


        /// <summary>에디터가 활성화될 때 이벤트를 등록합니다.</summary>
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= this.OnPlayNodeStateChanged;
            EditorApplication.playModeStateChanged += this.OnPlayNodeStateChanged;

            Undo.undoRedoPerformed -= this.BehaviourEditorUndoPerformed;
            Undo.undoRedoPerformed += this.BehaviourEditorUndoPerformed;

            EditorSceneManager.sceneClosed -= this.OnSceneClosed;
            EditorSceneManager.sceneClosed += this.OnSceneClosed;

            if (EditorApplication.isPlaying)
            {
                EditorApplication.update -= this.RuntimeUpdate;
                EditorApplication.update += this.RuntimeUpdate;
            }
        }


        /// <summary>에디터가 비활성화될 때 이벤트를 해제합니다.</summary>
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= this.OnPlayNodeStateChanged;
            EditorApplication.delayCall -= this.OnSelectionChange;
            EditorApplication.update -= this.RuntimeUpdate;

            Undo.undoRedoPerformed -= this.BehaviourEditorUndoPerformed;

            EditorSceneManager.sceneClosed -= this.OnSceneClosed;
        }


        /// <summary> 하이어라키창에 변경사항이 생겼을 때, 현재 게임 오브젝트의 graphAsset 삭제됐는지 확인합니다. </summary>
        private void OnHierarchyChange()
        {
            //유니티의 Object 타입에 구현된 Equals 함수를 사용하여 Fake null을 검사.
            if (_graphAsset != null)
            {
                return;
            }

            this.Initialize();
        }


        /// <summary> 프로젝트에 변경사항이 생겼을 때, 현재 사용 중인 게임 Graph Asset이 삭제됐는지 확인합니다. </summary>
        private void OnProjectChange()
        {
            if (_graphAsset != null)
            {
                return;
            }
            
            this.Initialize();
        }


        /// <summary>씬이 닫힐 때 에디터를 초기화합니다.</summary>
        private void OnSceneClosed(Scene _)
        {
            this.Initialize();
        }


        /// <summary>플레이 모드에서 노드 뷰를 업데이트합니다.</summary>
        private void RuntimeUpdate()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (_graphAsset?.main is null)
            {
                return;
            }

            _graphView.UpdateNodeView();
        }


        /// <summary>언두/리두 작업이 수행될 때 에디터를 업데이트합니다.</summary>
        private void BehaviourEditorUndoPerformed()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            isLoadingTreeToView = true;
            _graphView?.TrySetupGraphEditorView(_focusedGraph);
            _inspectorView?.ClearInspectorView();
            _blackboardView?.RefreshItemsWhenUndoPerformed();
            isLoadingTreeToView = false;
        }


        /// <summary>플레이 모드 상태가 변경될 때 호출됩니다.</summary>
        private void OnPlayNodeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                {
                    EditorApplication.update -= this.RuntimeUpdate;
                    bool clickedNewAsset = this.TryGetGraphAsset(out TaskStreamer newTaskStreamer);
                    Debug.Assert(clickedNewAsset, "Task Streamer component not found.");
                    this.ChangeGraph(newTaskStreamer?.graphAsset.main);
                    return;
                }

                case PlayModeStateChange.EnteredPlayMode:
                {
                    EditorApplication.update += this.RuntimeUpdate;
                    //Play mode 진입할 땐, 알아서 CreateGUI-OnSelectionChange가 호출됨.
                    return;
                }
            }
        }


        /// <summary>에디터에서 선택된 객체가 변경될 때 호출됩니다.</summary>
        private void OnSelectionChange()
        {
            bool foundTree = this.TryGetGraphAsset(out TaskStreamer newTaskStreamer);

            if (foundTree && newTaskStreamer.graphAsset != null)
            {
                //오브젝트를 선택해서 에디터를 활성화시킬땐, Main Graph를 연다.
                this.ChangeGraph(newTaskStreamer.graphAsset.main);
            }
        }


        /// <summary>현재 선택된 객체에서 Behaviour Tree를 찾습니다.</summary>
        private bool TryGetGraphAsset(out TaskStreamer newTaskStreamer)
        {
            GameObject gameObject = Selection.activeGameObject;

            if (gameObject != null && gameObject.TryGetComponent(out TaskStreamer streamer))
            {
                _graphAsset = streamer.graphAsset;
                newTaskStreamer = streamer;
                return true;
            }
            else
            {
                newTaskStreamer = null;
                return false;
            }
        }


        /// <summary>에디터에서 편집할 그래프를 변경합니다.</summary>
        public void ChangeGraph(Graph graph, bool isSubGraph = false)
        {
            if (graph is null)
            {
                return;
            }

            canEditGraph = !Application.isPlaying;

            if (isSubGraph == false)
            {
                _graphBreadcrumbs.Clear();
            }

            _graphBreadcrumbs.PushItem(graph, () =>
            {
                _graphBreadcrumbs.PopToClickItems(graph.guid);
                this.OpenGraph(graph);
            });

            this.OpenGraph(graph);
        }


        private void OpenGraph(Graph drawGraph)
        {
            bool openedEditorWindow = EditorWindow.HasOpenInstances<TaskStreamerEditor>();

            if ((_graphAsset is not null && Application.isPlaying) || openedEditorWindow)
            {
                isLoadingTreeToView = true;
                _focusedGraph = drawGraph;

                _inspectorView?.ClearInspectorView();
                _graphView?.TrySetupGraphEditorView(drawGraph);
                _blackboardView?.TrySetupBlackboard(_graphAsset?.blackboard);

                isLoadingTreeToView = false;
            }
        }
    }
}