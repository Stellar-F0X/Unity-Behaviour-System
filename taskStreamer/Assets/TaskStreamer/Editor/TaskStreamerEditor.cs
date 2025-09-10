using System;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using ObjectFactory = TaskStreamer.Utility.ObjectFactory;

namespace TaskStreamer.Tool
{
    /// <summary> Task Streamer 작업을 수행하기 위한 유니티 에디터 커스텀 창 클래스입니다. </summary>
    internal class TaskStreamerEditor : EditorWindow
    {
        /// <summary>Task Streamer 에디터의 설정 정보를 위한 정적 참조입니다.</summary>
        private static EditorSettings _settings;


        /// <summary>그래프 탐색 시 그래프 계층 구조를 표시하고 관리하는 ToolbarBreadcrumbs를 나타냅니다.</summary>
        private NavigationBreadcrumbs _navigationBreadcrumbs;


        /// <summary>Graph 블랙보드를 연결하고 변경 사항을 처리하는 ObjectField 필드를 나타냅니다.</summary>
        private ObjectField _blackboardField;


        /// <summary>Behavior Tree 편집기에서 사용되는 Blackboard 뷰를 나타냅니다.</summary>
        private FloatingBlackboardView _blackboardView;


        /// <summary>그래프 업데이트가 필요한 상태를 나타내는 플래그입니다.</summary>
        private bool _requiresGraphUpdate;




        /// <summary>Task Streamer 에디터의 설정 정보를 제공하는 정적 속성입니다.</summary>
        public static EditorSettings settings
        {
            get
            {
                _settings ??= EditorUtility.FindAssetByName<EditorSettings>($"t:{nameof(EditorSettings)}");

                Debug.Assert(_settings != null, $"{nameof(TaskStreamerEditor)}: EditorSettings asset not found.");

                return _settings;
            }
        }


        /// <summary>Task Streamer 에디터의 싱글톤 인스턴스를 참조합니다.</summary>
        public static TaskStreamerEditor Instance
        {
            get;
            private set;
        }

        /// <summary>그래프를 편집할 수 있는지 여부를 나타냅니다.</summary>
        public static bool canEditGraph
        {
            get;
            private set;
        }

        /// <summary>작업 트리 데이터를 뷰에 로드 중인지 여부를 나타냅니다.</summary>
        public static bool isLoadingTreeToView
        {
            get;
            private set;
        }

        /// <summary>현재 에디터 인스턴스가 존재하며, 블랙보드 데이터가 유효한지 여부를 반환합니다.</summary>
        public static bool hasBlackboard
        {
            get { return Instance != null && Instance.graphAsset?.blackboard != null; }
        }

        /// <summary>작업 스트리머 에디터에 로드된 그래프 에셋을 참조합니다.</summary>
        public GraphAsset graphAsset
        {
            get;
            private set;
        }

        /// <summary>Task Streamer 에디터에서 현재 작업 중인 그래프를 나타냅니다.</summary>
        public Graph currentGraph
        {
            get;
            private set;
        }

        /// <summary>미니 맵 뷰를 관리하며 TaskGraphView에 추가되는 UI 요소입니다.</summary>
        public MiniMapView miniMapView
        {
            get;
            private set;
        }

        /// <summary>Task Streamer 에디터의 Floating Inspector View를 관리합니다.</summary>
        public FloatingInspector inspectorView
        {
            get;
            private set;
        }

        /// <summary>Task Streamer 에디터에서 그래프를 표시 및 조작할 수 있는 뷰를 참조합니다.</summary>
        public TaskGraphView taskGraphView
        {
            get;
            private set;
        }



#region Static Methods

        /// <summary>Task Streamer 편집기 창을 Unity 메뉴에서 엽니다.</summary>
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


        /// <summary>Task Streamer 에디터 창을 엽니다.</summary>
        /// <param name="graphAsset">로딩해야 할 GraphAsset 인스턴스입니다.</param>
        public static void OpenWindow(GraphAsset graphAsset)
        {
            if (Instance != null)
            {
                Instance.Focus();
            }

            //Call CreateGUI
            TaskStreamerEditor.OpenWindow();

            if (graphAsset == null)
            {
                TaskStreamerEditor.ClearWindow();
                return;
            }

            if (Instance.graphAsset != graphAsset)
            {
                Instance.graphAsset = graphAsset;
                Instance.ChangeGraph(graphAsset.main);
            }
        }


