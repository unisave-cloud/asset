using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Unisave.Editor.BackendUploading.Hooks
{
    /// <summary>
    /// Contains implementations for individual hooks
    /// </summary>
    public static class HookImplementations
    {
        /// <summary>
        /// Called often, whenever code in the Unity project changes.
        /// It recalculates the backend hash and if it changes and automatic
        /// upload is enabled, it will perform the automatic upload.
        /// </summary>
        public static void OnAssemblyCompilationFinished()
        {
            var uploader = Uploader.GetDefaultInstance();
            
            bool upload = uploader.RecalculateBackendHash();

            if (upload && uploader.AutomaticUploadingEnabled)
            {
                uploader.UploadBackend(
                    verbose: false,
                    useAnotherThread: false
                );
            }
        }

        /// <summary>
        /// Called before a build starts. It uploads the backend if automatic
        /// upload is enabled.
        /// </summary>
        /// <param name="report"></param>
        public static void OnPreprocessBuild(BuildReport report)
        {
            var uploader = Uploader.GetDefaultInstance();
            
            if (uploader.AutomaticUploadingEnabled)
            {
                uploader.UploadBackend(
                    verbose: true, // here we ARE verbose, since we're building
                    useAnotherThread: false
                );
            }
        }
        
        /// <summary>
        /// Called when a build finishes. It checks backend hash and registers
        /// the new build in the Unisave cloud.
        /// </summary>
        /// <param name="report"></param>
        public static void OnPostprocessBuild(BuildReport report)
        {
            // skip unsuccessful builds
            if (report.summary.totalErrors > 0)
            {
                Debug.LogWarning(
                    "[Unisave] Skipping build registration because " +
                    $"the build had {report.summary.totalErrors} errors."
                );
                return;
            }

            // check that the backendHash in preferences is up to date
            bool uploadNeeded = Uploader.GetDefaultInstance()
                .RecalculateBackendHash();

            if (uploadNeeded)
            {
                Debug.LogWarning(
                    "[Unisave] This backend has not yet been uploaded, " +
                    "therefore build registration is being skipped. " +
                    "Enable automatic backend upload or upload the backend " +
                    "manually before you build your game to resolve this issue."
                );
                return;
            }
            
            // register the build
            
            BuildRegistrator
                .GetDefaultInstance()
                .RegisterBuild(report);
        }
    }
}