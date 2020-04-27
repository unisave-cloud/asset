using Unisave.Sessions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Unisave.Editor.DataWindow.TreeItems
{
    public sealed class SessionIdItem : TreeViewItem
    {
        public SessionIdRepository Repository { get; }
        private readonly IdAllocator idAllocator;
        
        public SessionIdItem(
            SessionIdRepository repository,
            IdAllocator idAllocator
        )
        {
            this.Repository = repository;
            this.idAllocator = idAllocator;

            string sessionId = Repository.GetSessionId() ?? "null";
            
            id = idAllocator.NextId();
            displayName = "Session ID [" + sessionId + "]";
            icon = (Texture2D)EditorGUIUtility.IconContent(
                "CloudConnect"
            ).image;
        }
    }
}