﻿using System.Collections.Generic;
using MEC;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Config;
using UnityEngine;

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
			DamagePercentages.Initialize(this, Smod2.Events.Priority.Low);
		}
		public static System.Random random = new System.Random();

		public IEnumerator<float> SetClass(Player player, Subclass subclass)
		{
			List<int> indexesToRemove = new List<int>();
			if (subclass.role != Role.NTF_CADET) player.ChangeRole(subclass.role, false, false, true, true);
			yield return Timing.WaitForSeconds(0.1f);
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
			if (Events.IMbool)
			{
				for(int i = 0; i < 16; i++)
				{
					if (subclass.imInv[i] < 0) continue;
					if (ItemManager.Items.Handlers.ContainsKey(subclass.imInv[i])) ItemManager.Items.Handlers[subclass.imInv[i]].Create((player.GetGameObject() as GameObject).GetComponent<Inventory>(), i);
					else
					{
						Error("Custom item (ItemManager) with ID: " + subclass.imInv[i] + " doesn't exist/isn't installed!");
						indexesToRemove.Add(i); // That's the index inside the inventory, and before a coin was placed there
					}
				}
			}
			for (int i = 0; i < 3; i++)
			{
				if (subclass.ammo[i] > 0) player.SetAmmo((AmmoType)i, subclass.ammo[i]);
			}
			List<Smod2.API.Item> inv = player.GetInventory();
			foreach (int i in indexesToRemove)
			{
				inv[i].Remove();
			}
			if (!string.IsNullOrWhiteSpace(subclass.broadcast))
			{
				player.PersonalBroadcast(5, subclass.broadcast, false);
			}
		}
	}
}
