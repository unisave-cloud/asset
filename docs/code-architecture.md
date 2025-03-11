# Code architecture

This document describes the top-level architecture of the asset's codebase.

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
to reflect the new preferences.

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
preferences file (`Resources/UnisavePreferencesFile.asset`):

```csharp
new ClientApplication(unisavePreferences);
```
