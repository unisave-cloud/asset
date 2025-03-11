# Unisave fixture test suite

The folder `Assets/UnisaveFixture` contains automated tests that verify correct behaviour of the asset, the framework, and the cloud environment. These are called *fullstack tests* are the most common test type in the suite. There are also a few unit tests and other legacy tests, but these form a minority.

The entire test suite can be run by connecting the unity project to a unisave cluster (ideally the minikube local cluster), openning the test runner and running all of them in the play mode.


## Folder structure

```bash
UnisaveFixture/

    # Tests are grouped by the tested logic into standalone folders.
    # (test types are intermixed here, though most are fullstack)

    ExampleFullstackTest/
        #> an example fullstack test
        /Backend
            #> with its own backend folder uploaded
            #> from within the test source code
    
    ExampleUnitTest/
        #> an example unit test

    Core/
        #> tests core platform functionality
        Arango/
        Authentication/
        Broadcasting/
        Facets/
        Logging/
        Sessions/
    
    Modules/
        #> tests unisave modules that are shipped with the asset
        EmailAuthentication/
        Heapstore/
    

    # Then there are legacy tests that have not been categorized
    # by this scheme yet and they don't use the FullstackFixture
    # test case which uploads their backend code programmatically.
    
    /Tests
        #> legacy mess here...
    
    /Backend
        #> Legacy tests use this backend folder that must be
        # explicitly uploaded via backend definition files.
        # These should be updated to the new fullstack tests.
    

    # Finally, there are legacy files that should be removed.
    # (both to be extracted to SteamAuth and SteamMtx modules)

    /Scripts
    /TemplateChangelogs
```


## Fullstack tests

Fullstack tests run custom backend code on the (minikube) cluster and assert its behaviour via a few built-in database-probing facet methods. They are the default goto test type if a unit test does not suffice.

Investigate the `ExampleFullstackTest` and create your own accordingly.

The backend file upload is completely overriden for the test, backend folders must be specified in the test case itself. The default backend folder definition files are completely ignored and the backend is automatically uploaded before the test fixure is executed. (This causes many cold starts and thus puts strain on the worker system - this is why you should run these tests primarily against the minikube cluster).

```cs
protected override string[] BackendFolders => new string[] {
    "Assets/UnisaveFixture/ExampleFullstackTest/Backend"
};
```

The list of these backend folders also gets appended with the `Unisave/Testing/FullstackBackend` folder which contains helper facets to query the database and assert its proper state.

Since facet calls are best made via `await`ing, you can use the `Asyncize` helper to turn the body of a Unity `IEnumerator` test into an `async` body test:

```cs
[UnityTest]
public IEnumerator ItCallsFacetMethod()
    => Asyncize.UnityTest(async () =>
{
    // you can await tasks here
    var result = await MyTask();
});
```
