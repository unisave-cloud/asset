using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Unisave.Editor
{
    public static class AssetCreation
    {
        [MenuItem("Assets/Create/Unisave/Backend folder", false, 10)]
        public static void CreateBackendFolder()
        {
            var path = GetCurrentDirectoryPath();

            AssetDatabase.CreateFolder(path, "Backend");
            AssetDatabase.CreateFolder(path + "/Backend", "Entities");
            AssetDatabase.CreateFolder(path + "/Backend", "Facets");
            AssetDatabase.CreateFolder(path + "/Backend", "Migrations");
        }

        [MenuItem("Assets/Create/Unisave/Entity", false, 1)]
        public static void CreateEntity()
        {
            CreateAsset(
                GetCurrentDirectoryPath() + "/NewEntity.cs",
                Resources.Load<Texture2D>("UnisaveLogo"),
                (pathName) => {
                    AssetDatabase.CreateAsset(
                        new TextAsset(),
                        pathName
                    );

                    var name = Path.GetFileNameWithoutExtension(pathName);
                    
                    File.WriteAllText(
                        pathName,
                        Resources.Load<TextAsset>("EntityTemplate").text
                            .Replace("#ENTITYNAME#", name)
                    );

                    AssetDatabase.ImportAsset(pathName);
                }
            );
        }

        [MenuItem("Assets/Create/Unisave/Facet", false, 2)]
        public static void CreateFacet()
        {
            CreateAsset(
                GetCurrentDirectoryPath() + "/NewFacet.cs",
                Resources.Load<Texture2D>("UnisaveLogo"),
                (pathName) => {
                    AssetDatabase.CreateAsset(
                        new TextAsset(),
                        pathName
                    );

                    var name = Path.GetFileNameWithoutExtension(pathName);
                    
                    File.WriteAllText(
                        pathName,
                        Resources.Load<TextAsset>("FacetTemplate").text
                            .Replace("#FACETNAME#", name)
                    );

                    AssetDatabase.ImportAsset(pathName);
                }
            );
        }

        /////////////
        // Helpers //
        /////////////

        /// <summary>
        /// Get directory where we want to create the new asset
        /// </summary>
        public static string GetCurrentDirectoryPath()
        {
            var activeObject = Selection.activeObject;

            if (Selection.activeObject == null)
                return "Assets";

            var path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());

            if (File.Exists(path))
                return Path.GetDirectoryName(path);

            return path;
        }

        /// <summary>
        /// Starts the asset creation UI in project window and calls back the inserted name on success
        /// </summary>
        public static void CreateAsset(string defaultPathName, Texture2D icon, Action<string> callback)
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                new CreateAssetCallback(callback),
                defaultPathName,
                icon,
                null
            );
        }

        private class CreateAssetCallback : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            private Action<string> callback;

            public CreateAssetCallback(Action<string> callback)
            {
                this.callback = callback;
            }

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                callback(pathName);
            }
        }
    }
}
