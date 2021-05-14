# ReduceUI
 
A mod for Receiver 2 that allows you to hide individual UI elements. The list of options includes:
- Inventory slots (frame, numbers, empty indicators, and holster frame can be toggled individually)
- Rank icon
- Tape info (frame, icon, counter, and queue again individually)
- Subtitle region (frame and/or waveform icon)
- Various help texts, that (as far as I can tell) have no in-game settings to turn off

## Install

Install [BepInEx](https://github.com/BepInEx/BepInEx) [(the x64 version)](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.11) into the Receiver 2 folder (the one containing `Receiver2.exe`), then start and exit Receiver 2 to have BepInEx generate its folder structure.
Then place the ReduceUI folder in `BepInEx/plugins`.

It is recommended to use [BepInEx's Config Manager](https://github.com/BepInEx/BepInEx.ConfigurationManager) (installed the same way as this mod) to have an in-game UI to change the mod's settings.

## Dependencies (for developers)

The source code depends on `BepInEx.dll`, `UnityEngine.dll`, `UnityEngine.CoreModule.dll`, `UnityEngine.InputLegacyModule.dll`, `0Harmony.dll`, `UnityEngine.UI`, and `Wolfire.Receiver2.dll`. It is set up to expect these DLLs to be located in the parent folder of the repository root folder. All of these DLLs can be found as part of either Receiver 2's or BepInEx's install.
