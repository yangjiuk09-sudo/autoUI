#if UNITY_EDITOR
using System.Text.Json;
using McpBridge.Editor.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace McpBridge.Editor.Commands
{
    public static class InstantiatePrefabCmd
    {
        public static McpResponse Execute(McpRequest req)
        {
            try
            {
                var args = req.args;
                var guid = args.GetProperty("prefabGuid").GetString();
                var parentPath = args.TryGetProperty("parentPath", out var pp) ? pp.GetString() : null;
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path))
                    return new McpResponse { id = req.id, ok = false, errors = new[] { $"Invalid GUID: {guid}" } };
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                    return new McpResponse { id = req.id, ok = false, errors = new[] { $"Not a prefab: {path}" } };

                GameObject instance;
                var parent = FindByPath(parentPath)?.transform;
                if (parent != null)
                    instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                else
                    instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                Undo.RegisterCreatedObjectUndo(instance, "Instantiate Prefab");
                EditorSceneManager.MarkSceneDirty(instance.scene);
                return new McpResponse { id = req.id, ok = true, data = new { createdPath = GetPath(instance) } };
            }
            catch (System.Exception ex)
            {
                return new McpResponse { id = req.id, ok = false, errors = new[] { ex.Message } };
            }
        }
        static GameObject FindByPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var parts = path.Split('/');
            Transform t = null;
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                if (root.name == parts[0]) { t = root.transform; break; }
            if (t == null) return null;
            for (int i=1;i<parts.Length;i++){ t = t.Find(parts[i]); if (t==null) return null; }
            return t.gameObject;
        }
        static string GetPath(GameObject go)
        {
            System.Collections.Generic.Stack<string> s = new();
            var t = go.transform; while (t != null) { s.Push(t.name); t = t.parent; }
            return string.Join("/", s.ToArray());
        }
    }
}
#endif
