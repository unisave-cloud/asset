using LightJson;
using Unisave.Editor.DataWindow.SelectionWrappers;
using Unisave.Editor.DataWindow.TreeItems;
using UnityEditor;
using UnityEngine;

namespace Unisave.Editor.DataWindow.ItemEditors
{
    [CustomEditor(typeof(DocumentSelectionWrapper))]
    public class DocumentItemEditor : UnityEditor.Editor
    {
        /// <summary>
        /// The tree view item being displayed
        /// </summary>
        private DocumentItem item;
        
        private JsonEditor.JsonEditor jsonEditor;

        private void OnEnable()
        {
            item = (target as DocumentSelectionWrapper)?.TreeItem;
            
            if (item == null)
                return;
            
            jsonEditor = new JsonEditor.JsonEditor();
            jsonEditor.SetValue(item.DocumentData);
            jsonEditor.OnChange += OnDocumentChange;
        }
        
        public override void OnInspectorGUI()
        {
            if (item == null)
                return;

            UnisaveEditorHelper.InspectorHeading("Document", item.icon);

            UnisaveEditorHelper.LabeledBox("Properties", () => {
                UnisaveEditorHelper.ReadOnlyField("_id", item.DocumentId);
                UnisaveEditorHelper.ReadOnlyField("_rev", item.DocumentRev);
                UnisaveEditorHelper.ReadOnlyField("_key", item.DocumentKey);
            });
            
            jsonEditor.OnGUI();
            
            UnisaveEditorHelper.LabeledBox("Document actions", () => {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Delete"))
                    DeleteDocument();
                EditorGUILayout.EndHorizontal();
            });
        }

        public void DeleteDocument()
        {
            item.Parent.Collection.RemoveDocument(
                item.DocumentKey,
                item.DocumentRev,
                new JsonObject()
                    .Add("ignoreRevs", true)
            );
            item.Parent.Parent.HasBeenModified();
            Selection.SetActiveObjectWithContext(null, null);
        }

        public void OnDocumentChange()
        {
            JsonObject newDocument = item.Parent.Collection.ReplaceDocument(
                item.DocumentKey,
                jsonEditor.GetValue(),
                new JsonObject()
                    .Add("ignoreRevs", true)
            );
            
            // update displayed properties, such as _rev
            item.SetDocument(newDocument);
            
            // this does mean that the tree view item reference is lost,
            // but the target document key is still valid, so that's not
            // a problem. Same with the collection and database reference.
            item.Parent.Parent.HasBeenModified();
        }
    }
}