using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Text.Json;
using McpBridge.Editor.Core;
using McpBridge.Editor.Commands;

public class InstantiatePrefabCommandTests
{
    string _assetPath;

    [SetUp]
    public void Setup()
    {
        // Create a simple prefab under Project/Assets for testing
        var go = new GameObject("PrefabRoot", typeof(RectTransform), typeof(Image));
        _assetPath = "Assets/Test_Prefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, _assetPath);
        Object.DestroyImmediate(go);
        AssetDatabase.ImportAsset(_assetPath);
    }

    [TearDown]
    public void TearDown()
    {
        if (!string.IsNullOrEmpty(_assetPath))
        {
            AssetDatabase.DeleteAsset(_assetPath);
        }
    }

    [Test]
    public void Instantiate_ByGuid_Works()
    {
        var guid = AssetDatabase.AssetPathToGUID(_assetPath);
        var json = $"{{\"prefabGuid\":\"{guid}\",\"parentPath\":\"Canvas\"}}";
        var req = new McpRequest { id = "inst1", cmd = "instantiate_prefab", args = JsonDocument.Parse(json).RootElement };

        // Ensure Canvas via place_ui once
        var reqCanvas = new McpRequest { id = "p", cmd = "place_ui", args = JsonDocument.Parse("{\"parentPath\":\"Canvas\",\"kind\":\"panel\"}").RootElement };
        PlaceUi.Execute(reqCanvas);

        var resp = InstantiatePrefabCmd.Execute(req);
        Assert.IsTrue(resp.ok, string.Join(";", resp.errors ?? System.Array.Empty<string>()));
    }
}
