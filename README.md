# ManualRotation
Inspired by Samsung OneUI manual rotation.

## Philosophy 
This program is designed to be as stealthy and simple as possible, designed to be forgotten once set up. It even doesn't have any kind of configuration other than registering for startup. 

## Usage 
Once installed and running, if Auto Rotate is disengaged and the device rotated to another direction, a notification toast will pop out like this:

Tap the `Rotate` button to rotate the screen. Tap elsewhere or swipe to the side to dismiss the notification. 

## Installation
Currently there's no installer available. A manual installation is possible by copying files to `%LOCALAPPDATA%/Programs/ManualRotation` and run `ManualRotation.exe install` to register for startup. Similarly, uninstallation is as simple as kill the process in Task Manager, run `ManualRotation.exe remove` and delete the files. 

## Build
Open the `.sln` file with Visual Studio 2019+. 
When building X64 binearies, make sure the line `<PlatformTarget>ARM64</PlatformTarget>` is commented out, and use `x64` configuration.
Similarly, when building ARM64 binearies, make sure the line `<PlatformTarget>ARM64</PlatformTarget>` is NOT commented out, and use`ARM64` configuration.
Please note that X64 build binaries will not work on ARM64 machines, and ARM64 build binaries will not run at all on X64 machines.
