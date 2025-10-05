-----

# MelonBepInExChainloader

A MelonLoader plugin designed to load and run **BepInEx 6** from within a MelonLoader environment for IL2CPP games.

This project is an attempt to create a compatibility layer that allows BepInEx 6 plugins to be used in games that are being launched and modded with MelonLoader. This is a unique and complex challenge, as no official solution for chainloading BepInEx from MelonLoader currently exists.

## ⚠️ Current Status: Not Functional

While this plugin successfully initializes the BepInEx chainloader from MelonLoader's perspective, BepInEx itself fails to start correctly due to a deep-level dependency conflict.

### The Problem

When the game is launched, MelonLoader reports that the chainloader has been initialized successfully:

```
[11:23:33.664] [MelonBepInExChainloader] [MelonBepInExChainloader] Preparing BepInEx 6 IL2CPP...
[11:23:33.809] [MelonBepInExChainloader] [MelonBepInExChainloader] IL2CPPChainloader initialized and executed successfully.
```

However, the internal BepInEx log reveals a **fatal crash** during its own startup sequence:

```log
[Fatal  :   BepInEx] Unable to execute IL2CPP chainloader
[Error  :   BepInEx] System.TypeInitializationException: The type initializer for 'BepInEx.Unity.IL2CPP.Il2CppInteropManager' threw an exception.
 ---> System.MissingMethodException: Method not found: 'Microsoft.Extensions.Logging.ILoggerFactory Microsoft.Extensions.Logging.LoggerFactory.Create(System.Action`1<Microsoft.Extensions.Logging.ILoggingBuilder>)'.
   at BepInEx.Unity.IL2CPP.Il2CppInteropManager..cctor()
```

This initial crash prevents BepInEx from properly setting up its environment, which then causes all BepInEx plugins to fail with `System.TypeLoadException` errors because they cannot find game types from `Assembly-CSharp.dll`:

```log
[Error  :   BepInEx] Error loading [MegabonkShrineHunt 0.2.0]: System.TypeLoadException: Could not load type 'SpawnPlayerPortal' from assembly 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'.
```

The root cause appears to be a version mismatch between the `Microsoft.Extensions.Logging` library used by MelonLoader's .NET 6 runtime and the version that this specific build of BepInEx 6 (`be.738`) was compiled against.

## How It's Supposed to Work

This plugin hooks into MelonLoader's `OnApplicationLateStart` event to:

1.  Dynamically configure BepInEx's internal path variables to the correct folders (`BepInEx/`, `BepInEx/plugins`, etc.).
2.  Load the core BepInEx assemblies and invoke the `IL2CPPChainloader` to start the BepInEx plugin loading process.
