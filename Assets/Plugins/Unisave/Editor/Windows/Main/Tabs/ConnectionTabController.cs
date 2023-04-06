using System;
using LightJson;
using Unisave.Editor.Auditing;
using Unisave.Foundation;
using UnityEngine;
using UnityEngine.UIElements;
using Application = UnityEngine.Application;

namespace Unisave.Editor.Windows.Main.Tabs
{
    public class ConnectionTabController : ITabContentController
    {
        public Action<TabTaint> SetTaint { get; set; }
        
        private readonly VisualElement root;

        private UnisavePreferences preferences;

        private TextField serverUrlField;
        private TextField gameTokenField;
        private TextField editorKeyField;

        private Button openDashboardButton;

        private VisualElement emptyWarningBox;
        private Button saveButton;

        public ConnectionTabController(VisualElement root)
        {
            this.root = root;
        }

        public void OnCreateGUI()
        {
            preferences = UnisavePreferences.Resolve();
            
            openDashboardButton = root.Q<Button>(name: "open-dashboard-button");
            serverUrlField = root.Q<TextField>(name: "server-url-field");
            gameTokenField = root.Q<TextField>(name: "game-token-field");
            editorKeyField = root.Q<TextField>(name: "editor-key-field");
            emptyWarningBox = root.Q(name: "empty-warning");
            saveButton = root.Q<Button>(name: "save-button");

            openDashboardButton.clicked += () => {
                Application.OpenURL("https://unisave.cloud/app");
            };

            saveButton.clicked += SaveButtonClicked;

            gameTokenField.RegisterValueChangedCallback(e => {
                RenderNotConnectedWarning();
                RenderSaveButton();
            });
            
            editorKeyField.RegisterValueChangedCallback(e => {
                RenderNotConnectedWarning();
                RenderSaveButton();
            });

            serverUrlField.RegisterValueChangedCallback(e => {
                RenderSaveButton();
            });
        }

        public void OnObserveExternalState()
        {
            // NOTE: Do not load preferences here, since this gets called during various
            // re-imports during compilation and at these times, calling the
            // Resolve method will re-create the file despite it already existing.
            
            serverUrlField.value = preferences.ServerUrl;
            gameTokenField.value = preferences.GameToken;
            editorKeyField.value = preferences.EditorKey;

            RenderNotConnectedWarning();
            RenderSaveButton();
        }

        private void RenderNotConnectedWarning()
        {
            bool notConnected = string.IsNullOrWhiteSpace(gameTokenField.value) ||
                string.IsNullOrWhiteSpace(editorKeyField.value);
            
            emptyWarningBox.EnableInClassList(
                "is-hidden", enable: !notConnected
            );

            SetTaint?.Invoke(notConnected ? TabTaint.Warning : TabTaint.None);
        }

        private void RenderSaveButton()
        {
            if (preferences.ServerUrl == serverUrlField.value
                && preferences.GameToken == gameTokenField.value
                && preferences.EditorKey == editorKeyField.value)
            {
                // no change
                saveButton.SetEnabled(false);
                saveButton.text = "Saved";
            }
            else
            {
                // there is a change
                saveButton.SetEnabled(true);
                saveButton.text = "Save";
            }
        }

        private void SaveButtonClicked()
        {
            SaveUnisavePreferences();
            
            UnisaveAuditing.EmitEvent(
                eventType: "asset.saveConnection",
                message: "Cloud connection details have been saved.",
                data: new JsonObject {
                    ["gameToken"] = gameTokenField.value,
                    ["hasEditorKey"] = !string.IsNullOrEmpty(editorKeyField.value)
                }
            );
        }

        public void OnWriteExternalState()
        {
            if (preferences.ServerUrl == serverUrlField.value
                && preferences.GameToken == gameTokenField.value
                && preferences.EditorKey == editorKeyField.value)
                return; // no need to save anything (IMPORTANT!)
            
            SaveUnisavePreferences();
        }

        void SaveUnisavePreferences()
        {
            // important, sometimes after re-import, the instance looses track
            // of the asset state
            preferences = UnisavePreferences.Resolve();
            
            preferences.ServerUrl = serverUrlField.value;
            preferences.GameToken = gameTokenField.value;
            preferences.EditorKey = editorKeyField.value;
            
            preferences.Save();
            
            OnObserveExternalState();
        }
    }
}