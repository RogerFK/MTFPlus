using DMP;
using MEC;
using Smod2.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MTFplus
{
    internal static class Methods
    {
        public static void SetClass(this Player player, Subclass subclass)
        {
            Timing.RunCoroutine(_SetClass(player, subclass), Segment.FixedUpdate);
        }
        public static IEnumerator<float> _SetClass(Player player, Subclass subclass)
        {
            List<int> indexesToRemove = new List<int>();
            if (subclass.role != Role.NTF_CADET) player.ChangeRole(subclass.role, false, false, true, true);
            yield return Timing.WaitForSeconds(MTFplus.Instance.delay);
            if (subclass.inventory.Count > 0)
            {
                foreach (Smod2.API.Item item in player.GetInventory())
                {
                    item.Remove();
                }
                foreach (ItemType item in subclass.inventory)
                {
                    player.GiveItem(item);
                }
            }
            // /* Take out the // before this to remove the ItemManager stuff
            #region ItemManager Stuff
            for (int i = 0; i < 16; i++)
            {
                if (subclass.imInv[i] < 0) continue;
                if (!GrantCustomItem(player, subclass, subclass.imInv[i], i))
                {
                    MTFplus.Instance.Error("Custom item (ItemManager) with ID: " + subclass.imInv[i] + " doesn't exist/isn't installed!");
                    indexesToRemove.Add(i); // That's the index inside the inventory, and a coin was there as a placeholder
                }
            }
            #endregion
            // */
            for (int i = 0; i < 3; i++)
            {
                if (subclass.ammo[i] > 0) player.SetAmmo((AmmoType)i, subclass.ammo[i]);
            }
            List<Smod2.API.Item> inv = player.GetInventory();
            foreach (int i in indexesToRemove)
            {
                inv[i].Remove();
            }
            if (subclass.maxHP > 0)
            {
                DamagePercentages.AddOrModify(player.PlayerId, subclass.maxHP, subclass.role);
            }
            if (!string.IsNullOrWhiteSpace(subclass.broadcast))
            {
                player.PersonalBroadcast(5, subclass.broadcast, false);
            }
        }
        public static int LoadClasses(MTFplus plugin = null)
        {
            bool verbose = plugin != null;
            int SuccessfulCount = 0;
            string directory = FileManager.GetAppFolder() + @"MTFplus";

            MTFplus.subclasses.Clear();
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
                if (verbose) plugin.Info("Created " + directory + ". Fill it with your own MTF classes!\nAdditionally, a template class (Medic) was created with it");
            }
            string[] filenames = Directory.GetFiles(directory, "*.txt");
            foreach (string filename in filenames)
            {
                string name = filename.Remove(0, directory.Length + 1);
                if (verbose) plugin.Info("Fetching " + name + "...");
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
                            if (item.StartsWith("IM:"))
                            {
                                if (int.TryParse(item.Substring(3), out int aux))
                                {
                                    try
                                    {
                                        if (ItemManagerExists(aux))
                                        {
                                            IMinventory[j] = aux;
                                            inventory.Add(ItemType.COIN);
                                        }
                                        else
                                        {
                                            if (verbose) plugin.Error("Custom item (ItemManager) with ID: " + aux + " doesn't exist/isn't installed!");
                                            j--;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        if (verbose) plugin.Error("ItemManager not found or threw an error!\n" + e);
                                    }
                                }
                                else
                                {
                                    if (verbose) plugin.Error("Invalid CustomItem \"" + item + " (" + item.Substring(3) + ")" + "\" in " + filename + "!");
                                }
                                continue;
                            }
                            #endregion
                            // */
                            if (!Enum.TryParse(item, out ItemType parsedItem))
                            {
                                if (verbose) plugin.Error("Invalid item \"" + item + "\" in " + filename + '!');
                            }
                            else
                            {
                                inventory.Add(parsedItem);
                            }
                        }
                        if (inventory.Count == 0 && IMinventory.Length == 0)
                        {
                            if (verbose) plugin.Error("\"" + filename + "\" doesn't have any valid items. Are you sure this is intended?");
                        }
                    }
                    else if (data.StartsWith("Role"))
                    {
                        string roleData = data.Remove(0, 5).Trim();
                        if (!Enum.TryParse(roleData, out Role roleParsed))
                        {
                            if (verbose) plugin.Error("Invalid role \"" + roleData + "\" in " + filename + '!');
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
                            if (verbose) plugin.Error("Invalid maximum count \"" + maxData + "\" in " + filename + '!');
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
                            if (verbose) plugin.Error("Invalid probability \"" + prob + "\" in " + filename + '!');
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
                            if (verbose) plugin.Error("\"Ammo\" \"" + data + "\" unrecognized in " + filename + '!');
                        }
                        int ammoType = (ammoTyperino - 5) / 2;
                        if (ammoType < 0 || ammoType > 2)
                        {
                            if (verbose) plugin.Error(data[4].ToString() + " is not a type of ammo! (in line: " + data + " in " + filename);
                        }
                        string ammoStr = data.Remove(0, 6).Trim();
                        if (!int.TryParse(ammoStr, out int parsedAmmo))
                        {
                            if (verbose) plugin.Error("Invalid Ammo \"" + ammoStr + "\" in " + filename + '!');
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
                            if (verbose) plugin.Error("Invalid HP \"" + HPstr + "\" in " + filename + '!');
                        }
                        else
                        {
                            HP = HPaux;
                        }
                    }
                    else
                    {
                        if (verbose) plugin.Error("Unknown line: " + data + " in file " + filename);
                    }
                }
                name = name.Substring(0, name.Length - 4);
                Subclass subclass = new Subclass(name, role, inventory, IMinventory, probability, ammo, broadcast, HP);
                for (int i = 0; i < maxCount; i++) MTFplus.subclasses.Add(subclass);

                if (verbose)
                {
                    plugin.Info("Success! Loaded " + name + " as a new class" + (plugin.debug ? ":\n" + subclass.ToString() : string.Empty));
                    if (plugin.debug) plugin.Info(subclass.ToString());
                }
                SuccessfulCount++;
            }

            return SuccessfulCount;
        }
        // /*
        public static bool ItemManagerExists(int id)
        {
            return ItemManager.Items.Handlers.ContainsKey(id);
        }
        private static bool GrantCustomItem(Player player, Subclass subclass, int id, int index)
        {
            if (ItemManager.Items.Handlers.ContainsKey(id))
            {
                ItemManager.Items.Handlers[subclass.imInv[index]].Create((player.GetGameObject() as GameObject).GetComponent<Inventory>(), index);
                return true;
            }
            return false;
        }
        // */

        internal static void DelayListMessage(this Player player)
        {
            MEC.Timing.RunCoroutine(ListCoroutine(player), Segment.FixedUpdate);
        }
        private static IEnumerator<float> ListCoroutine(Player player)
        {
            yield return MEC.Timing.WaitForSeconds(0.1f);
            string message = string.Empty;
            Subclass[] distinctSubclasses = MTFplus.subclasses.Distinct().ToArray();
            int count = distinctSubclasses.Length;
            if (MTFplus.Instance.userConsoleList == 1)
            {
                for (int i = 0; i < count; i++)
                {
                    message += distinctSubclasses[i].name + (i != count - 1 ? ", " : string.Empty);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    message += distinctSubclasses[i].ToString() + Environment.NewLine + "------------" + Environment.NewLine;
                }
            }
            player.SendConsoleMessage(message, "white");
        }
    }
}
