#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using McpBridge.Editor.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_TEXTMESHPRO
using TMPro;
#endif

namespace McpBridge.Editor.Commands
{
    public static class ValidateLayout
    {
        public class Finding { public string ruleId; public string targetPath; public string severity; public string message; public string hint; }

        public static McpResponse Execute(McpRequest req)
        {
            var findings = new List<Finding>();

            // R-02: CanvasScaler standardization
            var scalers = GameObject.FindObjectsOfType<CanvasScaler>();
            if (scalers.Length == 0)
            {
                findings.Add(F("R-02","/","Error","No CanvasScaler found","Use Scale With Screen Size, 1080x1920, match 0.5"));
            }
            else
            {
                foreach (var s in scalers)
                {
                    if (s.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize || s.referenceResolution != new Vector2(1080,1920))
                        findings.Add(F("R-02",P(s.transform),"Warning","Non-standard CanvasScaler","ScaleWithScreenSize 1080x1920 match 0.5"));
                }
            }

            // R-04: EventSystem exists
            if (GameObject.FindObjectOfType<EventSystem>() == null)
                findings.Add(F("R-04","/","Error","No EventSystem in scene","Add EventSystem"));

            // R-01: Anchor ranges valid (0..1 and min<=max)
            foreach (var rt in GameObject.FindObjectsOfType<RectTransform>())
            {
                var aMin = rt.anchorMin; var aMax = rt.anchorMax; bool bad=false; string why="";
                if (aMin.x > aMax.x || aMin.y > aMax.y) { bad=true; why = "anchorMin greater than anchorMax"; }
                if (aMin.x < 0||aMin.x>1||aMin.y<0||aMin.y>1||aMax.x < 0||aMax.x>1||aMax.y<0||aMax.y>1)
                { bad=true; if (why!="") why += "; "; why += "anchors outside [0,1]"; }
                if (bad) findings.Add(F("R-01",P(rt),"Error",why,"Set anchors within 0..1 and min<=max"));
            }

            // R-07: Offscreen detection (approx, ScreenSpaceOverlay)
            var canvas = GameObject.FindObjectsOfType<Canvas>().FirstOrDefault(c=>c.isRootCanvas);
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                foreach (var rt in GameObject.FindObjectsOfType<RectTransform>())
                {
                    if (!rt.gameObject.activeInHierarchy) continue;
                    Vector3[] corners = new Vector3[4]; rt.GetWorldCorners(corners);
                    bool outside = corners.Any(c => c.x < 0 || c.y < 0 || c.x > Screen.width || c.y > Screen.height);
                    if (outside) findings.Add(F("R-07",P(rt),"Warning","Element may be offscreen","Ensure within screen bounds for target aspect"));
                }
            }

            // R-10: Safe Area (approx)
            var safe = Screen.safeArea; var full = new Rect(0,0,Screen.width,Screen.height);
            if (safe.width>0 && safe.height>0 && (safe != full))
            {
                foreach (var rt in GameObject.FindObjectsOfType<RectTransform>())
                {
                    if (!rt.gameObject.activeInHierarchy) continue;
                    Vector3[] corners = new Vector3[4]; rt.GetWorldCorners(corners);
                    bool outsideSafe = corners.Any(c => c.x < safe.xMin || c.y < safe.yMin || c.x > safe.xMax || c.y > safe.yMax);
                    if (outsideSafe) findings.Add(F("R-10",P(rt),"Info","Outside safe area","Consider padding to safe area on mobile"));
                }
            }

            // A11Y: Tap target >= 44x44
            foreach (var btn in GameObject.FindObjectsOfType<Button>())
            {
                var rt = btn.GetComponent<RectTransform>();
                var size = rt.rect.size;
                if (size.x < 44 || size.y < 44)
                    findings.Add(F("A11Y-TapTarget",P(rt),"Warning",$"Small target {size.x}x{size.y}","Aim for >=44x44"));
            }

#if UNITY_TEXTMESHPRO
            // TMP overflow/fit check (basic heuristic)
            foreach (var tmp in GameObject.FindObjectsOfType<TMP_Text>())
            {
                var rt = tmp.rectTransform; var rect = rt.rect.size;
                var pref = tmp.GetPreferredValues(tmp.text, rect.x, 0);
                if (!tmp.enableAutoSizing && (pref.y > rect.y + 1f))
                    findings.Add(F("TMP-Overflow",P(rt),"Warning","Text may overflow","Enable Auto Size or adjust container"));
            }
#endif

            return new McpResponse { id = req.id, ok = true, data = new { findings } };
        }

        static Finding F(string id, string path, string sev, string msg, string hint) => new Finding{ ruleId=id, targetPath=path, severity=sev, message=msg, hint=hint };
        static string P(Transform tr)
        {
            var stack = new Stack<string>(); var t = tr; while (t != null) { stack.Push(t.name); t = t.parent; } return string.Join("/", stack.ToArray());
        }
    }
}
#endif
