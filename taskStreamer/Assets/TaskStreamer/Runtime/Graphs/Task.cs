using System;
using System.Collections.Generic;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Assertions;
using TypeUtility = TaskStreamer.Utility.TypeUtility;

namespace TaskStreamer
{
    /// <summary> TaskStreamer 라이브러리의 기본 추상 클래스. 이름, 태그, 설명 등의 공통 필드를 포함하며, 상속을 통해 특정 작업의 세부 동작을 정의할 수 있음. </summary>
    [Serializable, Readable]
    public abstract class Task
    {
        /// 태스크의 이름을 나타내는 문자열 필드로, 태스크를 식별하거나 UI에 표시하기 위해 사용됩니다.
        [DontCreateProperty]
        public string name;


        /// 태스크를 식별하거나 분류하기 위해 사용되는 문자열 필드입니다.
        [DontCreateProperty]
        public string tag;


        /// 작업(Task) 개체에 대한 설명을 저장하는 문자열 필드입니다.
        [DontCreateProperty]
        public string description;


        /// 태스크를 고유하게 식별하기 위해 사용되는 UGUID 형식의 필드입니다.
        [SerializeField, DontCreateProperty]
        protected UGUID _guid;


#if UNITY_EDITOR
        /// 특정 Task에서 이름 편집 가능 여부를 나타내는 불리언 필드입니다.
        [SerializeField, DontCreateProperty]
        internal bool canEditName = true;


        /// Task 클래스와 관련된 MonoScript 객체를 참조하며, 스크립트 타입 정보를 제공합니다.
        [SerializeField, DontCreateProperty]
        private UnityEditor.MonoScript _script;


        [NonSerialized, DontCreateProperty]
        private List<VariableHandle> _variableHandles;



        /// 지정된 MonoScript를 반환하거나, 없을 경우 현재 클래스 타입에 해당하는 스크립트를 가져오는 프로퍼티입니다.
        internal UnityEditor.MonoScript script
        {
            get
            {
                this._script = _script != null ? _script : TypeUtility.GetScriptByType(this.GetType());
                Assert.IsNotNull(_script, $"MonoScript for {this.GetType().Name} is not found.");
                return _script;
            }
        }


        internal List<VariableHandle> variableHandles
        {
            get
            {
                this._variableHandles = this._variableHandles ?? TypeUtility.TryGetFieldHandles(this.GetType(), this); 
                Assert.IsNotNull(this._variableHandles, $"Properties is null. Type: {this.GetType().FullName}");
                return _variableHandles;
            }
        }

#endif

        /// `guid` 속성은 각 `Task` 객체를 식별하기 위해 사용되는 고유한 식별자입니다.
        /// 읽기 전용으로 외부에서 접근 가능하며 내부적으로만 수정할 수 있습니다.
        public UGUID guid
        {
            get { return _guid; }

            internal set { _guid = value; }
        }
    }
}