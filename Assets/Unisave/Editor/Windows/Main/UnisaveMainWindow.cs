using System;
using System.Collections.Generic;
using Unisave.Editor.Windows.Main.Tabs;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unisave.Editor.Windows.Main
{
    public class UnisaveMainWindow : EditorWindow
    {
        public const string WindowTitle = "Unisave";

        /// <summary>
        /// Controls the tab opening and tab heads
        /// </summary>
        private TabsController tabsController;
        
        /// <summary>
        /// Tab content controllers for each tab
        /// </summary>
        private Dictionary<MainWindowTab, ITabContentController> tabContents;
        
        [MenuItem("Window/Unisave/Unisave Main Window", false, 1)]
        public static void ShowWindow()
        {
            ShowTab(MainWindowTab.Home);
        }

        /// <summary>
        /// Call this to open/focus the window on a specific tab when something
        /// important happens and the user needs to know about it
        /// </summary>
        /// <param name="tab">Which tab should the window show</param>
        public static void ShowTab(MainWindowTab tab)
        {
            var window = EditorWindow.GetWindow<UnisaveMainWindow>(
                utility: false,
                title: WindowTitle,
                focus: true
            );
            window.Show();
            window.OpenTab(tab);
        }

        private void OpenTab(MainWindowTab tab)
        {
            // save the old tab content
            OnWriteExternalState();
            
            // open the new tab
            tabsController.RenderOpenedTab(tab);
            
            // let the new tab refresh its content
            OnObserveExternalState();
        }
        
        private void CreateGUI()
        {
            titleContent.image = AssetDatabase.LoadAssetAtPath<Texture>(
                EditorGUIUtility.isProSkin ?
                    "Assets/Unisave/Images/WindowIconWhite.png" :
                    "Assets/Unisave/Images/WindowIcon.png"
            );
            
            // set up UI tree
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Unisave/Editor/Windows/Main/UI/UnisaveMainWindow.uxml"
            );
            rootVisualElement.Add(visualTree.Instantiate());
            
            // register mouse leave event
            rootVisualElement.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            
            // create individual tab controllers
            tabContents = new Dictionary<MainWindowTab, ITabContentController>();
            
            tabContents[MainWindowTab.Backend] = new BackendTabController(
                rootVisualElement.Q(name: "tab-content__Backend")
            );
            tabContents[MainWindowTab.Connection] = new ConnectionTabController(
                rootVisualElement.Q(name: "tab-content__Connection")
            );
            
            foreach (var content in tabContents.Values)
                content.OnCreateGUI();

            // set up the tab head controller
            tabsController = new TabsController(rootVisualElement, OpenTab);
            tabsController.RegisterCallbacks();
            
            // open the home tab
            OpenTab(MainWindowTab.Home);
        }

        private void OnFocus() => OnObserveExternalState();
        private void OnLostFocus() => OnWriteExternalState();
        private void OnMouseLeave(MouseLeaveEvent e) => OnWriteExternalState();
        
        /// <summary>
        /// Called when the content of the window should update to correspond
        /// with the reality outside the window and inside the unity editor
        /// and the filesystem. (When the window should refresh what it displays)
        /// </summary>
        private void OnObserveExternalState()
        {
            if (tabContents == null || tabsController == null)
                return;
            
            if (tabContents.ContainsKey(tabsController.CurrentTab))
                tabContents[tabsController.CurrentTab].OnObserveExternalState();
        }

        /// <summary>
        /// Called when the modified content of the window should be written
        /// to the surrounding editor and filesystem. (When the window should
        /// save any modifications)
        /// </summary>
        private void OnWriteExternalState()
        {
            if (tabContents == null || tabsController == null)
                return;
            
            if (tabContents.ContainsKey(tabsController.CurrentTab))
                tabContents[tabsController.CurrentTab].OnWriteExternalState();
        }
    }
}