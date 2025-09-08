using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Text.Json;
using McpBridge.Editor.Core;
using McpBridge.Editor.Commands;

public class BindReferenceCommandTests
{
    [Test]
    public void Bind_Button_TargetGraphic_To_ChildText()
    {
        // Create Button via command
        var reqBtn = new McpRequest { id = "b1", cmd = "place_ui", args = JsonDocument.Parse("{\"parentPath\":\"Canvas\",\"kind\":\"button\",\"label\":\"Bind Me\"}").RootElement };
        var respBtn = PlaceUi.Execute(reqBtn);
        Assert.IsTrue(respBtn.ok);

        // Bind Button.m_TargetGraphic to its Text (Graphic)
        var bindJson = "{\"scenePath\":\"Canvas/Button\",\"componentType\":\"UnityEngine.UI.Button, UnityEngine.UI\",\"propertyPath\":\"m_TargetGraphic\",\"reference\":{\"scenePath\":\"Canvas/Button/Text\"}}";
        var req = new McpRequest { id = "bind1", cmd = "bind_reference", args = JsonDocument.Parse(bindJson).RootElement };
        var resp = BindReference.Execute(req);
        Assert.IsTrue(resp.ok, string.Join(";", resp.errors ?? System.Array.Empty<string>()));
    }
}
