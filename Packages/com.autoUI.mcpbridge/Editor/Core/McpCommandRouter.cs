#if UNITY_EDITOR
using McpBridge.Editor.Core;

namespace McpBridge.Editor.Core
{
    public static class McpCommandRouter
    {
        public static McpResponse Execute(McpRequest req)
        {
            switch (req.cmd)
            {
                case "get_scene_graph": return Commands.SceneGraph.Execute(req);
                case "place_ui": return Commands.PlaceUi.Execute(req);
                case "instantiate_prefab": return Commands.InstantiatePrefabCmd.Execute(req);
                case "bind_reference": return Commands.BindReference.Execute(req);
                case "validate_layout": return Commands.ValidateLayout.Execute(req);
                case "create_script": return Commands.CreateScript.Execute(req);
                default:
                    return new McpResponse { id = req.id, ok = false, errors = new[] { $"Unknown cmd: {req.cmd}" } };
            }
        }
    }
}
#endif
