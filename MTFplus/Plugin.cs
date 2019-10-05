using System.Collections.Generic;
using Smod2;
using Smod2.Attributes;
using Smod2.Config;
using DMP;

namespace MTFplus
{
	[PluginDetails(
		author = "RogerFK",
		name = "MTF Plus",
		description = "A plugin so NTF cadets aren't just shit and not fun to play",
		id = "rogerfk.mtfplus",
		version = "1.0",
		SmodMajor = 3,
		SmodMinor = 5,
		SmodRevision = 0,
		configPrefix = "mtfp"
		)]
	public class MTFplus : Plugin
	{
        internal static MTFplus Instance;

        public static List<Subclass> subclasses = new List<Subclass>();

		[ConfigOption]
		public bool enable = true;
		[ConfigOption]
		public bool debug = false;
		[ConfigOption]
		public string[] aliases = new string[] { "mtfp", "mtfplus", "m+" };
		[ConfigOption]
		public string[] ranks = new string[] { "owner", "admin", "e-girl" };
		[ConfigOption]
		public int userConsoleList = 2;
		[ConfigOption]
		public float listDelay = 0.3f;
		[ConfigOption]
		public float delay = 0.1f;

		public override void OnDisable()
		{
			this.Info("Disabled MTF Plus");
		}
		public override void OnEnable()
		{
			this.Info("Enabled MTF Plus");
		}
        public static System.Random random = new System.Random();

		public override void Register()
		{
			this.AddEventHandlers(new Events(this), Smod2.Events.Priority.Low);
			DamagePercentages.Initialize(this, Smod2.Events.Priority.Highest);
			this.AddCommands(aliases, new MTFPlusCommands(this));
			if (debug) Info("MTFPlus loaded in Debug Mode. The console will get spammed as hell.");
            Instance = this;
		}
	}
}
