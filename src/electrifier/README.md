# electrifier' read me first

## Requirements for Windows App SDK

 > Backtracking: [WinUI 3 official Requirement:](https://github.com/dahall/Vanara/pull/494#issuecomment-2595456933)
> 
> > Okay, I found the requirements: https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/system-requirements
> > _The Windows App SDK has the following minimum system requirements:_
> > 
> > * Windows 10, version 1809 (build 17763) or later.
> > * Visual Studio 2022, version 17.0 or later, with the [required workloads and components](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment#required-workloads-and-components).
> > * Visual Studio 2019, version 16.9 or later, with the [required workloads and components](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment#required-workloads-and-components).
> > * Windows SDK, version 2004 (build 19041) or later (included with Visual Studio 2019 and 2022 by default).
> > * If you plan to build .NET apps, you'll also need .NET 6 or later (see [Download .NET](https://dotnet.microsoft.com/en-us/download)).



## Getting Started to build a Windows App SDK project with Template Studio

Browse and address `TODO:` comments in `View -> Task List` to learn the codebase and understand next steps for turning the generated code into production code.

Explore the [WinUI Gallery](https://www.microsoft.com/store/productId/9P3JFPWWDZRC) to learn about available controls and design patterns.

Relaunch Template Studio to modify the project by right-clicking on the project in `View -> Solution Explorer` then selecting `Add -> New Item (Template Studio)`.

## Publishing

For projects with MSIX packaging, right-click on the application project and select `Package and Publish -> Create App Packages...` to create an MSIX package.

For projects without MSIX packaging, follow the [deployment guide](https://docs.microsoft.com/windows/apps/windows-app-sdk/deploy-unpackaged-apps) or add the `Self-Contained` Feature to enable xcopy deployment.

## CI Pipelines

See [README.md](https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/pipelines/README.md) for guidance on building and testing projects in CI pipelines.

## Changelog

See [releases](https://github.com/microsoft/TemplateStudio/releases) and [milestones](https://github.com/microsoft/TemplateStudio/milestones).

## Feedback

_Bugs and feature requests should be filed at https://aka.ms/templatestudio._

---

*Recommended Markdown Viewer: [Markdown Editor](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.MarkdownEditor2)*
