using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary> Quaternion형 값을 처리하는 필드를 제공하는 클래스 </summary>
    public class QuaternionField : BindableElement, INotifyValueChanged<Quaternion>
    {
        /// <summary> Quaternion형 값을 처리하는 필드를 제공하는 클래스 </summary>
        public QuaternionField()
        {
            this._vector4Field = new Vector4Field(string.Empty);
            this._vector4Field.RegisterValueChangedCallback(this.SetValueOnChangeEvent);
            base.Add(_vector4Field);
        }


        /// <summary> Vector4 데이터를 처리하는 내부 필드입니다. </summary>
        private readonly Vector4Field _vector4Field;

        /// <summary> 내부적으로 관리되는 Quaternion 값 </summary>
        private Quaternion _value;


        /// <summary> Quaternion 값을 나타내며 변경 시 알림을 발생시킵니다. </summary>
        public Quaternion value
        {
            get { return this._value; }

            set { this.SetValueAndNotify(value); }
        }


        /// <summary> Notifies components of changes without dispatching a value change event. </summary>
        /// <param name="newValue">The new Quaternion value to set.</param>
        public void SetValueWithoutNotify(Quaternion newValue)
        {
            this._value = newValue;
            this._vector4Field.SetValueWithoutNotify(new Vector4(value.x, value.y, value.z, value.w));
        }


        /// <summary> 새로운 값을 설정하고 변경 이벤트를 트리거합니다. </summary>
        /// <param name="newValue"> 설정할 새로운 Quaternion 값입니다. </param>
        private void SetValueAndNotify(Quaternion newValue)
        {
            Quaternion oldValue = this._value;
            this.SetValueWithoutNotify(newValue);

            using (ChangeEvent<Quaternion> evt = ChangeEvent<Quaternion>.GetPooled(oldValue, newValue))
            {
                evt.target = this;
                base.SendEvent(evt); //값이 변경되었을 때, Event 발동.
            }
        }


        /// <summary> Vector4 값 변경 이벤트에 따라 Quaternion 값을 설정합니다. </summary>
        /// <param name="evt"> Vector4 타입의 변경 이벤트 데이터를 포함합니다. </param>
        private void SetValueOnChangeEvent(ChangeEvent<Vector4> evt)
        {
            Vector4 v4 = evt.newValue;
            this.SetValueAndNotify(new Quaternion(v4.x, v4.y, v4.z, v4.w));
        }
    }
}