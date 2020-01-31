using MEC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MTFplus
{
    internal static class Methods
    {
        public static void SetClass(this ReferenceHub player, Subclass subclass)
        {
            Timing.RunCoroutine(_SetClass(player, subclass), Segment.FixedUpdate);
        }
        public static IEnumerator<float> _SetClass(ReferenceHub player, Subclass subclass)
        {
            List<int> indexesToRemove = new List<int>();
            if (subclass.role != RoleType.NtfCadet) player.characterClassManager.SetPlayersClass(subclass.role, player.gameObject);
            yield return Timing.WaitForSeconds(MTFplus.Instance.Configs.delay);
            if (subclass.inventory.Count > 0)
            {
                player.inventory.items.Clear();

                foreach (ItemType item in subclass.inventory)
                {
                    player.inventory.AddNewItem(item);
                }
            }
            #if ItemManager
            for (int i = 0; i < 16; i++)
            {
                if (subclass.imInv[i] < 0) continue;
                if (!GrantCustomItem(player, subclass, subclass.imInv[i], i))
                {
                    MTFplus.Instance.Error("Custom item (ItemManager) with ID: " + subclass.imInv[i] + " doesn't exist/isn't installed!");
                    indexesToRemove.Add(i); // That's the index inside the inventory, and a coin was there as a placeholder
                }
            }
            #endif
            for (int i = 0; i < 3; i++)
            {
                player.ammoBox.Networkamount = string.Concat(new string[]
                    {
                        subclass.ammo[0].ToString(),
                        ":",
                        subclass.ammo[1].ToString(),
                        ":",
                        subclass.ammo[2].ToString()
                    });
            }
            #if ItemManager // Not even updated lol
            List<Smod2.API.Item> inv = player.GetInventory();
            foreach (int i in indexesToRemove)
            {
                inv[i].Remove();
            }
            #endif
            if (subclass.maxHP > 0)
            {
                player.playerStats.maxHP = subclass.maxHP;
            }
            if (!string.IsNullOrWhiteSpace(subclass.broadcast))
            {
                player.GetComponent<Broadcast>().TargetAddElement(player.characterClassManager.connectionToClient, subclass.broadcast, 5u, false);
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
                if (verbose) plugin.DebugMessage("Created " + directory + ". Fill it with your own MTF classes!\nAdditionally, a template class (Medic) was created with it");
            }
            string[] filenames = Directory.GetFiles(directory, "*.txt");
            foreach (string filename in filenames)
            {
                string name = filename.Remove(0, directory.Length + 1);
                if (verbose) plugin.DebugMessage("Fetching " + name + "...");
                string[] lines = FileManager.ReadAllLines(filename).Where(x => !string.IsNullOrWhiteSpace(x)).Where(x => x[0] != '#').ToArray();

                // Default values
                RoleType role = RoleType.NtfCadet;
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
                            #if ItemManager
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
                            #endif
                            if (!Enum.TryParse(item, out ItemType parsedItem))
                            {
                                if (verbose) plugin.DebugMessage("[ERROR] Invalid item \"" + item + "\" in " + filename + '!');
                            }
                            else
                            {
                                inventory.Add(parsedItem);
                            }
                        }
                        if (inventory.Count == 0 && IMinventory.Length == 0)
                        {
                            if (verbose) plugin.DebugMessage("[WARNING] \"" + filename + "\" doesn't have any valid items. Are you sure this is intended?");
                        }
                    }
                    else if (data.StartsWith("Role"))
                    {
                        string roleData = data.Remove(0, 5).Trim();
                        if (!Enum.TryParse(roleData, out RoleType roleParsed))
                        {
                            if (verbose) plugin.DebugMessage("[ERROR] Invalid role \"" + roleData + "\" in " + filename + '!');
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
                            if (verbose) plugin.DebugMessage("[ERROR] Invalid maximum count \"" + maxData + "\" in " + filename + '!');
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
                            if (verbose) plugin.DebugMessage("[ERROR] Invalid probability \"" + prob + "\" in " + filename + '!');
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
                            if (verbose) plugin.DebugMessage("[ERROR] \"Ammo\" \"" + data + "\" unrecognized in " + filename + '!');
                        }
                        int ammoType = (ammoTyperino - 5) / 2;
                        if (ammoType < 0 || ammoType > 2)
                        {
                            if (verbose) plugin.DebugMessage("[ERROR] " + data[4].ToString() + " is not a type of ammo! (in line: " + data + " in " + filename);
                        }
                        string ammoStr = data.Remove(0, 6).Trim();
                        if (!int.TryParse(ammoStr, out int parsedAmmo))
                        {
                            if (verbose) plugin.DebugMessage("[ERROR] Invalid Ammo \"" + ammoStr + "\" in " + filename + '!');
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
                            if (verbose) plugin.DebugMessage("[ERROR] Invalid HP \"" + HPstr + "\" in " + filename + '!');
                        }
                        else
                        {
                            HP = HPaux;
                        }
                    }
                    else
                    {
                        if (verbose) plugin.DebugMessage("[ERROR] Unknown line: " + data + " in file " + filename);
                    }
                }
                name = name.Substring(0, name.Length - 4);
                Subclass subclass = new Subclass(name, role, inventory, IMinventory, probability, ammo, broadcast, HP);
                for (int i = 0; i < maxCount; i++) MTFplus.subclasses.Add(subclass);
                MTFplus.disctinctSubclasses.Add(subclass);

                if (verbose)
                {
                    plugin.DebugMessage("[INFO] Success! Loaded " + name + " as a new class" + (plugin.Configs.debug ? ":\n" + subclass.ToString() : string.Empty));
                    if (plugin.Configs.debug) plugin.DebugMessage(subclass.ToString());
                }
                SuccessfulCount++;
            }

            return SuccessfulCount;
        }
        #if ItemManager
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
        #endif

        internal static void DelayListMessage(this ReferenceHub player)
        {
            MEC.Timing.RunCoroutine(ListCoroutine(player), Segment.Update);
        }
        private static IEnumerator<float> ListCoroutine(ReferenceHub player)
        {
            yield return MEC.Timing.WaitForOneFrame;
            string message = string.Empty;

            int count = MTFplus.disctinctSubclasses.Count;
            if (MTFplus.Instance.Configs.userConsoleList == 1)
            {
                for (int i = 0; i < count; i++)
                {
                    message += MTFplus.disctinctSubclasses[i].name + (i != count - 1 ? ", " : string.Empty);
                }
            }
            else
            {
                // This is a very CPU intensive in term of frame times task.
                // This should never be a big deal if your server has a good
                // enough CPU, but we can't assume no-one will ever run it in
                // a Pentium from 2003.
                // Additionally: if this wasn't "optimized", this would
                // be a pretty easily exploitable command to crash
                // servers.

                // Let's try to optimize it doing the following approach:
                // - Compute a maximum of 3 subclasses each frame.
                // This will prevent an easily exploitable crash method and, 
                // in the user side, it will take around 10 frames to get
                // 30 classes. 10 frames are basically 166 miliseconds, while
                // human reaction times are around 200-300ms. And no-one has
                // 30 or more subclasses anyways.

                int computedInOneFrame = 0;
                for (int i = 0; i < count; i++, computedInOneFrame++)
                {
                    message += MTFplus.disctinctSubclasses[i].ToString() + Environment.NewLine + "------------" + Environment.NewLine;
                    
                    // If you computed 3 or more (unlikely since it's one unique
                    // process, but better be safe than sorry),
                    // wait one frame before continuing.
                    if(computedInOneFrame >= 3)
                    {
                        yield return MEC.Timing.WaitForOneFrame;
                        computedInOneFrame = 0;
                    }
                }

                // Nonetheless, please note:

                // A "smarter" approach would be saving the starting Unity's Time.time,
                // the target refresh rate (in this game is 60Hz) and basically
                // fetching the time after each iteration. If it goes above (0.5/60)
                // which would be "half a frame", then you wait one frame, and after
                // waiting the frame you save the new Time.time variable.

                // The only downside to this is that I don't personally know the inner
                // logic of Unity, and I don't know if the overhead of fetching
                // Time.time is worth to optimize it to its last bit.
            }
            player.GetComponent<GameConsoleTransmission>().SendToClient(player.scp079PlayerScript.connectionToClient, message, "white");
        }
    }
}
