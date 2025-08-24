using System;
using System.Collections;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable, Readable]
    public abstract class NodeBase : Task
    {
        protected Action onNodeEnter;
        protected Action onNodeExit;

#if UNITY_EDITOR
        [SerializeField, DontCreateProperty]
        internal Vector2Int position;
#endif

#region Properties

        public NodeCallState callState
        {
            get;
            protected set;
        }

        public ulong callCount
        {
            get;
            protected set;
        }

        public virtual string tooltip
        {
            get;
        }

        public TaskStreamer streamer
        {
            get;
            internal set;
        }

        public Transform transform
        {
            get { return streamer?.transform; }
        }

#endregion


        public void StartCoroutine(IEnumerator coroutine)
        {
            if (this.streamer is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] BehaviourTreeRunner is not set.");
                return;
            }

            if (coroutine is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] Coroutine to start is null.");
                return;
            }

            this.streamer.StartCoroutine(coroutine);
        }


        public void StartCoroutine(string methodName, object value = null)
        {
            if (this.streamer is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] BehaviourTreeRunner is not set.");
                return;
            }

            if (string.IsNullOrEmpty(methodName))
            {
                Debug.LogError($"[{nameof(NodeBase)}] Method name is null or empty.");
                return;
            }

            this.streamer.StartCoroutine(methodName, value);
        }


        public void StopCoroutine(IEnumerator coroutine)
        {
            if (this.streamer is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] BehaviourTreeRunner is not set.");
                return;
            }

            if (coroutine is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] Coroutine to stop is null.");
                return;
            }

            this.streamer.StopCoroutine(coroutine);
        }


        public void StopCoroutine(string methodName)
        {
            if (this.streamer is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] BehaviourTreeRunner is not set.");
                return;
            }

            if (string.IsNullOrEmpty(methodName))
            {
                Debug.LogError($"[{nameof(NodeBase)}] Method name is null or empty.");
                return;
            }

            this.streamer.StopCoroutine(methodName);
        }


        public void StopAllCoroutines()
        {
            if (this.streamer is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] BehaviourTreeRunner is not set.");
                return;
            }

            this.streamer.StopAllCoroutines();
        }


        /// <summary>Registers a callback to be executed during FixedUpdate. Used when the node needs to perform physics-based or time-consistent operations.</summary>
        protected void RegisterLateUpdateCallback(Action callback)
        {
            if (callback is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] LateUpdate callback is null.");
                return;
            }

            if (streamer is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] TaskStreamer is not set.");
                return;
            }

            streamer.onNodeLateUpdate += callback;
        }


        /// <summary>Unregisters a previously registered FixedUpdate callback.</summary>
        protected void UnregisterLateUpdateCallback(Action callback)
        {
            if (callback is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] LateUpdate callback to unregister is null.");
                return;
            }

            if (streamer is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] TaskStreamer is not set.");
                return;
            }

            streamer.onNodeLateUpdate -= callback;
        }


        /// <summary>Registers a callback to be executed during FixedUpdate. Used when the node needs to perform physics-based or time-consistent operations.</summary>
        protected void RegisterFixedUpdateCallback(Action callback)
        {
            if (callback is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] FixedUpdate callback is null.");
                return;
            }

            if (streamer is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] TaskStreamer is not set.");
                return;
            }

            streamer.onNodeFixedUpdate += callback;
        }


        /// <summary>Unregisters a previously registered FixedUpdate callback.</summary>
        protected void UnregisterFixedUpdateCallback(Action callback)
        {
            if (callback is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] FixedUpdate callback to unregister is null.");
                return;
            }

            if (streamer is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] TaskStreamer is not set.");
                return;
            }

            streamer.onNodeFixedUpdate -= callback;
        }


        /// <summary>Registers a callback for Gizmos rendering. Used when the node needs to draw debug visualization elements.</summary>
        protected void RegisterGizmosUpdateCallback(Action callback)
        {
            if (Application.isPlaying == false)
            {
                Debug.LogError($"[{nameof(NodeBase)}] Gizmos callbacks can only be registered in Play mode.");
                return;
            }

            if (callback is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] Gizmos callback is null.");
                return;
            }

            if (streamer is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] TaskStreamer is not set.");
                return;
            }

            streamer.onNodeGizmosUpdate += callback;
        }


        /// <summary>Unregisters a previously registered Gizmos rendering callback.</summary>
        protected void UnregisterGizmosUpdateCallback(Action callback)
        {
            if (Application.isPlaying == false)
            {
                Debug.LogError($"[{nameof(NodeBase)}] Gizmos callbacks can only be unregistered in Play mode.");
                return;
            }

            if (callback is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] Gizmos callback to unregister is null.");
                return;
            }

            if (streamer is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] TaskStreamer is not set.");
                return;
            }

            streamer.onNodeGizmosUpdate -= callback;
        }


        internal virtual void EnterNode() { }


        internal virtual void ExitNode() { }

        
        internal virtual void OnCreateInEditor() { }
        

        internal virtual void OnInstantiate() { }

        /// Function called after all nodes in the tree asset are created.
        /// This function is invoked using a breadth-first search (BFS) traversal pattern,
        /// processing nodes level by level starting from the root node.
        public virtual void OnAwake() { }


        /// Called when the node execution begins.
        /// Used for initialization when the node is first executed.
        protected virtual void OnEnter() { }


        /// Called when the node execution ends.
        /// Used for cleanup and state reset when the node is no longer active.
        protected virtual void OnExit() { }
    }
}