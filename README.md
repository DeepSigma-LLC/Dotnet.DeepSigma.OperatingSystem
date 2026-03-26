# DeepSigma.OperatingSystem

A focused C# utility library for interacting with operating-system features such as terminal commands, local processes, Git, Python discovery/execution, temporary folders, and file-system inspection. The package targets **.NET 10** and is published as **DeepSigma.OperatingSystem**. 

## Highlights

- Run shell commands and capture stdout/stderr
- Check whether a program is installed on the local machine
- Clone Git repositories through the system `git` executable
- Enumerate, query, and kill local processes
- Execute Python scripts and discover Python installations / virtual environments
- Create temporary folders inside the current user's **Downloads** directory
- Model files and directories as objects with metadata and recursive traversal
- Apply simple ignore-style file-system filters
- Force-delete directory trees, including read-only files

## Package

- **Target framework:** `net10.0`
- **Package ID:** `DeepSigma.OperatingSystem`
- **License:** MIT

## Installation

```bash
dotnet add package DeepSigma.OperatingSystem
```

## Namespace

```csharp
using DeepSigma.OperatingSystem;
using DeepSigma.OperatingSystem.FileSystem;
using DeepSigma.OperatingSystem.Enums;
using DeepSigma.OperatingSystem.Models;
```

## Core APIs

### `Terminal`

Runs commands and returns a `DeepSigma.General.Monads.ResultMonad`.

```csharp
using DeepSigma.OperatingSystem;

var result = Terminal.RunCommand("dotnet", "--info");

result.Switch(
    success => Console.WriteLine(success.Result),
    error => Console.WriteLine(error.Exception.Message)
);
```

Check whether a program is available on the machine:

```csharp
bool gitInstalled = Terminal.IsProgramInstalled("git");
bool pythonInstalled = Terminal.IsProgramInstalled("python");
```

### `GitHelper`

Clones a repository to a target directory by calling `git clone`.

```csharp
using DeepSigma.OperatingSystem;

Exception? ex = GitHelper.DownloadGitRepository(
    "https://github.com/example/repo.git",
    @"C:\Temp\repo"
);

if (ex is not null)
{
    Console.WriteLine(ex.Message);
}
```

### `ProcessManager`

Inspects and controls local processes.

```csharp
using DeepSigma.OperatingSystem;

// List all processes
var processes = ProcessManager.GetAllActiveProcess();

// Find by name
var findResult = ProcessManager.GetActiveProcessByName("notepad");
findResult.Switch(
    success => Console.WriteLine($"Found {((System.Diagnostics.Process[])success.Result!).Length} process(es)."),
    error => Console.WriteLine(error.Exception.Message)
);

// Kill by name
var killResult = ProcessManager.KillProcessByName("notepad");
killResult.Switch(
    success => Console.WriteLine($"Killed {success.Result} process(es)."),
    errors => Console.WriteLine($"Failed: {string.Join(", ", errors.Exceptions.Select(x => x.Message))}")
);
```

### `PythonExecutor`

Runs Python scripts and checks for Python installations.

```csharp
using DeepSigma.OperatingSystem;

var scriptResult = PythonExecutor.ExecuteScript("hello.py", "--name DeepSigma");

scriptResult.Switch(
    success => Console.WriteLine(success.Result),
    error => Console.WriteLine(error.Exception.Message)
);
```

Use a specific Python executable, such as one inside a virtual environment:

```csharp
var venvResult = PythonExecutor.ExecuteScript(
    "script.py",
    "--verbose",
    @"C:\path\to\venv\Scripts\python.exe"
);
```

Inspect Python availability:

```csharp
bool installed = PythonExecutor.IsPythonInstalled();

var installs = PythonExecutor.GetPythonInstallLocations();
var venvs = PythonExecutor.GetPythonVirtualEnvironments();
```

### `TempFolderUtility`

Creates temporary folders under the user's **Downloads** directory.

```csharp
using DeepSigma.OperatingSystem;

DirectoryInfo tempDir = TempFolderUtility.CreateTempDirectoryInDownloads("demo-");
Console.WriteLine(tempDir.FullName);
```

### `FileSystemObject`

Builds an object representation of a file or directory, including metadata and optional recursive traversal depth.

