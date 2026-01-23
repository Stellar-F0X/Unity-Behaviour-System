using System;
using TaskStreamer.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
     
    /// <summary> Enum은 모든 Enum (Byte, short, int, long .. etc)들을 포괄하는 클래스라서 그런지, 초기화 과정이 까다로워서 별도의 클래스로 구분했다. </summary>
    internal class BlackboardVariableEnumField<TEnumField> : BlackboardVariableField<Enum, TEnumField> where TEnumField : BaseField<Enum>, new()
    {
        public BlackboardVariableEnumField(VariableHandle variableHandle) : base(variableHandle)
        {
            Assert.IsTrue(_blackboardVariable.valueType.IsEnum, "The value type of the blackboard variable must be an enum type");
            
            Enum enumValue = (Enum)this._blackboardVariable.boxedValue;
            Assert.IsNotNull(enumValue, "The blackboard variable value must not be null");
            
            switch (this.localVariableInputField)
            {
                case EnumField enumField: enumField.Init(enumValue); break;

                case EnumFlagsField enumFlagsField: enumFlagsField.Init(enumValue); break;
            }
        }


        /// <summary> 구현체 Enum을 Generic Arg로 받은 BBVariable을 단순한 BBVariable Enum로 변경할 수 없어서 BoxedValue에 직접 대입. </summary>
        protected override void UpdateBlackboardVariableValue(Enum newValue)
        {
            Assert.IsTrue(_blackboardVariable.valueType.IsEnum, "The value type of the blackboard variable must be an enum type");

            Undo.RecordObject(TaskStreamerEditor.Instance.graphAsset, "TaskStreamer (ChangeBBVariableValue)");
            _blackboardVariable.boxedValue = newValue;
            UnityEditor.EditorUtility.SetDirty(TaskStreamerEditor.Instance.graphAsset);
        }
    }
}