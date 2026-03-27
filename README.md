# BeatSaberPlus HTTP Hook

A [BeatSaberPlus](https://github.com/hardcpp/BeatSaberPlus) module that adds HTTP webhook triggers to Chat Integrations. External tools, scripts, or devices can fire Chat Integration events by sending a simple HTTP request.

## Features

- Starts an HTTP server when Beat Saber launches
- Adds an **HTTPHook** event type to Chat Integrations
- Named hooks allow multiple independent triggers (e.g. `lights-on`, `confetti`, `scene-switch`)
- Works with any tool that can make HTTP requests (curl, PowerShell, StreamDeck, Home Assistant, etc.)

## Installation

1. Requires [BeatSaberPlus](https://github.com/hardcpp/BeatSaberPlus) with Chat Integrations enabled
2. Copy `BeatSaberPlus_HTTPHook.dll` and `manifest.json` to your Beat Saber `Plugins/` folder
3. Launch Beat Saber and enable **HTTP Hook** in the BeatSaberPlus settings

## Usage

### 1. Create a hook event

1. Open **BeatSaberPlus** > **Chat Integrations**
2. Create a new event of type **HTTPHook**
3. Set the **Hook Name** (e.g. `boop`)
4. Add any actions you want triggered (send chat message, emote rain, OBS scene switch, etc.)
5. Enable the event

### 2. Trigger it

Send an HTTP request to `http://localhost:2948/hook/{hookName}`:

```bash
# curl
curl http://localhost:2948/hook/boop

# PowerShell
Invoke-WebRequest http://localhost:2948/hook/boop

# POST also works
curl -X POST -H "Content-Length: 0" http://localhost:2948/hook/boop
```

The server responds with `{"ok":true}` on success.

### Multiple hooks

Create multiple HTTPHook events with different names. Each one triggers independently:

```bash
curl http://localhost:2948/hook/lights-on
curl http://localhost:2948/hook/confetti
curl http://localhost:2948/hook/scene-switch
```

## Configuration

The default port is **2948**. You can change it in the HTTP Hook settings panel within BeatSaberPlus.

The server listens on all network interfaces, so you can trigger hooks from other devices on your local network using your PC's IP address:

```bash
curl http://192.168.1.100:2948/hook/boop
```

## Building from source

1. Clone this repository
2. Create a `Refs` folder (or junction/symlink) pointing to your Beat Saber installation directory
3. Open `BeatSaberPlus_HTTPHook.sln` in Visual Studio or build with:
   ```
   dotnet msbuild BeatSaberPlus_HTTPHook.csproj -p:Configuration=Release
   ```
4. Output: `bin/Release/BeatSaberPlus_HTTPHook.dll`

### Required references (resolved via `Refs/`)

- `Plugins/ChatPlexSDK_BS.dll`
- `Plugins/BeatSaberPlus_ChatIntegrations.dll`
- `Beat Saber_Data/Managed/IPA.Loader.dll`
- `Beat Saber_Data/Managed/UnityEngine.dll`
- `Beat Saber_Data/Managed/UnityEngine.CoreModule.dll`
- `Beat Saber_Data/Managed/Unity.TextMeshPro.dll`
- `Libs/Newtonsoft.Json.dll`

## Compatibility

- Beat Saber 1.40.x
- BeatSaberPlus / ChatPlexSDK_BS 6.4.2+

## License

MIT
