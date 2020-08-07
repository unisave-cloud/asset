using System;
using Unisave.Facades;
using UnisaveFixture.Backend.ExampleTesting;
using UnityEngine;

namespace UnisaveFixture.ExampleTesting
{
    public class ExampleTestingClient : MonoBehaviour
    {
        public async void CallAddingFacet()
        {
            int result = await OnFacet<ExampleTestingFacet>.CallAsync<int>(
                nameof(ExampleTestingFacet.AddNumbers),
                4, 7
            );
            
            if (result != 11)
                throw new Exception("The result was not 11!");
        }
    }
}
