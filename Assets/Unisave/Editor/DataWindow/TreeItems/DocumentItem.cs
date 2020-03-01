using System;
using LightJson;
using LightJson.Serialization;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Unisave.Editor.DataWindow.TreeItems
{
    public sealed class DocumentItem : TreeViewItem
    {
        /// <summary>
        /// Document being represented by this node (with pseudo fields)
        /// </summary>
        public JsonObject Document { get; private set; }
        
        /// <summary>
        /// Document being represented by this node (without pseudo fields)
        /// </summary>
        public JsonObject DocumentData { get; private set; }
        
        public string DocumentId { get; private set; }
        public string DocumentKey { get; private set; }
        public string DocumentRev { get; private set; }
        
        /// <summary>
        /// Parent tree view item
        /// </summary>
        public CollectionItem Parent => parent as CollectionItem
            ?? throw new Exception("Parent item has unexpected type.");

        public DocumentItem(JsonObject document, IdAllocator idAllocator)
        {
            id = idAllocator.NextId();
            icon = (Texture2D)EditorGUIUtility.IconContent(
                "GameObject Icon"
            ).image;
            
            SetDocument(document);
        }

        /// <summary>
        /// Set value of the entire document with all special fields
        /// </summary>
        public void SetDocument(JsonObject document)
        {
            Document = JsonReader.Parse(document.ToString());
            DocumentId = document["_id"].AsString;
            DocumentKey = document["_key"].AsString;
            DocumentRev = document["_rev"].AsString;
            
            DocumentData = JsonReader.Parse(document.ToString());
            DocumentData.Remove("_key");
            DocumentData.Remove("_id");
            DocumentData.Remove("_rev");
            
            displayName = new JsonValue(DocumentKey).ToString(false) +
                          ": " + DocumentData;
        }
    }
}
