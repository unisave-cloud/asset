using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unisave.Authentication;
using Unisave.Facets;
using Unisave.Database;
using Unisave.Runtime;
using Unisave.Utils;
using Unisave.Exceptions;
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

            When the scenario 1) occurs, we don't know what player to log into. Developer
            has to explicitly state the desired player email address in Unisave preferences
            under the field "Auto-login email". Player with this email has to exist inside the
            emulated database otherwise the login cannot be performed. Create this player
            by using standard registration with explicit emulation enabled.

            Emulation can never take place in a built game. It only happens, when running
            inside the unity editor.
         */

        /// <summary>
        /// Version of this unisave asset
        /// </summary>
        public const string AssetVersion = "0.6.0";

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
                UnityEngine.Debug.LogException(e.Exception);

                // UnityEngine.Debug.LogError($"Unhandled promise exception: {e.Exception.ToString()}");
                // UnityEngine.Debug.LogError($"For more info on unhandled promise exceptions see:\n"
                //     + "https://github.com/Real-Serious-Games/C-Sharp-Promise#unhandled-errors");
            };

            // create new instance with proper preferences
            UnisaveServer instance = CreateFromPreferences(
                GetDefaultPreferencesWithOverriding()
            );

            // register framework endpoints
            Endpoints.DatabaseResolver = () => instance.Database;

            return instance;
        }

        /// <summary>
        /// List of overriding preferences.
        /// Only the topmost preference is used (last in the list)
        /// </summary>
        private static List<UnisavePreferences> overridingPreferences = new List<UnisavePreferences>();

        /// <summary>
        /// Adds preferences to be used for overriding default preferences for the default instance
        /// </summary>
        public static void AddOverridingPreferences(UnisavePreferences preferences)
        {
            if (preferences == null)
                throw new ArgumentNullException();

            if (overridingPreferences.Contains(preferences))
                return;

            overridingPreferences.Add(preferences);
            
            if (defaultInstance != null)
                defaultInstance.ReloadPreferences(GetDefaultPreferencesWithOverriding());
        }

        /// <summary>
        /// Removes preferences used for overriding
        /// </summary>
        public static void RemoveOverridingPreferences(UnisavePreferences preferences)
        {
            if (preferences == null)
                throw new ArgumentNullException();

            overridingPreferences.Remove(preferences);
            
            if (defaultInstance != null)
                defaultInstance.ReloadPreferences(GetDefaultPreferencesWithOverriding());
        }

        /// <summary>
        /// Applies overriding preferences to the default instance
        /// </summary>
        private static UnisavePreferences GetDefaultPreferencesWithOverriding()
        {
            if (overridingPreferences.Count == 0)
                return UnisavePreferences.LoadOrCreate();

            return overridingPreferences[overridingPreferences.Count - 1];
        }

        /// <summary>
        /// Creates the instance from UnisavePreferences
        /// </summary>
        public static UnisaveServer CreateFromPreferences(UnisavePreferences preferences)
        {
            var server = new UnisaveServer(
                CoroutineRunnerComponent.GetInstance(),
                preferences.ServerUrl,
                preferences.GameToken,
                preferences.EditorKey,
                preferences.EmulatedDatabaseName,
                preferences.AutoLoginPlayerEmail
            );

            if (preferences.AlwaysEmulate)
                server.IsEmulating = true;

            return server;
        }

        public UnisaveServer(
            CoroutineRunnerComponent coroutineRunner,
            string apiUrl,
            string gameToken,
            string editorKey,
            string emulatedDatabaseName,
            string autoLoginPlayerEmail
        )
        {
            this.ApiUrl = new ApiUrl(apiUrl);
            this.GameToken = gameToken;
            this.EditorKey = editorKey;
            this.emulatedDatabaseName = emulatedDatabaseName;
            this.autoLoginPlayerEmail = autoLoginPlayerEmail;

            this.coroutineRunner = coroutineRunner;

            IsEmulating = false;
        }

        /// <summary>
        /// Call this method when preferences have been changed to apply the changes
        /// </summary>
        public void ReloadPreferences(UnisavePreferences preferences)
        {
            // emulated database name
            emulatedDatabaseName = preferences.EmulatedDatabaseName;
            emulatedDatabase = null; // make it reload once needed

            // always emulate
            if (preferences.AlwaysEmulate)
                IsEmulating = true;

            // email for auto login
            autoLoginPlayerEmail = preferences.AutoLoginPlayerEmail;

            // TODO: apply remaining preferences
        }

        /// <summary>
        /// Url of the unisave server's API entrypoint ending with a slash
        /// </summary>
        public ApiUrl ApiUrl { get; private set; }

        /// <summary>
        /// Something, that can run coroutines
        /// </summary>
        private readonly CoroutineRunnerComponent coroutineRunner;

        /// <summary>
        /// Token that identifies this game to unisave servers
        /// </summary>
        public string GameToken { get; private set; }

        /// <summary>
        /// Token that authenticates this editor to unisave servers
        /// </summary>
        public string EditorKey { get; private set; }

        /// <summary>
        /// Is the server being emulated
        /// </summary>
        public bool IsEmulating
        {
            get => isEmulating;

            set
            {
                if (value == isEmulating)
                    return;

                if (value)
                {
                    // emulation cannot be started in runtime
                    // this may happen if the developer forgets the "Always emulate" option enabled during build
                    if (!Application.isEditor)
                        return;

                    Debug.LogWarning("Unisave: Starting server emulation.");
                    isEmulating = true;
                }
                else
                {
                    if (IsTesting)
                    {
                        Debug.LogWarning(
                            "Unisave: Stopping testing because emulation was disabled.\n" +
                            "Tests can only be run during emulation."
                        );
                        TearDownTesting();
                    }

                    isEmulating = false;
                }
            }
        }
        private bool isEmulating = false;

        /// <summary>
        /// Is the server being tested
        /// 
        /// Tests automatically run during emulation. So this value only
        /// determines what database to use during emulation.
        /// </summary>
        public bool IsTesting { get; private set; } = false;

        /// <summary>
        /// Emulated database that stores all the emulated entities
        /// </summary>
        public EmulatedDatabase EmulatedDatabase
        {
            get
            {
                if (emulatedDatabase == null)
                {
                    emulatedDatabase = EmulatedDatabaseRepository
                        .GetInstance()
                        .GetDatabase(emulatedDatabaseName);
                    
                    emulatedDatabase.PreventAccess = true;
                }

                return emulatedDatabase;
            }
        }
        private EmulatedDatabase emulatedDatabase;

        /// <summary>
        /// Name of the emulated database that is used
        /// </summary>
        private string emulatedDatabaseName;

        /// <summary>
        /// Empty, if no tests ran so far. Not deleted once test finishes, but left
        /// alone to be checked in the uniarchy if needed.
        /// </summary>
        public EmulatedDatabase TestingDatabase
        {
            get
            {
                if (testingDatabase == null)
                {
                    testingDatabase = EmulatedDatabaseRepository
                        .GetInstance()
                        .GetDatabase("testing");
                }

                return testingDatabase;
            }
        }
        private EmulatedDatabase testingDatabase;

        /// <summary>
        /// Resolves the database endpoint for the unisave framework
        /// </summary>
        public IDatabase Database
        {
            get
            {
                if (IsEmulating)
                {
                    if (IsTesting)
                        return TestingDatabase;
                    else
                        return EmulatedDatabase;
                }
                else
                {
                    return new FakeDatabase();
                }
            }
        }

        /// <summary>
        /// Email of the player that should be automatically logged in
        /// </summary>
        private string autoLoginPlayerEmail;

        /// <summary>
        /// Authenticator that authenticates via the unisave servers
        /// </summary>
        public UnisaveAuthenticator UnisaveAuthenticator
        {
            get
            {
                if (unisaveAuthenticator == null)
                {
                    unisaveAuthenticator = new UnisaveAuthenticator(
                        ApiUrl,
                        GameToken,
                        EditorKey
                    );
                }

                return unisaveAuthenticator;
            }
        }
        private UnisaveAuthenticator unisaveAuthenticator;

        /// <summary>
        /// Authenticator that runs against the emulated or testing database
        /// </summary>
        public EmulatedAuthenticator EmulatedAuthenticator
        {
            get
            {
                if (emulatedAuthenticator == null)
                {
                    emulatedAuthenticator = new EmulatedAuthenticator(
                        () => IsTesting ? TestingDatabase : EmulatedDatabase
                    );
                }

                return emulatedAuthenticator;
            }
        }
        private EmulatedAuthenticator emulatedAuthenticator;

        /// <summary>
        /// Authenticator used to authenticate players
        /// </summary>
        public IAuthenticator Authenticator
        {
            get
            {
                if (IsEmulating)
                    return EmulatedAuthenticator;
                else
                    return UnisaveAuthenticator;
            }
        }

        /// <summary>
        /// Facet caller that performs the calls against unisave servers
        /// </summary>
        public UnisaveFacetCaller UnisaveFacetCaller
        {
            get
            {
                if (unisaveFacetCaller == null)
                {
                    unisaveFacetCaller = new UnisaveFacetCaller(
                        () => UnisaveAuthenticator.AccessToken,
                        ApiUrl,
                        coroutineRunner
                    );
                }

                return unisaveFacetCaller;
            }
        }
        private UnisaveFacetCaller unisaveFacetCaller;

        /// <summary>
        /// Facet caller that emulates the calls locally against the emulated database
        /// </summary>
        public EmulatedFacetCaller EmulatedFacetCaller
        {
            get
            {
                if (emulatedFacetCaller == null)
                {
                    emulatedFacetCaller = new EmulatedFacetCaller(
                        () => EmulatedAuthenticator.Player
                    );
                }

                return emulatedFacetCaller;
            }
        }
        private EmulatedFacetCaller emulatedFacetCaller;

        /// <summary>
        /// Handles facet calling once the player is authenticated
        /// If no player authenticated, emulated player gets logged in
        /// </summary>
        public FacetCaller FacetCaller
        {
            get
            {
                if (IsEmulating)
                {
                    if (!EmulatedAuthenticator.LoggedIn)
                    {
                        EmulatedAuthenticator.AutoLogin(autoLoginPlayerEmail);
                    }

                    return EmulatedFacetCaller;
                }
                else
                {
                    if (!UnisaveAuthenticator.LoggedIn)
                    {
                        if (!Application.isEditor)
                            throw new Exception("Cannot call facet methods without a logged-in player.");

                        IsEmulating = true;

                        EmulatedAuthenticator.AutoLogin(autoLoginPlayerEmail);

                        return EmulatedFacetCaller;
                    }

                    return UnisaveFacetCaller;
                }
            }
        }

        /// <summary>
        /// Setup environment for running tests
        /// </summary>
        public void SetUpTesting()
        {
            IsEmulating = true;
            IsTesting = true;
            
            TestingDatabase.Clear();
        }

        /// <summary>
        /// Testing has finished, tear the testing environment down
        /// </summary>
        public void TearDownTesting()
        {
            IsTesting = false;

            EmulatedAuthenticator.Logout();
        }
    }
}
