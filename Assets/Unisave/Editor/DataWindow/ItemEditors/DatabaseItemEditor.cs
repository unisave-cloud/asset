using Unisave.Arango;
using Unisave.Editor.DataWindow.SelectionWrappers;
using Unisave.Editor.DataWindow.TreeItems;
using UnityEditor;
using UnityEngine;

namespace Unisave.Editor.DataWindow.ItemEditors
{
    [CustomEditor(typeof(DatabaseSelectionWrapper))]
    public class DatabaseItemEditor : UnityEditor.Editor
    {
        /// <summary>
        /// The tree view item being displayed
        /// </summary>
        private DatabaseItem item;

        private int documentCount;
        
        private string newCollectionName;
        private string newCollectionError;

        private void OnEnable()
        {
            item = (target as DatabaseSelectionWrapper)?.TreeItem;
            
            if (item == null)
                return;

            documentCount = 0;
            foreach (var pair in item.Database.Collections)
                documentCount += pair.Value.DocumentCount;
        }

        public override void OnInspectorGUI()
        {
            if (item == null)
                return;

            UnisaveEditorHelper.InspectorHeading("Database", item.icon);

            UnisaveEditorHelper.LabeledBox("Properties", () => {
                UnisaveEditorHelper.ReadOnlyField("Name", item.DatabaseName);
                UnisaveEditorHelper.ReadOnlyField(
                    "Collections",
                    item.Database.Collections.Count.ToString()
                );
                UnisaveEditorHelper.ReadOnlyField(
                    "Documents",
                    documentCount.ToString()
                );
            });
            
            UnisaveEditorHelper.StringCreationBox(
                title: "Create collection",
                fieldLabel: "Collection name",
                buttonText: "Create",
                fieldValue: ref newCollectionName,
                errorMessage: newCollectionError,
                submit: CreateCollection
            );
            
            UnisaveEditorHelper.LabeledBox("Database actions", () => {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Truncate"))
                    TruncateDatabase();
                if (GUILayout.Button("Clear"))
                    ClearDatabase();
                if (GUILayout.Button("Delete"))
                    DeleteDatabase();
                EditorGUILayout.EndHorizontal();
            });
        }

        private void CreateCollection()
        {
            if (string.IsNullOrEmpty(newCollectionName))
            {
                newCollectionError = "No collection name provided";
                return;
            }
            
            try
            {
                item.Database.CreateCollection(
                    newCollectionName,
                    CollectionType.Document
                );
            }
            catch (ArangoException e)
            {
                newCollectionError = "Error: " + e.ErrorMessage;
                return;
            }

            newCollectionError = null;
            item.HasBeenModified();
        }

        private void ClearDatabase()
        {
            item.Database.Clear();
            item.HasBeenModified();
        }

        private void TruncateDatabase()
        {
            foreach (var pair in item.Database.Collections)
                pair.Value.Truncate();
            
            item.HasBeenModified();
        }

        private void DeleteDatabase()
        {
            item.Parent.Repository.DeleteDatabase(item.DatabaseName);
            DataEditorWindow.Refresh();
            Selection.SetActiveObjectWithContext(null, null);
        }
    }
}