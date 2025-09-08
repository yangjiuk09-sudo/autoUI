#if UNITY_EDITOR
using System;
using McpBridge.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace McpBridge.Editor.Commands
{
    public static class BindReference
    {
        public static McpResponse Execute(McpRequest req)
        {
            try
            {
                var args = req.args;
                var targetPath = args.GetProperty("scenePath").GetString();
                var componentType = Type.GetType(args.GetProperty("componentType").GetString());
                var propertyPath = args.GetProperty("propertyPath").GetString();
                var referenceElem = args.GetProperty("reference");
                var targetGo = FindByScenePath(targetPath);
                if (targetGo == null) return new McpResponse { id = req.id, ok = false, errors = new[]{ $"Target not found: {targetPath}" } };
                var comp = targetGo.GetComponent(componentType);
                if (comp == null) return new McpResponse { id = req.id, ok = false, errors = new[]{ $"Component missing: {componentType}" } };

                UnityEngine.Object refObj = null;
                if (referenceElem.TryGetProperty("scenePath", out var sp))
                {
                    var refGo = FindByScenePath(sp.GetString());
                    if (refGo == null) return new McpResponse { id = req.id, ok = false, errors = new[]{ $"Reference not found: {sp.GetString()}" } };
                    refObj = refGo;
                }
                else if (referenceElem.TryGetProperty("assetGuid", out var ag))
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(ag.GetString());
                    refObj = AssetDatabase.LoadMainAssetAtPath(assetPath);
                }

                var so = new SerializedObject(comp);
                var prop = so.FindProperty(propertyPath);
                if (prop == null) return new McpResponse { id = req.id, ok = false, errors = new[]{ $"PropertyPath invalid: {propertyPath}" } };
                Undo.RecordObject(comp, "Bind Reference");
                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    prop.objectReferenceValue = refObj;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(comp);
                    return new McpResponse { id = req.id, ok = true, data = new { bound = true } };
                }
                else
                {
                    return new McpResponse { id = req.id, ok = false, errors = new[]{ "Property is not an ObjectReference" } };
                }
            }
            catch (Exception ex)
            {
                return new McpResponse { id = req.id, ok = false, errors = new[]{ ex.Message } };
            }
        }

        static GameObject FindByScenePath(string path)
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
    }
}
#endif
