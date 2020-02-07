using EXILED;
using System.Collections.Generic;

namespace MTFplus
{
	public class MTFPConfigs
	{
		public bool debug;

		public HashSet<string> aliases;

		public HashSet<string> ranks;
		
		public int userConsoleList;
		
		public float listDelay;

		public float delay;

		// great constructor yes
		public static MTFPConfigs ReloadConfigs()
		{
			var config = new MTFPConfigs
			{
				debug = Plugin.Config.GetBool("mtfp_debug", false),
				userConsoleList = Plugin.Config.GetInt("mtfp_user_console_list", 2),
				listDelay = Plugin.Config.GetFloat("mtfp_list_delay", 0.3f),
				delay = Plugin.Config.GetFloat("mtfp_delay", 0.1f),
				aliases = new HashSet<string>() { "MTFP", "MTFPLUS", "M+" },
				ranks = new HashSet<string>() { "owner", "admin", "e-girl" }
			};

			if (config.userConsoleList > 2 || config.userConsoleList < 0)
			{
				config.userConsoleList = 2;
			}
			
			var tempAliases = Plugin.Config.GetStringList("mtfp_aliases");
			if (tempAliases != null && tempAliases.Count > 0)
			{
				config.aliases.Clear();
				foreach(string alias in tempAliases)
				{
					if(!config.aliases.Contains(alias)) config.aliases.Add(alias.ToUpperInvariant());
				}
			}
			
			var tempRanks = Plugin.Config.GetStringList("mtfp_ranks");
			if (tempRanks != null)
			{
				config.ranks.Clear();
				foreach (string rank in tempRanks)
				{
					if (rank == "all")
					{
						config.ranks.Clear();
						break;
					}
					if (!config.aliases.Contains(rank)) config.ranks.Add(rank);
				}
			}

			return config;
		}
	}
}
