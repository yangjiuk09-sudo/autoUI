#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using McpBridge.Editor.Commands;
using McpBridge.Editor.Core;
using System.Text.Json;

namespace McpBridge.Editor.Validation
{
    public static class ValidationRunner
    {
        public static IEnumerable<object> RunAllInternal()
        {
            var req = new McpRequest { id = "validate", cmd = "validate_layout", args = JsonDocument.Parse("{}").RootElement };
            var resp = ValidateLayout.Execute(req);
            var findings = (resp.data as System.Collections.IDictionary) != null ? new object[0] : null; // fallback
            // Since data is anonymous type, re-run to serialize then parse
            var json = System.Text.Json.JsonSerializer.Serialize(resp);
            using var doc = JsonDocument.Parse(json);
            var arr = doc.RootElement.GetProperty("data").GetProperty("findings");
            foreach (var el in arr.EnumerateArray())
            {
                yield return new { ruleId = el.GetProperty("ruleId").GetString(), severity = el.GetProperty("severity").GetString() };
            }
        }
    }
}
#endif
