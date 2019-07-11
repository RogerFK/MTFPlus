using System.Collections.Generic;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Config;

namespace MTFplus
{
	[PluginDetails(
		author = "RogerFK",
		name = "MTF Plus",
		description = "A plugin so NTF cadets aren't just shit and not fun to play",
		id = "rogerfk.mtfplus",
		version = "1.0",
		SmodMajor = 3,
		SmodMinor = 5,
		SmodRevision = 0,
		configPrefix = "mtfp"
		)]
	public class MTFplus : Plugin
	{
		public static List<Subclass> subclasses = new List<Subclass>();

		[ConfigOption]
		public bool enable = true;

		[ConfigOption]
		public bool useGlobal = false;

		public override void OnDisable()
		{
			this.Info("Disabled MTF Plus");
		}
		public override void OnEnable()
		{
			this.Info("Enabled MTF Plus");
		}

		public override void Register()
		{
			this.AddEventHandlers(new Events(this), Smod2.Events.Priority.Low);
		}
		public static System.Random random = new System.Random();

		public IEnumerator<float> SetClass(Player player, Subclass subclass)
		{
			yield return 0.1f;
			if(subclass.role != Role.NTF_CADET) player.ChangeRole(subclass.role, false, false, true, true);
			yield return 0.1f;
			if (subclass.inventory.Count > 0)
			{
				foreach (Smod2.API.Item item in player.GetInventory())
				{
					item.Remove();
				}
				foreach (ItemType item in subclass.inventory)
				{
					player.GiveItem(item);
				} 
			}
			for(int i = 0; i<3; i++)
			{
				if (subclass.ammo[i] > 0) player.SetAmmo((AmmoType)i, subclass.ammo[i]);
			}
		}
	}
}
