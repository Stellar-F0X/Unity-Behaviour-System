using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public class QuaternionField : BindableElement, INotifyValueChanged<Quaternion>
    {
        public QuaternionField() : base()
        {
            this._vector3Field = new Vector3Field();
            this._vector3Field.value = Vector3.zero;
            this.Add(_vector3Field);
            
            this.UnregisterValueChangedCallback(this.SetValueOnChangeEvent);
            this.RegisterValueChangedCallback(this.SetValueOnChangeEvent);
        }
        
        private readonly Vector3Field _vector3Field;
        
        
        public Quaternion value
        {
            get;
            set;
        }
        
        
        public void SetValueWithoutNotify(Quaternion newValue)
        {
            this.value = newValue;
            this._vector3Field.SetValueWithoutNotify(value.eulerAngles);
        }

        private void SetValueOnChangeEvent(ChangeEvent<Quaternion> evt)
        {
            this.SetValueWithoutNotify(evt.newValue);
        }
    }
}