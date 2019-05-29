using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MTFplus
{
	class Events : IEventHandlerTeamRespawn, IEventHandlerWaitingForPlayers
	{
		private MTFplus plugin;
		public Events(MTFplus plugin)
		{
			this.plugin = plugin;
		}
		System.Random random = new System.Random();
		Dictionary<string, int> totalRespawned = new Dictionary<string, int>();
		public void OnTeamRespawn(TeamRespawnEvent ev)
		{
			// We use a secondary list for subclasses to save execution time
			List<Subclass> filtered = new List<Subclass>(plugin.subclasses);
			// Done this to save execution time at the expense of using a little bit more of memory
			List<Player> validPlayers = new List<Player>(ev.PlayerList.Where(x => x.TeamRole.Role == Role.NTF_CADET));
			while (filtered.Count > 0)
			{
				foreach(Subclass subclass in filtered.ToArray())
				{
					// Jump out if there's no players left to avoid problems
					if(validPlayers.Count() == 0)
					{
						break;
					}
					// Nested ifs are better than doing everything in the same if,
					// as you don't have to compute a Dictionary entry if you don't need to,
					// saving execution time if the list of subclasses is really high
					// (but doing two "jump if X isn't true (or branch if not equal)", which
					// doesn't consume much CPU time)
					if(subclass.probability * 100 >= random.Next(0, 10000))
					{
						if(totalRespawned[subclass.name] != subclass.maxCount)
						{
							Player luckyBoi = validPlayers.First();
							validPlayers.Remove(luckyBoi);
							plugin.SetClass(luckyBoi, subclass);
							if (totalRespawned.ContainsKey(subclass.name))
							{
								totalRespawned[subclass.name]++;
								if (subclass.maxCount == totalRespawned[subclass.name]) filtered.Remove(subclass);
							}
							else
							{
								totalRespawned.Add(subclass.name, 1);
								if (subclass.maxCount == 1) filtered.Remove(subclass);
							}
						}
					}
				}
			}
			totalRespawned.Clear();
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			foreach(string filename in Directory.GetFiles(Environment.CurrentDirectory + @"\MTFplus", @"*.txt"))
			{
				string[] data = FileManager.ReadAllLines(filename).Where(x => !string.IsNullOrWhiteSpace(x)).Where(x => x[0] != '#').ToArray();

				if(data.Count() != 4)
				{
					plugin.Error("Bad format in " + filename + ". Please, read the plugin's GitHub.");
					continue;
				}
				int i = 0;
				// random values because I don't literally know how to do it otherwise without VS being mad about it
				Role role = Role.SCP_173;
				int maxCount = 0;
				ItemType[] inventory = new ItemType[0];
				float probability = 0f;
				// These things below are me being nostalgic of C
				for (; i < 4; i++)
				{
					if (data[i].StartsWith("Inventory"))
					{
						string[] invData = data[i].Remove(0, 9).Replace(" ", string.Empty).Split(',');
						ItemType[] inv = new ItemType[invData.Count()];
						foreach (string item in invData)
						{
							if (!Enum.TryParse(item, out ItemType parsedItem))
							{
								plugin.Error("Invalid item \"" + item + "\" in " + filename + '!');
							}
							else inv.Append(parsedItem);
						}
						if(inv.Count() == 0)
						{
							plugin.Error('\"' + filename + "\" doesn't have any valid items. Are you sure this is right?");
						}
						else inventory = inv;
					}
					else if (data[i].StartsWith("Role"))
					{
						string roleData = data[i].Remove(0, 4).Trim();
						if(!Enum.TryParse(roleData, out role))
						{
							plugin.Error("Invalid role \"" + roleData + "\" in " + filename + '!');
							i--;
							break;
						}
					}
					else if (data[i].StartsWith("Max"))
					{
						string maxData = data[i].Remove(0, 4).Trim();
						if (!int.TryParse(maxData, out maxCount))
						{
							plugin.Error("Invalid maximum count \"" + maxData + "\" in " + filename + '!');
							i--;
							break;
						}
					}
					else if (data[i].StartsWith("Probability"))
					{
						string prob = data[i].Remove(0, 4).Trim();
						if (!float.TryParse(prob, out probability))
						{
							plugin.Error("Invalid maximum count \"" + prob + "\" in " + filename + '!');
							i--;
							break;
						}
					}
					else
					{
						plugin.Error("Unknown line: " + data[i]);
						i--;
						break;
					}
				}
				if(i == 3)
				{
					plugin.subclasses.Add(new Subclass(filename.Substring(0, filename.Count() - 4), role, inventory, maxCount, probability));
				}
			}
		}
	}
}