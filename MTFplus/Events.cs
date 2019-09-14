using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MTFplus
{
    internal class Events : IEventHandlerTeamRespawn, IEventHandlerWaitingForPlayers, IEventHandlerCallCommand
    {
        private readonly MTFplus plugin;
        public static bool IMbool { get; set; }
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
            Stack<Player> cadets = new Stack<Player>(Players.Where(ply => ((GameObject)ply.GetGameObject()).GetComponent<CharacterClassManager>().curClass == 13));
            foreach (Subclass subclass in MTFplus.subclasses)
            {
                if (cadets.Count <= 0)
                {
                    if (plugin.debug) plugin.Info("One instance of subclass " + subclass.name + " didn't spawn this wave (not enough players)!");
                    continue;
                }
                if (subclass.probability * 100 >= MTFplus.random.Next(0, 10000))
                {
                    Player luckyBoi = cadets.Pop();
                    if (plugin.debug) plugin.Info("Spawning " + luckyBoi.Name + " as " + subclass.name);
                    MEC.Timing.RunCoroutine(plugin.SetClass(luckyBoi, subclass), 1);
                }
                else
                {
                    if (plugin.debug) plugin.Info("Bad luck for one instance of " + subclass.name + ". Skipping to next subclass/another instance of this class.");
                }
            }
        }

        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            // Check if ItemManager is installed and enabled on the server
            if (PluginManager.Manager.EnabledPlugins.Where(plugin => plugin.Details.id == "4aiur.custom.itemmanager").Count() > 0) IMbool = true;

            if (!plugin.enable) return;
            MTFplus.subclasses.Clear();
            string directory = FileManager.GetAppFolder() + @"MTFplus";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                File.WriteAllText(directory + @"/medic.txt",
                    "Inventory: SENIOR_GUARD_KEYCARD, P90, RADIO, DISARMER, MEDKIT, MEDKIT, MEDKIT, MEDKIT\n" +
                    "Max: 2\n" +
                    "Role: NTF_CADET\n" +
                    "Probability: 80\n" +
                    "Ammo5: 200\n" +
                    "Ammo7: 70\n" +
                    "Ammo9: 50");
                plugin.Info("Created " + directory + ". Fill it with your own MTF classes!\nAdditionally, a template class (Medic) was created with it");
            }
            string[] filenames = Directory.GetFiles(directory, "*.txt");
            foreach (string filename in filenames)
            {
                string name = filename.Remove(0, directory.Length + 1);
                plugin.Info("Fetching " + name + "...");
                string[] lines = FileManager.ReadAllLines(filename).Where(x => !string.IsNullOrWhiteSpace(x)).Where(x => x[0] != '#').ToArray();

                // Default values
                Role role = Role.NTF_CADET;
                int maxCount = 1;
                float probability = 100f;

                List<ItemType> inventory = new List<ItemType>();
                int[] IMinventory = new int[16];
                for (int i = 0; i < 16; i++) IMinventory[i] = -1;
                int HP = 0;

                int[] ammo = new int[3] { 0, 0, 0 };
                string broadcast = string.Empty;
                foreach (string data in lines)
                {
                    if (data.StartsWith("Inventory"))
                    {
                        string[] invData = data.Remove(0, 10).Split(',');
                        for (int i = 0, j = 0; i < invData.Length; i++, j++)
                        {
                            string item = invData[i].Trim();
                            // /* Take out the two // before this to remove the ItemManager stuff
                            #region ItemManager Stuff
                            if (IMbool)
                            {
                                if (item.StartsWith("IM:"))
                                {
                                    if (int.TryParse(item.Substring(3), out int aux))
                                    {
                                        try
                                        {
                                            if (MTFplus.ItemManagerExists(aux))
                                            {
                                                IMinventory[j] = aux;
                                                inventory.Add(ItemType.COIN);
                                            }
                                            else
                                            {
                                                plugin.Error("Custom item (ItemManager) with ID: " + aux + " doesn't exist/isn't installed!");
                                                j--;
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            plugin.Error("ItemManager not found or threw an error!\n" + e);
                                        }
                                    }
                                    else
                                    {
                                        plugin.Error("Invalid CustomItem \"" + item + " (" + item.Substring(3) + ")" + "\" in " + filename + "!");
                                    }
                                    continue;
                                }
                            }
                            #endregion
                            // */
                            if (!Enum.TryParse(item, out ItemType parsedItem))
                            {
                                plugin.Error("Invalid item \"" + item + "\" in " + filename + '!');
                            }
                            else
                            {
                                inventory.Add(parsedItem);
                            }
                        }
                        if (inventory.Count == 0 && IMinventory.Length == 0)
                        {
                            plugin.Error("\"" + filename + "\" doesn't have any valid items. Are you sure this is intended?");
                        }
                    }
                    else if (data.StartsWith("Role"))
                    {
                        string roleData = data.Remove(0, 5).Trim();
                        if (!Enum.TryParse(roleData, out Role roleParsed))
                        {
                            plugin.Error("Invalid role \"" + roleData + "\" in " + filename + '!');
                        }
                        else
                        {
                            role = roleParsed;
                        }
                    }
                    else if (data.StartsWith("Max"))
                    {
                        string maxData = data.Remove(0, 4).Trim();
                        if (!int.TryParse(maxData, out int probablyMaxCount))
                        {
                            plugin.Error("Invalid maximum count \"" + maxData + "\" in " + filename + '!');
                        }
                        else
                        {
                            maxCount = probablyMaxCount;
                        }
                    }
                    else if (data.StartsWith("Probability"))
                    {
                        string prob = data.Remove(0, 12).Trim();
                        if (!float.TryParse(prob, out float probabilitey))
                        {
                            plugin.Error("Invalid probability \"" + prob + "\" in " + filename + '!');
                        }
                        else
                        {
                            probability = probabilitey;
                        }
                    }
                    else if (data.StartsWith("Ammo"))
                    {
                        if (!int.TryParse(data[4].ToString(), out int ammoTyperino))
                        {
                            plugin.Error("\"Ammo\" \"" + data + "\" unrecognized in " + filename + '!');
                        }
                        int ammoType = (ammoTyperino - 5) / 2;
                        if (ammoType < 0 || ammoType > 2)
                        {
                            plugin.Error(data[4].ToString() + " is not a type of ammo! (in line: " + data + " in " + filename);
                        }
                        string ammoStr = data.Remove(0, 6).Trim();
                        if (!int.TryParse(ammoStr, out int parsedAmmo))
                        {
                            plugin.Error("Invalid Ammo \"" + ammoStr + "\" in " + filename + '!');
                        }
                        else
                        {
                            ammo[ammoType] = parsedAmmo;
                        }
                    }
                    else if (data.StartsWith("Broadcast"))
                    {
                        broadcast = data.Remove(0, 10).Trim();
                    }
                    else if (data.StartsWith("HP"))
                    {
                        string HPstr = data.Substring(3).Trim();
                        if (!int.TryParse(HPstr, out int HPaux))
                        {
                            plugin.Error("Invalid HP \"" + HPstr + "\" in " + filename + '!');
                        }
                        else
                        {
                            HP = HPaux;
                        }
                    }
                    else
                    {
                        plugin.Error("Unknown line: " + data + " in file " + filename);
                    }
                }
                name = name.Substring(0, name.Length - 4);
                Subclass subclass = new Subclass(name, role, inventory, IMinventory, probability, ammo, broadcast, HP);
                for (int i = 0; i < maxCount; i++) MTFplus.subclasses.Add(subclass);

                plugin.Info("Success! Loaded " + name + " as a new class" + (plugin.debug ? ":\n" + subclass.ToString() : string.Empty));
                if (plugin.debug) plugin.Info(subclass.ToString());
            }
        }

        public void OnCallCommand(PlayerCallCommandEvent ev)
        {
            if (ev.Command.StartsWith("mtfplist"))
            {
                ev.ReturnMessage = "List:";
                switch (plugin.userConsoleList)
                {
                    case 1:
                        IEnumerable<Subclass> distinctSubclasses = MTFplus.subclasses.Distinct();
                        string sclistName = string.Empty;
                        int count = distinctSubclasses.Count();
                        for (int i = 0; i < count; i++)
                        {
                            sclistName += distinctSubclasses.ElementAt(i).name + (i != count - 1 ? ", " : string.Empty);
                        }
                        MEC.Timing.RunCoroutine(DelayConsoleMessage(ev.Player, sclistName));
                        return;
                    case 2:
                        IEnumerable<Subclass> distinctSubclasses2 = MTFplus.subclasses.Distinct();
                        string sclistFull = string.Empty;
                        int counterino = distinctSubclasses2.Count();
                        for (int i = 0; i < counterino; i++)
                        {
                            sclistFull += distinctSubclasses2.ElementAt(i).ToString() + Environment.NewLine + "------------" + Environment.NewLine;
                        }
                        MEC.Timing.RunCoroutine(DelayConsoleMessage(ev.Player, sclistFull));
                        return;
                    default:
                        ev.ReturnMessage = "You are not allowed to see the list of MTFPlus classes in this server!";
                        return;
                }
            }
        }
        public IEnumerator<float> DelayConsoleMessage(Player player, string message)
        {
            yield return MEC.Timing.WaitForSeconds(0.5f);
            player.SendConsoleMessage(message, "white");
        }
    }
}