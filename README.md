# jir-package

This repo contains the Windows packaging bits for `jir`. In plain English: it takes the built `jir-cli.exe` and wraps it in a small graphical installer, so Windows users can install `jir` with a few clicks instead of wiring up paths by hand.

`jir` is a Java Runtime Manager. It helps you install, switch, and manage JDKs quickly. This repo focuses on making the Windows install experience smooth.

[中文说明](README.zh-CN.md)

## What's Inside

- `windows/build-installer.ps1`: the packaging entry point. It runs `cargo build --release`, then bundles the generated `jir-cli.exe` with the uninstaller.
- `windows/JirSetup.cs`: the Windows GUI installer. It lets users choose an install path, add `jir` to `PATH`, and optionally set `JAVA_HOME`.
- `windows/JirUninstall.cs`: the Windows GUI uninstaller. It removes the installed files and cleans up environment variables.
- `LICENSE`: MIT license.

## Requirements

You'll need these on Windows:

- Rust toolchain, with `cargo build --release` working
- `csc.exe` from .NET Framework
- PowerShell

On most Windows machines, `csc.exe` is already available somewhere under the .NET Framework directory. If the script cannot find it, it will stop and tell you.

## Build The Installer

Run this in PowerShell:

```powershell
.\windows\build-installer.ps1
```

To set a version explicitly:

```powershell
.\windows\build-installer.ps1 -Version 0.1.0
```

The script will:

1. Build `jir-cli.exe`
2. Build `uninstall.exe`
3. Embed both files into the final installer
4. Write out a Windows GUI setup executable

The default output looks like this:

```text
dist\jir-0.1.0-windows-x64-gui-setup.exe
```

## What The Installer Does

When you run the installer, you can choose where `jir` should be installed. By default, it installs to:

```text
%LOCALAPPDATA%\Programs\jir
```

During installation, you can choose:

- `Add jir to PATH`: adds `jir` to the current user's `PATH`
- `Set JAVA_HOME`: points `JAVA_HOME` to the placeholder directory managed by `jir`

If system-level `JAVA_HOME` or old JDK paths need to be changed, the installer will ask to restart with administrator permissions.

## Uninstall

The install directory includes `uninstall.exe`. Run it to remove `jir` and clean up the environment variables written by the installer.

If system-level environment variables point to this installation, the uninstaller will also ask for administrator permissions.

## Notes For Development

- Build outputs, temporary directories, and `target/` should not be committed.
- If you're changing the installer UI, start with `windows/JirSetup.cs`.
- If you're changing cleanup behavior, start with `windows/JirUninstall.cs`.
- If you're changing output paths or package names, start with `windows/build-installer.ps1`.

## License

MIT License.