        /// <summary>Task Streamer 에디터 창의 상태를 초기화하고 내용을 비웁니다.</summary>
        public static void ClearWindow()
        {
            if (Instance == null)
            {
                return;
            }

            TaskStreamerEditor.Instance.Initialize();
        }

#endregion


#region Create GUIs

        /// <summary>에디터 창의 GUI를 생성하고 필요한 UI 요소를 초기화합니다.</summary>
        private void CreateGUI()
        {
            TaskStreamerEditor.Instance = this;
            this._requiresGraphUpdate = true;

            TaskStreamerResourceLoader.window.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(TaskStreamerResourceLoader.windowStyle);

            this.taskGraphView = rootVisualElement.Q<TaskGraphView>();
            this._navigationBreadcrumbs = rootVisualElement.Q<NavigationBreadcrumbs>();
            this._blackboardField = rootVisualElement.Q<ObjectField>("blackboard-field");

            this._blackboardField.RegisterValueChangedCallback(this.OnChangeBlackboardAsset);
            this._blackboardField.enabledSelf = !EditorApplication.isPlayingOrWillChangePlaymode;

            this.inspectorView = new FloatingInspector();
            this._blackboardView = new FloatingBlackboardView(taskGraphView);
            this.miniMapView = new MiniMapView(taskGraphView);

            ToolbarToggle blackboardToggle = rootVisualElement.Q<ToolbarToggle>("toggle-blackboard");
            ToolbarToggle minimapToggle = rootVisualElement.Q<ToolbarToggle>("toggle-minimap");

            blackboardToggle.RegisterValueChangedCallback(this._blackboardView.Show);
            minimapToggle.RegisterValueChangedCallback(this.miniMapView.Show);

            this.taskGraphView.Add(this.miniMapView);
            this.taskGraphView.Add(this._blackboardView);
            this.taskGraphView.Add(this.inspectorView);

            this.taskGraphView.onElementSelected += inspectorView.UpdateSelection;

            this.OnSelectionChange();
        }

#endregion


        /// <summary>에디터 창이 활성화될 때 필요한 이벤트를 등록하고 초기화 작업을 수행합니다.</summary>
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



        /// <summary>에디터 창이 비활성화될 때 등록된 이벤트와 업데이트 핸들러를 해제합니다.</summary>
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= this.OnEditorStateChanged;
            EditorApplication.update -= this.RuntimeUpdate;

            Undo.undoRedoPerformed -= this.OnEditorUndoPerformed;

            EditorSceneManager.sceneClosed -= this.OnSceneClosed;
        }



        /// <summary>TaskStreamer 에디터의 상태를 초기화합니다.</summary>
        private void Initialize()
        {
            TaskStreamerEditor.isLoadingTreeToView = false;
            TaskStreamerEditor.canEditGraph = false;

            this.currentGraph = null;
            this.graphAsset = null;

            this.inspectorView?.ClearInspector();
            this.taskGraphView?.ClearEditorView();
            this._blackboardView?.ClearView();
        }



        /// <summary>하이어라키 창에서 변경이 발생하면 현재 graphAsset의 유효성을 확인하고, 필요시 에디터 상태를 초기화합니다.</summary>
        private void OnHierarchyChange()
        {
            //유니티의 Object 타입에 구현된 Equals 함수를 사용하여 Fake null을 검사.
            if (this.graphAsset != null)
            {
                return;
            }

            this.Initialize();
        }



        /// <summary>프로젝트 변경 시, 에디터 내 활성 상태의 그래프 에셋을 재검증하고 필요에 따라 초기화합니다.</summary>
        private void OnProjectChange()
        {
            if (this.graphAsset == null)
            {
                this.Initialize();
                return;
            }

            this.OnSelectionChange();
        }



        /// <summary>씬이 닫힐 때 TaskStreamer 에디터를 초기화합니다.</summary>
        /// <param name="_">닫힌 씬 정보를 나타내는 Scene 타입 매개변수입니다.</param>
        private void OnSceneClosed(Scene _)
        {
            this.Initialize();
        }



        /// <summary>플레이 모드에서 Task Graph의 노드 뷰를 정기적으로 갱신합니다.</summary>
        private void RuntimeUpdate()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (this.graphAsset?.main is null)
            {
                return;
            }

