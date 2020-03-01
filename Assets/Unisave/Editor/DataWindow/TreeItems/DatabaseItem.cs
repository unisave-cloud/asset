using System;
using Unisave.Arango.Emulation;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Unisave.Editor.DataWindow.TreeItems
{
    public sealed class DatabaseItem : TreeViewItem
    {
        /// <summary>
        /// Database being represented by this node
        /// </summary>
        public ArangoInMemory Database { get; }
        
        /// <summary>
        /// Name of the represented database
        /// </summary>
        public string DatabaseName { get; }
        
        /// <summary>
        /// Parent tree view item
        /// </summary>
        public EmulatedDatabasesItem Parent => parent as EmulatedDatabasesItem
            ?? throw new Exception("Parent item has unexpected type.");

        private readonly IdAllocator idAllocator;

        public DatabaseItem(
            string databaseName,
            ArangoInMemory database,
            IdAllocator idAllocator
        )
        {
            DatabaseName = databaseName;
            Database = database;
            this.idAllocator = idAllocator;

            id = idAllocator.NextId();
            displayName = databaseName;
            icon = (Texture2D)EditorGUIUtility.IconContent(
                "Collab.BuildSucceeded"
            ).image;
            
            BuildChildren();
        }

        private void BuildChildren()
        {
            foreach (var pair in Database.Collections)
            {
                AddChild(new CollectionItem(
                    pair.Key,
                    pair.Value,
                    idAllocator
                ));
            }
        }

        /// <summary>
        /// This is called when data inside the database have been
        /// modified by the custom inspector
        /// </summary>
        public void HasBeenModified()
        {
            Parent.Repository.SaveDatabase(DatabaseName, Database);
            DataEditorWindow.Refresh();
        }
    }
}
