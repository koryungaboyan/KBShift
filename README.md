# KBShift

KBShift is a lightweight Windows utility designed to make cycling through keyboard input languages faster and more customizable. It allows users to define specific modifier keys (like Left Alt+Shift, Right Alt+Shift, or Ctrl+Shift) to cycle through different sets of languages.

## Features

- **Custom Hotkeys**: Assign separate hotkeys for different language groups.
- **Language Groups**: Cycle through all installed languages or specific subsets.
- **Dark/Light Theme**: Automatically matches your Windows theme for a seamless experience.
- **On-Screen Keyboard Shortcut**: Quick access to the Windows On-Screen Keyboard.
- **System Tray Integration**: Runs quietly in the background with quick access to settings.
- **Universal Compatibility**: Built on .NET Framework 4.6.2 (x86) to work on Windows 7, 10, and 11 (32-bit and 64-bit).

## Installation

### Using the Installer (Recommended)

To install KBShift, you can simply download and run the pre-built installer:

1. Navigate to the `Installer/` folder in this repository.
2. Download `KBShift_Setup.exe`.
3. Run the installer and follow the on-screen instructions.
4. You can choose to have KBShift start automatically with Windows during installation.

### From Source

1. Clone this repository.
2. Open the solution in Visual Studio 2022.
3. Restore NuGet packages (requires `Newtonsoft.Json`).
4. Build the solution in `Release` mode targeting `x86`.
5. Run `KBShift.UI.exe` from the output directory.

## Usage

1. Open KBShift from the Start Menu or System Tray.
2. Select your preferred hotkeys for "Layout Group 1" and "Layout Group 2".
3. Check the languages you want to include in each group.
4. Use the assigned hotkeys to cycle through your chosen languages instantly.

## Developed By

**Koryun Gaboyan** - [GitHub Profile](https://github.com/koryungaboyan)

## License

This project is open source and available for personal use.
