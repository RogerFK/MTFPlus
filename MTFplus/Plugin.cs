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
		public static List<Subclass> subclasses = new List<Subclass>();
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
		public static System.Random random = new System.Random();
		public IEnumerator<float> SetClass(Player player, Role role, ItemType[] inventory)
		{
			if(role != Role.NTF_CADET) player.ChangeRole(role, false, false, true, true);
			yield return 0.1f;
			foreach (Smod2.API.Item item in player.GetInventory())
			{
				item.Remove();
			}
			foreach (ItemType item in inventory)
			{
				player.GiveItem(item);
			}
		}
	}
	public static class ExtMethod
	{
		// found at https://stackoverflow.com/questions/273313/randomize-a-listt
		public static List<T> Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = MTFplus.random.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
			return (List<T>) list;
		}
	}
}
