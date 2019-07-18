using System;
using Unisave;

namespace Unisave.Examples.Cloud.Motorbike
{
    /// <summary>
    /// Controller contains server-executed code that performs operations on entities
    /// </summary>
    public class GarageController : Facet
	{
		public void ChangeMotorbikeName(string newName)
		{
			// var pd = GetEntity<PlayerData>.OfPlayer(Caller).Get();
			// pd.MotorbikeName = newName;
			// pd.Save();
		}
	}
}
