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

		public string GetUsage()
		{
			string aliases = string.Join("/", plugin.Configs.aliases);

			return "Usage:" + Environment.NewLine +
				$"{aliases} LIST [COMPLETE] - Displays the list of Subclasses, with each stat if you add COMPLETE, true or 1 to the end of the command (works with anything really)." + Environment.NewLine +
				$"{aliases} DISPLAY <name> - Displays the class with that name" + Environment.NewLine +
				$"{aliases} SPAWN <player name/player id> <class name> - Spawn a player as a class.";
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
						ev.Sender.RAMessage(theOneAndOnly.ToString());
						return;
					case "SPAWN":
						var player = Plugin.GetPlayer(ev.Sender.SenderId);
						if (player != null && !plugin.Configs.ranks.Contains(player.serverRoles.GlobalBadge))
						{
							ev.Sender.RAMessage("You aren't allowed to run this command.", false);
							return;
						}
						if (args.Length < 4)
						{
							ev.Sender.RAMessage($"Usage: {string.Join("/", args[0])} SPAWN <player name/player id> <class name>", false);
							return;
						}
						ReferenceHub target = Plugin.GetPlayer(args[2]);
						if (target == null)
						{
							ev.Sender.RAMessage("Player not found.", false);
							return;
						}
						Subclass pickedClass = MTFplus.subclasses.Get(args[3]);
						if (pickedClass.Equals(Subclass.Empty))
						{
							ev.Sender.RAMessage("Subclass not found.", false);
							return;
						}
						target.characterClassManager.SetPlayersClass(pickedClass.role, target.gameObject, false, false);
						player.SetClass(pickedClass);
						
						if (player != null)
						{
							Plugin.Info($"Player {target.nicknameSync.MyNick} spawned as subclass {pickedClass.name} by admin {player.nicknameSync.MyNick}" +
							$" with ID: ({player.characterClassManager.UserId})");
						}
						ev.Sender.RAMessage("Set player " + target.nicknameSync.MyNick + " as " + pickedClass.name);
						return;
				}
			ev.Sender.RAMessage(GetUsage(), false);
		}
	}
}
