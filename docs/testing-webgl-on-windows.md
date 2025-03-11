# Testing WebGL on Windows

Unity ships with a simple web server that can host a static website from a folder. This can be used on Windows to launch a built WebGL game locally.

To start the WebGL development server on windows, open GitBash in:

```
/c/Program Files/Unity/Hub/Editor/2022.3.49f1/Editor/Data/PlaybackEngines/WebGLSupport/BuildTools
```

And start it:

```
SimpleWebServer.exe ~/Downloads/BuiltFolder 8080
```


## On Linux

Linux has this in:

```
~/Unity/Hub/Editor/2022.3.49f1/Editor/Data/PlaybackEngines/WebGLSupport/BuildTools
```

The `exe` file can be run via `mono` or `dotnet`. But you can instead just lanuch a PHP web server in the folder to be hosted:

```
php -S localhost:8080
```
