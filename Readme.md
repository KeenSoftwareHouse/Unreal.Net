# Unreal.Net
.Net 5 integration with UE4.

Source code for our talk @ GIC 2021 ([slides](https://docs.google.com/presentation/d/12pPg2VLyonZqtbyggIeud4JHGFqzTUZwwhI6Ng3nSws/edit?usp=sharing)).

You may also consult the [long slides](https://docs.google.com/presentation/d/1Ey1DpEYcRjs5gGU9ReLz4tckQFY5KN_iXAaHozA0xEA/edit?usp=sharing), my original presentation before we had to trim it to fit in the 45 + 10 minute format.

## 🔥This Repository is not Maintained🔥
This code sample is provided *as is* and we do not currently have any plans to update or maintain it.

We provide the source code as learning resource in hopes of helping other developers looking to achieve a similar functionality in their own codebases.

# Features
* Bidirectional bindings for methods.
* Bidirectional generation of reference types (UObject).
* Generation of native struct (minus inheritance) and enum types.
* Marshalling of primitive types, structs, enumerations and reference types.

## Missing Features
* Marshalling of strings.
* Delegates.
* Collections.
* Reference type properties
* Interfaces.
* Object lifetime handling.

# Getting Started

## Prerequisites
In order to build this project you'll need the .Net 5 SDK as well as a recent version of Unreal Engine 4. We last tested 4.26.1, but it should work on newer versions of the 4.* series.

## Set up
Clone this repository, it contains two main folders:
* DotNetPlugin: Main project which is hooked up for development and testing of the .Net integration.
* DotNetBinder: Project which contains the binder plugin, which is responsible for hooking into the UnrealBuildTool and collecting metadata about the types in the project.

## Install the plugin
The binder plugin only works when installed to your engine.
Navigate to the DotNetBinder folder and update the `Build.bat` script to point to your UE4 installation folder. After that run the script to install the plugin.

## Building
Metadata for the native types must be collected before the generation can occur. This creates a bit of a chicken and egg scenario, so in the very first build after cloning the repo you must:
1. Generate the solution from the main uproject file (in DotNetPlugin, via the right click menu, "Generate Visual Studio project files").
2. Build once from Visual Studio (or Rider, or CLI) (this will not bring in any managed modules since they have not been generated yet), this step generates the metadata info for the first time in `Intermediate\DotNet\..`.
3. Comment 17 from `DotNetPlugin\Source\DotNetPlugin\DotNetPlugin.Build.cs` and uncomment line 19.
4. Delete The Intermediate and Build folders, as well as the generated project solution.
5. Run `Build Managed.bat` to build all managed code.
6. Regenerate the visual studio solution (to include the two new projects).
7. You may now open the project in the Unreal Editor (or run from IDE).

Now you should be all setup and future builds are as simple as:
1. Run `Build Managed.bat` to build all managed code.
2. Build the unreal project.

## Developing

Work must be done in the Tests managed project (inside the DotNetPlugin folder). You may open the managed project directly using the `OpenManagedFolder.bat` script which is provided.

Inside you'll find the managed solution which contains all managed project files.

## Known Issues
* Incremental builds do not re-run source generators if only the generator project has changed, this can lead to stale bindings and in that case a full rebuild is necessary.
* Sometimes UHT produces incomplete reflection information with no bindings for some types. If you encounter that rebuild the native project and then the managed solution, that's usually sufficient.