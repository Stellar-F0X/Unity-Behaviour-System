using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace TaskStreamer.Utility
{
    /// <summary> 키-값 쌍의 컬렉션 변경 알림을 지원하는 Dictionary 클래스입니다. </summary>
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        public delegate void OnCollectionItemChangedCallback(Dictionary<TKey, TValue> dic, NotifyCollectionChangedAction action, TKey key, TValue value);
        
        
        public ObservableDictionary()
        {
            _dict = new Dictionary<TKey, TValue>();
        }
        

        public ObservableDictionary(IEqualityComparer<TKey> comparer)
        {
            _dict = new Dictionary<TKey, TValue>(comparer);
        }
        
        
        
        /// <summary> 내부 데이터 저장소 역할을 하는 readonly Dictionary 타입의 변수입니다. </summary>
        private readonly Dictionary<TKey, TValue> _dict;


        /// <summary> 컬렉션 요소의 변경 이벤트를 처리하기 위한 델리게이트로 변경된 작업 타입(추가, 제거 등), 키, 값 정보를 제공. </summary>
        public event OnCollectionItemChangedCallback onCollectionItemChanged;

        
        /// <summary> ObservableDictionary 클래스의 인덱서를 나타냄. </summary>
        public TValue this[TKey key]
        {
            get { return _dict[key]; }
            
            set { this.Set(key, value); }
        }

        
        /// <summary>
        /// 딕셔너리에 포함된 모든 키를 반환하는 속성입니다.
        /// 키의 컬렉션은 읽기 전용으로 제공됩니다.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get { return _dict.Keys; }
        }

        
        /// <summary>
        /// 딕셔너리에 포함된 모든 값(TValue)을 반환합니다.
        /// </summary>
        public ICollection<TValue> Values
        {
            get { return _dict.Values; }
        }

        
        /// <summary>
        /// 현재 ObservableDictionary에 포함된 키-값 쌍의 총 개수를 가져옵니다.
        /// </summary>
        public int Count
        {
            get { return _dict.Count; }
        }

        
        /// <summary>
        /// 현재 ObservableDictionary가 읽기 전용인지 여부를 나타냅니다. 항상 false를 반환하며, 수정 가능한 상태임을 의미합니다.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        
        /// <summary>
        /// 키와 값을 설정하고 적절한 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="key">설정할 키</param>
        /// <param name="value">설정할 값</param>
        private void Set(TKey key, TValue value)
        {
            bool keyExists = _dict.ContainsKey(key);
            
            _dict[key] = value;

            if (keyExists)
            {
                this.OnCollectionChanged(NotifyCollectionChangedAction.Replace, key, value);
                return;
            }

            this.OnCollectionChanged(NotifyCollectionChangedAction.Add, key, value);
        }

        
        /// <summary>
        /// 컬렉션 변경 시 호출되어 관련 이벤트를 트리거하는 내부 메서드입니다.
        /// </summary>
        /// <param name="action">변경 작업의 유형을 나타내는 <see cref="NotifyCollectionChangedAction"/> 값입니다.</param>
        /// <param name="key">변경된 요소의 키입니다.</param>
        /// <param name="value">변경된 요소의 값입니다.</param>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, TKey key, TValue value)
        {
            onCollectionItemChanged?.Invoke(_dict, action, key, value);
        }

        
        /// <summary>
        /// 지정된 키와 값을 딕셔너리에 추가하고 변경 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="key">추가할 항목의 키입니다.</param>
        /// <param name="value">추가할 항목의 값입니다.</param>
        public void Add(TKey key, TValue value)
        {
            this._dict.Add(key, value);
            this.OnCollectionChanged(NotifyCollectionChangedAction.Add, key, value);
        }

        
        /// <summary>
        /// 지정된 키에 해당하는 요소를 제거합니다.
        /// </summary>
        /// <param name="key">제거할 요소의 키입니다.</param>
        /// <returns>제거에 성공하면 true, 실패하면 false를 반환합니다.</returns>
        public bool Remove(TKey key)
        {
            if (_dict.TryGetValue(key, out TValue value) && _dict.Remove(key))
            {
                this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, key, value);
                return true;
            }

            return false;
        }

        
        /// <summary>
        /// 알림을 발생시키지 않고 Dictionary에 항목을 추가합니다.
        /// </summary>
        /// <param name="key">추가할 항목의 키입니다.</param>
        /// <param name="value">추가할 항목의 값입니다.</param>
        public void AddWithoutNotify(TKey key, TValue value)
        {
            _dict.Add(key, value);
        }
        

        /// <summary>
        /// 지정된 키를 갖는 항목을 사전에서 이벤트 알림 없이 제거합니다.
        /// </summary>
        /// <param name="key">제거할 항목의 키입니다.</param>
        /// <returns>항목이 성공적으로 제거되었는지 여부를 반환합니다.</returns>
        public bool RemoveWithoutNotify(TKey key)
        {
            return _dict.Remove(key);
        }

        
        /// <summary>
        /// 지정된 키가 ObservableDictionary에 존재하는지 확인합니다.
        /// </summary>
        /// <param name="key">확인하려는 키 값</param>
        /// <returns>키가 존재하면 true, 그렇지 않으면 false</returns>
        public bool ContainsKey(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        
        /// <summary>
        /// 주어진 키에 해당하는 값을 검색하고 값을 반환합니다.
        /// </summary>
        /// <param name="key">검색할 키.</param>
        /// <param name="value">키가 존재할 경우 해당 값이 저장될 변수.</param>
        /// <returns>키가 존재하면 true, 그렇지 않으면 false를 반환.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dict.TryGetValue(key, out value);
        }

        
        /// <summary>
        /// ObservableDictionary에 새로운 키-값 쌍을 추가하고 변경 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="item">딕셔너리에 추가할 키-값 쌍</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        
        /// <summary>
        /// 컬렉션의 모든 항목을 제거하고, onCollectionCleared 이벤트를 호출합니다.
        /// </summary>
        public void Clear()
        {
            _dict.Clear();
            this.OnCollectionChanged(NotifyCollectionChangedAction.Reset, default, default);
        }

        
        /// <summary>
        /// 지정된 키-값 쌍이 사전에 포함되어 있는지 확인합니다.
        /// </summary>
        /// <param name="item">확인할 키-값 쌍입니다.</param>
        /// <returns>키-값 쌍이 있으면 true, 없으면 false를 반환합니다.</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (_dict.TryGetValue(item.Key, out TValue value))
            {
                return EqualityComparer<TValue>.Default.Equals(value, item.Value);
            }

            return false;
        }

        
        /// <summary>
        /// 딕셔너리의 요소들을 지정된 배열로 복사합니다.
        /// </summary>
        /// <param name="array">복사할 대상 배열</param>
        /// <param name="arrayIndex">복사를 시작할 인덱스</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dict).CopyTo(array, arrayIndex);
        }

        
        /// <summary>
        /// 지정된 키-값 쌍을 제거합니다.
        /// </summary>
        /// <param name="item">제거할 키-값 쌍</param>
        /// <returns>제거에 성공하면 true, 실패하면 false</returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (Contains(item))
            {
                return Remove(item.Key);
            }

            return false;
        }

        
        /// <summary>
        /// 딕셔너리의 키-값 쌍을 열거하는 열거자를 반환합니다.
        /// </summary>
        /// <returns>키-값 쌍의 열거자</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        
        /// <summary>
        /// 딕셔너리의 키-값 쌍을 열거하는 열거자를 반환합니다.
        /// </summary>
        /// <returns>키-값 쌍의 열거자</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}