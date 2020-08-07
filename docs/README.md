Unisave asset documentation
===========================

This folder contains internal documentation of the Unisave asset. It's
intended for someone, who wants to understand how the asset works and
how it's structured internally. It's not for someone who just wants to
use it. For documentation on plain usage of Unisave, go to
[https://unisave.cloud/docs](https://unisave.cloud/docs)

This documentation is not exhaustive, it's meant to contain information
that isn't obvious from the source code. To understand concepts that
aren't covered here, you have to refer to the source code.

There might also be README.md files scattered throughout the codebase.
Those files typically explain purpose of the folder they are located
in.


## Folder structure

    - Assets
        - Unisave
            - Components
                > Contains MonoBehaviour scripts you might use in your game
            - Editor
                > All the code that integrates Unisave with UnityEditor
            - Examples
                > Examples of Unisave usage (required by Asset Store)
            - Images
                > All images and icons used by the asset
            - Libraries
                > All the libraries required by Unisave + UnisaveFramework.dll
            - Resources
                > Contains the UnisavePreferencesFile containing configuration
            - Scripts
                > Here is the meat of the asset
            - Templates
                > Code templates (Create Asset > Unisave > ...)
            - Testing
                > Support code for writing integration tests for your backend.
                  (analogous to Scripts or Editor, but for your tests)
            - Tests
                > Unit tests for the Unisave Scripts & Editor folders.
                  It's not really used yet. More likely it will be
                  replaced by the UnisaveFixture in the future.
        - UnisaveFixture
            > See the README.md inside

The important folder is `Assets/Unisave/Scripts` which contains the code
your game uses during runtime. Internally, it mirrors the structure of
the UnisaveFramework.


## Code architecture

The user interacts with the asset via facades. Facades are static
classes located in `Unisave/Scripts/Facades` that do not implement their
own logic, they just delegate the requests into the Unisave client
application.

Behind those facades is a `Unisave.Foundation.ClientApplication`. This
class acts as a service container for other services and those other
services do the heavy lifting (calling facets, remembering session ID,
etc...).

Facades delegate user requests into the `ClientApplication`, which
allows us to replace a `ClientApplication` with a different instance
with different services inside. This is precisely what happens during
testing. Inside integration tests the `ClientApplication` is swapped out
so that facet calls don't go to the Unisave cloud but are instead run
locally, giving us access to the server-side application.

A `ClientApplication` requires an instance of `UnisavePreferences`, that
specify configuration options for the services inside. If we want to
change `UnisavePreferences`, we have to recreate the `ClientApplication`
to reflect the new preferences .

When your game starts, facades have no client application assigned.

User should never access a `ClientApplication` directly but always via
the facades, since they handle it's lifecycle properly.


### Creating client application

When a facade needs to resolve a `ClientApplication` in order to resolve
a specific service from it, it does so via the `ClientFacade`:

```csharp
var clientApp = ClientFacade.ClientApp;
```

The property will return the application that is currently assigned, or
will create a new default instance of the application.

The assigning and de-assigning happens inside integration tests:

```csharp
ClientFacade.SetApplication(myMockedApplication);
ClientFacade.SetApplication(null); // de-assign
```

Outside tests, in regular usage, the application is created via the
constructor by resolving Unisave preferences from the default
preferences file (`Unisave/Resources/UnisavePreferencesFile.asset`):

```csharp
new ClientApplication(unisavePreferences);
```
