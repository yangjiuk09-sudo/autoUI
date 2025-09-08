#if UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using UnityEditor;

namespace McpBridge.Editor.Core
{
    [InitializeOnLoad]
    public static class MainThreadDispatcher
    {
        static readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();
        static MainThreadDispatcher()
        {
            EditorApplication.update += Pump;
        }
        public static void Enqueue(Action a) => _queue.Enqueue(a);
        static void Pump()
        {
            while (_queue.TryDequeue(out var a))
            {
                try { a?.Invoke(); } catch (Exception e) { UnityEngine.Debug.LogException(e); }
            }
        }
    }
}
#endif
