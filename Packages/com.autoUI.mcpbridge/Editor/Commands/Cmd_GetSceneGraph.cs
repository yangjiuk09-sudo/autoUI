#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using McpBridge.Editor.Core;
using UnityEngine;

namespace McpBridge.Editor.Commands
{
    public static class SceneGraph
    {
        public class Node
        {
            public string path;
            public string name;
            public string type;
            public string[] components;
        }
        public static McpResponse Execute(McpRequest req)
        {
            var nodes = new List<Node>();
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
                Traverse(root.transform, nodes);
            return new McpResponse { id = req.id, ok = true, data = new { nodes } };
        }
        static void Traverse(Transform t, List<Node> nodes)
        {
            var comps = t.GetComponents<Component>();
            nodes.Add(new Node {
                path = GetPath(t), name = t.name, type = t.GetType().Name,
                components = comps.Where(c=>c!=null).Select(c=>c.GetType().FullName).ToArray()
            });
            for (int i=0;i<t.childCount;i++) Traverse(t.GetChild(i), nodes);
        }
        static string GetPath(Transform tr)
        {
            var stack = new Stack<string>();
            var t = tr; while (t != null) { stack.Push(t.name); t = t.parent; }
            return string.Join("/", stack.ToArray());
        }
    }
}
#endif
