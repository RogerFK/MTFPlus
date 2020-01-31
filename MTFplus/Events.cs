using EXILED;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MTFplus
{
    public class MTFPEvents
    {
        private readonly MTFplus plugin;
        public MTFPEvents(MTFplus plugin)
        {
            this.plugin = plugin;
        }

        internal void TeamRespawnEvent(ref TeamRespawnEvent ev)
        {
            // Don't compute the MTFPlus algorithm if the plugin is disabled
            if (!plugin.enable || ev.IsChaos || ev.ToRespawn != null || ev.ToRespawn.Count == 0) return;

            MTFplus.subclasses.ShuffleList();
            MEC.Timing.RunCoroutine(RespawnPlus(ev.ToRespawn));
        }
        private IEnumerator<float> RespawnPlus(List<ReferenceHub> Players)
        {
            yield return MEC.Timing.WaitForSeconds(plugin.Configs.listDelay);

            ReferenceHub luckyBoi;
            int curr = MTFplus.RNG.Next(0, Players.Count), timesIterated = 0;
            
            /*
                There's 3 million and a half ways to do this in many other ways.
                If you're sure you know a better, easier to read and mantainable way to do it
                (even if that would imply changing the Subclass class)
                feel free to PR it to me and I'll give it a check.
             */
            foreach (Subclass subclass in MTFplus.subclasses)
            {
                if (timesIterated == Players.Count)
                {
                    if (plugin.Configs.debug) plugin.DebugMessage($"[DEBUG] One instance of subclass {subclass.name} didn't spawn this wave (not enough players)!");
                    else yield break; // Yield break ends the execution if the config "mtfp_debug" is set to false (by default)
                    continue;
                }
                if (subclass.probability * 100 >= MTFplus.RNG.Next(0, 10000))
                {
                    // Evaluate players until a NTF Cadet is found
                    do
                    {
                        luckyBoi = Players[curr];
                        timesIterated++;
                        curr = (curr + 1) % Players.Count;
                    } while (luckyBoi.characterClassManager.CurClass != RoleType.NtfCadet
                    && timesIterated < Players.Count);

                    // Skip the "SetClass" method if a given luckyBoi isn't a NTF Cadet in case the loop above didn't find one
                    if (luckyBoi.characterClassManager.CurClass == RoleType.NtfCadet)
                    {
                        if (plugin.Configs.debug) plugin.DebugMessage($"[DEBUG] Spawning {luckyBoi.nicknameSync.MyNick} as {subclass.name}");

                        luckyBoi.SetClass(subclass);
                    }
                }
                else
                {
                    if (plugin.Configs.debug) plugin.DebugMessage($"[DEBUG] Bad luck for one instance of {subclass.name}. Skipping to next subclass/another instance of this class.");
                }
            }
        }

        public void OnWaitingForPlayers()
        {
            if (!plugin.enable) return;

            Methods.LoadClasses(plugin);
        }

        public void OnCallCommand(ConsoleCommandEvent ev)
        {
            if (!plugin.enable) return;

            if (ev.Command.StartsWith("mtfplist"))
            {
                if (plugin.Configs.userConsoleList != 1 && plugin.Configs.userConsoleList != 2)
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