using System;
using UnityEngine;
using Unisave;

namespace Unisave.Examples.Cloud
{
    /// <summary>
    /// Overrides player-defined unisave preferences to make sure the examples
    /// do not interfere with user's game
    /// 
    /// Just put this script into each scene of an example
    /// </summary>
    public class IsolateExample : MonoBehaviour
    {
        [Header("Makes the example code run in isolation from your game")]
        [Header("- no network communication, run locally")]
        [Header("- no code uploading")]
        [Space]
        public int readme;

        private static bool overriden = false;

        public static void Override()
        {
            if (overriden)
                return;

            overriden = true;

            var preferences = ScriptableObject.CreateInstance<UnisavePreferences>();

            preferences.runAgainstLocalDatabase = true;

            UnisaveCloud.CreateBackendFromPreferences(preferences);
        }

        void Awake()
        {
            Override();
        }
    }
}
