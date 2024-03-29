![Unisave Logo](Assets/Plugins/Unisave/Images/PropertiesLogo.png)

Unisave Asset
=============

<a href="https://unisave.cloud/" target="_blank">
    <img alt="Website" src="https://img.shields.io/badge/Website-unisave.cloud-blue">
</a>
<a href="https://discord.gg/XV696Tp" target="_blank">
    <img alt="Discord" src="https://img.shields.io/discord/564878084499832839?label=Discord">
</a>

Unisave is a backend service for games developed in Unity. It attempts
to not only solve common problems like player registration but also
lets you develop custom backend logic in C#, the language you already
use for your Unity project.

This repository contains the [asset](https://assetstore.unity.com/packages/slug/142705),
distributed via the Unity Asset Store. It contains the code that facilitates Unity editor integration.
Your backend code, however, interacts primarily with
the [Unisave Framework](https://github.com/Jirka-Mayer/UnisaveFramework),
bundled within this asset as a `.dll` library.

### Internal asset documentation

For documentation of the asset structure, see
[internal asset documentation](docs).


### Starting Unity `SimpleWebServer` for WebGL testing

To start the WebGL development server on windows, open GitBash in:

    /c/Program Files/Unity/Hub/Editor/2020.3.42f1/Editor/Data/PlaybackEngines/WebGLSupport/BuildTools

And start it:

    SimpleWebServer.exe ~/Downloads/BuiltFolder 8080
