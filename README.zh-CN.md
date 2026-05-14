# jir-package

这里放的是 `jir` 的 Windows 打包脚本。简单说，它负责把已经编译好的 `jir-cli.exe` 包成一个带界面的安装程序，让 Windows 用户可以点几下就装好 `jir`，不用自己手动配路径。

`jir` 本身是一个 Java Runtime Manager，也就是帮你更快地管理、切换和使用 JDK 的小工具。这个仓库关注的是“怎么把它在 Windows 上舒服地装起来”。

## 这个项目里有什么

- `windows/build-installer.ps1`：打包入口。它会先执行 `cargo build --release`，然后把生成的 `jir-cli.exe` 和卸载器一起塞进安装器里。
- `windows/JirSetup.cs`：Windows 图形安装器。可以选择安装路径、是否把 `jir` 加进 `PATH`、是否设置 `JAVA_HOME`。
- `windows/JirUninstall.cs`：Windows 图形卸载器。卸载时会清理安装目录和相关环境变量。
- `LICENSE`：MIT 协议。

## 打包前需要准备什么

你需要在 Windows 上准备好这些东西：

- Rust 工具链，也就是能正常运行 `cargo build --release`
- .NET Framework 自带的 `csc.exe`
- PowerShell

通常 Windows 自带的 .NET Framework 里就能找到 `csc.exe`。如果脚本找不到，会直接报错提醒你。

## 怎么打包

在 PowerShell 里运行：

```powershell
.\windows\build-installer.ps1
```

如果要指定版本号：

```powershell
.\windows\build-installer.ps1 -Version 0.1.0
```

脚本会做几件事：

1. 编译 `jir-cli.exe`
2. 编译 `uninstall.exe`
3. 把 `jir-cli.exe` 和 `uninstall.exe` 嵌进最终安装器
4. 输出一个 Windows GUI 安装包

默认产物名类似：

```text
dist\jir-0.1.0-windows-x64-gui-setup.exe
```

## 安装器会做什么

运行安装器后，你可以选择安装目录。默认会装到当前用户目录下的：

```text
%LOCALAPPDATA%\Programs\jir
```

安装时可以勾选：

- `Add jir to PATH`：把 `jir` 加到当前用户的 `PATH`
- `Set JAVA_HOME`：把 `JAVA_HOME` 指向 `jir` 管理的占位目录

如果需要改系统级的 `JAVA_HOME` 或清理系统级 JDK 路径，安装器会提示你用管理员权限重新启动。

## 卸载

安装目录里会带一个 `uninstall.exe`。运行它就可以卸载 `jir`，并清理这个安装器写入的环境变量。

如果系统级环境变量也指向这个安装位置，卸载器同样会提示使用管理员权限。

## 开发时的小提醒

- 打包产物、临时构建目录、`target/` 这些都不应该提交。
- 如果只是改安装器界面，主要看 `windows/JirSetup.cs`。
- 如果是改卸载清理逻辑，主要看 `windows/JirUninstall.cs`。
- 如果打包路径或产物名要调整，优先看 `windows/build-installer.ps1`。

## 协议

MIT License。
