using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unisave.Editor.BackendFolders;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unisave.Editor.Windows.Main
{
    public class BackendTabController : ITabContentController
    {
        private readonly VisualElement root;
        
        private VisualTreeAsset backendDefinitionItem;
        private VisualElement enabledBackendDefinitions;
        private VisualElement disabledBackendDefinitions;

        public BackendTabController(VisualElement root)
        {
            this.root = root;
        }

        public void OnCreateGUI()
        {
            // === Backend upload and compilation ===
            
            // ...
            
            // === Backend folder definition files ===
            
            backendDefinitionItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Unisave/Editor/Windows/Main/BackendDefinitionItem.uxml"
            );
            enabledBackendDefinitions = root.Q(name: "enabled-backend-definitions");
            disabledBackendDefinitions = root.Q(name: "disabled-backend-definitions");

            root.Q<Button>(className: "add-backend-folder__button").clicked
                += AddExistingBackendFolder;
            
            BackendFolderDefinition.OnAnyChange += OnObserveExternalState;
            
            // === Other ===
            
            root.RegisterCallback<DetachFromPanelEvent>(e => {
                OnDetachFromPanel();
            });
        }

        private void OnDetachFromPanel()
        {
            BackendFolderDefinition.OnAnyChange -= OnObserveExternalState;
        }

        public void OnObserveExternalState()
        {
            var defs = BackendFolderDefinition.LoadAll();
            
            RenderBackendFolderDefinitions(defs);

            // field.value = defs[0];
            // field.objectType = typeof(BackendFolderDefinition);
        }

        public void OnWriteExternalState()
        {
            // nothing so far
        }

        private void RenderBackendFolderDefinitions(BackendFolderDefinition[] defs)
        {
            enabledBackendDefinitions.Clear();
            disabledBackendDefinitions.Clear();
            
            foreach (var def in defs)
            {
                bool isEnabled = def.IsEligibleForUpload();
                
                VisualElement item = backendDefinitionItem.Instantiate();
                
                var label = item.Q<Label>(className: "backend-def__label");
                label.text = def.FolderPath;
                
                var button = item.Q<Button>(className: "backend-def__button");
                button.text = isEnabled ? "Disable" : "Enable";
                button.SetEnabled(
                    def.UploadBehaviour == UploadBehaviour.Always
                    || def.UploadBehaviour == UploadBehaviour.Never
                );
                button.clicked += () => {
                    if (def.UploadBehaviour == UploadBehaviour.Always)
                        def.UploadBehaviour = UploadBehaviour.Never;
                    else if (def.UploadBehaviour == UploadBehaviour.Never)
                        def.UploadBehaviour = UploadBehaviour.Always;
                    OnObserveExternalState();
                };
                
                var field = item.Q<ObjectField>(className: "backend-def__field");
                field.objectType = typeof(BackendFolderDefinition);
                field.value = def;
                
                if (isEnabled)
                    enabledBackendDefinitions.Add(item);
                else
                    disabledBackendDefinitions.Add(item);
            }
        }
        
        private void AddExistingBackendFolder()
        {
            // display select folder dialog
            string selectedPath = EditorUtility.OpenFolderPanel(
                "Add Existing Backend Folder", "Assets", ""
            );

            // action cancelled
            if (string.IsNullOrEmpty(selectedPath))
                return;
			
            // get path inside the assets folder
            string assetsPath = Path.GetFullPath("Assets/");
            if (selectedPath.StartsWith(assetsPath))
            {
                selectedPath = "Assets/" + selectedPath.Substring(assetsPath.Length);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    title: "Action failed",
                    message: "Selected folder is not inside the Assets " +
                             "folder of this Unity project. It cannot be added.",
                    ok: "OK"
                );
                return;
            }
			
            BackendFolderUtility.CreateDefinitionFileInFolder(selectedPath);
        }
    }
}