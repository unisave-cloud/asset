using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.Callbacks;
using UnityEditor.Build.Reporting;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityAssembly = UnityEditor.Compilation.Assembly;

namespace Unisave.Weaver
{


    // [InitializeOnLoad]
    // public class DisableScripReloadInPlayMode
    // {
    //     static DisableScripReloadInPlayMode()
    //     {
    //         EditorApplication.playModeStateChanged
    //             += OnPlayModeStateChanged;
    //     }
    
    //     static void OnPlayModeStateChanged(PlayModeStateChange stateChange)
    //     {
    //         switch (stateChange) {
    //             case (PlayModeStateChange.EnteredPlayMode): {
    //                 EditorApplication.LockReloadAssemblies();
    //                 Debug.Log ("Assembly Reload locked as entering play mode");
    //                 break;
    //             }
    //             case (PlayModeStateChange.ExitingPlayMode): {
    //                 Debug.Log ("Assembly Reload unlocked as exiting play mode");
    //                 EditorApplication.UnlockReloadAssemblies();
    //                 break;
    //             }
    //         }
    //     }
    
    // }









    public static class CompilationFinishedHook
    {
        [InitializeOnLoadMethod]
        static void OnInitializeOnLoad()
        {
            CompilationPipeline.assemblyCompilationFinished += OnCompilationFinished;
        }

        static void OnCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            // Do nothing if there were compile errors on the target
            if (CompilerMessagesContainError(messages))
            {
                Debug.Log("Weaver: stop because compile errors on target");
                return;
            }

            // Should not run on the editor only assemblies
            if (assemblyPath.Contains("-Editor") || assemblyPath.Contains(".Editor"))
                return;

            // don't weave unisave files
            string assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            if (assemblyName == Weaver.UnisaveFrameworkAssemblyName ||
                assemblyName == Weaver.UnisaveWeaverAssemblyName)
                return;

            // weave the assembly
            var weaver = new Weaver(assemblyPath);
            weaver.Weave();
        }

        static bool CompilerMessagesContainError(CompilerMessage[] messages)
        {
            return messages.Any(msg => msg.type == CompilerMessageType.Error);
        }
    }
}
