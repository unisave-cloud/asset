using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unisave.Editor.Windows.Main
{
    public class UnisaveMainWindow : EditorWindow
    {
        [MenuItem("Window/Unisave/Unisave Main Window", false, 1)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(
                typeof(UnisaveMainWindow),
                false,
                "Unisave"
            );
        }

        private TabController tabController;
        
        private void CreateGUI()
        {
            titleContent.image = AssetDatabase.LoadAssetAtPath<Texture>(
                EditorGUIUtility.isProSkin ?
                    "Assets/Unisave/Images/WindowIconWhite.png" :
                    "Assets/Unisave/Images/WindowIcon.png"
            );
            
            // set up UI tree
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Unisave/Editor/Windows/Main/UnisaveMainWindow.uxml"
            );
            rootVisualElement.Add(visualTree.Instantiate());

            // set up the tab controller
            tabController = new TabController(rootVisualElement);
            tabController.RegisterCallbacks();
            
            // open the home tab
            tabController.OpenTab(MainWindowTab.Home);
        }
    }
}