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
    }
}
