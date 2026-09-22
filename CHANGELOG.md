# Changelog

## 0.2.15

- Support SC2 input over Bluetooth, USB and Puck/receiver; hide unconfirmed receiver slots and remove stale offline choices.
- Pause firmware keyboard/mouse emulation on the selected USB/receiver interface during standalone virtual Xbox output, as with Bluetooth; restore on release.
- Rename the tray action to Enable/Pause controller input, consistent with the main window.
- Simplify device selection: remove experimental labels and the long connection hint. Device diagnostics are available in About.
- Move runtime and diagnostic logs to `%LOCALAPPDATA%/Talaria/logs`; copy legacy logs without overwriting existing files or deleting originals. Preserve profiles and local signing.
- Update Chinese and English setup and connection documentation. USB gamepad prompts verified with the physical mouse idle. USB/receiver haptics remain unavailable.

## 0.2.14

- Migrate logs from the legacy PadHop directory to Talaria and update all log-opening actions.

## 0.2.13

- Extend standalone firmware keyboard/mouse suppression to selected USB and receiver SC2 state-report interfaces, including HID fallback. Steam must be absent; haptics remain separately gated.
- Remove disconnected connection placeholders from the device list; preserve the remembered selection in status only.

- Show only state-confirmed receiver slots in the device picker; retain all candidates in diagnostics.
- Consolidate the device diagnostic log entry under About.

## 0.2.12

- Merge SetupAPI HID discovery with Raw Input, deduplicated by interface path.
- Probe candidate receiver/HID-only slots with bounded shared read-only input; report confirmed state, no state yet or read failure.
- Add selected-interface HID read fallback when Raw Input is absent. No exclusive access, firmware or haptic writes on fallback. Hardware compatibility remains experimental.

## 0.2.11

- Add experimental SC2 USB/receiver Raw Input eligibility (1302–1305) and path-based transport labels adapted from SteamlessController.
- Validate equivalent USB/BLE state mappings; retain exact selected-collection isolation and BLE-only firmware/haptic commands.
- Direct HID discovery/read fallback is not included; empty receiver slots may be listed.

## 0.2.10

- Device refresh writes a shareable HID diagnostic log including product names, VID/PID/Usage and filtering reasons, with paths omitted and common address/ID patterns redacted.
- Add an Open device diagnostics button; preserve BLE-only takeover filtering.

## 0.2.9

- Add opt-in current-user Windows sign-in startup in About, disabled by default. Starts in tray without enabling takeover; duplicate startup does not raise the existing window.
- Uninstall removes the matching current-user startup entry.

## 0.2.8

- Distinguish built-in presets from personal profiles with persisted origin.
- Reset built-in defaults into the editor; saved and applied snapshots stay unchanged until explicitly saved/applied.
- Preserve all legacy profiles as personal entries because older versions did not track their origin.

## 0.2.7

- Add per-pad drag-start tolerance to prevent click jitter from turning double clicks into drags; release settling does not delay button edges.
- Apply saved application rules to active takeover immediately; mode switches save and apply automatically. Empty whitelist stays independent of game profiles.
- Preserve existing pointer speed, haptic recordings and custom settings.

## 0.2.6

- Correct physical Menu/View input bit routing.
- New profiles copy current edits; the mouse starter preset includes Steamless feel, without a separate automatically added reference preset.

- Pause SC2 firmware keyboard/mouse emulation during standalone virtual Xbox takeover and restore it on release. Never overwrite settings while Steam is running.
- Add per-trackpad scroll inversion and reversal hysteresis to suppress jitter without losing fine same-direction scrolling.
- Reset scroll state on release, press and focus changes. Persist direction and buffer with profiles.
- Add local numeric input summaries (no keys or paths) to diagnose gamepad/keyboard prompt switching; Metaphor now keeps controller prompts in the user's BLE/no-Steam test.

## 0.2.5 — Talaria

- Rename the app to Talaria with the approved anime mecha wing icon in the UI, tray and installer.
- Preserve PadHop installation, settings and signing identities for upgrades.
- Update GitHub links and accept both old and new release asset URLs.
- Older versions need one manual installer download after the repository rename.

## 0.2.4 — experimental

- 软件、安装向导及本机签名维护支持简体中文 / English；关于页可选择语言，重启生效。
- 新增英文 README 与双向语言链接，品牌增加 Steam Controller 副标题。
- 翻译覆盖参数帮助、映射编辑、录制提示和错误信息；用户配置名与路径保持原样。

## 0.2.3 — experimental

- 新增「关于」：版本、项目/作者链接、日志入口、证书维护及更新；可选自动检查并提示，下载和安装分别由用户主动点击。

- 关于页显示本机签名有效期，提前 30 天提醒，支持管理员确认后的一键续期。
- 续期重新签名并清理旧信任与私钥，保留用户配置；取消提权不改变签名。

## 0.2.2 — experimental

- 本机自签移至安装组件选择页；明确说明重新运行安装程序可启用或撤销，个人配置保留。
- 静默安装改用组件参数，仍要求显式同意本机信任变更。

## 0.2.1 — experimental

- Add opt-in local signing in the installer with explicit trust disclosure, one-year local certificates, private-key cleanup, rollback and uninstall cleanup.
- Include the UIAccess payload and recovery script; users do not need to compile source.

## 0.2.0 — experimental

- Name the project PadHop and separate source, user data and build outputs.
- Add reproducible dependency restoration, standard/UIAccess build modes and installable ZIP packaging.
- Add MIT license, third-party notices and privacy/release documentation.
- Add sanitized single-preset sharing and limit default engine logs.
- Configure optional capture tools per user and retain diagnostic recovery state.

- Add a Chinese native install.exe with upgrade/uninstall and optional bundled official ViGEmBus installation; preserve existing shared drivers.
- Validate UIAccess publisher signatures again on the target machine; release assets contain install.exe only.
