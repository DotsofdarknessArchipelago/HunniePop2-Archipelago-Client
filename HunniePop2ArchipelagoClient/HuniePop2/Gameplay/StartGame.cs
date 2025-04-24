using HarmonyLib;
using HunniePop2ArchipelagoClient.Archipelago;
using HunniePop2ArchipelagoClient.HuniePop2.Girls;
using HunniePop2ArchipelagoClient.HuniePop2.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HunniePop2ArchipelagoClient.HuniePop2.Gameplay
{
    [HarmonyPatch]
    public class StartGame
    {

        /// <summary>
        /// Patch to Stop players continuing a game if not connected to an archipelago server
        /// </summary>
        /// <returns>RETURNS FALSE TO SKIP ORIGINAL METHOD</returns>
        [HarmonyPatch(typeof(UiCellphoneAppContinue), "OnSaveFileButtonPressed")]
        [HarmonyPrefix]
        public static bool continueoverite(UiAppSaveFile saveFile, UiCellphoneAppContinue __instance, ref UiTitleCanvas ____titleCanvas)
        {
            //just return if the ui is in erase mode
            if (saveFile.eraseMode)
            {
                return false;
            }

            //return and put error message if save file selected is not the bottom right file
            if (saveFile.saveFileIndex != 3 && saveFile.playerFile.started)
            {
                CellPhoneError.cellerror("CANNOT PLAY NON ARCHIPELAGO SAVE FILE", __instance.cellphone);
                return false;
            }

            //only contine a game if its started
            if (saveFile.playerFile.started)
            {
                //only continue a game if you are connected to a archipelago server
                if (ArchipelagoClient.Authenticated)
                {
                    //check if the seed saved is the correct one on the connected server
                    if (saveFile.playerFile.GetFlagValue(ArchipelagoClient.seed()) == -1)
                    {
                        CellPhoneError.cellerror("GAME FILE NOT COMPATIBLE WITH CONNECTED SERVER", __instance.cellphone);
                    }
                    else
                    {
                        //get player file for processing items
                        PlayerFile playerFile = Game.Persistence.playerData.files[3];

                        // process recieved items
                        DepartLocation.archflagprocess(playerFile);

                        //overwrite baggage here with either temp one or the normal baggage if unlocked
                        for (int g = 1; g < 13; g++)
                        {
                            PlayerFileGirl gl = playerFile.GetPlayerFileGirl(Game.Data.Girls.Get(g));
                            int bflag = ((g - 1) * 3) + 69420189;
                            List<ItemDefinition> deflist = new List<ItemDefinition>();
                            if (ArchipelagoClient.alist.hasitem(bflag)) { deflist.Add(gl.girlDefinition.baggageItemDefs[0]); }
                            else { deflist.Add(Baggage.baggagestuff()); }
                            if (ArchipelagoClient.alist.hasitem(bflag + 1)) { deflist.Add(gl.girlDefinition.baggageItemDefs[1]); }
                            else { deflist.Add(Baggage.baggagestuff()); }
                            if (ArchipelagoClient.alist.hasitem(bflag + 2)) { deflist.Add(gl.girlDefinition.baggageItemDefs[2]); }
                            else { deflist.Add(Baggage.baggagestuff()); }
                        }

                        //save and apply any changes
                        Game.Persistence.Apply(3);
                        Game.Persistence.SaveGame();

                        //save arch data
                        using (StreamWriter archfile = File.CreateText(Application.persistentDataPath + "/archdata"))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            serializer.Serialize(archfile, ArchipelagoClient.alist);
                        }

                        //load into the game
                        ____titleCanvas.LoadGame(3, "MainScene");
                    }
                }
                else
                {
                    CellPhoneError.cellerror("PLEASE CONNECT TO A ARCHIPELAGO SERVER IN TOP LEFT CORNER", __instance.cellphone);
                }

            }
            return false;
        }

        /// <summary>
        /// Patch that sets up the save file when starting a new game
        /// </summary>
        /// <returns>RETURNS FALSE TO SKIP ORIGINAL METHOD</returns>
        [HarmonyPatch(typeof(UiCellphoneAppNew), "OnStartButtonPressed")]
        [HarmonyPrefix]
        public static bool Newsavefileoverite(UiCellphoneAppNew __instance, ref UiTitleCanvas ____titleCanvas, ref int ____newSaveFileIndex, ref UiAppFileIconSlot ____selectedFileIconSlot)
        {
            //make sure we are connected to an archipelago server and that the game slot hasnt already been started
            if (ArchipelagoClient.Authenticated && !Game.Persistence.playerData.files[3].started)
            {
                // we only use the bottom left slot(index 3) at the moment
                int savindex = 3;

                //get player file and reset the data in it just in case
                PlayerFile playerFilea = Game.Persistence.playerData.files[savindex];
                playerFilea = new PlayerFile(new SaveFile());
                Game.Persistence.Apply(savindex);
                Game.Persistence.SaveGame();

                //probally dont have to reget the player file but it works
                PlayerFile playerFile = Game.Persistence.playerData.files[savindex];

                //set the setting for the game as chosen by the player
                playerFile.started = true;
                playerFile.fileIconGirlDefinition = ____selectedFileIconSlot.girlDefinition;
                playerFile.settingGender = (SettingGender)__instance.settingSelectorGender.selectedIndex;
                playerFile.settingDifficulty = (SettingDifficulty)__instance.settingSelectorDifficulty.selectedIndex;
                playerFile.SetFlagValue("pollys_junk", __instance.settingSelectorPolly.selectedIndex);

                //skip the tutorial and first_location_id to 0 since it was set in a new game after the tutorial
                playerFile.SetFlagValue("first_location_id", 0);
                playerFile.SetFlagValue("skip_tutorial", 1);

                //set the starting number of fruit seeds based on the randomiser
                playerFile.fruitCounts[0] = Convert.ToInt32(ArchipelagoClient.ServerData.slotData["number_blue_seed"]);
                playerFile.fruitCounts[1] = Convert.ToInt32(ArchipelagoClient.ServerData.slotData["number_green_seed"]);
                playerFile.fruitCounts[2] = Convert.ToInt32(ArchipelagoClient.ServerData.slotData["number_orange_seed"]);
                playerFile.fruitCounts[3] = Convert.ToInt32(ArchipelagoClient.ServerData.slotData["number_red_seed"]);

                //save the randomiser seed as a flag for use when checking a continued game
                playerFile.SetFlagValue(ArchipelagoClient.seed(), 1);

                //set various flags for easy retreval later
                //if questions are locations
                if (ArchipelagoClient.ServerData.slotData["enable_questions"].ToString() == 1.ToString())
                {
                    playerFile.SetFlagValue("questions_skiped", 0);
                }
                else
                {
                    playerFile.SetFlagValue("questions_skiped", 1);
                }

                //if the normal baggage items are in the randomiser
                if (ArchipelagoClient.ServerData.slotData["disable_baggage"].ToString() == 1.ToString())
                {
                    playerFile.SetFlagValue("baggage_disabled", 1);
                }
                else
                {
                    playerFile.SetFlagValue("baggage_disabled", 0);
                }

                //if yuo need to have sex with all pairs instead of collecting all the wings
                if (ArchipelagoClient.ServerData.slotData["lovers_instead_wings"].ToString() == 1.ToString())
                {
                    playerFile.SetFlagValue("loversinsteadwings", 1);
                }
                else
                {
                    playerFile.SetFlagValue("loversinsteadwings", 0);
                }

                //various values for puzzles
                playerFile.SetFlagValue("affection base", Convert.ToInt32(ArchipelagoClient.ServerData.slotData["affection_start"]));
                playerFile.SetFlagValue("affection addition", Convert.ToInt32(ArchipelagoClient.ServerData.slotData["affection_add"]));
                playerFile.SetFlagValue("affection boss", Convert.ToInt32(ArchipelagoClient.ServerData.slotData["boss_affection"]));
                playerFile.SetFlagValue("starting moves", Convert.ToInt32(ArchipelagoClient.ServerData.slotData["start_moves"]));
                
                //if locations in shops show what type of item they contain
                playerFile.SetFlagValue("disableshopslots", Convert.ToInt32(ArchipelagoClient.ServerData.slotData["hide_shop_item_details"]));

                //check if a girl is disabled in the randomiser and add all their pairs to the completed pairs list
                if (ArchipelagoClient.ServerData.slotData["lola"].ToString() == 1.ToString())
                {
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(1).girlPairDefs[1])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(1).girlPairDefs[1]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(1).girlPairDefs[2])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(1).girlPairDefs[2]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(1).girlPairDefs[3])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(1).girlPairDefs[3]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(1).girlPairDefs[0])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(1).girlPairDefs[0]); }
                }
                if (ArchipelagoClient.ServerData.slotData["jessie"].ToString() == 1.ToString())
                {
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(2).girlPairDefs[1])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(2).girlPairDefs[1]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(2).girlPairDefs[2])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(2).girlPairDefs[2]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(2).girlPairDefs[3])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(2).girlPairDefs[3]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(2).girlPairDefs[0])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(2).girlPairDefs[0]); }
                }
                if (ArchipelagoClient.ServerData.slotData["lillian"].ToString() == 1.ToString())
                {
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(3).girlPairDefs[1])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(3).girlPairDefs[1]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(3).girlPairDefs[2])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(3).girlPairDefs[2]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(3).girlPairDefs[3])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(3).girlPairDefs[3]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(3).girlPairDefs[0])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(3).girlPairDefs[0]); }
                }
                if (ArchipelagoClient.ServerData.slotData["zoey"].ToString() == 1.ToString())
                {
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(4).girlPairDefs[1])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(4).girlPairDefs[1]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(4).girlPairDefs[2])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(4).girlPairDefs[2]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(4).girlPairDefs[3])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(4).girlPairDefs[3]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(4).girlPairDefs[0])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(4).girlPairDefs[0]); }
                }
                if (ArchipelagoClient.ServerData.slotData["sarah"].ToString() == 1.ToString())
                {
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(5).girlPairDefs[1])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(5).girlPairDefs[1]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(5).girlPairDefs[2])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(5).girlPairDefs[2]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(5).girlPairDefs[3])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(5).girlPairDefs[3]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(5).girlPairDefs[0])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(5).girlPairDefs[0]); }
                }
                if (ArchipelagoClient.ServerData.slotData["lailani"].ToString() == 1.ToString())
                {
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(6).girlPairDefs[1])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(6).girlPairDefs[1]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(6).girlPairDefs[2])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(6).girlPairDefs[2]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(6).girlPairDefs[3])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(6).girlPairDefs[3]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(6).girlPairDefs[0])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(6).girlPairDefs[0]); }
                }
                if (ArchipelagoClient.ServerData.slotData["candace"].ToString() == 1.ToString())
                {
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(7).girlPairDefs[1])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(7).girlPairDefs[1]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(7).girlPairDefs[2])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(7).girlPairDefs[2]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(7).girlPairDefs[3])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(7).girlPairDefs[3]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(7).girlPairDefs[0])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(7).girlPairDefs[0]); }
                }
                if (ArchipelagoClient.ServerData.slotData["nora"].ToString() == 1.ToString())
                {
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(8).girlPairDefs[1])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(8).girlPairDefs[1]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(8).girlPairDefs[2])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(8).girlPairDefs[2]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(8).girlPairDefs[3])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(8).girlPairDefs[3]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(8).girlPairDefs[0])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(8).girlPairDefs[0]); }
                }
                if (ArchipelagoClient.ServerData.slotData["brooke"].ToString() == 1.ToString())
                {
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(9).girlPairDefs[1])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(9).girlPairDefs[1]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(9).girlPairDefs[2])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(9).girlPairDefs[2]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(9).girlPairDefs[3])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(9).girlPairDefs[3]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(9).girlPairDefs[0])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(9).girlPairDefs[0]); }
                }
                if (ArchipelagoClient.ServerData.slotData["ashley"].ToString() == 1.ToString())
                {
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(10).girlPairDefs[1])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(10).girlPairDefs[1]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(10).girlPairDefs[2])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(10).girlPairDefs[2]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(10).girlPairDefs[3])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(10).girlPairDefs[3]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(10).girlPairDefs[0])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(10).girlPairDefs[0]); }
                }
                if (ArchipelagoClient.ServerData.slotData["abia"].ToString() == 1.ToString())
                {
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(11).girlPairDefs[1])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(11).girlPairDefs[1]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(11).girlPairDefs[2])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(11).girlPairDefs[2]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(11).girlPairDefs[3])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(11).girlPairDefs[3]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(11).girlPairDefs[0])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(11).girlPairDefs[0]); }
                }
                if (ArchipelagoClient.ServerData.slotData["polly"].ToString() == 1.ToString())
                {
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(12).girlPairDefs[1])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(12).girlPairDefs[1]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(12).girlPairDefs[2])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(12).girlPairDefs[2]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(12).girlPairDefs[3])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(12).girlPairDefs[3]); }
                    if (!playerFile.completedGirlPairs.Contains(Game.Data.Girls.Get(12).girlPairDefs[0])) { playerFile.completedGirlPairs.Add(Game.Data.Girls.Get(12).girlPairDefs[0]); }
                }

                //check if the randomiser has locations in the shop and add flags for each item
                if (Convert.ToInt32(ArchipelagoClient.ServerData.slotData["number_shop_items"]) > 0)
                {
                    playerFile.SetFlagValue("shopslots", Convert.ToInt32(ArchipelagoClient.ServerData.slotData["number_shop_items"]));
                    for (int s = 0; s < Convert.ToInt32(ArchipelagoClient.ServerData.slotData["number_shop_items"]); s++)
                    {
                        playerFile.SetFlagValue("shopslot" + s.ToString(), 0);
                    }
                }

                //process any items recieved by the client
                DepartLocation.archflagprocess(playerFile);

                //set the selected girl in the wardrobe to be the first girl that is unlocked
                for (int p = 1; p < playerFile.girls.Count; p++)
                {
                    if (playerFile.girls[p].playerMet)
                    {
                        playerFile.SetFlagValue("wardrobe_girl_id", playerFile.girls[p].girlDefinition.id);
                        break;
                    }
                }

                //overwirite girls baggage with either a temp one or their normal one based on if the player has it unlocked
                //also set all girls outfits to be what you select in the wardrobe to prevent any out of logic stuff
                for (int g = 1; g < 13; g++)
                {
                    PlayerFileGirl gl = playerFile.GetPlayerFileGirl(Game.Data.Girls.Get(g));
                    int bflag = ((g - 1) * 3) + 69420189;
                    List<ItemDefinition> deflist = new List<ItemDefinition>();
                    if (ArchipelagoClient.alist.hasitem(bflag)) { deflist.Add(gl.girlDefinition.baggageItemDefs[0]); }
                    else { deflist.Add(Baggage.baggagestuff()); }
                    if (ArchipelagoClient.alist.hasitem(bflag + 1)) { deflist.Add(gl.girlDefinition.baggageItemDefs[1]); }
                    else { deflist.Add(Baggage.baggagestuff()); }
                    if (ArchipelagoClient.alist.hasitem(bflag + 2)) { deflist.Add(gl.girlDefinition.baggageItemDefs[2]); }
                    else { deflist.Add(Baggage.baggagestuff()); }

                    gl.girlDefinition.baggageItemDefs = deflist;

                    gl.stylesOnDates = true;
                }

                //generate finder and shop slots/items based on our logic
                playerFile.finderSlots = Finder.genfinder(playerFile);
                playerFile.storeProducts = Store.genStore(playerFile);

                //set some default values that are based on a game just after the tutorial
                playerFile.storyProgress = 7;
                playerFile.daytimeElapsed = 10;
                playerFile.finderRestockTime = 10;
                playerFile.storeRestockDay = 2;

                //set the player location to be the hotel
                playerFile.locationDefinition = Game.Data.Locations.Get(21);

                //save everything
                Game.Persistence.Apply(savindex);
                Game.Persistence.SaveGame();

                //save archdata
                using (StreamWriter archfile = File.CreateText(Application.persistentDataPath + "/archdata"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(archfile, ArchipelagoClient.alist);
                }

                //load into the game finally
                ____titleCanvas.LoadGame(savindex, "MainScene");
            }
            if (Game.Persistence.playerData.files[3].started)
            {
                ///error to make players erase the game so no corruptoion occours
                CellPhoneError.cellerror("PLEASE ERASE THE BOTTOM RIGHT SAVE FILE TO START A GAME", __instance.cellphone);
            }
            else if (!ArchipelagoClient.Authenticated)
            {
                //error in case player hasnt connected to a archipelago server
                CellPhoneError.cellerror("PLEASE CONNECT TO ARCHIPELAGO SERVER(Top Right) TO START GAME", __instance.cellphone);
            }
            return false;
        }
    }
}