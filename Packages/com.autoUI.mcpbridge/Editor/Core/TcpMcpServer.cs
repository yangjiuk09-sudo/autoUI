#if UNITY_EDITOR
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace McpBridge.Editor.Core
{
    [Serializable] public class McpRequest { public string id; public string cmd; public JsonElement args; }
    [Serializable] public class McpResponse { public string id; public bool ok; public object data; public string[] warnings; public string[] errors; }

    public static class TcpMcpServer
    {
        static TcpListener _listener;
        static Thread _thread;
        static volatile bool _running = false;
        public static int Port { get; private set; } = 17890;

        [MenuItem("MCP Bridge/Start Server")] public static void Start()
        {
            if (_running) return;
            // Load configured port (EditorPrefs) if present
            try
            {
                if (UnityEditor.EditorPrefs.HasKey("McpBridge.Port"))
                {
                    Port = UnityEditor.EditorPrefs.GetInt("McpBridge.Port", Port);
                }
            }
            catch { }

            _running = true;
            _listener = new TcpListener(IPAddress.Loopback, Port);
            _listener.Start();
            // If port was 0 (ephemeral), capture the actual bound port
            try { Port = ((IPEndPoint)_listener.LocalEndpoint).Port; } catch { }
            _thread = new Thread(ListenLoop) { IsBackground = true };
            _thread.Start();
            Debug.Log($"[MCP] Server started on 127.0.0.1:{Port}");
            try { McpDiscovery.Write("127.0.0.1", Port); } catch (Exception ex) { Debug.LogWarning($"[MCP] Discovery write failed: {ex.Message}"); }
        }

        [MenuItem("MCP Bridge/Stop Server")] public static void Stop()
        {
            _running = false;
            try { _listener?.Stop(); } catch { }
            try { _thread?.Join(200); } catch { }
            Debug.Log("[MCP] Server stopped.");
        }

        static void ListenLoop()
        {
            while (_running)
            {
                try
                {
                    var client = _listener.AcceptTcpClient();
                    _ = new Thread(() => HandleClient(client)) { IsBackground = true }.Start();
                }
                catch { if (!_running) break; }
            }
        }

        static void HandleClient(TcpClient client)
        {
            using (client)
            using (var stream = client.GetStream())
            {
                var buf = new byte[8192];
                var sb = new StringBuilder();
                int read;
                while ((read = stream.Read(buf, 0, buf.Length)) > 0)
                {
                    sb.Append(Encoding.UTF8.GetString(buf, 0, read));
                    int idx;
                    while ((idx = sb.ToString().IndexOf('\n')) >= 0)
                    {
                        var line = sb.ToString(0, idx).Trim();
                        sb.Remove(0, idx + 1);
                        if (string.IsNullOrEmpty(line)) continue;

                        try
                        {
                            var req = JsonSerializer.Deserialize<McpRequest>(line);
                            MainThreadDispatcher.Enqueue(() =>
                            {
                                var resp = McpCommandRouter.Execute(req);
                                var json = JsonSerializer.Serialize(resp);
                                var bytes = Encoding.UTF8.GetBytes(json + "\n");
                                try { stream.Write(bytes, 0, bytes.Length); } catch { }
                            });
                        }
                        catch (Exception ex)
                        {
                            var error = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new McpResponse {
                                id = null, ok = false, errors = new[] { $"Bad request: {ex.Message}" }
                            }) + "\n");
                            try { stream.Write(error, 0, error.Length); } catch { }
                        }
                    }
                }
            }
        }
    }

    public static class McpDiscovery
    {
        public static void Write(string host, int port)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(new {
                    name = "Unity MCP Bridge",
                    version = "1.0.0",
                    host,
                    port,
                    pid = System.Diagnostics.Process.GetCurrentProcess().Id,
                    projectPath = System.IO.Directory.GetCurrentDirectory().Replace("\\", "/"),
                    timestamp = System.DateTime.UtcNow.ToString("o"),
                    commands = new[]{"get_scene_graph","place_ui","instantiate_prefab","bind_reference","validate_layout","create_script"}
                });
                // Project-local discovery
                var root = System.IO.Directory.GetCurrentDirectory();
                var dir = System.IO.Path.Combine(root, ".mcp");
                System.IO.Directory.CreateDirectory(dir);
                System.IO.File.WriteAllText(System.IO.Path.Combine(dir, "unity-mcp.json"), json);
                // User-wide discovery (AppData)
                var app = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                var udir = System.IO.Path.Combine(app, "autoUI","mcp");
                System.IO.Directory.CreateDirectory(udir);
                System.IO.File.WriteAllText(System.IO.Path.Combine(udir, "unity-mcp.json"), json);
            }
            catch { }
        }
    }

    [InitializeOnLoad]
    public static class McpAutoStart
    {
        static McpAutoStart()
        {
            try
            {
                var env = System.Environment.GetEnvironmentVariable("AUTOUI_MCP_AUTOSTART");
                bool pref = false;
                try { pref = UnityEditor.EditorPrefs.GetBool("McpBridge.AutoStart", false); } catch {}
                if ((env == "1" || pref))
                {
                    TcpMcpServer.Start();
                }
            }
            catch { }
        }
    }
}
#endif
