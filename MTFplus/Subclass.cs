using Smod2.API;

namespace MTFplus
{
	public struct Subclass
	{
		public readonly string name;
		public readonly Role role;
		public readonly ItemType[] inventory;
		public readonly float probability;

		public Subclass(string name, Role role, ItemType[] inventory, float probability)
		{
			this.name = name;
			this.role = role;
			this.inventory = inventory;
			this.probability = probability;
		}
	}
}
