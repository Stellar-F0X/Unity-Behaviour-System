using System;
using TaskStreamer.Runtime.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// KR: 필드를 만들 수 없는 타입이 들어왔을 경우, 이 필드를 기본으로 반환한다.
    /// EN: When a type that cannot create a field is entered, this field is returned as default.
    /// </summary>
    public class UnsupportedTypeField : BindableElement, INotifyValueChanged<object>
    {
        public UnsupportedTypeField(string context) : base()
        {
            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.marginLeft = 3f;
            container.style.marginRight = 2f;
            container.style.marginTop = 2f;

            Image icon = new Image();
            icon.style.flexGrow = 1;
            icon.style.flexShrink = 0;
            icon.style.width = 16;
            icon.style.height = 16;
            icon.image = EditorGUIUtility.IconContent("console.warnicon").image;

            VisualElement content = new VisualElement();
            content.style.flexDirection = FlexDirection.Row;
            content.style.paddingRight = 2f;
            content.style.paddingLeft = 2f;
            
            Label titleLabel = new Label(StringUtility.ToNicifyName(context));
            Label contentLabel = new Label("Unsupported type");

            titleLabel.style.marginRight = 2f;
            titleLabel.style.color = Color.yellow;
            contentLabel.style.color = Color.yellow;
            
            content.Add(icon);
            content.Add(contentLabel);
            container.Add(titleLabel);
            container.Add(content);
            this.Add(container);
        }


        public object value
        {
            get { throw new FieldAccessException(); }

            set { throw new FieldAccessException(); }
        }


        public void SetValueWithoutNotify(object newValue)
        {
            throw new FieldAccessException();
        }
    }
}