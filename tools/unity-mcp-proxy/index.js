#!/usr/bin/env node
import { Server, StdioServerTransport, Tool } from "@modelcontextprotocol/sdk";
import net from "net";
import fs from "fs";

function readDiscovery() {
  const env = process.env.UNITY_MCP_DISCOVERY;
  const candidates = [
    env,
    process.cwd() + "/.mcp/unity-mcp.json",
    process.env.APPDATA ? process.env.APPDATA + "/autoUI/mcp/unity-mcp.json" : null
  ].filter(Boolean);
  for (const p of candidates) {
    try { const j = JSON.parse(fs.readFileSync(p, "utf8")); return j; } catch {}
  }
  throw new Error("Unity discovery file not found");
}

function makeUnityClient() {
  const disc = readDiscovery();
  const host = disc.host || "127.0.0.1";
  const port = disc.port || 17890;
  return {
    request: (cmd, args) => new Promise((resolve, reject) => {
      const socket = net.createConnection({ host, port }, () => {
        const req = { id: String(Date.now()), cmd, args: args || {} };
        socket.write(JSON.stringify(req) + "\n");
      });
      let buf = "";
      socket.on("data", (d) => {
        buf += d.toString("utf8");
        const i = buf.indexOf("\n");
        if (i >= 0) {
          const line = buf.slice(0, i);
          try { const json = JSON.parse(line); resolve(json); } catch (e) { reject(e); }
          socket.end();
        }
      });
      socket.on("error", reject);
      socket.on("end", ()=>{});
    })
  };
}

const tools = {
  get_scene_graph: new Tool({ name: "get_scene_graph", description: "Fetch Unity scene graph", inputSchema: { type: "object", properties: { includeComponents: { type: "boolean" } } } }),
  place_ui: new Tool({ name: "place_ui", description: "Place UI element", inputSchema: { type: "object" } }),
  instantiate_prefab: new Tool({ name: "instantiate_prefab", description: "Instantiate a prefab by GUID", inputSchema: { type: "object" } }),
  bind_reference: new Tool({ name: "bind_reference", description: "Bind a serialized reference", inputSchema: { type: "object" } }),
  validate_layout: new Tool({ name: "validate_layout", description: "Run layout validation", inputSchema: { type: "object" } }),
  create_script: new Tool({ name: "create_script", description: "Create C# script and optionally attach", inputSchema: { type: "object" } })
};

const server = new Server({ name: "unity-mcp-proxy", version: "0.1.0" }, {
  capabilities: { tools: Object.values(tools).map(t=>t.getInfo()) }
});

const client = makeUnityClient();

for (const [name, tool] of Object.entries(tools)) {
  server.setRequestHandler(tool.getName(), async (input) => {
    const resp = await client.request(name, input || {});
    if (resp && resp.ok) return resp.data ?? { ok: true };
    throw new Error(resp?.errors?.join("; ") || "Unknown MCP bridge error");
  });
}

const transport = new StdioServerTransport();
server.connect(transport).then(() => {
  console.error("unity-mcp-proxy started (stdio)");
}).catch(err => {
  console.error("Failed to start unity-mcp-proxy:", err);
  process.exit(1);
});

