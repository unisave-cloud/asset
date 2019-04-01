using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave
{
	/// <summary>
	/// Distributes data from some data repository into target instances
	/// It can collect the data back, but it does so via remembered field references
	/// </summary>
	public class Distributor
	{
		public Distributor(/* IDataRepository repository */)
		{

		}

		/// <summary>
		/// Distributes data into the target instance and remembers references for collection
		/// </summary>
		public void Distribute(object target)
		{

		}

		/// <summary>
		/// Collects data based on references remembered for this instance
		/// </summary>
		public void Collect(object target)
		{

		}

		/// <summary>
		/// Collects data from all remembered instances
		/// </summary>
		public void Collect()
		{

		}
	}
}
