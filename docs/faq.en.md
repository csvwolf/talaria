# Frequently asked questions

[简体中文](faq.md) · [README](../README.en.md) · [Report an issue](https://github.com/csvwolf/talaria/issues)

This FAQ is versioned with the source. Submit questions through Issues or improvements through PRs.

## Steam Input versus Xbox output

Steam Input is Steam's own mapping system. Talaria creates an additional virtual Xbox controller; it does not convert the physical Steam Controller. Avoid competing mappings for the same game. Without Talaria Xbox output, keyboard, mouse and macros remain available; Steam Input or native game support can provide controller input.

## Device sounds when switching windows

Windows may play a sound when the virtual controller disconnects or reconnects. The global keep-connected switch on Input status reduces this and defaults to on. Outside active apps, a retained controller sends no actions but remains visible and may occupy a player slot. Pausing or exiting removes it.

## Does each app get a separate Xbox controller?

No. Talaria reuses one virtual controller, releases held inputs and loads the foreground profile. Apps that both need Xbox output reuse it directly. Otherwise the global keep-connected setting determines whether it disconnects; the previous app's profile does not decide this.

## Why can excluded games still see a controller?

Exclusions stop Talaria output; they do not hide physical devices or disable Steam Input. A retained virtual controller may also remain visible. Pause Talaria to remove it during diagnosis. Exclude the game's executable, not just steam.exe.

## A Steam game does not respond

Pause Talaria for comparison, then check Steam device detection, the game's Steam Input mapping and in-game input settings. Device detection and usable mapping are separate questions. Some games may need restarting after reconnecting a controller; save progress first. A failure that persists with Talaria paused should not automatically be attributed to Talaria.

## Why can USB/Puck play haptics but not record them?

Output and recording use different paths. Bluetooth, USB and Puck support sending haptic feedback, but the recorder currently extracts Steam feedback parameters from Windows Bluetooth traces only. Record over Bluetooth, then use the saved profile on any of the three connections. See the [recording validation guide](capture.md) (Chinese).

## Recording components are already installed

Use Check recording components; it does not download or reinstall anything. Failed checks offer repair. First-time installation shows detection, download, verification and installation stages. See the [recording guide](capture.md).

## Sharing diagnostic logs

About provides recent logs, device diagnostics and the logs folder. The recording section provides component setup logs. Viewers support refresh and copy. Files live in `%LOCALAPPDATA%/Talaria/logs`. Include app version, connection type, reproduction steps and the relevant time range. Review paths and device identifiers before sharing; do not upload entire raw Bluetooth capture directories. See [privacy](privacy.md).
