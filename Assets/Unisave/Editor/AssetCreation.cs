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
        [MenuItem("Assets/Create/Unisave/Entity", false, 1)]
        public static void CreateEntity()
        {
            CreateScriptFromTemplate(
                defaultName: "NewEntity",
                templateName: "Templates/EntityTemplate",
                wildcard: "#ENTITYNAME#"
            );
        }

        [MenuItem("Assets/Create/Unisave/Facet", false, 2)]
        public static void CreateFacet()
        {
            CreateScriptFromTemplate(
                defaultName: "NewFacet",
                templateName: "Templates/FacetTemplate",
                wildcard: "#FACETNAME#"
            );
        }

        [MenuItem("Assets/Create/Unisave/Hook/OnPlayerRegistration", false, 3)]
        public static void CreatePlayerRegistrationHook()
        {
            CreateScriptFromTemplate(
                defaultName: "OnPlayerRegistration",
                templateName: "Templates/Hooks/PlayerRegistrationHookTemplate",
                wildcard: "#HOOKNAME#"
            );
        }

        [MenuItem("Assets/Create/Unisave/Boilerplate/CreatePlayerEntityOnPlayerRegistration", false, 4)]
        public static void CreateBoilerplateCreatePlayerEntity()
        {
            CreateScriptFromTemplate(
                defaultName: "CreatePlayerEntityOnPlayerRegistration",
                templateName: "Templates/Boilerplate/CreatePlayerEntityOnRegistrationHookTemplate",
                wildcard: "#HOOKNAME#"
            );
        }
        
        [MenuItem("Assets/Create/Unisave/Matchmaking/Matchmaker", false, 5)]
        public static void CreateMatchmaker()
        {
            CreateScriptFromTemplate(
                defaultName: "Matchmaker",
                templateName: "Templates/Components/MatchmakerTemplate",
                wildcard: "#NO-WILDCARD-PRESENT#"
            );
        }
        
        [MenuItem("Assets/Create/Unisave/Matchmaking/MatchmakerClient", false, 6)]
        public static void CreateMatchmakerClient()
        {
            CreateScriptFromTemplate(
                defaultName: "MatchmakerClient",
                templateName: "Templates/Components/MatchmakerClientTemplate",
                wildcard: "#MATCHMAKERCLIENT#"
            );
        }

        [MenuItem("Assets/Create/Unisave/Backend folder", false, 20)]
        public static void CreateBackendFolder()
        {
            var path = GetCurrentDirectoryPath();

            AssetDatabase.CreateFolder(path, "Backend");
            AssetDatabase.CreateFolder(path + "/Backend", "Entities");
            AssetDatabase.CreateFolder(path + "/Backend", "Facets");
            AssetDatabase.CreateFolder(path + "/Backend", "Migrations");
        }

        /////////////
        // Helpers //
        /////////////

        /// <summary>
        /// Creates a CS script from template
        /// </summary>
        /// <param name="defaultName">Default name of the file and the main class</param>
        /// <param name="templateName">Name of the template resource file</param>
        /// <param name="wildcard">Wildcard for the class name</param>
        public static void CreateScriptFromTemplate(string defaultName, string templateName, string wildcard)
        {
            CreateAsset(
                GetCurrentDirectoryPath() + "/" + defaultName + ".cs",
                Resources.Load<Texture2D>("UnisaveLogo"),
                (pathName) => {
                    AssetDatabase.CreateAsset(
                        new TextAsset(),
                        pathName
                    );

                    var name = Path.GetFileNameWithoutExtension(pathName);
                    
                    File.WriteAllText(
                        pathName,
                        Resources.Load<TextAsset>(templateName).text
                            .Replace(wildcard, name)
                    );

                    AssetDatabase.ImportAsset(pathName);
                }
            );
        }

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
