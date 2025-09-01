using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public class BlackboardBase : Blackboard
    {
        public event Action<Blackboard, BlackboardField> removeItemRequest; // 삭제된 항목 알림

        public BlackboardBase()
        {
            // Delete 키 가로채기
            this.RegisterCallback<KeyDownEvent>(this.OnDeleteKeyDown, TrickleDown.TrickleDown);

            // 컨텍스트 메뉴 교체(내 Delete 추가)
            this.AddManipulator(new ContextualMenuManipulator(BuildMyContextMenu));
        }

        
        private void OnDeleteKeyDown(KeyDownEvent evt)
        {
#if UNITY_EDITOR_OSX
            bool isDelete = evt.commandKey && evt.keyCode == KeyCode.Backspace;
#else
            bool isDelete = evt.keyCode == KeyCode.Delete;
#endif
            if (isDelete == false)
            {
                return;
            }

            this.DeleteSelectedBlackboardItems(null);
            evt.StopPropagation(); // 기본 처리 막기
        }

        
        private void BuildMyContextMenu(ContextualMenuPopulateEvent evt)
        {
            // 기본 메뉴 대신 내가 넣고 싶으면 아래처럼 직접 구성
            evt.menu.AppendAction("Delete", this.DeleteSelectedBlackboardItems);
            
            evt.StopImmediatePropagation(); // 기본 메뉴 빌드 방지
        }

        
        private void DeleteSelectedBlackboardItems(DropdownMenuAction action)
        {
            // Blackboard.selection은 선택된 요소 컬렉션
            List<BlackboardField> items = this.selection?.OfType<BlackboardField>().ToList();

            if (items == null || items.Count == 0)
            {
                return;
            }

            foreach (BlackboardField field in items)
            {
                // 보통 BlackboardRow/Field가 대상
                field.RemoveFromHierarchy(); // UI에서 제거
                removeItemRequest?.Invoke(this, field);  // 콜백 알림(여기서 데이터 모델도 싱크)
            }
        }
    }
}