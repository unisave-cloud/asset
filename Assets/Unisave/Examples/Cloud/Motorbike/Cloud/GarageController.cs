using System;
using Unisave.Framework;

namespace Unisave.Examples.Cloud.Motorbike
{
    /// <summary>
    /// Controller contains server-executed code that performs operations on entities
    /// </summary>
    public class GarageController : Controller
	{
		[Action]
		public void ChangeMotorbikeName(string newName)
		{
			var pd = GetEntity.OfPlayer(this.CurrentPlayer).Get<PlayerData>();
			pd.MotorbikeName = newName;
			pd.Save();
		}
	}
}
