using UnityEngine;
using UnityEngine.UIElements;

namespace Unisave.Editor.Windows.Main.Tabs
{
    public class HomeTabController : ITabContentController
    {
        private readonly VisualElement root;

        public HomeTabController(VisualElement root)
        {
            this.root = root;
        }

        public void OnCreateGUI()
        {
            root.Q<Button>(name: "link-guides").clicked += () => {
                Application.OpenURL("https://unisave.cloud/guides");
            };
            root.Q<Button>(name: "link-documentation").clicked += () => {
                Application.OpenURL("https://unisave.cloud/docs");
            };
            root.Q<Button>(name: "link-discord").clicked += () => {
                Application.OpenURL("https://discord.gg/XV696Tp");
            };
            root.Q<Button>(name: "link-pricing").clicked += () => {
                Application.OpenURL("https://unisave.cloud/#pricing");
            };
        }

        public void OnObserveExternalState()
        {
            // nothing
        }

        public void OnWriteExternalState()
        {
            // nothing
        }
    }
}