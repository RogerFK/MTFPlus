using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MTFplus
{
    internal class Events : IEventHandlerTeamRespawn, IEventHandlerWaitingForPlayers, IEventHandlerCallCommand
    {
        private readonly MTFplus plugin;
        public Events(MTFplus plugin)
        {
            this.plugin = plugin;
        }

        public void OnTeamRespawn(TeamRespawnEvent ev)
        {
            if (!plugin.enable) return;

            if (ev.SpawnChaos || ev.PlayerList.Count == 0) return;

            MTFplus.subclasses.OrderBy(x => MTFplus.random.Next());
            MEC.Timing.RunCoroutine(RespawnPlus(ev.PlayerList), MEC.Segment.FixedUpdate);
        }

        private IEnumerator<float> RespawnPlus(List<Player> Players)
        {
            yield return MEC.Timing.WaitForSeconds(plugin.listDelay);
            Player luckyBoi;
            int curr = MTFplus.random.Next(0, Players.Count), timesIterated = 0;
            
            foreach (Subclass subclass in MTFplus.subclasses)
            {
                if (timesIterated == Players.Count)
                {
                    if (plugin.debug) plugin.Info($"One instance of subclass {subclass.name} didn't spawn this wave (not enough players)!");
                    else yield break;
                    continue;
                }
                if (subclass.probability * 100 >= MTFplus.random.Next(0, 10000))
                {
                    do
                    {
                        luckyBoi = Players[curr];
                        timesIterated++;
                        curr = (curr + 1) % Players.Count;
                    } while (((GameObject)luckyBoi.GetGameObject()).GetComponent<CharacterClassManager>()
                    .curClass != 13 // NTF Cadet number
                    && timesIterated < Players.Count);

                    if (((GameObject)luckyBoi.GetGameObject()).GetComponent<CharacterClassManager>().curClass == 13)
                    {
                        if (plugin.debug) plugin.Info($"Spawning {luckyBoi.Name} as {subclass.name}");

                        MEC.Timing.RunCoroutine(luckyBoi.SetClass(subclass), 1);
                    }
                }
                else
                {
                    if (plugin.debug) plugin.Info($"Bad luck for one instance of {subclass.name}. Skipping to next subclass/another instance of this class.");
                }
            }
        }

        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            if (!plugin.enable) return;

            Methods.LoadClasses(plugin);
        }

        public void OnCallCommand(PlayerCallCommandEvent ev)
        {
            if (!plugin.enable) return;

            if (ev.Command.StartsWith("mtfplist"))
            {
                if (plugin.userConsoleList != 1 && plugin.userConsoleList != 2)
                {
                    ev.ReturnMessage = "You are not allowed to see the list of MTFPlus classes in this server!";
                    return;
                }
                ev.ReturnMessage = "<color=\"white\">List:</color>";
                ev.Player.DelayListMessage();
            }
        }
    }
}