```csharp
using DeepSigma.OperatingSystem.FileSystem;

var fsObject = new FileSystemObject(@"C:\Projects\SampleRepo", max_number_of_levels_to_navigate_down: 2);

Console.WriteLine(fsObject.DirectoryName);
Console.WriteLine(fsObject.ObjectSystemType);

foreach (var directory in fsObject.Directories)
{
    Console.WriteLine($"Dir: {directory.DirectoryName}");
}

foreach (var file in fsObject.Files)
{
    Console.WriteLine($"File: {file.FileProperties.FileName}");
}
```

Open or delete items:

```csharp
var file = new FileSystemObject(@"C:\Temp\notes.txt", 0);
Exception? openError = file.OpenFile();
Exception? deleteError = file.DeleteFile();

var folder = new FileSystemObject(@"C:\Temp", 1);
Exception? folderOpenError = folder.OpenDirectory();
```

### `FileSystemSelectionService`

Loads ignore-style filter tokens from a file and checks whether a file-system item matches any saved filter.

```csharp
using DeepSigma.OperatingSystem.FileSystem;

var selector = new FileSystemSelectionService(
    directory_path: @"C:\Projects\SampleRepo",
    file_name: ".dsignore"
);

bool shouldIgnoreBin = selector.MatchingFilterFound("bin");
bool shouldIgnoreLog = selector.MatchingFilterFound("error.log");
```

Supported matching behavior is simple and token-based:

- `text` matches exact text
- `*text` matches values that end with `text`
- `text*` matches values that start with `text`
- `*text*` matches values that contain `text`
- tokens beginning with `\` are marked internally as directory filters
- `#` starts a comment for the rest of the line

### `FileSystemUtilities`

Recursively deletes a directory tree and clears read-only attributes first.

```csharp
using DeepSigma.OperatingSystem.FileSystem;

FileSystemUtilities.ForceDeleteAllFilesAndDirectoriesRecursively(
    @"C:\Temp\cloned-repo"
);
```

## Typical workflow

```csharp
using DeepSigma.OperatingSystem;
using DeepSigma.OperatingSystem.FileSystem;

// 1. Clone a repository
var cloneError = GitHelper.DownloadGitRepository(
    "https://github.com/example/repo.git",
    @"C:\Temp\repo"
);

if (cloneError is not null)
{
    throw cloneError;
}

// 2. Inspect the cloned directory
var repo = new FileSystemObject(@"C:\Temp\repo", 1);

// 3. Run a command inside the environment
var command = Terminal.RunCommand("cmd.exe", "/C dir C:\\Temp\\repo");
command.Switch(
    success => Console.WriteLine(success.Result),
    error => Console.WriteLine(error.Exception.Message)
);

// 4. Clean up
FileSystemUtilities.ForceDeleteAllFilesAndDirectoriesRecursively(@"C:\Temp\repo");
```

## Notes on platform behavior

This library is best described as **Windows-oriented** in its current form:

- `GitHelper` invokes `cmd.exe`
- `PythonExecutor.GetPythonInstallLocations()` uses `where`
- `TempFolderUtility` calls `shell32.dll` to resolve the Downloads folder
- examples and tests assume Windows-style paths and tooling

Some APIs, such as `ProcessManager` and parts of `Terminal`, may still work cross-platform, but the package as a whole should currently be treated as Windows-centric.

## Error handling

Most command/process helpers return `DeepSigma.General.Monads` result types rather than throwing immediately. Expect to handle:

- `Success`
- `Error`
- `Errors`

For APIs that return `Exception?`, `null` means success.

## Project structure

```text
DeepSigma.OperatingSystem/
├── DeepSigma.OperatingSystem/
│   ├── Enums/
│   ├── FileSystem/
│   ├── Models/
│   ├── GitHelper.cs
│   ├── ProcessManager.cs
│   ├── PythonExecutor.cs
│   ├── TempFolderUtility.cs
│   └── Terminal.cs
└── DeepSigma.OperatingSystem.Tests/
    ├── Data/
    ├── Tests/
    └── TestConfig.cs
```

## Development

```bash
dotnet restore
dotnet build
dotnet test
```

Because parts of the library and tests depend on Windows-specific executables and paths, running the full test suite is most reliable on Windows.

## Opportunities for improvement

A few areas stand out for future enhancement:

- add explicit cross-platform support for macOS and Linux
- expose async command execution APIs
- provide richer Git operations beyond cloning
- support glob patterns with full path-awareness
- add XML docs / examples for package consumers
- include CI badges, NuGet badge, and release notes once available

## License

MIT
