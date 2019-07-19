// Made by RogerFK. If any bug is to be found, it ought to be reported to that guy
// Keep in mind this may consume a lot of CPU for each and every player.
using System.Collections.Generic;
using System.Linq;
using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using UnityEngine;

namespace DMP
{
	public class DamagePercentages : IEventHandlerWaitingForPlayers, IEventHandlerSetConfig, IEventHandlerPlayerHurt, IEventHandlerMedkitUse, IEventHandlerPlayerDie, IEventHandlerDisconnect
	{
		private static Plugin plugin;
		private static Dictionary<int, float> multipliers = new Dictionary<int, float>();
		private static Dictionary<string, Role> maxHpKeys = new Dictionary<string, Role>()
		{
			{ "scp049_hp", Role.SCP_049 },
			{ "scp049-2_hp", Role.SCP_049_2 },
			{ "scp079_hp", Role.SCP_079 },
			{ "scp096_hp", Role.SCP_096 },
			{ "scp106_hp", Role.SCP_106 },
			{ "scp173_hp", Role.SCP_173 },
			{ "scp939_53_hp", Role.SCP_939_53 },
			{ "scp939_89_hp", Role.SCP_939_89 },
			{ "classd_hp", Role.CLASSD },
			{ "scientist_hp", Role.SCIENTIST },
			{ "ci_hp", Role.CHAOS_INSURGENCY },
			{ "ntfg_hp", Role.NTF_CADET },
			{ "ntfscientist_hp", Role.NTF_SCIENTIST },
			{ "ntfl_hp", Role.NTF_LIEUTENANT },
			{ "ntfc_hp", Role.NTF_COMMANDER },
			{ "tutorial_hp", Role.TUTORIAL },
			{ "facilityguard_hp", Role.FACILITY_GUARD }
		};
		private static Dictionary<Role, int> maxHps = new Dictionary<Role, int>();

		public static void Initialize(Plugin plugin, Priority priority = Priority.Normal)
		{
			plugin.AddEventHandlers(new DamagePercentages(plugin), priority);
		}
		public DamagePercentages(Plugin plugin)
		{
			DamagePercentages.plugin = plugin;
		}

		public static List<Room> rooms = null;
		public static bool AddOrModify(int PlayerId, float maxHP, Role role)
		{
			if (!maxHps.ContainsKey(role))
			{
				plugin.Error("Error! Role " + role + " is currently unsupported for DamagePercentages!");
				return false;
			}
			// Since I'm assuming you just want to change his Max HP
			if (multipliers.ContainsKey(PlayerId)) multipliers[PlayerId] = maxHps[role] / maxHP;
			else multipliers.Add(PlayerId, maxHps[role] / maxHP);
			return true;
		}

		public static bool Delete(int PlayerId)
		{
			if (multipliers.ContainsKey(PlayerId))
			{
				multipliers.Remove(PlayerId);
				return true;
			}
			return false;
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			multipliers.Clear();
		}

		public void OnSetConfig(SetConfigEvent ev)
		{
			if (ev.Key.EndsWith("_hp"))
			{
				if (!maxHpKeys.ContainsKey(ev.Key))
				{
					plugin.Debug("Key " + ev.Key + " not supported as HP value. Is this a bug?");
					return;
				}

				if(!maxHps.ContainsKey(maxHpKeys[ev.Key])) maxHps.Add(maxHpKeys[ev.Key], (int) ev.Value);
				else maxHps[maxHpKeys[ev.Key]] = (int) ev.Value;
			}
		}

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (multipliers.ContainsKey(ev.Player.PlayerId))
			{
				ev.Damage *= multipliers[ev.Player.PlayerId];
			}
		}

		public void OnMedkitUse(PlayerMedkitUseEvent ev)
		{
			if (multipliers.ContainsKey(ev.Player.PlayerId))
			{
				ev.RecoverHealth = (int)(ev.RecoverHealth * multipliers[ev.Player.PlayerId]);
			}
		}

		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			if (multipliers.ContainsKey(ev.Player.PlayerId))
			{
				multipliers.Remove(ev.Player.PlayerId);
			}
		}

		public void OnDisconnect(DisconnectEvent ev)
		{
			MEC.Timing.RunCoroutine(CheckDisconnects(), 1);
		}
		
		private IEnumerator<float> CheckDisconnects()
		{
			yield return MEC.Timing.WaitForSeconds(3f);
			List<int> idsToKeep = PluginManager.Manager.Server.GetPlayers()
										   .Select(player => player.PlayerId)
										   .Intersect(multipliers.Keys)
										   .ToList(); // List because according to most people iterating over an IEnumerable constantly is a bad practice and I don't wanna make a binary tree
			foreach (int id in multipliers.Keys)
			{
				if(!idsToKeep.Contains(id)) multipliers.Remove(id);
			}
		}
	}
}