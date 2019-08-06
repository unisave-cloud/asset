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
        /// Instance of the emulated player.
        /// This is a player that always exists inside the emulated database
        /// and it's the player you log into when emulation begins.
        /// </summary>
        public static UnisavePlayer EmulatedPlayer => EmulatedDatabase.EmulatedPlayer;

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
    }
}
