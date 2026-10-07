# Route Tracker Commander DLC

**English** | [Polski](README.pl.md)

This standalone repository contains only the extension sources, project, tests and documentation. It does not contain the EDDiscovery application source tree.

Downloads: [Route Tracker Commander 1.1.2](https://github.com/SirAdams/RouteTrackerCommander/releases/tag/v1.1.2).

Standalone managed DLL extension for EDDiscovery. Adds **Route Tracker — Commander** as a separate native panel. No replacement of EDDiscovery.exe or host libraries is required.

Two separate packages are available:
- **EDD19.1.11**: tested with the original EDDiscovery 19.1.11.0 release.
- **EDD20.x**: tested with unmodified master f9af793b8e4055cc384868fc902efd1c2900a8f0, executable version 20.0.0.0. Future 20.x builds may change the panel API and require an updated DLL.

Both packages contain a file named RouteTrackerCommander.dll. Install only the one matching your host. The extension checks the host version during initialization.

## Features

- A toolbar copy icon lets you copy the displayed route target again after another application overwrites the clipboard. It works with auto-copy disabled, does not advance the route and is disabled when no target is available.

- Selected saved route or Nav Route, route progress and panel options are saved separately for each commander.
- Switching commanders restores their route and settings. A commander with no saved route sees an explicit empty state.
- Stale asynchronous scan results and delayed menu actions cannot overwrite another commander's state.
- Route updates follow the latest entry in the selected commander's history independently of the history cursor.
- Uses EDDiscovery's existing route definitions, history, ship/FSD data, user database, theme and panel layout. No separate systems database is required.
- Default interface strings are English; existing host translations are used where available. No host dictionaries are replaced.

Saved route definitions remain shared. Only the selected route and its tracking settings are per commander and per panel instance/UI profile. Choose a route once for each commander; the standard tracker's settings are not migrated automatically.

## Install

1. Close EDDiscovery.
2. Download the ZIP matching the EDDiscovery version and extract it.
3. Copy RouteTrackerCommander.dll into `%LOCALAPPDATA%\EDDiscovery\DLL`. If using a custom EDDiscovery app folder, use its DLL subdirectory instead. You may alternatively run the included Install.ps1, with `-AppDataDirectory` for a custom app folder.
4. Unblock the downloaded DLL in Windows file Properties if Windows marks it as blocked.
5. Start EDDiscovery and allow the extension when prompted. DLL permissions can also be configured in Settings.
6. Add the **Route Tracker — Commander** tab/panel from EDDiscovery's panel list. Select a commander, then select a route and its settings.
7. Use only one tracker with automatic clipboard copying or target setting enabled. Disable those options in other route panels to avoid competing updates.

Keep the DLC panel open to track live jumps; selecting another tab is fine. Closing/removing the panel stops its tracking until reopened. Normal events and history are supplied by EDDiscovery.

Do not copy the host/reference/test folders from the source tree into EDDiscovery. Only RouteTrackerCommander.dll is installed. These packages do not contain a full EDDiscovery distribution.

## Update checks

The toolbar refresh icon checks this repository's GitHub releases for a newer ZIP matching your EDDiscovery version. It checks in the background when the panel opens and every 12 hours while it remains open. Requests are shared between panels and have a 15-second timeout. Network failures do not interrupt route tracking.

A newer matching release adds an exclamation mark to the icon. Click it to view the version and open the release page. When no update is known, clicking checks again. Right-click for **Check now** or to disable **Automatically check for updates**. That preference is shared across commanders and saved in EDDiscovery's user database.

Preview releases are included and labelled. A newer release without a matching host package is ignored. Checking contacts the public GitHub API without sending commander, journal or route data; GitHub still receives ordinary connection information such as your IP address. No GitHub login or token is required.

This feature checks and links to downloads; it does not automatically download, install or replace DLLs. Close EDDiscovery before installing an update. The main program does not need modification.

Selection logic has 19 additional offline assertions in tests/UpdateTests.cs. Compile that test with shared/ReleaseChecker.cs and references to System.Net.Http.dll and System.Web.Extensions.dll; it does not contact GitHub.

## Upgrade and remove

Close EDDiscovery before replacing the DLL with the appropriate new version. The installer backs up a previously installed DLL. Saved settings remain in EDDiscovery's user database.

To uninstall, close EDDiscovery and move RouteTrackerCommander.dll out of the DLL folder. The standard Route Tracker remains available. If upgrading EDDiscovery from 19 to 20, replace the DLL with the EDD20.x package.

## Build

Use Visual Studio 2022 / MSBuild with .NET Framework 4.8 developer tools. Provide the corresponding unmodified host installation as HostDir:

```
MSBuild RouteTrackerCommander.csproj /t:Rebuild /p:TargetHost=edd19 /p:HostDir="C:\Program Files\EDDiscovery"
MSBuild RouteTrackerCommander.csproj /t:Rebuild /p:TargetHost=edd20 /p:HostDir="C:\Program Files\EDDiscovery20"
```

Sources are split into edd19/src and edd20/src because EDDiscovery 20 changes the Surveyor/Route Tracker implementation and translator APIs. Output DLLs are in bin/edd19 or bin/edd20. Do not interchange them.

## Validation

Each variant passes 65 standalone assertions, including commander isolation, empty state, route progress, reopening, invalid positions, startup callbacks, unload/reload, failed initialization and wrong-host rejection. Both were also tested with the actual unmodified host startup, DLL initialization, native panel display and shutdown, using isolated test databases. Live game jumps still benefit from user testing.

Tests are in each variant's tests folder, plus tests/StartupSmoke.cs. Test fixtures do not use the user's database. No test executable or database is included in the download packages.

## License and origin

Apache License 2.0; see LICENSE.md. Panel code is derived from EDDiscovery, with original copyright notices retained. The EDDiscovery 19 variant is based on Release_19.1.11 (a3cbe2190779ea4dcd186db65aeb3f4fa1baccb9); the 20 variant follows master f9af793b8e4055cc384868fc902efd1c2900a8f0. This is a community extension, not an official EDDiscovery release.
