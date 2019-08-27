using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unisave.Database;

namespace Unisave
{
    public static class Testing
    {
        /// <summary>
        /// Setup unsiave testing
        /// </summary>
        public static void SetUp()
        {
            UnisaveServer.DefaultInstance.SetUpTesting();
        }

        /// <summary>
        /// Tear down unisave testing
        /// </summary>
        public static void TearDown()
        {
            UnisaveServer.DefaultInstance.TearDownTesting();
        }

        /// <summary>
        /// Performs some action logged in as a certain player
        /// </summary>
        public static void AsPlayer(UnisavePlayer player, Action action)
        {
            if (!UnisaveServer.DefaultInstance.IsTesting)
                throw new InvalidOperationException(
                    $"Cannot use {nameof(AsPlayer)} when not testing. " +
                    $"Make sure you call {nameof(Testing.SetUp)} and {nameof(Testing.TearDown)} properly."
                );

            UnisaveServer.DefaultInstance.EmulatedAuthenticator.AsPlayer(player, action);
        }

        /// <summary>
        /// Registers a new player for testing
        /// </summary>
        public static UnisavePlayer RegisterPlayer(string email, Dictionary<string, object> hookArguments)
        {
            return UnisaveServer.DefaultInstance.EmulatedAuthenticator.RegisterPlayer(
                email, "password", hookArguments
            );
        }

        /// <summary>
        /// Log a player in
        /// </summary>
        public static void LoginPlayer(UnisavePlayer player)
        {
            UnisaveServer.DefaultInstance.EmulatedAuthenticator.LoginPlayer(player);
        }
    }
}
