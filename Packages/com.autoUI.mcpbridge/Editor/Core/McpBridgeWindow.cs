#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace McpBridge.Editor.Core
{
    public class McpBridgeWindow : EditorWindow
    {
        [MenuItem("MCP Bridge/Window")]
        public static void Open() => GetWindow<McpBridgeWindow>("MCP Bridge");

        void OnGUI()
        {
            GUILayout.Label("MCP Editor Bridge", EditorStyles.boldLabel);
            if (GUILayout.Button("Start Server")) TcpMcpServer.Start();
            if (GUILayout.Button("Stop Server")) TcpMcpServer.Stop();
            GUILayout.Space(10);
            GUILayout.Label("Connect TCP 127.0.0.1:17890 and send JSON lines {id,cmd,args}");
        }
    }
}
#endif
