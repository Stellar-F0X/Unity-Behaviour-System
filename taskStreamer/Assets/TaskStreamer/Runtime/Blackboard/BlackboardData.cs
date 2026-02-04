using System;
using System.Collections.Generic;
using TaskStreamer.Runtime.Utility;
using UnityEngine;

namespace TaskStreamer.Runtime
{
    /// <summary>Blackboard 데이터 관리 클래스입니다.</summary>
    [Serializable]
    internal class BlackboardData : ISerializationCallbackReceiver
    {
        /// <summary>GUID를 키로 사용하는 BlackboardVariable의 매핑을 관리하는 사전입니다.</summary>
        private readonly Dictionary<UGUID, BlackboardVariable> _variablesByGuid = new Dictionary<UGUID, BlackboardVariable>();
        
        
        /// <summary>런타임에서 객체의 초기화 여부를 나타냅니다.</summary>
        [NonSerialized]
        private bool _initialized = false;

        
        /// <summary>
        /// Runtime 블랙보드와의 동기화를 위한 버전 관리 용도.
        /// 블랙보드 변수가 변경 또는 수정될 때마다 버전이 갱신되며, 
        /// 연결된 TS의 Runtime 블랙보드는 버전이 다를 경우에만 동기화를 수행한다. 
        /// </summary>
        [SerializeField]
        private UGUID _appliedVersion;

        
        /// <summary>블랙보드 시스템에서 변수 목록을 저장합니다.</summary>
        [SerializeReference]
        private List<BlackboardVariable> _variables = new List<BlackboardVariable>(); //bbView에서 사용해서 readonly로 하면 안 됨.
        
        
        /// <summary>키를 기준으로 BlackboardVariable 객체를 매핑하는 Dictionary입니다.</summary>
        private Dictionary<string, BlackboardVariable> _variableByKey;

        
        
        public List<BlackboardVariable> variables
        {
            get { return this._variables; }
        }

        
        public int count
        {
            get { return this._variables?.Count ?? 0; }
        }



        /// <summary>
        /// Runtime 블랙보드와의 동기화를 위한 버전 관리 용도.
        /// 블랙보드 변수가 변경 또는 수정될 때마다 버전이 갱신되며, 
        /// 연결된 TS의 Runtime 블랙보드는 버전이 다를 경우에만 동기화를 수행한다. 
        /// </summary>
        internal UGUID appliedVersion
        {
            get { return this._appliedVersion; }
        }



        /// <summary>BlackboardAsset의 적용 버전을 업데이트합니다.</summary>
        public void UpdateAppliedVersion()
        {
            this._appliedVersion = UGUID.Create();
        }


        /// <summary>런타임 진입 시 BlackboardData를 초기화합니다.</summary>
        public void InitializeOnEnterRuntime()
        {
            if (_initialized)
            {
                return;
            }

            _variableByKey = new Dictionary<string, BlackboardVariable>();

            _variables.ForEach(v => _variableByKey.Add(v.key, v));

            _initialized = true;
        }


        /// <summary>적용된 버전과 다른지 여부를 확인합니다.</summary>
        /// <param name="lastAppliedVersion">마지막으로 적용된 버전의 UGUID입니다.</param>
        /// <returns>현재 저장된 버전과 전달된 버전이 다르면 true를 반환합니다.</returns>
        public bool IsRequiresUpdate(UGUID lastAppliedVersion)
        {
            if (this._appliedVersion != lastAppliedVersion)
            {
                this._appliedVersion = lastAppliedVersion;
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>키에 대응하는 BlackboardVariable을 찾습니다.</summary>
        /// <param name="variableKey">찾고자 하는 변수의 키입니다.</param>
        /// <returns>찾은 BlackboardVariable을 반환하며, 키가 없거나 유효하지 않으면 null을 반환합니다.</returns>
        public BlackboardVariable FindVariable(string variableKey)
        {
            if (string.IsNullOrEmpty(variableKey))
            {
                return null;
            }

            //런타임이 아니라면 리스트 탐색으로 찾는다.
            if (_initialized == false)
            {
                int hash = StringUtility.StringToHash(variableKey);

                return _variables.Find(v => hash == v.keyHash);
            }

            if (this._variableByKey.TryGetValue(variableKey, out BlackboardVariable variable))
            {
                return variable;
            }
            else
            {
                return null;
            }
        }


        /// <summary>주어진 키를 사용하여 BlackboardVariable을 검색합니다.</summary>
        /// <param name="key">검색할 변수의 UGUID 키입니다.</param>
        /// <returns>키에 해당하는 BlackboardVariable을 반환하며, 없으면 null을 반환합니다.</returns>
        public BlackboardVariable FindVariable(in UGUID key)
        {
            if (key.IsEmpty())
            {
                return null;
            }

            if (this._variablesByGuid.TryGetValue(key, out BlackboardVariable variable))
            {
                return variable;
            }
            else
            {
                return null;
            }
        }


        /// <summary>지정된 GUID 키를 가진 변수가 존재하는지 확인합니다.</summary>
        /// <param name="key">확인할 변수의 GUID 키입니다.</param>
        /// <returns>키에 해당하는 변수가 존재하면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public bool HasVariable(UGUID key)
        {
            if (key.IsEmpty())
            {
                return false;
            }
            else
            {
                return _variablesByGuid.ContainsKey(key);
            }
        }

        
        /// <summary>BlackboardVariable을 추가합니다.</summary>
        /// <param name="variable">추가할 BlackboardVariable 객체입니다.</param>
        internal void AddVariable(in BlackboardVariable variable)
        {
            if (this._initialized)
            {
                Debug.LogWarning($"{typeof(BlackboardData)}: Cannot add or remove variables at runtime.");
                return;
            }

            if (this._variables != null)
            {
                this._variables.Add(variable);
            }

            if (this._variablesByGuid != null)
            {
                this._variablesByGuid[variable.guid] = variable;
            }

            this.UpdateAppliedVersion();
        }


        /// <summary>지정된 변수를 BlackBoard에서 제거합니다.</summary>
        /// <param name="variable">제거할 변수 객체입니다.</param>
        internal void RemoveVariable(in BlackboardVariable variable)
        {
            if (this._initialized)
            {
                Debug.LogWarning($"{typeof(BlackboardData)}: Cannot add or remove variables at runtime.");
                return;
            }

            bool completion = false;

            if (variable is not null)
            {
                completion = this._variablesByGuid.Remove(variable.guid);
            }

            if (completion)
            {
                this._variables.Remove(variable);
            }

            this.UpdateAppliedVersion();
        }


        /// <summary>Blackboard에 등록된 모든 변수를 제거합니다.</summary>
        internal void ClearVariables()
        {
            if (this._initialized)
            {
                Debug.LogWarning($"{typeof(BlackboardData)}: Cannot add or remove variables at runtime.");
                return;
            }
            
            if (this._variables != null)
            {
                this._variables.Clear();
            }

            if (this._variablesByGuid != null)
            {
                this._variablesByGuid.Clear();
            }
        }


        /// <summary>Serialization 이전에 호출됩니다.</summary>
        public void OnBeforeSerialize() { }


        /// <summary>Serialization 후 GUID에 대한 매핑 데이터를 초기화합니다.</summary>
        public void OnAfterDeserialize()
        {
            _variablesByGuid.Clear();

            variables?.ForEach(v => _variablesByGuid[v.guid] = v);
        }
    }
}