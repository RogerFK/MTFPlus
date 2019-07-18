using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;
using DMP;

namespace MTFplus
{
	internal class Events : IEventHandlerTeamRespawn, IEventHandlerWaitingForPlayers
	{
		private readonly MTFplus plugin;
		public static bool IMbool { get; set; }
		public Events(MTFplus plugin)
		{
			this.plugin = plugin;
		}

		public void OnTeamRespawn(TeamRespawnEvent ev)
		{
			if (!plugin.enable) return;
			
			//if (ev.SpawnChaos || ev.PlayerList.Count == 0) return;
			MTFplus.subclasses.OrderBy(x => MTFplus.random.Next());
			MEC.Timing.RunCoroutine(RespawnPlus(ev.PlayerList.Select(ply => ply.PlayerId)), MEC.Segment.FixedUpdate);
		}

		private IEnumerator<float> RespawnPlus(IEnumerable<int> PlayerIds)
		{
			yield return MEC.Timing.WaitForSeconds(0.3f);
			Stack<Player> cadets = new Stack<Player>(PluginManager.Manager.Server.GetPlayers(Role.CHAOS_INSURGENCY).Where(ply => PlayerIds.Contains(ply.PlayerId)));
			foreach (Subclass subclass in MTFplus.subclasses)
			{
				if (cadets.Count <= 0)
				{
					plugin.Debug("Subclass " + subclass.name + " didn't spawn this wave (not enough players)!");
					continue;
				}
				if (subclass.probability * 100 >= MTFplus.random.Next(0, 10000))
				{
					Player luckyBoi = cadets.Pop();
					plugin.Debug("Spawning " + luckyBoi.Name + " as " + subclass.name);
					MEC.Timing.RunCoroutine(plugin.SetClass(luckyBoi, subclass), 1);
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
			if (PluginManager.Manager.EnabledPlugins.Where(plugin => plugin.Details.id == "4aiur.custom.itemmanager").Count() > 0) IMbool = true;

			if (!plugin.enable) return;
			MTFplus.subclasses.Clear();
			string directory = FileManager.GetAppFolder() + @"MTFplus";
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
				File.WriteAllText(directory + @"/medic.txt",
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
				plugin.Info("Fetching " + name + "...");
				string[] lines = FileManager.ReadAllLines(filename).Where(x => !string.IsNullOrWhiteSpace(x)).Where(x => x[0] != '#').ToArray();

				// Default values
				Role role = Role.NTF_CADET;
				int maxCount = 1;
				float probability = 100f;

				List<ItemType> inventory = new List<ItemType>();
				int[] IMinventory = new int[16];
				for (int i = 0; i < 16; i++) IMinventory[i] = -1;
				int HP = 0;

				int[] ammo = new int[3] { 0, 0, 0 };
				string broadcast = string.Empty;
				foreach(string data in lines)
				{
					if (data.StartsWith("Inventory"))
					{
						string[] invData = data.Remove(0, 10).Split(',');
						for(int i = 0; i < invData.Length; i++)
						{
							string item = invData[i].Trim();
							if (IMbool)
							{
								if (item.StartsWith("IM:"))
								{
									if(int.TryParse(item.Substring(3), out int aux))
									{
										if (ItemManager.Items.Handlers.ContainsKey(aux))
										{
											IMinventory[i] = aux;
											inventory.Add(ItemType.COIN);
										}
										else
										{
											plugin.Error("Custom item (ItemManager) with ID: " + aux + " doesn't exist/isn't installed!");
										}
									}
									else
									{
										plugin.Error("Invalid CustomItem \"" + item + " (" + item.Substring(3) + ")" + "\" in " + filename + "!");
									}
									continue;
								}
							}
							if (!Enum.TryParse(item, out ItemType parsedItem))
							{
								plugin.Error("Invalid item \"" + item + "\" in " + filename + '!');
							}
							else
							{
								inventory.Add(parsedItem);
							}
						}
						if (inventory.Count == 0 && IMinventory.Length == 0)
						{
							plugin.Error("\"" + filename + "\" doesn't have any valid items. Are you sure this is intended?");
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
					else if (data.StartsWith("HP"))
					{
						string HPstr = data.Substring(3).Trim();
						if (!int.TryParse(HPstr, out int HPaux))
						{
							plugin.Error("Invalid HP \"" + HPstr + "\" in " + filename + '!');
						}
						else
						{
							HP = HPaux;
						}
					}
					else
					{
						plugin.Error("Unknown line: " + data + " in file " + filename);
					}
				}
				name = name.Substring(0, name.Length - 4);
				for (int i = 0; i < maxCount; i++) MTFplus.subclasses.Add(new Subclass(name, role, inventory, IMinventory, probability, ammo, broadcast, HP));

				plugin.Info("Success! Loaded " + name + " as a new class");
			}
		}
	}
}