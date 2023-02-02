using System.IO;
using Unisave.Editor.BackendFolders;
using Unisave.Editor.BackendUploading;
using Unisave.Foundation;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unisave.Editor.Windows.Main.Tabs
{
    public class BackendTabController : ITabContentController
    {
        private readonly VisualElement root;
        
        private VisualTreeAsset backendDefinitionItem;
        private VisualElement enabledBackendDefinitions;
        private VisualElement disabledBackendDefinitions;

        private Toggle automaticUploadToggle;
        private Button manualUploadButton;
        private Label lastUploadAtLabel;
        private Label backendHashLabel;

        public BackendTabController(VisualElement root)
        {
            this.root = root;
        }

        public void OnCreateGUI()
        {
            // === Backend upload and compilation ===
            
            automaticUploadToggle = root.Q<Toggle>(name: "automatic-upload-toggle");
            manualUploadButton = root.Q<Button>(name: "manual-upload-button");
            lastUploadAtLabel = root.Q<Label>(name: "last-upload-at-label");
            backendHashLabel = root.Q<Label>(name: "backend-hash-label");
            
            manualUploadButton.clicked += RunManualCodeUpload;
            
            // === Backend folder definition files ===
            
            backendDefinitionItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Unisave/Editor/Windows/Main/UI/BackendDefinitionItem.uxml"
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
            // === Backend upload and compilation ===
            
            var preferences = UnisavePreferences.LoadOrCreate();
            automaticUploadToggle.value = preferences.AutomaticBackendUploading;
            lastUploadAtLabel.text = preferences.LastBackendUploadAt
                ?.ToString("yyyy-MM-dd H:mm:ss") ?? "Never";
            backendHashLabel.text = string.IsNullOrWhiteSpace(preferences.BackendHash)
                ? "<not computed yet>"
                : preferences.BackendHash;
            
            // === Backend folder definition files ===
            
            var defs = BackendFolderDefinition.LoadAll();
            RenderBackendFolderDefinitions(defs);
        }

        public void OnWriteExternalState()
        {
            var preferences = UnisavePreferences.LoadOrCreate();
            preferences.AutomaticBackendUploading = automaticUploadToggle.value;
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
                        DisableBackendFolder(def);
                    else if (def.UploadBehaviour == UploadBehaviour.Never)
                        EnableBackendFolder(def);
                    OnObserveExternalState(); // refresh window
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
        
        ////////////////////
        // Action Methods //
        ////////////////////
		
        void RunManualCodeUpload()
        {
            Uploader
                .GetDefaultInstance()
                .UploadBackend(
                    verbose: true,
                    useAnotherThread: true // yes, here we can run in background
                );
        }

        void EnableBackendFolder(BackendFolderDefinition def)
        {
            def.UploadBehaviour = UploadBehaviour.Always;
            EditorUtility.SetDirty(def);
			
            HighlightBackendFolderInInspector(def);
        }
		
        void DisableBackendFolder(BackendFolderDefinition def)
        {
            def.UploadBehaviour = UploadBehaviour.Never;
            EditorUtility.SetDirty(def);
			
            HighlightBackendFolderInInspector(def);
        }

        void HighlightBackendFolderInInspector(BackendFolderDefinition def)
        {
            Selection.activeObject = def;
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