            this.taskGraphView.UpdateNodeView();
        }



        /// <summary>언두/리두 작업 시 그래프 에디터 뷰와 블랙보드 뷰를 갱신합니다.</summary>
        private void OnEditorUndoPerformed()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            isLoadingTreeToView = true;
            this.taskGraphView?.TrySetupGraphEditorView(currentGraph);
            this._blackboardField.SetValueWithoutNotify(graphAsset?.blackboard);
            this._blackboardView?.OnUndoPerformed();
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



        /// <summary>에디터에서 선택된 객체 변경 시 그래프 에셋을 확인하고 관련 작업을 수행합니다.</summary>
        private void OnSelectionChange()
        {
            if (Instance == null || taskGraphView == null || inspectorView == null)
            {
                return;
            }

            GraphAsset previousAsset = this.graphAsset;

            //변경할 그래프가 없거나, 
            if (this.TryGetGraphAsset() == false)
            {
                return;
            }

            //아니면 그래프가 같거나, 뷰가 초기화되어 그래프를 강제로 갱신할 필요가 없다면 종료합니다.
            
            
            if (_requiresGraphUpdate || previousAsset != this.graphAsset)
            {
                this._requiresGraphUpdate = false;

                this.ChangeGraph(graphAsset.main);

                
                if (graphAsset.blackboard != null)
                {
                    this._blackboardField.SetValueWithoutNotify(graphAsset.blackboard);
                }
            }
        }



        /// <summary>선택된 객체에서 TaskStreamer 또는 GraphAsset을 검색하고 이에 따라 graphAsset을 설정합니다.</summary>
        /// <returns>성공적으로 graphAsset을 설정하면 true, 설정에 실패하면 false를 반환합니다.</returns>
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
            
            //현재 에디터가 그려지고 있는지 판단하는 로직도 필요한가?

            if (isSubGraph == false)
            {
                this._navigationBreadcrumbs.Clear();
            }

            this._navigationBreadcrumbs.PushItem(graph, () => this.OpenGraph(graph));

            this.OpenGraph(graph);
        }



        /// <summary>그래프를 열고 관련된 에디터 뷰 설정을 초기화합니다.</summary>
        /// <param name="drawGraph">열려는 그래프 객체입니다.</param>
        private void OpenGraph(Graph drawGraph)
        {
            bool hasOpenInstances = HasOpenInstances<TaskStreamerEditor>();

            if ((graphAsset is null || Application.isPlaying == false) && hasOpenInstances == false)
            {
                return;
            }

            isLoadingTreeToView = true;
            currentGraph = drawGraph;

            inspectorView?.ClearInspector(true);
            taskGraphView?.TrySetupGraphEditorView(drawGraph);
            _blackboardView?.ChangeBlackboard(graphAsset?.blackboard);

            isLoadingTreeToView = false;
        }



        /// <summary>블랙보드 에셋이 변경되었을 때, 새 블랙보드와 그래프 및 UI를 동기화합니다.</summary>
        /// <param name="evt">변경된 블랙보드 자산 값을 포함하는 ChangeEvent 객체입니다.</param>
        private void OnChangeBlackboardAsset(ChangeEvent<Object> evt)
        {
            if (canEditGraph == false)
            {
                return;
            }

            if (evt.newValue is null)
            {
                this._blackboardField.SetValueWithoutNotify(graphAsset.blackboard);
                ShowNotification(new GUIContent("Blackboard cannot be empty."));
                return;
            }

            if (Undo.isProcessing == false)
            {
                Undo.RecordObject(graphAsset, "TaskStreamer(SetBlackboard)");
            }

            BlackboardAsset fresh = ObjectFactory.CloneBlackboardAsset(evt.newValue as BlackboardAsset);
            AssetDatabase.RemoveObjectFromAsset(graphAsset.blackboard);
            AssetDatabase.AddObjectToAsset(fresh, graphAsset);

            this.graphAsset.blackboard = fresh;
            this._blackboardView.ChangeBlackboard(fresh);
            this._blackboardField.SetValueWithoutNotify(fresh);

            //블랙보드가 교체될 때, 기존 블랙보드가 있었다면 블랙보드의 변수가 등록되어 있는 노드들의 variable들을 초기화.
            this.graphAsset.TrySynchronizeVariablesOfNodes();
            this.inspectorView.ClearInspector(true);

            UnityEditor.EditorUtility.SetDirty(graphAsset);
            UnityEditor.EditorUtility.SetDirty(fresh);
            AssetDatabase.SaveAssets();
        }
    }
}