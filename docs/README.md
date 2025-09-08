# MCP Editor Bridge (com.autoUI.mcpbridge)

Editor-only MCP bridge for Unity 2022.3: TCP server, UI placement, prefab instantiation, reference binding, layout validation, and script generation.

## Quick Start
- Open Unity (2022.3.56f1) with this repo as project (`Project/`).
- Menu: MCP Bridge > Start Server.
- Connect to 127.0.0.1:17890 and send JSON lines `{id, cmd, args}`.

## Commands
- get_scene_graph
- place_ui
- instantiate_prefab
- bind_reference
- validate_layout
- create_script

## Samples
- Samples~/ExampleScene contains `QuitAfterHold.cs` long-press example.


## Cursor/프록시 자동 연결
- Unity에서 MCP Bridge > Auto Start on Load 활성화하고 포트를 자동(0) 또는 기본(17890)으로 설정하세요.
- 서버 시작 시 .mcp/unity-mcp.json 디스커버리 파일을 생성합니다.
- Node 프록시(	ools/unity-mcp-proxy)는 MCP stdio 서버로 동작하며 Cursor에서 MCP 서버로 등록합니다.
- Cursor 예시 설정: .cursor/mcp.example.json 참고 (node로 프록시 실행)
- 환경변수 UNITY_MCP_DISCOVERY로 디스커버리 파일 경로를 지정할 수 있습니다.
