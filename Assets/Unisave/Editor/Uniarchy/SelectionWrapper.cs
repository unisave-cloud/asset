using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unisave.Uniarchy
{
    /// <summary>
    /// Wraps an object inside a scriptable object
    /// so that it can be assigned to Unity editor selection
    /// </summary>
    public class SelectionWrapper : ScriptableObject
    {
        public object content;

        public SelectionWrapper Set(object content)
        {
            this.content = content;
            return this;
        }

        /// <summary>
        /// Select any kind of object
        /// </summary>
        public static void SelectObject(object obj)
        {
            Selection.SetActiveObjectWithContext(
                ScriptableObject.CreateInstance<SelectionWrapper>().Set(obj),
                null
            );
        }
    }
}
