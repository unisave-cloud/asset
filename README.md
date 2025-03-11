# Unisave Asset

<a href="https://unisave.cloud/" target="_blank">
    <img alt="Website" src="https://img.shields.io/badge/Website-unisave.cloud-blue">
</a>
<a href="https://discord.gg/XV696Tp" target="_blank">
    <img alt="Discord" src="https://img.shields.io/discord/564878084499832839?label=Discord">
</a>

Unisave Asset is a [Unity Engine](https://unity.com/) asset that integrates your game with the [Unisave cloud](https://unisave.cloud/). The asset is available [on the Unity asset store](https://assetstore.unity.com/packages/slug/142705).

If you just want to use Unisave, please refer to the online documentation on the [Unisave website](https://unisave.cloud/). Everything in this repository is intended for Unisave developers, not its users.

## Documentation

- [Folder structure](docs/folder-structure.md)
- [Code architecture](docs/code-architecture.md)
- [Testing WebGL on Windows](docs/testing-webgl-on-windows.md)


## After Cloning

You need to install the precise version of Unity that the project currently uses, see the version in [ProjectSettings/ProjectVersion.txt](ProjectSettings/ProjectVersion.txt).

After cloning do:

1. Open the project in Unity (check the Unity version).
2. Ignore compile errors.
3. Install required Unity packages in `Window > Package Manager` in `Packages: Unity Registry`:
    - `TextMeshPro` (so that examples compile)
    - `Test Framework` (so that tests compile and run)
    - `JetBrains Rider Editor` (so that csproj and sln files are generated and Rider works well)
    - If some package installation fails, just restart Unity and retry.
    - Restart Unity once all are installed, if errors don't disappear right away.
5. Set up Unisave cloud connection (ideally to the minikube cluster) so that examples and unit test can be run.
    - Minikube connection does not use HTTPS, thus you get an `Insecure connection not allowed`. Go to `Project Settings > Player > Other Settings > Configuration > Allow downloads over HTTP` and select `Always allowed`.

Optional:

1. Import Text Mesh Pro `Window > TextMeshPro > Import TMP Essential Resources` so that examples work.
2. Enable `Heapstore` backend definition file to run its tests as well (this option is remembered in PlayerPrefs only).

Once this is done, you can try running all the tests. Open `Window > General > Test Runner` and select the `Play Mode` tab. You should see all the tests. To try out the connection, run the `UnisaveFixture.ExampleFullstackTest.ItCallsFacetMethod` by double-clicking on it. Then double-click the root node to run all the tests.

> **Note:** To kill running tests, click on the `[>]` play button.

> **Note:** Email auth tests do some funky stuff with the backend folder, so run those separately and not as parts of one global run otherwise all the other tests will fail. See the backend hashes on the made facet calls.
