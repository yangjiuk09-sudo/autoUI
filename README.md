# autoUI MCP Bridge

Editor 전용 Unity UPM 패키지(`com.autoUI.mcpbridge`)와 Unity 프로젝트(`Project/`)가 포함된 모노리포입니다. 로컬 TCP 기반 MCP 브릿지(127.0.0.1:17890), 명령 핸들러(6종), 기본 검증 규칙, GameCI 워크플로, Cursor/프록시 연동을 제공합니다.

## 구성
- UPM 패키지: `Packages/com.autoUI.mcpbridge`
- Unity 프로젝트: `Project/`
- GameCI 워크플로: `.github/workflows/unity-validate.yml`
- Cursor/프록시: `.cursor/`, `tools/unity-mcp-proxy/`
- 디스커버리: `.mcp/unity-mcp.json` (자동 생성)

## 빠른 시작
1) Unity 2022.3.56f1로 `Project/` 열기 → 메뉴 `MCP Bridge > Start Server`
2) 자동 시작/포트 설정: `MCP Bridge > Auto Start on Load`, `Use Auto Port (0)`
3) 서버 시작 시 `.mcp/unity-mcp.json` 생성됨

TCP(JSON line): `{id, cmd, args}`
- 예) place_ui: Canvas 보장 + 버튼 생성

## Cursor 연동(MCP 프록시)
- Node 설치 후: `cd autoUI/tools/unity-mcp-proxy && npm i`
- Cursor 설정 예시: `autoUI/.cursor/mcp.example.json`
- 환경변수: `UNITY_MCP_DISCOVERY=${workspaceFolder}/.mcp/unity-mcp.json`

## CI
- GitHub Actions(GameCI)로 EditMode 테스트 실행: `.github/workflows/unity-validate.yml`
- `UNITY_LICENSE` 시크릿 필요

## 개발 가이드
- Editor 전용 코드는 `#if UNITY_EDITOR`와 `McpBridge.Editor.asmdef`(includePlatforms: Editor)에 한정
- 프리팹은 `PrefabUtility.InstantiatePrefab`으로 링크 유지
- 스크립트 생성은 `AssetDatabase.ImportAsset`으로 컴파일 트리거

