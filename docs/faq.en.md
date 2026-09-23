# Frequently asked questions

[简体中文](faq.md) · [README](../README.en.md) · [Report an issue](https://github.com/csvwolf/talaria/issues)

This FAQ is versioned with the source. Submit questions through Issues or improvements through PRs.

## Steam Input versus Xbox output

Steam Input is Steam's own mapping system. Talaria creates an additional virtual Xbox controller; it does not convert the physical Steam Controller. Avoid competing mappings for the same game. Without Talaria Xbox output, keyboard, mouse and macros remain available; Steam Input or native game support can provide controller input.

<a id="steam-desktop-conflicts"></a>

## Why do Steam and Talaria produce duplicate input or overlapping haptics?

Both can read the same controller. Enabling Talaria means it outputs its own mappings; it does not claim exclusive physical-device access or modify Steam's desktop layout. One action can therefore trigger both mouse, keyboard or haptic mappings, causing duplicate clicks, fast scrolling or unexpected feedback. This is separate from enabling Talaria's virtual Xbox output.

### Why does a single click become a double-click?

If the same controller button or trackpad press is bound to left-click in both Steam's desktop layout and Talaria, each app may send a click from one press and release. The target app may interpret those as a double-click. Check right-click and shortcut bindings for duplicates too.

Check both **button bindings and trackpad press actions** so only one mapping system outputs the action in that context. Disabling trackpad cursor movement does not remove its press-to-click binding. Pause Talaria for comparison, then back up and adjust Steam's desktop layout as described below. Changing the system double-click speed should not be the first workaround for duplicate input.

### How should I set up Talaria for desktop use?

1. Open Steam **Settings → Controller → Desktop Layout** and save or export your original layout first. Exact labels may vary by Steam version.
2. Apply an empty **Steam desktop layout**, or remove overlapping button, trackpad and haptic bindings individually. Removing mouse actions alone can leave Steam haptics active.
3. Select your device in Talaria, apply your profile and enable input. Check clicks, scrolling and haptics in an ordinary desktop window first.
4. Keep per-game Steam Input layouts. Exclude the actual game executable in Talaria for games that Steam should handle. Leave Big Picture to Steam; there is no need to disable Steam Input globally.

**An empty desktop layout persists; it is not restored automatically on window switches.** Pausing Talaria or entering an excluded desktop app will not bring the original Steam desktop controls back. Restore your saved layout manually when needed. Configure game layouts separately from the desktop layout.

### Recommended setups: non-Steam games or a Steam game with an elevated trainer

For a **non-Steam game**, exit Steam and enable Talaria. Enable Xbox output in the game profile if required. Steam then has no competing mapping output.

For a **Steam game with an administrator-level trainer**, you can keep Steam's desktop and game layouts:

1. Run Steam normally and the trainer as administrator.
2. Enable Talaria's administrator-window control capability (UIAccess, for example through the installer's local-signing option). Standard mode alone cannot cross this integrity boundary.
3. Use Talaria inclusion mode and add only the trainer's actual executable, not the game. Use keyboard/mouse mappings for the trainer; Xbox output is unnecessary.
4. Talaria handles the foreground trainer. Returning to the game stops Talaria output and leaves game input to Steam Input.

Windows UIPI restrictions prevent ordinary, unelevated Steam keyboard/mouse injection from controlling the elevated trainer. This separation avoids two sets of clicks reaching that window without clearing Steam's desktop layout. If Steam is also elevated or the trainer is not elevated, that condition no longer holds; check overlapping bindings as you would for an ordinary window.

This boundary restricts window input; **it does not block Steam from sending controller haptics**. If feedback overlaps, adjust the overlapping Steam desktop haptic settings. Inclusion mode still does not claim exclusive device access.

### Does inclusion mode solve this? I want to keep Steam desktop controls.

Inclusion mode narrows the overlap, but does not provide exclusive access inside the list:

- Outside the list, Talaria sends no actions; Steam follows its own rules.
- Inside the list, Talaria outputs its mappings. If Steam's desktop layout also applies, duplicate input remains possible.
- An app profile chooses Talaria's mappings; it does not disable Steam's layout.

If Steam normally handles your desktop, pause Talaria and enable it when needed, or use inclusion mode while avoiding overlapping bindings in included apps. Exclusion mode also controls Talaria only; excluding `steam.exe` does not exclude all Steam games.

### Could exclusive access inside the list switch silently?

**Exclusive access for included apps is not currently implemented.** Inclusion mode controls output and does not take the controller away from Steam. A future approach that closes shared access and reacquires the device might need to cycle its connection while Steam holds it open. That could cause Windows device sounds, temporary input loss or game re-enumeration; silent, seamless switching cannot be promised.

**Keep Xbox controller connected when switching windows** only controls Talaria's **virtual controller**. It does not block Steam desktop mappings or prevent sounds caused by reconnecting the physical controller.

### What about recording Steam haptics?

Pause Talaria input and other haptic tests, then restore the Steam desktop layout you want to record so Steam provides the feedback. After recording, reapply the empty desktop layout or remove overlapping settings before enabling Talaria for desktop use again. Recording does not automatically switch, back up or restore Steam layouts.

### How can I confirm a mapping conflict?

Compare the same actions in the same window: pause Talaria and test Steam alone, then back up and clear Steam's desktop layout and test Talaria alone. If both work separately but behave incorrectly together, inspect overlapping bindings. Haptic strength alone does not identify its source; controller firmware can also generate feedback. Include connection type, whether Steam is running, both layouts and reproduction steps when reporting a problem.

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
