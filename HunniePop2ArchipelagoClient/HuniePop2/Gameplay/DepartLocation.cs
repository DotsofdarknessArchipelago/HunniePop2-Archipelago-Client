using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
using HunniePop2ArchipelagoClient.Archipelago;
using HunniePop2ArchipelagoClient.HuniePop2.Girls;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace HunniePop2ArchipelagoClient.HuniePop2.Gameplay
{
    [HarmonyPatch]
    public class DepartLocation
    {

        /// <summary>
        /// stuff to do when moving locations
        /// - check/process the item recieved list
        /// - check that randomiser completion goal has been reached
        /// - overwirite baggage
        /// - overwrite finder slots with new generated finder slots
        /// </summary>
        [HarmonyPatch(typeof(LocationManager), "Depart")]
        [HarmonyPostfix]
        public static void processarch()
        {
            PlayerFile file = Game.Persistence.playerFile;
            //check goal completion for beating nymphojin
            if (file.storyProgress >= 12)
            {
                ArchipelagoClient.complete();
            }

            //overwite girls baggage since need to be able to give them all gifts without problem
            //possibly unessiary since its done on the read for girls data but just to make sure it here also
            for (int i = 0; i < file.girls.Count; i++)
            {

                //fill all baggages slots with temp baggage if not all slots have baggage in them
                int id = file.girls[i].girlDefinition.id;
                if (file.girls[i].girlDefinition.baggageItemDefs.Count != 3)
                {
                    List<ItemDefinition> newlist = new List<ItemDefinition>();
                    newlist.Add(Baggage.baggagestuff());
                    newlist.Add(Baggage.baggagestuff());
                    newlist.Add(Baggage.baggagestuff());
                    file.girls[i].girlDefinition.baggageItemDefs = newlist;
                }

                //check if the 1st baggage item has been obtained and put it in the 1st slot otherwise put a temp baggage instead
                if (ArchipelagoClient.alist.hasitem(69420189 + ((id - 1) * 3)) && file.girls[i].girlDefinition.baggageItemDefs[0] != Game.Data.Items.Get(((id - 1) * 3) + 93))
                {
                    file.girls[i].girlDefinition.baggageItemDefs[0] = Game.Data.Items.Get(((id - 1) * 3) + 93);
                }
                else if (file.girls[i].girlDefinition.baggageItemDefs[0] != Baggage.baggagestuff() && !ArchipelagoClient.alist.hasitem(69420189 + ((id - 1) * 3)))
                {
                    file.girls[i].girlDefinition.baggageItemDefs[0] = Baggage.baggagestuff();
                }

                //check if the 2nd baggage item has been obtained and put it in the 1st slot otherwise put a temp baggage instead
                if (ArchipelagoClient.alist.hasitem(69420190 + ((id - 1) * 3)) && file.girls[i].girlDefinition.baggageItemDefs[1] != Game.Data.Items.Get(((id - 1) * 3) + 94))
                {
                    file.girls[i].girlDefinition.baggageItemDefs[1] = Game.Data.Items.Get(((id - 1) * 3) + 94);
                }
                else if (file.girls[i].girlDefinition.baggageItemDefs[1] != Baggage.baggagestuff() && !ArchipelagoClient.alist.hasitem(69420190 + ((id - 1) * 3)))
                {
                    file.girls[i].girlDefinition.baggageItemDefs[1] = Baggage.baggagestuff();
                }

                //check if the 3rd baggage item has been obtained and put it in the 1st slot otherwise put a temp baggage instead
                if (ArchipelagoClient.alist.hasitem(69420191 + ((id - 1) * 3)) && file.girls[i].girlDefinition.baggageItemDefs[2] != Game.Data.Items.Get(((id - 1) * 3) + 95))
                {
                    file.girls[i].girlDefinition.baggageItemDefs[2] = Game.Data.Items.Get(((id - 1) * 3) + 95);
                }
                else if (file.girls[i].girlDefinition.baggageItemDefs[2] != Baggage.baggagestuff() && !ArchipelagoClient.alist.hasitem(69420191 + ((id - 1) * 3)))
                {
                    file.girls[i].girlDefinition.baggageItemDefs[2] = Baggage.baggagestuff();
                }
            }

            //check/process recieved item list
            if (ArchipelagoClient.alist.needprocessing())
            {
                archflagprocess(file);
            }

            //save current archipelago data to file
            ArchipelagoClient.session.DataStorage[Scope.Slot, "savefile"] = JsonConvert.SerializeObject(Game.Persistence.playerData.files[4].WriteData());
            ArchipelagoClient.session.DataStorage[Scope.Slot, "archdata"] = JsonConvert.SerializeObject(ArchipelagoClient.alist);

            //generate new finder and overwrite the finder slots with it since normal finder logic isnt that great when playing this
            //TODO overwite the finder UI so dont hae to do this?
            file.finderSlots = Finder.genfinder(file);

        }

        /// <summary>
        /// method to process recieved archipelago items
        /// </summary>
        public static void archflagprocess(PlayerFile file)
        {
            ArchipelagoConsole.LogMessage("<color=yellow>PROCESSING ITEMS</color>");
            //ArchipelagoConsole.LogMessage(ArchipelagoClient.itemstoprocess.Dequeue().ToString());
            //itterate over entire list
            for (int i = 0; i < ArchipelagoClient.alist.list.Count; i++)
            {
                //if the item has already been processed continue with next item
                if (ArchipelagoClient.alist.list[i].processed) { continue; }

                ArchipelagoConsole.debugLogMessage("PROCESSING ITEM ID: " + ArchipelagoClient.alist.list[i].Id + " FROM PLAYER: " + ArchipelagoClient.alist.list[i].PlayerName + " FROM LOC:" + ArchipelagoClient.alist.list[i].LocationId);

                //if item id is between 69420000 and 69420025 process item as Fairy Wings
                if (ArchipelagoClient.alist.list[i].Id > 69420000 && ArchipelagoClient.alist.list[i].Id < 69420025)
                {
                    //get girl pair based on the id-69420000 to get the girl id
                    GirlPairDefinition def = Game.Data.GirlPairs.Get((int)ArchipelagoClient.alist.list[i].Id - 69420000);
                    //add girl pair to list of completed girl pairs or if pair is already in the list output warning
                    if (!file.completedGirlPairs.Contains(def))
                    {
                        file.completedGirlPairs.Add(def);
                        ArchipelagoConsole.LogMessage("<color=green>" + def.name + " WING ITEM PROCESSED</color>");
                        ArchipelagoClient.alist.list[i].processed = true;
                    }
                    else
                    {
                        ArchipelagoConsole.LogMessage("<color=orange>" + def.name + " WING ITEM ALREADY PROCESSED</color>");
                        ArchipelagoClient.alist.list[i].processed = true;
                    }
                }
                //if item id is between 69420024 and 69420057 process item as Token Power Level Up
                else if (ArchipelagoClient.alist.list[i].Id > 69420024 && ArchipelagoClient.alist.list[i].Id < 69420057)
                {
                    //just say the item is prcessed as the token power up is handled elsewhere
                    ArchipelagoConsole.LogMessage("<color=green>TOKEN LV-UP PROCESSED</color>");
                    ArchipelagoClient.alist.list[i].processed = true;
                }
                //if item id is between 69420056 and 69420069 process item as Girl Unlock
                else if (ArchipelagoClient.alist.list[i].Id > 69420056 && ArchipelagoClient.alist.list[i].Id < 69420069)
                {
                    //unlock girl by setting playermet to true
                    GirlDefinition def = Game.Data.Girls.Get((int)ArchipelagoClient.alist.list[i].Id - 69420056);
                    file.GetPlayerFileGirl(def).playerMet = true;
                    ArchipelagoClient.alist.list[i].processed = true;
                    ArchipelagoConsole.LogMessage("<color=green>" + def.girlName + " IS UNLOCKED</color>");

                }
                //if item id is between 69420056 and 69420069 process item as Pair Unlock
                else if (ArchipelagoClient.alist.list[i].Id > 69420068 && ArchipelagoClient.alist.list[i].Id < 69420093)
                {
                    //unlock pair by setting RelationshipType from UNKNOWN to COMPATIABLE
                    GirlPairDefinition def = Game.Data.GirlPairs.Get((int)ArchipelagoClient.alist.list[i].Id - 69420068);
                    PlayerFileGirlPair pair = file.GetPlayerFileGirlPair(def);
                    if (pair.relationshipType == GirlPairRelationshipType.UNKNOWN)
                    {
                        pair.relationshipType = GirlPairRelationshipType.COMPATIBLE;
                        file.metGirlPairs.Add(def);
                        ArchipelagoConsole.LogMessage("<color=green>" + def.name + " UNLOCKED PAIR</color>");
                        ArchipelagoClient.alist.list[i].processed = true;
                    }
                    else
                    {
                        ArchipelagoConsole.LogMessage("<color=orange>" + def.name + " PAIR ALREADY PROCESSED</color>");
                        ArchipelagoClient.alist.list[i].processed = true;

                    }
                }
                //if item id is between 69420092 and 69420141 process item as Unique Gift Unlock
                else if (ArchipelagoClient.alist.list[i].Id > 69420092 && ArchipelagoClient.alist.list[i].Id < 69420141)
                {
                    //check if the players inventory is full if not add the relevient unique item
                    if (!file.IsInventoryFull())
                    {
                        ItemDefinition def = Game.Data.Items.Get(IDs.flagtoid((int)ArchipelagoClient.alist.list[i].Id));
                        file.AddInventoryItem(def);
                        ArchipelagoConsole.LogMessage("<color=green>" + def.itemName + " UNIQUE GIFT OBTAINED</color>");
                        ArchipelagoClient.alist.list[i].processed = true;
                    }
                    else
                    {
                        ArchipelagoConsole.LogMessage("<color=red>INVENTORY FULL COUDNT PROCESS ITEM</color>");
                    }
                }
                //if item id is between 69420140 and 69420189 process item as Shoe Gift Unlock
                else if (ArchipelagoClient.alist.list[i].Id > 69420140 && ArchipelagoClient.alist.list[i].Id < 69420189)
                {
                    //check if the players inventory is full if not add the relevient shoe item
                    if (!file.IsInventoryFull())
                    {
                        ItemDefinition def = Game.Data.Items.Get(IDs.flagtoid((int)ArchipelagoClient.alist.list[i].Id));
                        file.AddInventoryItem(def);
                        ArchipelagoConsole.LogMessage("<color=green>" + def.itemName + " SHOES GIFT PROCESSED</color>");
                        ArchipelagoClient.alist.list[i].processed = true;
                    }
                    else
                    {
                        ArchipelagoConsole.LogMessage("<color=red>INVENTORY FULL COUDNT PROCESS ITEM</color>");
                    }
                }
                //if item id is between 69420188 and 69420225 process item as Baggage Unlock
                else if (ArchipelagoClient.alist.list[i].Id > 69420188 && ArchipelagoClient.alist.list[i].Id < 69420225)
                {
                    //overwrite the custom baggage with the regular baggage
                    GirlDefinition def = Game.Data.Girls.Get((((int)ArchipelagoClient.alist.list[i].Id - 69420189) / 3) + 1);
                    ItemDefinition bagdef = Game.Data.Items.GetAllOfTypes(ItemType.BAGGAGE)[(int)ArchipelagoClient.alist.list[i].Id - 69420189];
                    file.GetPlayerFileGirl(def).girlDefinition.baggageItemDefs[(((int)ArchipelagoClient.alist.list[i].Id - 69420189) % 3)] = bagdef;
                    ArchipelagoConsole.LogMessage("<color=green>" + def.girlName + " OBTAINED NEW BAGGAGGE</color>");
                    ArchipelagoClient.alist.list[i].processed = true;

                }
                //if item id is between 69420224 and 69420345 process item as Outfit Unlock
                else if (ArchipelagoClient.alist.list[i].Id > 69420224 && ArchipelagoClient.alist.list[i].Id < 69420345)
                {
                    //add relevant outfit/style to the relivent girl
                    int u = (int)ArchipelagoClient.alist.list[i].Id - 69420225;
                    int girlid = (u / 10) + 1;
                    int styleid = u % 10;

                    GirlDefinition def = Game.Data.Girls.Get(girlid);
                    if (!file.GetPlayerFileGirl(Game.Data.Girls.Get(girlid)).unlockedOutfits.Contains(styleid))
                    {
                        ArchipelagoConsole.LogMessage("<color=green>OBTAINED " + file.GetPlayerFileGirl(Game.Data.Girls.Get(girlid)).girlDefinition.girlName + " OUTFIT #" + (styleid + 1) + "</color>");
                        file.GetPlayerFileGirl(Game.Data.Girls.Get(girlid)).unlockedOutfits.Add(styleid);
                        file.GetPlayerFileGirl(Game.Data.Girls.Get(girlid)).unlockedHairstyles.Add(styleid);
                    }
                    ArchipelagoClient.alist.list[i].processed = true;
                }
                //if item id is between 69420334 and 69420422 process item as Filler Items Unlock
                else if (ArchipelagoClient.alist.list[i].Id > 69420344 && ArchipelagoClient.alist.list[i].Id < 69420422)
                {
                    //if item id is 69420345 do nothing as its a nothing item
                    if ((int)ArchipelagoClient.alist.list[i].Id == 69420345)
                    {
                        //ArchipelagoConsole.LogMessage("nothing");
                        ArchipelagoConsole.LogMessage("<color=green>OBTAINED NOTHING</color>");
                        ArchipelagoClient.alist.list[i].processed = true;
                    }
                    //if item id is 69420421 and a random amount of token/seeds to the player
                    else if ((int)ArchipelagoClient.alist.list[i].Id == 69420421)
                    {
                        //ArchipelagoConsole.LogMessage("tokens");

                        int b = UnityEngine.Random.Range(0, 20);
                        int g = UnityEngine.Random.Range(0, 20);
                        int o = UnityEngine.Random.Range(0, 20);
                        int r = UnityEngine.Random.Range(0, 20);

                        file.AddFruitCount(PuzzleAffectionType.TALENT, b);
                        file.AddFruitCount(PuzzleAffectionType.FLIRTATION, g);
                        file.AddFruitCount(PuzzleAffectionType.ROMANCE, o);
                        file.AddFruitCount(PuzzleAffectionType.SEXUALITY, r);
                        ArchipelagoConsole.LogMessage("<color=green>OBTAINED SEEDS: </color><color=blue>" + b.ToString() + " Blue</color>, <color=green>" + g.ToString() + " Green</color>, <color=orange>" + o.ToString() + " Orange</color>, <color=red>" + r.ToString() + " Red</color>");
                        ArchipelagoClient.alist.list[i].processed = true;
                    }
                    //otherwise add the relivent item to the players inventory if not full
                    else
                    {
                        if (!file.IsInventoryFull())
                        {
                            ArchipelagoConsole.LogMessage(Game.Data.Items.Get(IDs.itemflagtoid((int)ArchipelagoClient.alist.list[i].Id)).name);
                            //file.AddInventoryItem(Game.Data.Items.Get(Util.itemflagtoid((int)ArchipelagoClient.alist.list[i].item.ItemId)));

                            for (int k = 0; k < 35; k++)
                            {
                                PlayerFileInventorySlot playerFileInventorySlot = file.GetPlayerFileInventorySlot(k);
                                if (playerFileInventorySlot.itemDefinition == null)
                                {
                                    playerFileInventorySlot.itemDefinition = Game.Data.Items.Get(IDs.itemflagtoid((int)ArchipelagoClient.alist.list[i].Id));
                                    playerFileInventorySlot.daytimeStamp = 0;
                                    break;
                                }
                            }

                            ArchipelagoConsole.LogMessage("<color=green>OBTAINED " + Game.Data.Items.Get(IDs.itemflagtoid((int)ArchipelagoClient.alist.list[i].Id)).name + " ITEM</color>");
                            ArchipelagoClient.alist.list[i].processed = true;
                        }
                        else
                        {
                            ArchipelagoConsole.LogMessage("<color=red>INVENTORY FULL COUDNT PROCESS ITEM</color>");
                        }
                    }
                }

            }
        }
    }
}