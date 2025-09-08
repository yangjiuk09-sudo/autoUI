#if UNITY_EDITOR
using System.IO;
using System.Text.Json;
using McpBridge.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace McpBridge.Editor.Commands
{
    public static class CreateScript
    {
        public static McpResponse Execute(McpRequest req)
        {
            var args = req.args;
            var path = args.GetProperty("path").GetString();
            var code = args.GetProperty("code").GetString();
            File.WriteAllText(path, code);
            AssetDatabase.ImportAsset(Rel(path));
                        // Optional attach
            if (args.TryGetProperty("attach", out var attach))
            {
                var scenePath = attach.GetProperty("scenePath").GetString();
                var componentTypeName = attach.GetProperty("componentType").GetString();
                var go = FindByScenePath(scenePath);
                var t = System.Type.GetType(componentTypeName) ?? System.AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a=>a.GetTypes()).FirstOrDefault(x=>x.Name==componentTypeName || x.FullName==componentTypeName);
                if (go != null && t != null)
                {
                    var comp = go.AddComponent(t);
                    if (attach.TryGetProperty("setProps", out var sp))
                    {
                        foreach (var p in sp.EnumerateObject())
                        {
                            var fi = t.GetField(p.Name);
                            var pi = t.GetProperty(p.Name);
                            if (fi != null)
                            {
                                var val = System.Text.Json.JsonSerializer.Deserialize(p.Value.GetRawText(), fi.FieldType);
                                fi.SetValue(comp, val);
                            }
                            else if (pi != null && pi.CanWrite)
                            {
                                var val = System.Text.Json.JsonSerializer.Deserialize(p.Value.GetRawText(), pi.PropertyType);
                                pi.SetValue(comp, val);
                            }
                        }
                    }
                }
            }
            return new McpResponse { id = req.id, ok = true, data = new { path } };
        }
        static string Rel(string p) => p.Replace("\\", "/").Substring(p.IndexOf("Packages/") >= 0 ? p.IndexOf("Packages/") : p.IndexOf("Assets/"));
    }
}
#endif

