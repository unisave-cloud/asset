using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unisave.Auth;
using Unisave.Facets;
using Unisave.Database;
using Unisave.Runtime;
using RSG;

namespace Unisave
{
    /// <summary>
    /// Represents the unisave servers for the client.
    /// 
    /// Here end up all the facade calls.
    /// Here server emulation begins.
    /// </summary>
    public class UnisaveServer
    {
        /*
            This is the main class through which all user requests towards unisave go.

            Emulation
            =========

            Server emulation is, when all requests are diverted, they don't go to
            unisave servers, but to a local, emulated server.

            Emulation takes place in two scenarios:
            1) Developer starts a scene that already expects a logged-in player (eg. GarageScene)
                Unisave detects this by receiving a facet call request, while not being logged in.
            2) Developer explicitly states in UnisavePreferences that he wants to emulate.
                For example when lacking internet connection, but wanting to develop.

            Emulation replaces all aspects of the server:
            - player login / registration
            - facet calling
            - entity persistence

            When the scenario 1) occurs, we don't know what player to log into. Developer has to
            either explicitly state it in the UnisavePreferences, or Unisave will log into
            the so called EmulatedPlayer. That's a player that always exists in the emulated
            database.

            Emulation can never take place in a built game. It only happens, when running
            inside the unity editor.
         */

#region "Default instance construction"

        /// <summary>
        /// The default instance used by all the facades
        /// </summary>
        public static UnisaveServer DefaultInstance
        {
            get
            {
                if (defaultInstance == null)
                    defaultInstance = CreateDefaultInstance();

                return defaultInstance;
            }
        }

        private static UnisaveServer defaultInstance = null;

        private static UnisaveServer CreateDefaultInstance()
        {
            // register promise exception handler
            Promise.UnhandledException += (object sender, ExceptionEventArgs e) => {
                UnityEngine.Debug.LogError($"Unhandled promise exception: {e.Exception.ToString()}");
                UnityEngine.Debug.LogError($"For more info on unhandled promise exceptions see:\n"
                    + "https://github.com/Real-Serious-Games/C-Sharp-Promise#unhandled-errors");
            };

            // register fake database to the framework endpoint
            Endpoints.Database = new FakeDatabase();

            // load preferences as defined by the user
            var preferences = UnisavePreferences.LoadPreferences();

            // override the preferences
            // (e.g. in an example scene)
            if (OverridePreferences != null)
                preferences = OverridePreferences(preferences);

            // create new instance with these preferences
            return CreateFromPreferences(preferences);
        }

        /// <summary>
        /// Intercepts default instance creation and swaps out the preferences
        /// </summary>
        public static Func<UnisavePreferences, UnisavePreferences> OverridePreferences = null;

#endregion

#region "General instance construction"

        /// <summary>
        /// Creates the instance from UnisavePreferences
        /// </summary>
        public static UnisaveServer CreateFromPreferences(UnisavePreferences preferences)
        {
            string editorKey = null;
			#if UNITY_EDITOR
				editorKey = UnityEditor.EditorPrefs.GetString("unisave.editorKey", null);
			#endif

            return new UnisaveServer(
                CoroutineRunnerComponent.GetInstance(),
                preferences.serverApiUrl,
                preferences.gameToken,
                editorKey
            );
        }

        public UnisaveServer(
            CoroutineRunnerComponent coroutineRunner,
            string apiUrl,
            string gameToken,
            string editorKey
        )
        {
            this.apiUrl = apiUrl;
            this.coroutineRunner = coroutineRunner;

            Authenticator = new UnisaveAuthenticator(
                coroutineRunner,
                new ServerAuthApi(apiUrl, gameToken, editorKey)
            );
        }

#endregion

#region "Parameters passed in the constructor"

        /// <summary>
        /// Url of the unisave server's API entrypoint
        /// </summary>
        private readonly string apiUrl;

        /// <summary>
        /// Something, that can run coroutines
        /// </summary>
        private readonly CoroutineRunnerComponent coroutineRunner;

#endregion

        /// <summary>
        /// Is the server being emulated
        /// </summary>
        public bool IsEmulating { get; private set; } = false;

        /// <summary>
        /// Authenticator used to authenticate players
        /// </summary>
        public IAuthenticator Authenticator { get; private set; }

        /// <summary>
        /// Emulated database that stores all the emulated entities
        /// </summary>
        public EmulatedDatabase Database { get; private set; }

        /// <summary>
        /// Handles facet calling once the player is authenticated
        /// </summary>
        public FacetCaller FacetCaller {
            get
            {
                if (facetCaler == null)
                    facetCaler = CreateFacetCaller();
                
                return facetCaler;
            }
        }
        private FacetCaller facetCaler;

        /// <summary>
        /// Called, when a facet caller instance is requested for the first time
        /// </summary>
        private FacetCaller CreateFacetCaller()
        {
            /*
                Here is the place Unisave starts the emulation, if no player is logged in.
             */

            if (!Authenticator.LoggedIn)
            {
                if (!Application.isEditor)
                    throw new Exception("Cannot call facet methods without a logged-in player.");

                Debug.LogWarning("Unisave: Starting server emulation.");
                StartEmulation();
                return facetCaler; // has been created during emulation start
            }

            if (IsEmulating)
                throw new UnisaveException("Emulated facet called instance should already be created.");

            return new UnisaveFacetCaller(Authenticator.AccessToken, apiUrl, coroutineRunner);
        }

        /// <summary>
        /// Starts server emulation
        /// </summary>
        private void StartEmulation()
        {
            IsEmulating = true;

            // initialize emulated database
            Database = new EmulatedDatabase();
            Database.LoadDatabase();
            Endpoints.Database = Database;

            // swap out authenticators
            var emulatedAuth = new EmulatedAuthenticator();
            Authenticator = emulatedAuth;

            // login emulated player
            emulatedAuth.LoginEmulatedPlayer();

            // swap out facet callers
            if (facetCaler == null || facetCaler.GetType() != typeof(EmulatedFacetCaller))
                facetCaler = new EmulatedFacetCaller(Authenticator.Player, Database);
        }
    }
}
