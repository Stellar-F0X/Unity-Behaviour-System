using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TaskStreamer.Runtime.Utility
{
    /// <summary> 우선순위 정렬 방향을 나타내는 열거형 </summary>
    public enum PriorityOrder
    {
        /// <summary>오름차순: 낮은 값이 높은 우선순위</summary>
        Ascending,

        /// <summary>내림차순: 높은 값이 높은 우선순위</summary>
        Descending
    }

    public class PriorityQueue<TItem> : IEnumerable<TItem>
    {
        private class PriorityQueueItem
        {
            public PriorityQueueItem(TItem element, int priority)
            {
                this.element = element;
                this.priority = priority;
            }

            public readonly TItem element;

            public readonly int priority;
        }

        /// <summary> 우선순위 정렬 방향을 지정하는 생성자 </summary>
        /// <param name="priorityOrder">우선순위 정렬 방향</param>
        public PriorityQueue(PriorityOrder priorityOrder)
        {
            _priorityOrder = priorityOrder;
        }
        
        
        public PriorityQueue() : this(PriorityOrder.Ascending) { }
        
        
        
        private readonly List<PriorityQueueItem> _heap = new List<PriorityQueueItem>();
        
        
        private readonly PriorityOrder _priorityOrder = PriorityOrder.Ascending;

        
        
        public int Count
        {
            get { return _heap.Count; }
        }
        
        
        public bool IsEmpty
        {
            get { return _heap.Count == 0; }
        }
        
        
        public PriorityOrder PriorityOrder
        {
            get { return _priorityOrder; }
        }

        
        
        /// <summary> 요소를 우선순위와 함께 큐에 추가 </summary>
        /// <param name="element">추가할 요소</param>
        /// <param name="priority">요소의 우선순위</param>
        public void Enqueue(TItem element, int priority)
        {
            PriorityQueueItem item = new PriorityQueueItem(element, priority);
            this._heap.Add(item);
            this.HeapifyUp(_heap.Count - 1);
        }

        
        
        /// <summary> 가장 높은 우선순위를 가진 요소를 제거하고 반환 </summary>
        /// <returns>가장 높은 우선순위를 가진 요소</returns>
        /// <exception cref="InvalidOperationException">큐가 비어있을 때</exception>
        public TItem Dequeue()
        {
            if (this.IsEmpty)
            {
                throw new InvalidOperationException("큐가 비어있습니다.");
            }

            TItem result = this._heap[0].element;

            // 마지막 요소를 루트로 이동
            this._heap[0] = this._heap.Last();
            this._heap.RemoveAt(this._heap.Count - 1);

            // 힙 속성 복구
            if (this._heap.Count > 0)
            {
                this.HeapifyDown(0);
            }

            return result;
        }

        
        
        /// <summary> 가장 높은 우선순위를 가진 요소를 제거하지 않고 반환 </summary>
        /// <returns>가장 높은 우선순위를 가진 요소</returns>
        /// <exception cref="InvalidOperationException">큐가 비어있을 때</exception>
        public TItem Peek()
        {
            if (this.IsEmpty)
            {
                throw new InvalidOperationException("큐가 비어있습니다.");
            }

            return this._heap[0].element;
        }
        
        

        /// <summary> 큐를 비웁니다 </summary>
        public void Clear()
        {
            this._heap.Clear();
        }

        
        
        /// <summary> 두 우선순위를 비교하여 첫 번째가 더 높은 우선순위인지 확인 </summary>
        /// <param name="priority1">첫 번째 우선순위</param>
        /// <param name="priority2">두 번째 우선순위</param>
        /// <returns>첫 번째가 더 높은 우선순위면 true</returns>
        private bool HasHigherPriority(int priority1, int priority2)
        {
            return this._priorityOrder == PriorityOrder.Ascending ? priority1 < priority2 : priority1 > priority2;
        }
        
        

        /// <summary> 힙의 상향 정렬 (부모와 비교하여 위로 올림) </summary>
        /// <param name="index">정렬할 인덱스</param>
        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;

                if (this.HasHigherPriority(this._heap[index].priority, this._heap[parentIndex].priority) == false)
                {
                    break;
                }

                this.Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        
        
        /// <summary> 힙의 하향 정렬 (자식과 비교하여 아래로 내림)  </summary>
        /// <param name="index">정렬할 인덱스</param>
        private void HeapifyDown(int index)
        {
            while (true)
            {
                int leftChild = 2 * index + 1;
                int rightChild = 2 * index + 2;
                int highestPriorityIndex = index;

                if (leftChild < this._heap.Count && HasHigherPriority(_heap[leftChild].priority, _heap[highestPriorityIndex].priority))
                {
                    highestPriorityIndex = leftChild;
                }

                if (rightChild < this._heap.Count && HasHigherPriority(_heap[rightChild].priority, _heap[highestPriorityIndex].priority))
                {
                    highestPriorityIndex = rightChild;
                }

                if (highestPriorityIndex == index)
                {
                    break;
                }

                this.Swap(index, highestPriorityIndex);
                index = highestPriorityIndex;
            }
        }

        
        
        /// <summary> 두 인덱스의 요소를 교환 </summary>
        /// <param name="i">첫 번째 인덱스</param>
        /// <param name="j">두 번째 인덱스</param>
        private void Swap(int i, int j)
        {
            if (i < 0 || i >= _heap.Count || j < 0 || j >= _heap.Count)
            {
                return;
            }
            
            PriorityQueueItem temp = this._heap[i];
            this._heap[i] = _heap[j];
            this._heap[j] = temp;
        }

        
        
        public IEnumerator<TItem> GetEnumerator()
        {
            if (this._heap is null || this._heap.Count == 0)
            {
                yield break;
            }

            foreach (PriorityQueueItem item in this._heap)
            {
                yield return item.element;
            }
        }

        
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        
        
        /// <summary> 우선순위 순서로 모든 요소를 열거 (큐를 비우지 않음) </summary>
        /// <returns>우선순위 순서의 열거자</returns>
        public IEnumerable<TItem> EnumerateByPriority()
        {
            // 임시 큐를 만들어서 우선순위 순서로 반환
            PriorityQueue<TItem> tempQueue = new PriorityQueue<TItem>(this._priorityOrder);
            
            foreach (PriorityQueueItem item in this._heap)
            {
                tempQueue.Enqueue(item.element, item.priority);
            }

            while (tempQueue.IsEmpty == false)
            {
                yield return tempQueue.Dequeue();
            }
        }
        
        

        /// <summary> 디버깅을 위한 우선순위와 함께 요소들을 문자열로 반환 </summary>
        /// <returns>우선순위와 요소들의 문자열 표현</returns>
        public override string ToString()
        {
            if (this.IsEmpty)
            {
                return "Empty PriorityQueue";
            }

            string orderText = this._priorityOrder == PriorityOrder.Ascending ? "Ascending" : "Descending";
            
            string elements = string.Join(", ", _heap.Select(item => $"[{item.element}:{item.priority}]"));
            
            return $"PriorityQueue ({orderText}): {elements}";
        }
    }
}