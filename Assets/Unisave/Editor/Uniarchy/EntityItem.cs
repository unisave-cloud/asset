using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using Unisave.Database;

namespace Unisave.Uniarchy
{
    class EntityItem : TreeViewItem
    {
        private RawEntity entity;

        public EntityItem(RawEntity entity, IdAllocator idAllocator) : base()
        {
            this.entity = entity;
            
            displayName = entity.type + " [" + entity.id + "]";
            id = idAllocator.NextId();
        }

        public RawEntity GetEntity()
        {
            return entity;
        }
    }
}
