using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Unisave.Editor
{
    /// <summary>
    /// This class is responsible for the "Create >> Unisave >> *" context menu
    /// </summary>
    public static class AssetCreation
    {
        [MenuItem("Assets/Create/Unisave/Entity", false, 2)]
        public static void CreateEntity()
        {
            CreateScriptFromTemplate(
                defaultName: "NewEntity",
                templateName: "EntityTemplate.txt",
                wildcard: "#ENTITYNAME#"
            );
        }

        [MenuItem("Assets/Create/Unisave/Facet", false, 3)]
        public static void CreateFacet()
        {
            CreateScriptFromTemplate(
                defaultName: "NewFacet",
                templateName: "FacetTemplate.txt",
                wildcard: "#FACETNAME#"
            );
        }
        
        [MenuItem("Assets/Create/Unisave/Auth/PlayerEntity", false, 4)]
        public static void CreatePlayerEntity()
        {
            CreateScriptFromTemplate(
                defaultName: "PlayerEntity",
                templateName: "Auth/PlayerEntityTemplate.txt",
                wildcard: "#ENTITYNAME#"
            );
        }
        
        [MenuItem("Assets/Create/Unisave/Auth/AuthFacet", false, 5)]
        public static void CreateAuthFacet()
        {
            CreateScriptFromTemplate(
                defaultName: "AuthFacet",
                templateName: "Auth/AuthFacetTemplate.txt",
                wildcard: "#FACETNAME#"
            );
        }
        
        [MenuItem("Assets/Create/Unisave/Auth/LoginController", false, 6)]
        public static void CreateLoginController()
        {
            CreateScriptFromTemplate(
                defaultName: "LoginController",
                templateName: "Auth/LoginControllerTemplate.txt",
                wildcard: "#CONTROLLERNAME#"
            );
        }
        
        [MenuItem("Assets/Create/Unisave/Auth/RegistrationController", false, 6)]
        public static void CreateRegistrationController()
        {
            CreateScriptFromTemplate(
                defaultName: "RegistrationController",
                templateName: "Auth/RegistrationControllerTemplate.txt",
                wildcard: "#CONTROLLERNAME#"
            );
        }

        [MenuItem("Assets/Create/Unisave/Backend folder", false, 20)]
        public static void CreateBackendFolder()
        {
            var path = GetCurrentDirectoryPath();

            AssetDatabase.CreateFolder(path, "Backend");
            AssetDatabase.CreateFolder(path + "/Backend", "Entities");
            AssetDatabase.CreateFolder(path + "/Backend", "Facets");
            
            CreateTextAssetFromTemplate(
                path + "/Backend/Entities/PlayerEntity.cs",
                "Auth/PlayerEntityTemplate.txt",
                "#ENTITYNAME#",
                "PlayerEntity"
            );
            CreateTextAssetFromTemplate(
                path + "/Backend/Facets/AuthFacet.cs",
                "Auth/AuthFacetTemplate.txt",
                "#FACETNAME#",
                "AuthFacet"
            );
        }

        /////////////
        // Helpers //
        /////////////

        /// <summary>
        /// Creates a text asset at given path from a given template
        /// (no wildcard substitution performed)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="templateName"></param>
        private static void CreateTextAssetFromTemplate(
            string path,
            string templateName,
            string wildcard,
            string wildcardValue
        )
        {
            AssetDatabase.CreateAsset(new TextAsset(), path);
                    
            File.WriteAllText(
                path,
                AssetDatabase.LoadAssetAtPath<TextAsset>(
                    "Assets/Unisave/Templates/" + templateName
                ).text.Replace(wildcard, wildcardValue)
            );

            AssetDatabase.ImportAsset(path);
        }

        /// <summary>
        /// Creates a CS script from template
        /// </summary>
        /// <param name="defaultName">Default name of the file and the main class</param>
        /// <param name="templateName">Name of the template resource file</param>
        /// <param name="wildcard">Wildcard for the class name</param>
        private static void CreateScriptFromTemplate(
            string defaultName,
            string templateName,
            string wildcard
        )
        {
            CreateAsset(
                GetCurrentDirectoryPath() + "/" + defaultName + ".cs",
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    "Assets/Unisave/Images/NewAssetIcon.png"
                ),
                (pathName) => {
                    string name = Path.GetFileNameWithoutExtension(pathName);
                    
                    File.WriteAllText(
                        pathName,
                        AssetDatabase.LoadAssetAtPath<TextAsset>(
                            "Assets/Unisave/Templates/" + templateName
                        ).text.Replace(wildcard, name)
                    );

                    AssetDatabase.ImportAsset(pathName);
                }
            );
        }

        /// <summary>
        /// Get directory where we want to create the new asset
        /// </summary>
        private static string GetCurrentDirectoryPath()
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
        private static void CreateAsset(string defaultPathName, Texture2D icon, Action<string> callback)
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<CreateAssetCallback>().SetCallback(callback),
                defaultPathName,
                icon,
                null
            );
        }

        // inherits indirectly from ScriptableObject, so cannot be created using constructor
        private class CreateAssetCallback : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            private Action<string> callback;

            public CreateAssetCallback SetCallback(Action<string> callback)
            {
                this.callback = callback;
                return this;
            }

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                callback(pathName);
            }
        }
    }
}
