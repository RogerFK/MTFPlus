using System;
using System.Collections.Generic;
using EXILED;

namespace MTFplus
{
	public class MTFplus : Plugin
	{
        internal static MTFplus Instance;
		public bool enable = true;
        public static List<Subclass> subclasses = new List<Subclass>();
		public static List<Subclass> disctinctSubclasses = new List<Subclass>();

		public override void OnDisable()
		{
			if (!enable) return;


		}
        internal static Random RNG { private set; get; }
		public override string getName => "MTFPlus";
		internal static MTFPEvents LocalEvents { private set; get; }
		public MTFPConfigs Configs { get; private set; }

		public override void OnEnable()
		{
			enable = Config.GetBool("mtfp_enable", true);
			if (!enable) return;
			RNG = new Random();
			Configs = MTFPConfigs.ReloadConfigs();
			LocalEvents = new MTFPEvents(this);

			if (this.Configs.debug) DebugMessage("MTFPlus loaded in Debug Mode. The console will get spammed as hell.");
            Instance = this;
			Events.TeamRespawnEvent += LocalEvents.TeamRespawnEvent;
			Events.ConsoleCommandEvent += LocalEvents.OnCallCommand;
			Events.WaitingForPlayersEvent += LocalEvents.OnWaitingForPlayers;
		}

		public override void OnReload()
		{
			
		}

		// Sorry, I don't like doing Assembly.GetCallingAssembly().FullName 30 times in one single frame.
		public void DebugMessage(string message)
		{
			ServerConsole.AddLog("[MTFPlus] " + message);
		}
	}
}
