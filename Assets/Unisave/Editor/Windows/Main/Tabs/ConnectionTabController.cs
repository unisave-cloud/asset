using Unisave.Foundation;
using UnityEngine;
using UnityEngine.UIElements;
using Application = UnityEngine.Application;

namespace Unisave.Editor.Windows.Main.Tabs
{
    public class ConnectionTabController : ITabContentController
    {
        private readonly VisualElement root;

        private TextField serverUrlField;
        private TextField gameTokenField;
        private TextField editorKeyField;

        private Button openDashboardButton;

        public ConnectionTabController(VisualElement root)
        {
            this.root = root;
        }

        public void OnCreateGUI()
        {
            serverUrlField = root.Q<TextField>(name: "server-url-field");
            gameTokenField = root.Q<TextField>(name: "game-token-field");
            editorKeyField = root.Q<TextField>(name: "editor-key-field");

            openDashboardButton = root.Q<Button>(name: "open-dashboard-button");

            openDashboardButton.clicked += () => {
                Application.OpenURL("https://unisave.cloud/app");
            };
        }

        public void OnObserveExternalState()
        {
            var preferences = UnisavePreferences.LoadOrCreate();
            
            serverUrlField.value = preferences.ServerUrl;
            gameTokenField.value = preferences.GameToken;
            editorKeyField.value = preferences.EditorKey;
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