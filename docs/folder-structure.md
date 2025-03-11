## Folder structure

This is the folder structure of the Unity asset:

```
- Assets
    - Plugins
        - Unisave
            - Editor
                > All the code that integrates Unisave with UnityEditor
            - Examples
                > Examples of how to use Unisave (required by Asset Store)
            - Heapstore
                > Library for direct database communication
                > TODO: move into the ./Modules folder
            - Images
                > All images and icons used by the asset
            - Libraries
                > All the libraries required by Unisave + UnisaveFramework.dll
            - Modules
                - Specific solutions built on Unisave that ship together with the Unisave asset
            - Scripts
                > Here is the meat of the asset
            - Templates
                > Code templates (Create Asset > Unisave > ...)
            - Testing
                > Support code for writing integration tests for your backend.
                    (analogous to Scripts or Editor, but for your tests)
    - Resources
        - UnisavePreferencesFile.json
            > Contains configuration data
    - UnisaveFixture
        > Contains automated tests. See the README.md inside.
```

The important folder is `Assets/Unisave/Scripts` which contains the code
your game uses in runtime. Internally, it mirrors the structure of
the [UnisaveFramework](https://github.com/unisave-cloud/framework).
