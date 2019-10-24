using MEC;
using Smod2;
using Smod2.API;
using Smod2.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTFplus
{
    class MTFPlusCommands : ICommandHandler
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
    }
}
