using System.Collections.Generic;
using Smod2.API;

namespace MTFplus
{
	public struct Subclass
	{
		public readonly string name;
		public readonly Role role;
		public readonly List<ItemType> inventory;
		public readonly float probability;
		public readonly int[] ammo;
		public readonly string broadcast;

		public Subclass(string name, Role role, List<ItemType> inventory, float probability, int[] ammo, string broadcast)
		{
			this.name = name;
			this.role = role;
			this.inventory = inventory;
			this.probability = probability;
			this.ammo = ammo;
			this.broadcast = broadcast;
		}
	}
}
