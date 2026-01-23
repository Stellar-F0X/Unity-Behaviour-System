using System;
using System.Linq;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.Utility;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary> 그래프 탐색 시 UI 계층적 히스토리를 관리하며, 그래프 항목 추가 및 클릭 시 동작 처리를 제공합니다. </summary>
    [UxmlElement]
    internal partial class NavigationBreadcrumbs : ToolbarBreadcrumbs
    {
        public NavigationBreadcrumbs() : base()
        {
            base.UnregisterCallback<GeometryChangedEvent>(this.GeometryChanged);
            base.RegisterCallback<GeometryChangedEvent>(this.GeometryChanged);
        }


        /// TextSizeOffset는 버튼 텍스트의 가로 길이에 추가 여백을 포함시키기 위해 사용되는 상수로, 값은 15f입니다.
        /// 버튼의 가시성 판단 및 동적 크기 조정에 활용됩니다.
        private const float TextSizeOffset = 15f;


        /// <summary>
        /// 새로운 그래프 항목을 푸시하고 클릭 이벤트를 추가합니다.
        /// </summary>
        /// <param name="graph">푸시할 그래프 객체입니다.</param>
        /// <param name="onItemClicked">그래프 항목 클릭 시 실행될 이벤트입니다.</param>
        public void PushItem(Graph graph, Action onItemClicked)
        {
            string guidString = graph.guid.ToString();

            onItemClicked += () => this.PopToClickItems(graph.guid);

            base.PushItem(graph.name, onItemClicked);

            this.Children().Last().AddToClassList(guidString);
        }


        /// <summary>
        /// 특정 GUID를 기준으로 그래프 아이템을 클릭한 상태로 남기고, 이후 아이템을 제거합니다.
        /// </summary>
        /// <param name="guid">대상 그래프 아이템의 GUID입니다.</param>
        private void PopToClickItems(in UGUID guid)
        {
            int targetIndex = this.FindItemIndex(guid.ToString());

            if (targetIndex < 0)
            {
                return;
            }

            for (int i = base.childCount - 1; i > targetIndex; i--)
            {
                this.PopItem();
            }
        }


        /// <summary>
        /// 지정된 GUID 문자열을 기반으로 Breadcrumbs에서 해당 아이템의 인덱스를 찾습니다.
        /// </summary>
        /// <param name="guidString">찾으려는 아이템의 GUID 문자열.</param>
        /// <returns>아이템이 존재하면 인덱스, 없으면 -1 반환.</returns>
        private int FindItemIndex(string guidString)
        {
            for (int index = 0; index < this.childCount; ++index)
            {
                if (this[index].ClassListContains(guidString))
                {
                    return index;
                }
            }

            return -1;
        }


        /// <summary>
        /// GeometryChanged 이벤트가 발생할 때 자식 버튼들의 가시성과 활성화 상태를 업데이트합니다.
        /// </summary>
        /// <param name="evt">GeometryChanged 이벤트 데이터입니다.</param>
        private void GeometryChanged(GeometryChangedEvent evt)
        {
            foreach (VisualElement child in this.Children())
            {
                if (child is not ToolbarButton button)
                {
                    continue;
                }

                this.UpdateButtonVisibility(button);
            }
        }


        /// <summary>
        /// 지정된 텍스트 크기와 버튼 크기를 비교하여 ToolbarButton의 가시성과 활성화 상태를 업데이트합니다.
        /// </summary>
        /// <param name="button">업데이트할 대상 ToolbarButton 객체입니다.</param>
        private void UpdateButtonVisibility(ToolbarButton button)
        {
            Vector2 textSize = button.MeasureTextSize(button.text, 0, MeasureMode.Undefined, 0, MeasureMode.Undefined);
            
            bool shouldBeVisible = button.resolvedStyle.width >= (textSize.x + TextSizeOffset);

            button.visible = shouldBeVisible;
            
            button.SetEnabled(shouldBeVisible);
        }
    }
}