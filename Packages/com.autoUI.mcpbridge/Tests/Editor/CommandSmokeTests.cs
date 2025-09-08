using NUnit.Framework;
using System.Linq;
using McpBridge.Editor.Core;
using McpBridge.Editor.Commands;
using System.Text.Json;

public class CommandSmokeTests
{
    [Test]
    public void PlaceUi_CreatesCanvasAndButton()
    {
        var req = new McpRequest { id = "1", cmd = "place_ui", args = JsonDocument.Parse("{\"parentPath\":\"Canvas\",\"kind\":\"button\",\"anchorMin\":[0.5,0.5],\"anchorMax\":[0.5,0.5],\"pivot\":[0.5,0.5],\"anchoredPos\":[0,0],\"sizeDelta\":[360,120],\"label\":\"Hold 4s to Quit\"}").RootElement };
        var resp = PlaceUi.Execute(req);
        Assert.IsTrue(resp.ok);
    }

    [Test]
    public void Validation_NoErrors()
    {
        var findings = McpBridge.Editor.Validation.ValidationRunner.RunAllInternal().ToList();
        Assert.IsTrue(findings.All(f => (string)f.GetType().GetProperty("severity").GetValue(f) != "Error"));
    }
}
