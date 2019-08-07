using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave.Uniarchy
{
    class IdAllocator
    {
        private int next = 1;

        public int NextId()
        {
            return next++;
        }
    }
}
