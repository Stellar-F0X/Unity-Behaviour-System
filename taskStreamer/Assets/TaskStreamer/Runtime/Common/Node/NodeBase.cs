using System;
using System.Collections;
using TaskStreamer.Runtime.Utility;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.Runtime
{
    [Serializable, Readable]
    public abstract class NodeBase : Task
    {
        protected Action<NodeBase> onNodeEnter;
        protected Action<NodeBase> onNodeExit;

        
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
            get { return streamer.transform; }
        }

        public GameObject gameObject
        {
            get { return streamer.gameObject; }
        }

#endregion


        public void StartCoroutine(IEnumerator coroutine)
        {
            Assert.IsNotNull(this.streamer, $"[{nameof(NodeBase)}] BehaviourTreeRunner is not set.");
            Assert.IsNotNull(coroutine, $"[{nameof(NodeBase)}] Coroutine to start is null.");
            
            this.streamer.StartCoroutine(coroutine);
        }


        public void StartCoroutine(string methodName, params object[] value)
        {
            Assert.IsNotNull(this.streamer, $"[{nameof(NodeBase)}] BehaviourTreeRunner is not set.");
            Assert.IsTrue(methodName.IsNotNullOrEmpty(), $"[{nameof(NodeBase)}] Method name is null or empty.");
            
            this.streamer.StartCoroutine(methodName, value);
        }


        public void StopCoroutine(IEnumerator coroutine)
        {
            Assert.IsNotNull(this.streamer, $"[{nameof(NodeBase)}] BehaviourTreeRunner is not set.");
            Assert.IsNotNull(coroutine, $"[{nameof(NodeBase)}] Coroutine to stop is null.");
            
            this.streamer.StopCoroutine(coroutine);
        }


        public void StopCoroutine(string methodName)
        {
            Assert.IsNotNull(this.streamer, $"[{nameof(NodeBase)}] BehaviourTreeRunner is not set.");
            Assert.IsTrue(methodName.IsNotNullOrEmpty(), $"[{nameof(NodeBase)}] Method name is null or empty.");
            
            this.streamer.StopCoroutine(methodName);
        }


        public void StopAllCoroutines()
        {
            Assert.IsNotNull(this.streamer, $"[{nameof(NodeBase)}] BehaviourTreeRunner is not set.");
            
            this.streamer.StopAllCoroutines();
        }


        /// <summary>Registers a callback to be executed during FixedUpdate. Used when the node needs to perform physics-based or time-consistent operations.</summary>
        protected void RegisterLateUpdateCallback(Action callback)
        {
            Assert.IsNotNull(callback, $"[{nameof(NodeBase)}] LateUpdate callback is null.");
            Assert.IsNotNull(streamer, $"[{nameof(NodeBase)}] TaskStreamer is not set.");
            
            streamer.onNodeLateUpdate += callback;
        }


        /// <summary>Unregisters a previously registered FixedUpdate callback.</summary>
        protected void UnregisterLateUpdateCallback(Action callback)
        {
            Assert.IsNotNull(callback, $"[{nameof(NodeBase)}] LateUpdate callback to unregister is null.");
            Assert.IsNotNull(streamer, $"[{nameof(NodeBase)}] TaskStreamer is not set.");
            
            streamer.onNodeLateUpdate -= callback;
        }


        /// <summary>Registers a callback to be executed during FixedUpdate. Used when the node needs to perform physics-based or time-consistent operations.</summary>
        protected void RegisterFixedUpdateCallback(Action callback)
        {
            Assert.IsNotNull(callback, $"[{nameof(NodeBase)}] FixedUpdate callback is null.");
            Assert.IsNotNull(streamer, $"[{nameof(NodeBase)}] TaskStreamer is not set.");
            
            streamer.onNodeFixedUpdate += callback;
        }


        /// <summary>Unregisters a previously registered FixedUpdate callback.</summary>
        protected void UnregisterFixedUpdateCallback(Action callback)
        {
            Assert.IsNotNull(callback, $"[{nameof(NodeBase)}] FixedUpdate callback to unregister is null.");
            Assert.IsNotNull(streamer, $"[{nameof(NodeBase)}] TaskStreamer is not set.");
            
            streamer.onNodeFixedUpdate -= callback;
        }


        /// <summary>Registers a callback for Gizmos rendering. Used when the node needs to draw debug visualization elements.</summary>
        protected void RegisterGizmosUpdateCallback(Action callback)
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning($"[{nameof(NodeBase)}] Gizmos callbacks can only be registered in Play mode.");
                return;
            }
            
            Assert.IsNotNull(callback, $"[{nameof(NodeBase)}] Gizmos callback is null.");
            Assert.IsNotNull(streamer, $"[{nameof(NodeBase)}] TaskStreamer is not set.");
            
            streamer.onNodeGizmosUpdate += callback;
        }


        /// <summary>Unregisters a previously registered Gizmos rendering callback.</summary>
        protected void UnregisterGizmosUpdateCallback(Action callback)
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning($"[{nameof(NodeBase)}] Gizmos callbacks can only be unregistered in Play mode.");
                return;
            }
            
            Assert.IsNotNull(callback, $"[{nameof(NodeBase)}] Gizmos callback to unregister is null.");
            Assert.IsNotNull(streamer, $"[{nameof(NodeBase)}] TaskStreamer is not set.");
            
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