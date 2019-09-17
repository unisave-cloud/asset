using UnityEditor;
using UnityEditor.Callbacks;

namespace Unisave.Editor.BackendUploading
{
    public static class PostProcessBuildHook
    {
        [PostProcessBuild]
        public static void OnPostprocessBuild(
            BuildTarget target, string pathToBuiltProject
        )
        {
            Uploader
                .GetDefaultInstance()
                .RunAutomaticUpload(isEditor: false);
        }
    }
}