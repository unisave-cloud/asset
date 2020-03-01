using LightJson;
using Unisave.Arango;
using Unisave.Editor.DataWindow.SelectionWrappers;
using Unisave.Editor.DataWindow.TreeItems;
using UnityEditor;
using UnityEngine;

namespace Unisave.Editor.DataWindow.ItemEditors
{
    [CustomEditor(typeof(CollectionSelectionWrapper))]
    public class CollectionItemEditor : UnityEditor.Editor
    {
        /// <summary>
        /// The tree view item being displayed
        /// </summary>
        private CollectionItem item;

        private string newDocumentKey;
        private string newDocumentError;
        
        private void OnEnable()
        {
            item = (target as CollectionSelectionWrapper)?.TreeItem;
        }
        
        public override void OnInspectorGUI()
        {
            if (item == null)
                return;

            UnisaveEditorHelper.InspectorHeading("Collection", item.icon);

            UnisaveEditorHelper.LabeledBox("Properties", () => {
                UnisaveEditorHelper.ReadOnlyField("Name", item.CollectionName);
                UnisaveEditorHelper.ReadOnlyField(
                    "Documents",
                    item.Collection.DocumentCount.ToString()
                );
            });
            
            UnisaveEditorHelper.StringCreationBox(
                title: "Create document",
                fieldLabel: "Document key",
                buttonText: "Create",
                fieldValue: ref newDocumentKey,
                errorMessage: newDocumentError,
                submit: CreateDocument
            );
            
            UnisaveEditorHelper.LabeledBox("Collection actions", () => {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Truncate"))
                    TruncateCollection();
                if (GUILayout.Button("Delete"))
                    DeleteCollection();
                EditorGUILayout.EndHorizontal();
            });
        }
        
        private void CreateDocument()
        {
            JsonValue key = JsonValue.Null;

            if (!string.IsNullOrEmpty(newDocumentKey))
                key = newDocumentKey;
            
            try
            {
                item.Collection.InsertDocument(
                    new JsonObject()
                        .Add("_key", key),
                    new JsonObject()
                        .Add("ignoreErrors", false)
                );
            }
            catch (ArangoException e)
            {
                newDocumentError = "Error: " + e.ErrorMessage;
                return;
            }

            newDocumentError = null;
            item.Parent.HasBeenModified();
        }

        private void TruncateCollection()
        {
            item.Collection.Truncate();
            item.Parent.HasBeenModified();
        }

        private void DeleteCollection()
        {
            item.Parent.Database.DeleteCollection(item.CollectionName);
            item.Parent.HasBeenModified();
            Selection.SetActiveObjectWithContext(null, null);
        }
    }
}