using UnityEditor;
using UnityEditor.Compilation;
using System.Linq;

namespace Unisave.Editor.BackendUploading
{
    /// <summary>
    /// Hooks into compilation finished event.
    /// Abstracts away compilation of individual assemblies
    /// into a single recompilation event.
    /// This single event then triggers backend uploading.
    /// </summary>
    public static class CompilationFinishedHook
    {
        /// <summary>
        /// Registers the assembly compilation finished hook
        /// </summary>
        [InitializeOnLoadMethod]
        private static void OnInitializeOnLoad()
        {
            CompilationPipeline.assemblyCompilationFinished
                += OnAssemblyCompilationFinished;
        }

        /// <summary>
        /// When any single assembly inside the project gets compiled
        /// </summary>
        private static void OnAssemblyCompilationFinished(
            string assemblyPath, CompilerMessage[] messages
        )
        {
            // we will pick on "Assembly-CSharp.dll" only
            // if another assembly is compiled, it bubbles up the dependency
            // structure and this main assembly will eventually get recompiled
            // (if no errors present on the dependencies)
            if (!assemblyPath.Contains("Assembly-CSharp.dll"))
                return;

            // also if errors are present on the main assembly,
            // we know it would fail on the server as well so just
            // ignore the upload immediately
            if (messages.Any(m => m.type == CompilerMessageType.Error))
                return;

            // now we've filtered out the event we are interested in
            OnCompilationFinished();
        }

        /// <summary>
        /// When the entire compilation process of all assemblies finishes
        /// </summary>
        private static void OnCompilationFinished()
        {
            Uploader
                .GetDefaultInstance()
                .RunAutomaticUpload(isEditor: true);
        }
    }
}
