using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Unisave.Editor.BackendUploading.Hooks
{
    /// <summary>
    /// Hooks into game building.
    /// Triggers the hook when a build ends.
    /// </summary>
    public class BuildPostprocessingHook : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;
        
        public void OnPostprocessBuild(BuildReport report)
        {
            HookImplementations.OnPostprocessBuild(report);
        }
    }
}