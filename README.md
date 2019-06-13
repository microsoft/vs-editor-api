# Microsoft Visual Studio Editor API

This repository contains the open source layers of the Microsoft Visual
Studio editor. This includes all public API definitions and some low level
implementations of the editor including the text model, text logic, and
editor primitives & operations subsystems. These layers are intended for
extension authors to better integrate with the editor.

With a few caveats, the layers in this repository power both Visual
Studio on Windows and the Visual Studio for Mac editors. While both editors
are built on this codebase, many aspects of the editor are not open source,
including the WPF and Cocoa UI layers.

## Visual Studio for Mac

Visual Studio for Mac 8.1 [introduced a brand new native macOS text editor](https://docs.microsoft.com/en-us/visualstudio/releasenotes/vs2019-mac-relnotes#---new-c-editor)
built on the "real" Visual Studio editor core. Central to our ongoing effort
to bring parity and performance benefits to developers by leveraging and
sharing more code with Visual Studio on Windows, the UI layers were ported
directly from WPF to modern Cocoa.

Most notably, the Cocoa editor uses Apple's
[Core Text](https://developer.apple.com/documentation/coretext),
[Core Graphics](https://developer.apple.com/documentation/coregraphics), and
[Core Animation](https://developer.apple.com/documentation/quartzcore)
technologies to perform retained-mode layout and high fidelity rendering of
text. Among other enhancements for and integrations with macOS, it
now supports all of the macOS input methods via the
[`NSTextInputClient`](https://developer.apple.com/documentation/appkit/nstextinputclient)
protocol.

Currently, the new native editor based on Visual Studio and the legacy editor
co-exist while we transition all language services away from the legacy
editor. Visual Studio for Mac supports C# and XAML (in 8.2 previews) so far,
but will continue to enable support for other languages over the coming
releases. Please refer to the
[Visual Studio for Mac roadmap](https://docs.microsoft.com/en-us/visualstudio/productinfo/mac-roadmap)
for details on what's next.

This diagram should help visualize the layering of Visual Studio for Mac
compared to Visual Studio while the legacy editor still exists for some
languages.

![Visual Studio for Mac Editor Architecture](https://docs.microsoft.com/en-us/visualstudio/mac/media/vs-editor-architecture.png)

## Caveats

In order to facilitate porting the WPF editor from Windows to macOS, some
breaking changes have been made to some of the lower-level interfaces. The
ongoing plan is to reconcile these differences such that there is no API
difference whatsoever between the WPF and Cocoa editors.

For now, however, there are two separate sets of NuGet packages for targeting
Visual Studio and Visual Studio for Mac, available in the respective sections
below.

While _most_ non-UI related interfaces are identical across WPF and Cocoa
implementations of the editor, many are new yet familiar: when targeting
Cocoa, `ICocoa*` interfaces can generally be found in place of analogous
`IWpf*` interfaces.

## Resources

The following resources should help extension authors become familiar with
the editor APIs and capabilities, and are relevant to both Visual Studio
and Visual Studio for Mac.

* [Managed Extensibility Framework](https://docs.microsoft.com/dotnet/framework/mef/index)
* [MEF in the Editor](https://docs.microsoft.com/visualstudio/extensibility/managed-extensibility-framework-in-the-editor)
* [Inside the Editor](https://docs.microsoft.com/visualstudio/extensibility/inside-the-editor)
* [Language Service and Editor Extension Points](https://docs.microsoft.com/visualstudio/extensibility/language-service-and-editor-extension-points)
* [A video introduction to the editor architecture](https://www.youtube.com/watch?v=PkYVztKjO9A)

## Editor SDK Installation

### Visual Studio for Mac

NuGet packages are forthcoming, but all assemblies are available to extensions
for Visual Studio for Mac when using Add-in Maker. The assemblies can also be
produced directly from this repository (see
[Building the Editor API](#build-the-editor-api) below).

Refer to the [Extending Visual Studio for Mac](https://docs.microsoft.com/en-us/visualstudio/mac/extending-visual-studio-mac) documentation for details.


### Visual Studio (Windows)

On Windows, the Visual Studio Editor API is available via NuGet and is also
installed with the Visual Studio Extension Development workload.

| NuGet Package | Current Version |
| ------------- | ------------- |
| [Microsoft.VisualStudio.CoreUtility](https://www.nuget.org/packages/Microsoft.VisualStudio.CoreUtility) | [![NuGet package](https://img.shields.io/nuget/v/Microsoft.VisualStudio.CoreUtility.svg)](https://www.nuget.org/packages/Microsoft.VisualStudio.CoreUtility) |
| [Microsoft.VisualStudio.Text.Data](https://www.nuget.org/packages/Microsoft.VisualStudio.Text.Data) | [![NuGet package](https://img.shields.io/nuget/v/Microsoft.VisualStudio.Text.Data.svg)](https://www.nuget.org/packages/Microsoft.VisualStudio.Text.Data) |
| [Microsoft.VisualStudio.Text.Logic](https://www.nuget.org/packages/Microsoft.VisualStudio.Text.Logic) | [![NuGet package](https://img.shields.io/nuget/v/Microsoft.VisualStudio.Text.Logic.svg)](https://www.nuget.org/packages/Microsoft.VisualStudio.Text.Logic) |
| [Microsoft.VisualStudio.Text.UI](https://www.nuget.org/packages/Microsoft.VisualStudio.Text.UI) | [![NuGet package](https://img.shields.io/nuget/v/Microsoft.VisualStudio.Text.UI.svg)](https://www.nuget.org/packages/Microsoft.VisualStudio.Text.UI) |

## Building The Editor API

While this repository is largely intended for reference, it can produce a
viable build of the lower levels of the editor. Either open `VSEditorCore.sln`
in Visual Studio or Visual Studio for Mac and build from the IDE, or build
on the command line.

Visual Studio 2019 or Visual Studio for Mac 8.0 or newer is required.

### Build

Assemblies will be available in the `bin/` directory at the root of the
repository.

```bash
$ msbuild /restore
```

### Package

NuGet packages may also be produced locally and will be available in
the `_artifacts/nuget/` directory at the root of the repository.

```bash
$ msbuild /t:Pack
```

## Contributing

This project has adopted the
[Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the
[Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any
additional questions or comments.

### Pull Requests

We are generally not accepting pull requests for this repository for the
core editor code at this time. Please feel free to submit pull requests for
other content in this repository, including new samples.