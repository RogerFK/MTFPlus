using System.Collections.Generic;
using Smod2;
using Smod2.API;
using Smod2.Attributes;

namespace MTFplus
{
	[PluginDetails(
		author = "RogerFK",
		name = "MTF Plus",
		description = "A plugin so NTF cadets aren't just shit and not fun to play",
		id = "rogerfk.mtfplus",
		version = "1.0",
		SmodMajor = 3,
		SmodMinor = 4,
		SmodRevision = 0
		)]
	public class MTFplus : Plugin
	{
		public List<Subclass> subclasses = new List<Subclass>();
		public override void OnDisable()
		{
			this.Info("Your CPU will now save 40ns congratulations");
		}
		public override void OnEnable()
		{
			this.Info("Enabled MTF Plus");
		}

		public override void Register()
		{
			this.AddEventHandlers(new Events(this));
		}

		public void SetClass(Player	player, Subclass subclass)
		{
			if(subclass.role != Role.NTF_CADET) player.ChangeRole(subclass.role, false, false, true, true);
			foreach(Smod2.API.Item item in player.GetInventory())
			{
				item.Remove();
			}
			foreach(ItemType item in subclass.inventory)
			{
				player.GiveItem(item);
			}
		}
	}
}
