<div align="center">
  <img src="assets/app.png" alt="Talaria icon" width="96" height="96" />
  <h1>Talaria · Steam Controller utility</h1>
  <p>Make Steam Controller 2 useful in more places.</p>

[![Windows build](https://github.com/csvwolf/talaria/actions/workflows/windows.yml/badge.svg?branch=master)](https://github.com/csvwolf/talaria/actions/workflows/windows.yml)
[![Release](https://img.shields.io/github/v/release/csvwolf/talaria?include_prereleases&label=release&color=5ecbf5)](https://github.com/csvwolf/talaria/releases)
[![Downloads](https://img.shields.io/github/downloads/csvwolf/talaria/total?color=5ecbf5)](https://github.com/csvwolf/talaria/releases)
[![MIT](https://img.shields.io/badge/license-MIT-5ecbf5)](LICENSE)
[![Windows](https://img.shields.io/badge/Windows-10%20%2F%2011-0078d4)](#installation)

**English · [简体中文](README.md)**

**[Download installer](https://github.com/csvwolf/talaria/releases) · [Report an issue](https://github.com/csvwolf/talaria/issues)**

</div>

A Windows input utility for Steam Controller 2: global and per-app profiles, independent trackpads and haptics, keyboard/mouse bindings, Windows shortcuts, macros, the system on-screen keyboard, and optional virtual Xbox controller output. This is an independent project, not affiliated with Valve.

**Steam Controller 2 input is supported over Bluetooth, USB and Puck/receiver connections.** Haptic feedback and recording currently require Bluetooth; USB/receiver connections support input mapping and virtual Xbox output. Steam Deck/Moonlight forwarded devices are excluded.

## Interface preview

Actual application screens with default settings. Screenshots show Simplified Chinese; the app also supports English.

**Input status** — Select a device and enable or pause controller input.

![Input status](assets/screenshots/status.png)

**Controller settings** — Select buttons by location, edit mappings, and try, apply or save profiles. Trackpad and haptic settings are on the same page.

![Controller settings](assets/screenshots/controller.png)

**Application rules** — Switch between inclusion and exclusion modes; import applications by clicking or dragging them in.

![Application rules](assets/screenshots/rules.png)

**App profiles** — Give each application or game its own button, trackpad and haptic settings. Applications without a profile use global settings; importing does not change inclusion/exclusion rules.

![App profiles](assets/screenshots/app-profiles.png)

## Installation

Requires Windows 10 1903+ / Windows 11 x64 and .NET Framework 4.8. Download `install.exe` from Releases. The installer supports English and Simplified Chinese. Install as administrator; the application normally runs unelevated. Upgrade by running the new installer. Installation and upgrades preserve profiles. Uninstall through Windows Installed apps.

- Standard mode controls ordinary windows. The optional **Administrator-window control / local signing** component creates a certificate on your PC and enables UIAccess after explicit consent. It is off by default on a fresh installation; Steam does not need to run as administrator.
- Local certificate trust affects all users of this PC. Misuse of the app or input path could affect administrator programs. Self-signing does not establish a public publisher identity or guarantee removal of security warnings. Only three Talaria executables are signed. The non-exportable private key is deleted after normal completion; no shared private key is distributed. UAC, Secure Boot and driver-signing policy are unchanged.
- Local certificates last one year. About shows the expiry date and offers renewal, with reminders during the final 30 days. Renewal requires administrator confirmation, closes Talaria, re-signs it and removes old trust. Reopen the app afterward. Run the installer again to enable or revoke local signing. Uninstall removes its recorded certificate. For interrupted operations, keep `.local-signing` and inspect `local-signing-last.log`; `Local-Signing.ps1 -Action Disable` restores original files, while `-Action RemoveTrust` only revokes trust before reinstallation.
- Optional Xbox output installs the bundled official ViGEmBus 1.22.0 driver only if needed. The driver is retired and future Windows compatibility is not guaranteed. Uninstalling Talaria keeps this shared driver. Driver failures and required restarts are reported. Choose output per application to avoid duplicate virtual controllers from Steam and Talaria.

## Profiles and language

Select your connected SC2 on Input status. Under Controller settings, choose or create a profile. **Try** previews the edited parameters; **Apply** controls the active profile; **Save** overwrites the named profile; **Save as** creates a copy. Trackpad actions and haptics belong to the same controller profile.

Exclusion mode handles applications except those excluded; inclusion mode handles only explicitly included applications. App profiles are independent of these rules. Steam ordinary windows follow app rules; Big Picture and overlays remain with Steam.

In **About**, select System, 简体中文 or English, then reopen Talaria. Existing profile names, application paths and mappings are not translated or reset. About also includes project/author links, log access, signature renewal and update controls.

Automatic update checks are **on by default** and can be disabled in About. Your choice is remembered. When enabled, Talaria checks at startup and every 24 hours while running, and only notifies you. Download and installation each require an explicit click. Downloads are checked against GitHub's SHA-256 digest, and installation still requires administrator confirmation. No logs or profiles are uploaded.

Closing the window minimizes to tray. Launching again activates the existing instance. Data lives in `%LOCALAPPDATA%/PadHop` and survives uninstall. Use Share current profile when sharing with others; a full backup includes local application paths. Logs may contain paths and device identifiers: review before sharing.

## Recording components and logs

Ordinary mappings and presets do not need recording components. For Bluetooth haptic recording, open the recording section under **Controller settings → Haptic feedback**:

- On first use, click **Install recording components**. After confirmation, Talaria downloads, verifies, installs and configures the dependencies. The button shows the current stage; Microsoft component installation may require administrator confirmation.
- Once configured, the button becomes **Check recording components**. You can record immediately or run the check; checking does not download or reinstall anything. A failed check offers **Repair recording components**.
- The private Python runtime does not change system PATH. Existing usable components are reused.
- **Open setup log** displays results inside Talaria with refresh and copy actions. **About → Diagnostics and logs** also opens recent logs and device diagnostics. Log files live in `%LOCALAPPDATA%/Talaria/logs`.

See the [recording guide](docs/capture.md) for capture and recovery details. Raw Bluetooth traces may contain other devices' data; do not publish entire capture directories.

## Build from source

Run in Windows PowerShell:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/bootstrap-installer.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build-installer.ps1
```

The build uses the system .NET Framework C# compiler, pinned NuGet dependencies, and SHA-256 checks for native DLLs. Initial dependency restoration requires internet access. Application binaries are in `bin/`; the installer is `dist/install.exe`. English translations are maintained in `languages/en.json`; the build embeds them in the application and input engine. Installer language strings live in `installer/PadHop.iss`.

Click **Install recording components** in the recording section to download, verify and configure Python and Microsoft Bluetooth analysis tools. Existing components are reused; failures provide a setup log and can be retried. Raw ETL/PCAP traces can include other Bluetooth traffic and sensitive device data; do not publish raw capture directories. Prefer exported profile parameters. Detailed technical guides (currently Chinese): [recording](docs/capture.md), [privacy](docs/privacy.md), [release validation](docs/release.md), [local signing recovery](docs/local-signing.md).

## License and acknowledgments

Talaria is [MIT licensed](LICENSE). Trackpad behavior references SteamlessController; virtual output uses ViGEmClient. The optional official ViGEmBus installer is BSD licensed. Bluetooth WPR profiles derive from Microsoft busiotools. See [third-party notices](THIRD-PARTY-NOTICES.txt) for attribution and modifications. Microsoft's BTETLParse is not redistributed. The app uses the approved wing emblem. See [branding provenance](branding/README.md) for the generation prompt.

## Upgrading from PadHop

Talaria is the new name for PadHop. Run the new `install.exe` to upgrade in place. Settings, language preference and installation identity are retained; installation and profile directories still use `PadHop`; logs now use `%LOCALAPPDATA%/Talaria/logs`, accessible from About. Legacy logs are copied without deleting originals. Older updaters do not accept the renamed download URL: download this upgrade manually from Releases once. Automatic checks only notify; downloading and installing remain explicit actions.

## Scrolling and game prompts

Each scrolling trackpad has independent wheel/natural direction and reversal buffering (default 0.4%; 0 disables it). Small reverse jitter is ignored. These settings are saved with the whole profile. New profiles copy current edits; the mouse starter preset includes Steamless reference feel.

With Steam closed, virtual Xbox takeover temporarily disables the selected SC2 firmware keyboard/mouse emulation, preventing duplicate gamepad and keyboard events. Leaving the active app, pausing or exiting normally restores firmware defaults. Settings are not overwritten while Steam is running: avoid competing Steam/Talaria mappings. Explicit keyboard/mouse bindings such as L4 → Win still intentionally switch game prompts.

Device discovery merges Raw Input and HID interfaces. Selected devices absent from Raw Input use shared read-only HID fallback. Only receiver slots that deliver valid controller state during refresh are shown. Turn the controller on, then refresh. Select the device again after changing connection type. For missing devices, open device diagnostics in About and review device names before sharing; do not share the entire logs folder.
