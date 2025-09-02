using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary> 유니티 에디터에서 Task Streamer 작업을 수행하기 위한 커스텀 에디터 창입니다. </summary>
    public class TaskStreamerEditor : EditorWindow
    {
        /// <summary>Task Streamer 에디터의 설정 정보를 참조합니다.</summary>
        private static EditorSettings _settings;

        //private NodeSearchFieldView _nodeSearchField;

        /// <summary>그래프 탐색 시 그래프 계층 구조를 표시하고 관리하는 UI 요소입니다.</summary>
        private GraphBreadcrumbs _graphBreadcrumbs; 
        
        private ObjectField _blackboardField;


        /// <summary>TaskStreamerEditor 설정 정보를 가져옵니다.</summary>
        public static EditorSettings settings
        {
            get
            {
                _settings ??= EditorUtility.FindAssetByName<EditorSettings>($"t:{nameof(EditorSettings)}");
                
                Debug.Assert(_settings != null, $"{nameof(TaskStreamerEditor)}: EditorSettings asset not found.");
                
                return _settings;
            }
        }


        /// <summary>TaskStreamerEditor의 단일 인스턴스를 가져옵니다.</summary>
        public static TaskStreamerEditor Instance
        {
            get;
            private set;
        }

        /// <summary>그래프를 수정할 수 있는지 여부를 나타냅니다.</summary>
        public static bool canEditGraph
        {
            get;
            private set;
        }

        /// <summary>그래프 편집기에서 트리 로딩 상태를 나타냅니다.</summary>
        public static bool isLoadingTreeToView
        {
            get;
            private set;
        }

        public static bool hasBlackboard
        {
            get { return Instance != null && Instance.graphAsset?.blackboard != null; }
        }

        /// <summary>현재 Task Streamer 에디터에서 사용 중인 그래프 데이터를 참조합니다.</summary>
        public GraphAsset graphAsset
        {
            get;
            private set;
        }

        /// <summary>현재 선택된 그래프를 나타냅니다.</summary>
        public Graph currentGraph
        {
            get;
            private set;
        }

        /// <summary>그래프의 미니맵 UI를 제어하기 위한 뷰입니다.</summary>
        public MiniMapView miniMapView
        {
            get;
            private set;
        }

        /// <summary>InspectorView 객체를 참조하여 그래프 요소의 상세 정보를 표시하거나 갱신합니다.</summary>
        public FloatingInspectorView inspectorView
        {
            get;
            private set;
        }

        /// <summary>Task 그래프 편집을 위한 사용자 인터페이스를 관리합니다.</summary>
        public TaskGraphView taskGraphView
        {
            get;
            private set;
        }

        /// <summary>Behavior Tree 편집기에서 사용되는 Blackboard 뷰를 나타냅니다.</summary>
        public FloatingBlackboardView blackboardView
        {
            get;
            private set;
        }
        
        

#region Static Methods
        /// <summary>Behaviour Tree 에디터 창을 Unity 메뉴에서 엽니다.</summary>
        [MenuItem("Tools/Task Streamer")]
        public static void OpenWindow()
        {
            TaskStreamerEditor wnd = GetWindow<TaskStreamerEditor>();
            wnd.titleContent = new GUIContent("Task Streamer");
            Instance = wnd;
        }


        /// <summary>Behaviour Tree 에셋을 더블클릭했을 때 에디터를 엽니다.</summary>
        /// <param name="instanceID">더블클릭된 에셋의 인스턴스 ID입니다.</param>
        /// <param name="line">선택된 에셋의 줄 번호를 나타냅니다.</param>
        /// <returns>에디터 창이 열렸으면 true, 그렇지 않으면 false를 반환합니다.</returns>
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


        /// <summary>주어진 GraphAsset을 기반으로 TaskStreamer 에디터 창을 엽니다.</summary>
        /// <param name="graphAsset">열려야 할 GraphAsset 인스턴스입니다.</param>
        public static void OpenWindow(GraphAsset graphAsset)
        {
            if (Instance != null)
            {
                Instance.Focus();
            }

            //Call CreateGUI
            TaskStreamerEditor.OpenWindow();

            if (Instance.graphAsset != graphAsset)
            {
                Instance.graphAsset = graphAsset;
                Instance.ChangeGraph(graphAsset.main);
            }
        }
#endregion


#region Create GUIs

        /// <summary>에디터 창의 GUI 요소를 생성하고 초기화합니다.</summary>
        private void CreateGUI()
        {
            TaskStreamerEditor.Instance = this;
            
            TaskStreamerResourcesLoader.Window.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(TaskStreamerResourcesLoader.WindowStyle);
            
            this.taskGraphView = rootVisualElement.Q<TaskGraphView>(); 
            this._graphBreadcrumbs = rootVisualElement.Q<GraphBreadcrumbs>();
            this._blackboardField = rootVisualElement.Q<ObjectField>("blackboard-field");
            
            this._blackboardField.RegisterValueChangedCallback(this.OnChangeBlackboard);
            this._blackboardField.enabledSelf = ! EditorApplication.isPlayingOrWillChangePlaymode;
            
            this.inspectorView = TaskStreamerResourcesLoader.FloatingInspectorView.Instantiate()[0] as FloatingInspectorView;
            this.blackboardView = new FloatingBlackboardView(rootVisualElement.Q<ToolbarToggle>("toggle-blackboard"), taskGraphView);
            this.miniMapView = new MiniMapView(rootVisualElement.Q<ToolbarToggle>("toggle-minimap"), taskGraphView);
            
            this.taskGraphView.Add(this.miniMapView);
            this.taskGraphView.Add(this.blackboardView);
            this.taskGraphView.Add(this.inspectorView);
            
            this.BindGraphViewEvents();
            this.OnSelectionChange();
        }
        

        /// <summary>GraphView의 요소 선택 및 해제를 처리하는 이벤트를 바인딩합니다.</summary>
        private void BindGraphViewEvents()
        {
            this.taskGraphView.onElementSelected -= inspectorView.UpdateSelection;
            this.taskGraphView.onElementSelected += inspectorView.UpdateSelection;
            
            this.taskGraphView.onElementUnselected = null;
            this.taskGraphView.onElementUnselected += _ => inspectorView.ClearInspector();
        }

#endregion

        

        /// <summary>에디터 창이 활성화될 때 이벤트 등록 및 초기 설정 작업을 수행합니다.</summary>
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= this.OnEditorStateChanged;
            EditorApplication.playModeStateChanged += this.OnEditorStateChanged;

            Undo.undoRedoPerformed -= this.OnEditorUndoPerformed;
            Undo.undoRedoPerformed += this.OnEditorUndoPerformed;

            EditorSceneManager.sceneClosed -= this.OnSceneClosed;
            EditorSceneManager.sceneClosed += this.OnSceneClosed;

            if (EditorApplication.isPlaying)
            {
                EditorApplication.update -= this.RuntimeUpdate;
                EditorApplication.update += this.RuntimeUpdate;
            }
        }


        /// <summary>에디터가 비활성화될 때 이벤트 핸들러 및 업데이트 작업을 해제합니다.</summary>
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= this.OnEditorStateChanged;
            EditorApplication.delayCall -= this.OnSelectionChange;
            EditorApplication.update -= this.RuntimeUpdate;

            Undo.undoRedoPerformed -= this.OnEditorUndoPerformed;

            EditorSceneManager.sceneClosed -= this.OnSceneClosed;
        }
        
        
        
        /// <summary>TaskStreamer 에디터를 초기 상태로 재설정합니다.</summary>
        private void Initialize()
        {
            TaskStreamerEditor.isLoadingTreeToView = false;
            TaskStreamerEditor.canEditGraph = false;

            this.currentGraph = null;
            this.graphAsset = null;

            this.inspectorView?.ClearInspector();
            this.taskGraphView?.ClearEditorView();
            this.blackboardView?.ClearView();
        }

        

        /// <summary>하이어라키 창에서 변경 사항이 발생하면 graphAsset의 삭제 여부를 확인하고, 필요 시 에디터를 초기화합니다.</summary>
        private void OnHierarchyChange()
        {
            //유니티의 Object 타입에 구현된 Equals 함수를 사용하여 Fake null을 검사.
            if (graphAsset != null)
            {
                return;
            }

            this.Initialize();
        }

        

        /// <summary>프로젝트 변경 시, 사용 중인 Graph Asset이 여전히 유효한지 확인하고, 유효하지 않으면 에디터를 초기화합니다.</summary>
        private void OnProjectChange()
        {
            if (graphAsset != null)
            {
                return;
            }

            this.Initialize();
        }


        
        /// <summary>씬이 닫힐 때 에디터를 초기화합니다.</summary>
        /// <param name="_">닫힌 씬의 정보를 나타내는 Scene 타입의 매개변수입니다.</param>
        private void OnSceneClosed(Scene _)
        {
            this.Initialize();
        }


        
        /// <summary>플레이 모드에서 그래프 노드 뷰를 최신 상태로 갱신합니다.</summary>
        private void RuntimeUpdate()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (graphAsset?.main is null)
            {
                return;
            }

            taskGraphView.UpdateNodeView();
        }

        

        /// <summary>언두/리두 작업 실행 시 그래프와 관련된 에디터 뷰를 갱신합니다.</summary>
        private void OnEditorUndoPerformed()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            isLoadingTreeToView = true;
            taskGraphView?.TrySetupGraphEditorView(currentGraph);
            _blackboardField.SetValueWithoutNotify(graphAsset?.blackboard);
            blackboardView?.OnUndoPerformed();
            isLoadingTreeToView = false;
        }


        
        /// <summary>플레이 모드 상태 변경 시 호출되어 상태에 따른 작업을 수행합니다.</summary>
        /// <param name="state">현재 플레이 모드 상태를 나타내는 PlayModeStateChange 값입니다.</param>
        private void OnEditorStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                {
                    EditorApplication.update -= this.RuntimeUpdate;
                    
                    if (this.TryGetGraphAsset())
                    {
                        this.ChangeGraph(graphAsset.main);
                    }
                    
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


        
        /// <summary>에디터에서 선택된 객체가 변경될 때 호출되며, 그래프 에셋을 확인하고 적절한 그래프를 로드합니다.</summary>
        private void OnSelectionChange()
        {
            if (Instance == null || taskGraphView == null || inspectorView == null)
            {
                Debug.LogError("TaskStreamerEditor is not initialized.");
                return;
            }

            if (this.TryGetGraphAsset())
            {
                this.ChangeGraph(graphAsset.main);
                this._blackboardField.value = graphAsset.blackboard;
            }
        }


        
        /// <summary>현재 선택된 게임오브젝트에서 TaskStreamer 컴포넌트를 검색하여 Behaviour Tree 정보를 가져옵니다.</summary>
        /// <param name="newTaskStreamer">찾은 TaskStreamer 컴포넌트를 반환합니다.</param>
        /// <returns>TaskStreamer를 성공적으로 찾으면 true를 반환합니다. 그렇지 않으면 false를 반환합니다.</returns>
        private bool TryGetGraphAsset()
        {
            GameObject gameObject = Selection.activeGameObject;

            if (gameObject != null && gameObject.TryGetComponent(out TaskStreamer streamer))
            {
                GraphAsset gotGraphAsset = streamer.graphAsset;

                graphAsset = gotGraphAsset != null ? gotGraphAsset : graphAsset;

                return graphAsset != null;
            }

            if (Selection.activeObject is GraphAsset selectedGraphAsset)
            {
                graphAsset = selectedGraphAsset;
                return true;
            }

            if (graphAsset is not null)
            {
                return true;
            }

            return false;
        }

        

        /// <summary>에디터에서 편집할 그래프를 변경하고 관련 설정을 초기화합니다.</summary>
        /// <param name="graph">변경하려는 그래프 인스턴스입니다.</param>
        /// <param name="isSubGraph">서브 그래프 여부를 나타냅니다.</param>
        public void ChangeGraph(Graph graph, bool isSubGraph = false)
        {
            if (graph is null)
            {
                return;
            }

            TaskStreamerEditor.canEditGraph = !Application.isPlaying;

            if (isSubGraph == false)
            {
                this._graphBreadcrumbs.Clear();
            }

            this._graphBreadcrumbs.PushItem(graph, () => this.OpenGraph(graph));
            
            this.OpenGraph(graph);
        }

        

        /// <summary>그래프를 열어 에디터 뷰와 관련된 설정을 초기화합니다.</summary>
        /// <param name="drawGraph">열고자 하는 그래프 객체를 지정합니다.</param>
        private void OpenGraph(Graph drawGraph)
        {
            bool hasOpenInstances = HasOpenInstances<TaskStreamerEditor>();

            if ((graphAsset is null || Application.isPlaying == false) && hasOpenInstances == false)
            {
                return;
            }

            isLoadingTreeToView = true;
            currentGraph = drawGraph;

            inspectorView?.ClearInspector();
            taskGraphView?.TrySetupGraphEditorView(drawGraph);
            blackboardView?.TryChangeBlackboard(graphAsset?.blackboard);

            isLoadingTreeToView = false;
        }
        
        
        
        /// <summary>블랙보드 변경을 처리하여 관련 데이터와 UI를 동기화합니다.</summary>
        /// <param name="evt">오브젝트 변경 이벤트를 나타내는 ChangeEvent 객체입니다.</param>
        private void OnChangeBlackboard(ChangeEvent<Object> evt)
        {
            if (canEditGraph == false)
            {
                return;
            }

            if (Undo.isProcessing == false)
            {
                Undo.RecordObject(graphAsset, "TaskStreamer(SetBlackboard)");
            }

            if (blackboardView.TryChangeBlackboard(evt.newValue as BlackboardAsset))
            {
                //블랙보드가 교체될 때, 기존 블랙보드가 있었다면 블랙보드의 변수가 등록되어 있는 노드들의 variable들을 초기화.
                this.graphAsset.TrySynchronizeVariablesOfNodes();
                this.inspectorView.ClearInspector();
            }
        }
    }
}