using EXILED;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTFplus
{
	public class MTFPlusCommands
	{
		private readonly MTFplus plugin;

		public MTFPlusCommands(MTFplus plugin)
		{
			this.plugin = plugin;
		}

		public string GetCommandDescription()
		{
			string aliases = string.Join("/", plugin.aliases);

			return $"See usage by typing {aliases}.";
		}

		public string GetUsage()
		{
			string aliases = string.Join("/", plugin.aliases);

			return "Usage:" + Environment.NewLine +
				$"{aliases} LIST [COMPLETE] - Displays the list of Subclasses, with each stat if you add COMPLETE, true or 1 to the end of the command (works with anything really)." + Environment.NewLine +
				$"{aliases} DISPLAY <name> - Displays the class with that name" + Environment.NewLine +
				$"{aliases} SPAWN <player name/player id> <class name> - Spawn a player as a class.";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (args.Length < 1) return new string[] { GetUsage() };
			else switch (args[0].ToUpperInvariant())
				{
					case "RELOAD":
						return new string[] { $"Reloaded {Methods.LoadClasses()} classes." };
					case "LIST":
						int i, count;
						IEnumerable<Subclass> distinctSubclasses = MTFplus.subclasses.Distinct();
						count = distinctSubclasses.Count();
						string[] subclassesList = new string[count];
						if (args.Length > 1)
						{
							// You don't actually have to type a 1, a COMPLETE or a true. Stoopid, u got bamboozled, lol
							for (i = 0; i < count; i++)
							{
								subclassesList[i] = distinctSubclasses.ElementAt(i).ToString() + Environment.NewLine + Environment.NewLine;
							}
						}
						else
						{
							for (i = 0; i < count; i++)
							{
								subclassesList[i] = distinctSubclasses.ElementAt(i).name + (i != count - 1 ? ", " : string.Empty);
							}
						}
						return new string[] { "List of " + (args.Length > 1 ? "subclasses: " : "names: "), string.Join(string.Empty, subclassesList) };
					case "DISPLAY":
						if (args.Length < 2)
						{
							return new string[] { "Please, introduce a name as your second argument." };
						}
						Subclass theOneAndOnly = MTFplus.subclasses.Get(args[1]);
						if (theOneAndOnly.Equals(SubclassMethods.Empty))
						{
							return new string[] { "Subclass not found." };
						}
						return new string[] { theOneAndOnly.ToString() };
					case "SPAWN":
						if (sender is Player p && !plugin.ranks.Contains(p.GetRankName()))
						{
							return new string[] { "Your rank is allowed to use this command" };
						}
						if (args.Length < 3)
						{
							return new string[] { $"Usage: {string.Join("/", plugin.aliases)} SPAWN <player name/player id> <class name>" };
						}
						Player player = null;
						if (int.TryParse(args[1], out int id))
						{
							try { player = PluginManager.Manager.Server.GetPlayer(id); } catch { return new string[] { "Error with ID (" + args[1] + "). Try again or use the name." }; }
						}
						else
						{
							List<Player> matchingPlayers = PluginManager.Manager.Server.GetPlayers(args[1]);
							player = matchingPlayers.OrderBy(x => x.Name.Length).FirstOrDefault();
						}
						if (player == null)
						{
							return new string[] { "Player not found." };
						}
						Subclass pickedClass = MTFplus.subclasses.Get(args[2]);
						if (pickedClass.Equals(SubclassMethods.Empty))
						{
							return new string[] { "Subclass not found." };
						}
						player.ChangeRole(pickedClass.role, false, true, true, true);
						player.SetClass(pickedClass);
						if (sender is Player pl) plugin.Info(pl.Name + " (" + pl.SteamId + ") spawned " + player.Name + " as " + pickedClass.name);
						return new string[] { "Set player " + player.Name + " as " + pickedClass.name };
				}
			return new string[] { GetUsage() };
		}

		internal void OnRACommand(ref RACommandEvent ev)
		{
			// big brain time
			int i;
			for (i = 0; i < ev.Command.Length && ev.Command[i] != ' '; i++) ;

			if (!plugin.Configs.aliases.Contains(ev.Command.Substring(0, i))) return;

			ev.Allow = false;
			string[] args = ev.Command.Split(' ');
			if (args.Length < 1)
			{
				ev.Sender.RAMessage(GetUsage(), false);
				return;
			}
			else switch (args[1].ToUpperInvariant())
				{
					case "RELOAD":
						ev.Sender.RAMessage($"Reloaded {Methods.LoadClasses()} classes." , true);
						return;
					case "LIST":
						if (args.Length == 2)
						{
							ev.Sender.RAMessage("List of names:\n" + string.Join(", ", from subclass in MTFplus.disctinctSubclasses 
																					   select subclass.name));
						}
						else Timing.RunCoroutine(Methods.FetchList(ev.Sender));
						return;
					case "DISPLAY":
						if (args.Length < 3)
						{
							ev.Sender.RAMessage("Please, introduce a name as your second argument.", false);
							return;
						}
						Subclass theOneAndOnly = MTFplus.subclasses.Get(args[2]);

						if (theOneAndOnly.Equals(Subclass.Empty))
						{
							ev.Sender.RAMessage("Subclass not found.", false);
							return;
						}
						return new string[] { theOneAndOnly.ToString() };
					case "SPAWN":
						if (sender is Player p && !plugin.ranks.Contains(p.GetRankName()))
						{
							return new string[] { "Your rank is allowed to use this command" };
						}
						if (args.Length < 3)
						{
							return new string[] { $"Usage: {string.Join("/", plugin.aliases)} SPAWN <player name/player id> <class name>" };
						}
						Player player = null;
						if (int.TryParse(args[1], out int id))
						{
							try { player = PluginManager.Manager.Server.GetPlayer(id); } catch { return new string[] { "Error with ID (" + args[1] + "). Try again or use the name." }; }
						}
						else
						{
							List<Player> matchingPlayers = PluginManager.Manager.Server.GetPlayers(args[1]);
							player = matchingPlayers.OrderBy(x => x.Name.Length).FirstOrDefault();
						}
						if (player == null)
						{
							return new string[] { "Player not found." };
						}
						Subclass pickedClass = MTFplus.subclasses.Get(args[2]);
						if (pickedClass.Equals(SubclassMethods.Empty))
						{
							return new string[] { "Subclass not found." };
						}
						player.ChangeRole(pickedClass.role, false, true, true, true);
						player.SetClass(pickedClass);
						if (sender is Player pl) plugin.Info(pl.Name + " (" + pl.SteamId + ") spawned " + player.Name + " as " + pickedClass.name);
						return new string[] { "Set player " + player.Name + " as " + pickedClass.name };
				}
			return new string[] { GetUsage() };
		}
	}
}
