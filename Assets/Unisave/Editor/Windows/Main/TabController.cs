using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unisave.Editor.Windows.Main
{
    public class TabController
    {
        private readonly VisualElement root;
        private readonly Action<MainWindowTab> openTabCallback;

        public MainWindowTab CurrentTab { get; private set; } = MainWindowTab.None;

        public TabController(
            VisualElement root,
            Action<MainWindowTab> openTabCallback
        )
        {
            this.root = root;
            this.openTabCallback = openTabCallback;
        }

        public void RegisterCallbacks()
        {
            root.Query<ToolbarToggle>(className: "tab-head")
                .ForEach(tabHead => {
                    tabHead.RegisterValueChangedCallback(e => {
                        TabHeadClick(tabHead, e);
                    });
                });
        }
        
        public void RenderOpenedTab(MainWindowTab tab)
        {
            CurrentTab = tab;
            
            string tabName = Enum.GetName(typeof(MainWindowTab), tab);
            string headName = "tab-head__" + tabName;
            string contentName = "tab-content__" + tabName;
            
            // set all tab head values
            root.Query<ToolbarToggle>(className: "tab-head")
                .ForEach(t => {
                    t.SetValueWithoutNotify(t.name == headName);
                });
            
            // set all tab content visibilities
            root.Query<VisualElement>(className: "tab-content")
                .ForEach(content => {
                    if (content.name == contentName)
                        content.AddToClassList("tab-content--active");
                    else
                        content.RemoveFromClassList("tab-content--active");
                });
        }

        private void TabHeadClick(ToolbarToggle tabHead, ChangeEvent<bool> e)
        {
            // ignore setting to false
            if (!e.newValue)
                return;

            string tabName = tabHead.name.Substring("tab-head__".Length);
            var tab = (MainWindowTab)Enum.Parse(typeof(MainWindowTab), tabName);
            openTabCallback.Invoke(tab);
        }
    }
}