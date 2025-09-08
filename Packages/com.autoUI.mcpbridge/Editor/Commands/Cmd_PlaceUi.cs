#if UNITY_EDITOR
using System;
using System.Linq;
using System.Text.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace McpBridge.Editor.Commands
{
    public static class PlaceUi
    {
        public static McpBridge.Editor.Core.McpResponse Execute(McpBridge.Editor.Core.McpRequest req)
        {
            var args = req.args;
            string parentPath = args.TryGetProperty("parentPath", out var pp) ? pp.GetString() : null;
            string kind = args.TryGetProperty("kind", out var k) ? k.GetString() : "button";

            EnsureCanvas(out var canvasGo);
            EnsureEventSystem();
            var parent = FindByPath(parentPath) ?? canvasGo;

            GameObject go;
            if (kind == "button")
            {
                go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(parent.transform, false);
                var img = go.GetComponent<Image>();
                img.raycastTarget = true;
                var label = args.TryGetProperty("label", out var l) ? l.GetString() : "Button";
                var txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
                txtGo.transform.SetParent(go.transform, false);
                var txt = txtGo.GetComponent<Text>();
                txt.text = label; txt.alignment = TextAnchor.MiddleCenter; txt.resizeTextForBestFit = true; txt.color = Color.black;
                var tr = txt.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
            }
            else
            {
                go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(parent.transform, false);
            }

            var rt = go.GetComponent<RectTransform>();
            ApplyRect(rt, args);

            return new McpBridge.Editor.Core.McpResponse { id = req.id, ok = true, data = new { createdPath = GetPath(go) }, warnings = Array.Empty<string>(), errors = Array.Empty<string>() };
        }

        static void EnsureCanvas(out GameObject canvasGo)
        {
            var canvas = GameObject.FindObjectsOfType<Canvas>().FirstOrDefault();
            if (canvas == null)
            {
                canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                var c = canvasGo.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }
            else canvasGo = canvas.gameObject;
        }

        static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                es.hideFlags = HideFlags.DontSaveInBuild;
            }
        }

        static GameObject FindByPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var parts = path.Split('/');
            Transform t = null;
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name == parts[0]) { t = root.transform; break; }
            }
            if (t == null) return null;
            for (int i = 1; i < parts.Length; i++)
            {
                t = t.Find(parts[i]);
                if (t == null) return null;
            }
            return t.gameObject;
        }

        static void ApplyRect(RectTransform rt, JsonElement args)
        {
            if (args.TryGetProperty("anchorMin", out var mn)) rt.anchorMin = new Vector2(mn[0].GetSingle(), mn[1].GetSingle());
            if (args.TryGetProperty("anchorMax", out var mx)) rt.anchorMax = new Vector2(mx[0].GetSingle(), mx[1].GetSingle());
            if (args.TryGetProperty("pivot", out var pv)) rt.pivot = new Vector2(pv[0].GetSingle(), pv[1].GetSingle());
            if (args.TryGetProperty("anchoredPos", out var ap)) rt.anchoredPosition = new Vector2(ap[0].GetSingle(), ap[1].GetSingle());
            if (args.TryGetProperty("sizeDelta", out var sd)) rt.sizeDelta = new Vector2(sd[0].GetSingle(), sd[1].GetSingle());
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
