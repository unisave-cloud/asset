using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unisave;

namespace Unisave.Components
{
    [ExecuteInEditMode]
    public class OverrideUnisavePreferences : MonoBehaviour
    {
        /*
            Allows example scenes to be isolated from the user's game.
            There should be one instance of this component in each example scene
            and it should point to the same overriding preferences file.

            Use with care! When using different overriding files in successive scenes,
            you might get into trouble because different databases have different players,
            but overriding preferences file does not refresh the authenticators.

            Just remember that this tool is sharp and can damage you unexpectedly if used badly.
         */

        public UnisavePreferences preferences;

        void Awake()
        {
            UnisaveServer.AddOverridingPreferences(preferences);
        }

        // used if awake is not called
        // e.g. after compilation
        // UnisaveServer makes sure no duplicate overriding takes place
        void OnEnable()
        {
            UnisaveServer.AddOverridingPreferences(preferences);
        }

        void OnDisable()
        {
            UnisaveServer.RemoveOverridingPreferences(preferences);
        }

        void OnDestroy()
        {
            UnisaveServer.RemoveOverridingPreferences(preferences);
        }
    }
}