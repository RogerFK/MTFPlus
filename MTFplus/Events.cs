using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;

namespace MTFplus
{
	internal class Events : IEventHandlerTeamRespawn, IEventHandlerWaitingForPlayers
	{
		private readonly MTFplus plugin;
		public Events(MTFplus plugin)
		{
			this.plugin = plugin;
		}
		public void OnTeamRespawn(TeamRespawnEvent ev)
		{
			if (ev.SpawnChaos || ev.PlayerList.Count == 0) return;

			MTFplus.subclasses.Shuffle();
			Stack<Player> validPlayers = new Stack<Player>(ev.PlayerList.Where(x => x.TeamRole.Role == Role.NTF_CADET).OrderBy(x => MTFplus.random.Next()));
			foreach (Subclass subclass in MTFplus.subclasses)
			{
				if (validPlayers.Count < 0)
				{
					plugin.Info("Subclass " + subclass.name + " didn't spawn this wave (not enough players)!");
					continue;
				}
				if (subclass.probability * 100 >= MTFplus.random.Next(0, 10000))
				{
					Player luckyBoi = validPlayers.Pop();
					plugin.Info("Spawning " + luckyBoi.Name + " as " + subclass.name);
					MEC.Timing.RunCoroutine(plugin.SetClass(luckyBoi, subclass.role, subclass.inventory));
				}
				else
				{
					plugin.Info("Too bad, but " + subclass.name + " didn't get a chance to spawn!");
				}
			}
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			string directory = FileManager.GetAppFolder() + @"MTFplus";
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
				plugin.Info("Created " + directory + ". Fill it with your own MTF classes!");
				return;
			}
			string[] filenames = Directory.GetFiles(directory, "*.txt");
			foreach (string filename in filenames)
			{
				plugin.Info("Fetching " + filename + "...");
				string[] data = FileManager.ReadAllLines(filename).Where(x => !string.IsNullOrWhiteSpace(x)).Where(x => x[0] != '#').ToArray();

				if (data.Count() != 4)
				{
					plugin.Error("Bad format in " + filename + ". Please, read the plugin's GitHub.");
					continue;
				}
				int i = 0;
				// random values because I don't literally know how to do it otherwise without VS being mad about it
				Role role = Role.NTF_CADET;
				int maxCount = 0;
				List<ItemType> inventory = new List<ItemType>();
				float probability = 0f;
				bool error = false;
				// These things below are me being nostalgic of C. Uncommented so it's harder to read. Don't copy this.
				for (; i < 4; i++)
				{
					if (data[i].StartsWith("Inventory"))
					{
						string[] invData = data[i].Remove(0, 10).Split(',');
						foreach (string item in invData)
						{
							if (!Enum.TryParse(item.Trim(), out ItemType parsedItem))
							{
								plugin.Error("Invalid item \"" + item.Trim() + "\" in " + filename + '!');
							}
							else
							{
								inventory.Add(parsedItem);
							}
						}
						if (inventory.Count() == 0)
						{
							plugin.Error('\"' + filename + "\" doesn't have any valid items. Are you sure this is right?");
							error = true;
							break;
						}
					}
					else if (data[i].StartsWith("Role"))
					{
						string roleData = data[i].Remove(0, 5).Trim();
						if (!Enum.TryParse(roleData, out role))
						{
							plugin.Error("Invalid role \"" + roleData + "\" in " + filename + '!');
							error = true;
							break;
						}
					}
					else if (data[i].StartsWith("Max"))
					{
						string maxData = data[i].Remove(0, 4).Trim();
						if (!int.TryParse(maxData, out maxCount))
						{
							plugin.Error("Invalid maximum count \"" + maxData + "\" in " + filename + '!');
							error = true;
							break;
						}
					}
					else if (data[i].StartsWith("Probability"))
					{
						string prob = data[i].Remove(0, 12).Trim();
						if (!float.TryParse(prob, out probability))
						{
							plugin.Error("Invalid probability \"" + prob + "\" in " + filename + '!');
							error = true;
							break;
						}
					}
					else
					{
						plugin.Error("Unknown line: " + data[i]);
						error = true;
						break;
					}
				}
				if (error) continue;

				string name = filename.Remove(0, directory.Count() + 1);
				name = name.Substring(0, name.Count() - 4);
				for (i = 0; i < maxCount; i++) MTFplus.subclasses.Add(new Subclass(name, role, inventory.ToArray(), probability));
			}
		}
	}
}