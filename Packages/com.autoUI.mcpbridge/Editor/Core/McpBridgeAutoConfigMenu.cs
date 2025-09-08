#if UNITY_EDITOR
using UnityEditor;

namespace McpBridge.Editor.Core
{
    public static class McpBridgeAutoConfigMenu
    {
        const string AutoStartKey = "McpBridge.AutoStart";
        const string PortKey = "McpBridge.Port";

        [MenuItem("MCP Bridge/Auto Start on Load", priority = 1)]
        public static void ToggleAutoStart()
        {
            bool cur = EditorPrefs.GetBool(AutoStartKey, false);
            EditorPrefs.SetBool(AutoStartKey, !cur);
            EditorUtility.DisplayDialog("MCP Bridge", $"Auto Start is now {(!cur ? "ON" : "OFF")}", "OK");
        }

        [MenuItem("MCP Bridge/Auto Start on Load", validate = true)]
        public static bool ToggleAutoStartValidate()
        {
            Menu.SetChecked("MCP Bridge/Auto Start on Load", EditorPrefs.GetBool(AutoStartKey, false));
            return true;
        }

        [MenuItem("MCP Bridge/Use Auto Port (0)", priority = 2)]
        public static void UseAutoPort()
        {
            EditorPrefs.SetInt(PortKey, 0);
            EditorUtility.DisplayDialog("MCP Bridge", "Port set to 0 (auto)", "OK");
        }

        [MenuItem("MCP Bridge/Use Default Port 17890", priority = 3)]
        public static void UseDefaultPort()
        {
            EditorPrefs.SetInt(PortKey, 17890);
            EditorUtility.DisplayDialog("MCP Bridge", "Port set to 17890", "OK");
        }
    }
}
#endif

