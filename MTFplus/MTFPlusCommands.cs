using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Smod2;
using Smod2.Commands;

namespace MTFplus
{
	class MTFPlusCommands : ICommandHandler
	{
		private readonly Plugin plugin;

		public MTFPlusCommands(Plugin plugin)
		{
			this.plugin = plugin;
		}

		public string GetCommandDescription()
		{
			return "dis command for mtfplus ploogin, us MTFPlus alon for sumthin";
		}

		public string GetUsage()
		{
			return "Usage:" + Environment.NewLine +
				"MTFPlus LIST [COMPLETE] - Displays the list of Subclasses, with each stat if you add COMPLETE, true or 1 to the end of the command." + Environment.NewLine +
				"MTFPlus DISPLAY <name> - Displays the class with that name" + Environment.NewLine +
				"MTFPlus SPAWN <name> - Not currently implemented. Implement it yourself and do a pull request.";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if(args.Length < 1) return new string[] { GetUsage() };
			else switch (args[0].ToUpper())
				{
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
								subclassesList[i] = distinctSubclasses.ElementAt(i).name + (i != count-1 ? ", " : string.Empty);
							}
						}
						return new string[] { "List of " + (args.Length > 1 ? "subclasses: " : "names: "), string.Join(string.Empty, subclassesList) };
					case "DISPLAY":
						if(args.Length < 2)
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
						return new string[] { "dis not work sry" };
				}
			return new string[] { GetUsage() };
		}
	}
}
