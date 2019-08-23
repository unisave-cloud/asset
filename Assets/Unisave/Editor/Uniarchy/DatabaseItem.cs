using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using Unisave.Database;

namespace Unisave.Uniarchy
{
    class DatabaseItem : TreeViewItem
    {
        public EmulatedDatabase Database { get; private set; }

        public DatabaseItem(EmulatedDatabase database, IdAllocator idAllocator) : base()
        {
            Database = database;

            this.displayName = database.Name;
            id = idAllocator.NextId();

            var gameEntities = new TreeViewItem { id = idAllocator.NextId(), displayName = "Game entities" };
            foreach (RawEntity entity in Database.EnumerateGameEntities())
            {
                gameEntities.AddChild(
                    new EntityItem(entity, idAllocator)
                );
            }
            AddChild(gameEntities);
            
            var players = new TreeViewItem { id = idAllocator.NextId(), displayName = "Players" };
            BuildPlayers(players, idAllocator);
            AddChild(players);
            
            var sharedEntities = new TreeViewItem { id = idAllocator.NextId(), displayName = "Shared entities" };
            foreach (RawEntity entity in Database.EnumerateSharedEntities())
            {
                sharedEntities.AddChild(
                    new EntityItem(entity, idAllocator)
                );
            }
            AddChild(sharedEntities);
        }

        private void BuildPlayers(TreeViewItem root, IdAllocator idAllocator)
        {
            foreach (EmulatedDatabase.PlayerRecord player in Database.EnumeratePlayers())
            {
                var playerItem = new TreeViewItem {
                    id = idAllocator.NextId(),
                    displayName = player.email + " [" + player.id + "]"
                };
                
                foreach (RawEntity entity in Database.EnumeratePlayerEntities(player.id))
                {
                    playerItem.AddChild(
                        new EntityItem(entity, idAllocator)
                    );
                }
                
                root.AddChild(playerItem);
            }
        }
    }
}
