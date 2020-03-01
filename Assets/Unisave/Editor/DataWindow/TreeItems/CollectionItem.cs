using System;
using Unisave.Arango.Emulation;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Unisave.Editor.DataWindow.TreeItems
{
    public sealed class CollectionItem : TreeViewItem
    {
        /// <summary>
        /// Collection being represented by this node
        /// </summary>
        public Collection Collection { get; }
        
        /// <summary>
        /// Name of the represented collection
        /// </summary>
        public string CollectionName { get; }
        
        /// <summary>
        /// Parent tree view item
        /// </summary>
        public DatabaseItem Parent => parent as DatabaseItem
            ?? throw new Exception("Parent item has unexpected type.");

        private readonly IdAllocator idAllocator;
        
        public CollectionItem(
            string collectionName,
            Collection collection,
            IdAllocator idAllocator
        )
        {
            CollectionName = collectionName;
            Collection = collection;
            this.idAllocator = idAllocator;
            
            id = idAllocator.NextId();
            displayName = collectionName;
            icon = (Texture2D)EditorGUIUtility.IconContent(
                "Folder Icon"
            ).image;
            
            BuildChildren();
        }

        private void BuildChildren()
        {
            foreach (var document in Collection)
            {
                AddChild(new DocumentItem(
                    document,
                    idAllocator
                ));
            }
        }
    }
}