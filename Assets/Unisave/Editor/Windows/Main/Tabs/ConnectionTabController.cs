using System;
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

        private TextField serverUrlField;
        private TextField gameTokenField;
        private TextField editorKeyField;

        private Button openDashboardButton;

        private VisualElement emptyWarningBox;

        public ConnectionTabController(VisualElement root)
        {
            this.root = root;
        }

        public void OnCreateGUI()
        {
            openDashboardButton = root.Q<Button>(name: "open-dashboard-button");
            serverUrlField = root.Q<TextField>(name: "server-url-field");
            gameTokenField = root.Q<TextField>(name: "game-token-field");
            editorKeyField = root.Q<TextField>(name: "editor-key-field");
            emptyWarningBox = root.Q(name: "empty-warning");

            openDashboardButton.clicked += () => {
                Application.OpenURL("https://unisave.cloud/app");
            };

            gameTokenField.RegisterValueChangedCallback(e => {
                RenderNotConnectedWarning();
            });
            
            editorKeyField.RegisterValueChangedCallback(e => {
                RenderNotConnectedWarning();
            });
        }

        public void OnObserveExternalState()
        {
            var preferences = UnisavePreferences.LoadOrCreate();
            
            serverUrlField.value = preferences.ServerUrl;
            gameTokenField.value = preferences.GameToken;
            editorKeyField.value = preferences.EditorKey;
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

        public void OnWriteExternalState()
        {
            var preferences = UnisavePreferences.LoadOrCreate();
            
            preferences.ServerUrl = serverUrlField.value;
            preferences.GameToken = gameTokenField.value;
            preferences.EditorKey = editorKeyField.value;
            
            preferences.Save();
        }
    }
}