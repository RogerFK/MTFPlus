using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;

namespace MTFplus
{
	internal class Events : IEventHandlerTeamRespawn, IEventHandlerWaitingForPlayers
	{
		private readonly MTFplus plugin;
		private bool IMbool { get; set; }
		public Events(MTFplus plugin)
		{
			this.plugin = plugin;
		}

		public void OnTeamRespawn(TeamRespawnEvent ev)
		{
			if (!plugin.enable) return;

			if (ev.SpawnChaos || ev.PlayerList.Count == 0) return;

			MTFplus.subclasses.OrderBy(x => MTFplus.random.Next());
			Stack<Player> validPlayers = new Stack<Player>(ev.PlayerList.Where(x => x.TeamRole.Role == Role.NTF_CADET).OrderBy(x => MTFplus.random.Next()));
			foreach (Subclass subclass in MTFplus.subclasses)
			{
				if (validPlayers.Count <= 0)
				{
					plugin.Debug("Subclass " + subclass.name + " didn't spawn this wave (not enough players)!");
					continue;
				}
				if (subclass.probability * 100 >= MTFplus.random.Next(0, 10000))
				{
					Player luckyBoi = validPlayers.Pop();
					plugin.Debug("Spawning " + luckyBoi.Name + " as " + subclass.name);
					MEC.Timing.RunCoroutine(plugin.SetClass(luckyBoi, subclass));
				}
				else
				{
					plugin.Debug("Bad luck for " + subclass.name + ". Skipping to next subclass");
				}
			}
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			// Check if ItemManager is installed and enabled on the server
			IMbool = PluginManager.Manager.EnabledPlugins.Where(plugin => plugin.Details.id == "4aiur.custom.itemmanager").Count() > 1;

			if (!plugin.enable) return;
			MTFplus.subclasses.Clear();
			string directory = FileManager.GetAppFolder() + @"MTFplus";
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
				File.WriteAllText(directory + "\\medic.txt",
					"Inventory: SENIOR_GUARD_KEYCARD, P90, RADIO, DISARMER, MEDKIT, MEDKIT, MEDKIT, MEDKIT\n" +
					"Max: 2\n" +
					"Role: NTF_CADET\n" +
					"Probability: 80\n" +
					"Ammo5: 200\n" +
					"Ammo7: 70\n" +
					"Ammo9: 50");
				plugin.Info("Created " + directory + ". Fill it with your own MTF classes!\nAdditionally, a template class (Medic) was created with it");
				return;
			}
			string[] filenames = Directory.GetFiles(directory, "*.txt");
			foreach (string filename in filenames)
			{
				string name = filename.Remove(0, directory.Length + 1);
				plugin.Debug("Fetching " + name + "...");
				string[] lines = FileManager.ReadAllLines(filename).Where(x => !string.IsNullOrWhiteSpace(x)).Where(x => x[0] != '#').ToArray();

				// Default values
				Role role = Role.NTF_CADET;
				int maxCount = 1;
				float probability = 100f;

				List<ItemType> inventory = new List<ItemType>(); //new List<ItemType>() { ItemType.SENIOR_GUARD_KEYCARD, ItemType.P90, ItemType.RADIO, ItemType.DISARMER, ItemType.MEDKIT, ItemType.WEAPON_MANAGER_TABLET };
				int[] ammo = new int[3] { 0, 0, 0 };
				string broadcast = string.Empty;
				foreach(string data in lines)
				{
					if (data.StartsWith("Inventory"))
					{
						string[] invData = data.Remove(0, 10).Split(',');
						List<ItemType> inventoryTemp = new List<ItemType>();
						foreach (string item in invData)
						{
							if (!Enum.TryParse(item.Trim(), out ItemType parsedItem))
							{
								plugin.Error("Invalid item \"" + item.Trim() + "\" in " + filename + '!');
							}
							else
							{
								inventoryTemp.Add(parsedItem);
							}
						}
						if (inventoryTemp.Count == 0)
						{
							plugin.Error("\"" + filename + "\" doesn't have any valid items. Are you sure this is intended?");
						}
						else
						{
							inventory.Clear(); // I don't trust C#'s garbage collector :smug:
							inventory = inventoryTemp;
						}
					}
					else if (data.StartsWith("Role"))
					{
						string roleData = data.Remove(0, 5).Trim();
						if (!Enum.TryParse(roleData, out Role roleParsed))
						{
							plugin.Error("Invalid role \"" + roleData + "\" in " + filename + '!');
						}
						else
						{
							role = roleParsed;
						}
					}
					else if (data.StartsWith("Max"))
					{
						string maxData = data.Remove(0, 4).Trim();
						if (!int.TryParse(maxData, out int probablyMaxCount))
						{
							plugin.Error("Invalid maximum count \"" + maxData + "\" in " + filename + '!');
						}
						else
						{
							maxCount = probablyMaxCount;
						}
					}
					else if (data.StartsWith("Probability"))
					{
						string prob = data.Remove(0, 12).Trim();
						if (!float.TryParse(prob, out float probabilitey))
						{
							plugin.Error("Invalid probability \"" + prob + "\" in " + filename + '!');
						}
						else
						{
							probability = probabilitey;
						}
					}
					else if (data.StartsWith("Ammo"))
					{
						if(!int.TryParse(data[4].ToString(), out int ammoTyperino))
						{
							plugin.Error("\"Ammo\" \"" + data + "\" unrecognized in " + filename + '!');
						}
						int ammoType = (ammoTyperino - 5) / 2;
						if(ammoType < 0 || ammoType > 2)
						{
							plugin.Error(data[4].ToString() + " is not a type of ammo! (in line: " + data + " in " + filename);
						}
						string ammoStr = data.Remove(0, 6).Trim();
						if (!int.TryParse(ammoStr, out int parsedAmmo))
						{
							plugin.Error("Invalid Ammo \"" + ammoStr + "\" in " + filename + '!');
						}
						else
						{
							ammo[ammoType] = parsedAmmo;
						}
					}
					else if (data.StartsWith("Broadcast"))
					{
						broadcast = data.Remove(0, 10);
					}
					else
					{
						plugin.Error("Unknown line: " + data + " in file " + filename);
					}
				}
				name = name.Substring(0, name.Length - 4);
				for (int i = 0; i < maxCount; i++) MTFplus.subclasses.Add(new Subclass(name, role, inventory, probability, ammo, broadcast));
			}
		}
	}
}