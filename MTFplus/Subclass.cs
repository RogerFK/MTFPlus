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

		public Subclass(string name, Role role, List<ItemType> inventory, float probability, int[] ammo)
		{
			this.name = name;
			this.role = role;
			this.inventory = inventory;
			this.probability = probability;
			this.ammo = ammo;
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
			return (List<T>)list;
		}
	}
}
