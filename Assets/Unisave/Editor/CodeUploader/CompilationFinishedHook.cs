using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using System.Linq;

namespace Unisave.CodeUploader
{
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
                Debug.Log("CodeUploader: stop because compile errors on target");
                return;
            }

            Uploader uploader = new Uploader();
            uploader.Run();
        }

        static bool CompilerMessagesContainError(CompilerMessage[] messages)
        {
            return messages.Any(msg => msg.type == CompilerMessageType.Error);
        }
    }
}
