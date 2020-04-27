using Unisave.Arango;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Unisave.Editor.DataWindow.TreeItems
{
    public sealed class EmulatedDatabasesItem : TreeViewItem
    {
        public ArangoRepository Repository { get; }
        private readonly IdAllocator idAllocator;
        
        public EmulatedDatabasesItem(
            ArangoRepository repository,
            IdAllocator idAllocator
        )
        {
            this.Repository = repository;
            this.idAllocator = idAllocator;
            
            id = idAllocator.NextId();
            displayName = "Emulated databases";
            icon = (Texture2D)EditorGUIUtility.IconContent(
                "d_Profiler.Physics"
            ).image;
            
            BuildChildren();
        }

        private void BuildChildren()
        {
            foreach (var pair in Repository.EnumerateDatabases())
            {
                AddChild(new DatabaseItem(
                    pair.Key,
                    pair.Value,
                    idAllocator
                ));
            }
        }
    }
}