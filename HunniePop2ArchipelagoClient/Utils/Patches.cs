using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using HarmonyLib;
using HunniePop2ArchipelagoClient.Archipelago;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;


namespace HunniePop2ArchipelagoClient.Utils
{
    public class Patches
    {

        public static ArchipelagoClient arch;
        public static void patch(ArchipelagoClient a)
        {
            arch = a;
            Harmony.CreateAndPatchAll(typeof(Patches));

        }

        /// <summary>
        /// DEBUG PATCH TO MAKE PUZZLES COMPLETE IN 1 MOVE
        /// </summary>
        [HarmonyPatch(typeof(PuzzleStatus), "AddPuzzleReward")]
        [HarmonyPrefix]
        public static void puzzleautocomplete(PuzzleStatus __instance)
        {
            //if (__instance.bonusRound) { return; }
            //__instance.AddResourceValue(PuzzleResourceType.AFFECTION, 100000, false);
        }

        [HarmonyPatch(typeof(UiTitleCanvas), "OnInitialAnimationComplete")]
        [HarmonyPostfix]
        public static void background()
        {
            Application.runInBackground = true;
        }

        /// <summary>
        /// PATCH TO STOP PLAYERS CONTINUING NON ARCHIPELAGO GAME BASED ON THE WORLD SEED
        /// </summary>
        /// <returns>RETURNS FALSE TO SKIP ORIGINAL METHOD</returns>
        [HarmonyPatch(typeof(UiCellphoneAppContinue), "OnSaveFileButtonPressed")]
        [HarmonyPrefix]
        public static bool continueoverite(UiAppSaveFile saveFile, UiCellphoneAppContinue __instance, ref UiTitleCanvas ____titleCanvas)
        {
            if (saveFile.eraseMode)
            {
                return false;
            }
            if (saveFile.saveFileIndex != 3 && saveFile.playerFile.started)
            {
                Util.cellerror("CANNOT PLAY NON ARCHIPELAGO SAVE FILE", __instance.cellphone);
                return false;
            }
            if (saveFile.playerFile.started)
            {
                if (ArchipelagoClient.Authenticated)
                {
                    if (saveFile.playerFile.GetFlagValue(ArchipelagoClient.seed()) == -1)
                    {
                        Util.cellerror("GAME FILE NOT COMPATIBLE WITH CONNECTED SERVER", __instance.cellphone);
                    }
                    else
                    {
                        PlayerFile playerFile = Game.Persistence.playerData.files[3];

                        Util.archflagprocess(playerFile);


                        Game.Persistence.Apply(3);
                        Game.Persistence.SaveGame();

                        ____titleCanvas.LoadGame(3, "MainScene");
                    }
                }
                else
                {
                    Util.cellerror("PLEASE CONNECT TO A ARCHIPELAGO SERVER IN TOP LEFT CORNER", __instance.cellphone);
                }

            }
            return false;
        }

        /// <summary>
        /// PATCH TO SETUP A NEW GAME AND LOAD INTO IT
        /// </summary>
        /// <returns>RETURNS FALSE TO SKIP ORIGINAL METHOD</returns>
        [HarmonyPatch(typeof(UiCellphoneAppNew), "OnStartButtonPressed")]
        [HarmonyPrefix]
        public static bool Newsavefileoverite(UiCellphoneAppNew __instance, ref UiTitleCanvas ____titleCanvas, ref int ____newSaveFileIndex, ref UiAppFileIconSlot ____selectedFileIconSlot)
        {
            if (ArchipelagoClient.Authenticated && !Game.Persistence.playerData.files[3].started)
            {
                int savindex = 3;
                PlayerFile playerFilea = Game.Persistence.playerData.files[savindex];
                playerFilea = new PlayerFile(new SaveFile());
                Game.Persistence.Apply(savindex);
                Game.Persistence.SaveGame();

                PlayerFile playerFile = Game.Persistence.playerData.files[savindex];

                playerFile.started = true;
                playerFile.fileIconGirlDefinition = ____selectedFileIconSlot.girlDefinition;
                playerFile.settingGender = (SettingGender)__instance.settingSelectorGender.selectedIndex;
                playerFile.settingDifficulty = (SettingDifficulty)__instance.settingSelectorDifficulty.selectedIndex; 
                playerFile.SetFlagValue("pollys_junk", __instance.settingSelectorPolly.selectedIndex);
                playerFile.SetFlagValue("first_location_id", 0);
                playerFile.SetFlagValue("skip_tutorial", 1);


                playerFile.fruitCounts[0] = Convert.ToInt32(ArchipelagoClient.ServerData.slotData["number_blue_seed"]);
                playerFile.fruitCounts[1] = Convert.ToInt32(ArchipelagoClient.ServerData.slotData["number_green_seed"]);
                playerFile.fruitCounts[2] = Convert.ToInt32(ArchipelagoClient.ServerData.slotData["number_orange_seed"]);
                playerFile.fruitCounts[3] = Convert.ToInt32(ArchipelagoClient.ServerData.slotData["number_red_seed"]);

                playerFile.SetFlagValue(ArchipelagoClient.seed(), 1);

                if (ArchipelagoClient.ServerData.slotData["enable_questions"].ToString() == 1.ToString())
                {
                    playerFile.SetFlagValue("questions_skiped", 0);
                }
                else
                {
                    playerFile.SetFlagValue("questions_skiped", 1);
                }

                if (ArchipelagoClient.ServerData.slotData["disable_baggage"].ToString() == 1.ToString())
                {
                    playerFile.SetFlagValue("baggage_disabled", 1);
                }
                else
                {
                    playerFile.SetFlagValue("baggage_disabled", 0);
                }
                if (ArchipelagoClient.ServerData.slotData["lovers_instead_wings"].ToString() == 1.ToString())
                {
                    playerFile.SetFlagValue("loversinsteadwings", 1);
                }
                else
                {
                    playerFile.SetFlagValue("loversinsteadwings", 0);
                }

                //for (int k = 69420346; k < 69420421; k++)
                //{
                //    playerFile.SetFlagValue(k.ToString(), 0);
                //}

                playerFile.SetFlagValue("affection base", Convert.ToInt32(ArchipelagoClient.ServerData.slotData["affection_start"]));
                playerFile.SetFlagValue("affection addition", Convert.ToInt32(ArchipelagoClient.ServerData.slotData["affection_add"]));
                playerFile.SetFlagValue("affection boss", Convert.ToInt32(ArchipelagoClient.ServerData.slotData["boss_affection"]));
                playerFile.SetFlagValue("starting moves", Convert.ToInt32(ArchipelagoClient.ServerData.slotData["start_moves"]));

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



                if (Convert.ToInt32(ArchipelagoClient.ServerData.slotData["number_shop_items"]) > 0)
                {
                    for (int s=0; s < Convert.ToInt32(ArchipelagoClient.ServerData.slotData["number_shop_items"]); s++)
                    {
                        playerFile.SetFlagValue("shopslot" + s.ToString(), 0);
                    }
                }

                for (int p = 1; p < 25; p++)
                {
                    playerFile.GetPlayerFileGirlPair(Game.Data.GirlPairs.Get(p));
                }
                /*
                int i;
                bool b = true;
                while (b){
                    if (ArchipelagoClient.itemstoprocess.Count > 0)
                    {
                        int flagi = (int)ArchipelagoClient.itemstoprocess.Dequeue();
                        string flag = flagi.ToString();
                        //ArchipelagoConsole.LogMessage("sdjgfjsdfsj");
                        //if (flagi >= 69420345 && flagi <= 69420421)
                        //{
                        //    ArchipelagoConsole.LogMessage(playerFile.GetFlagValue(flag).ToString());
                        //    if (playerFile.GetFlagValue(flag) == -1) { playerFile.SetFlagValue(flag, 0); }
                        //    playerFile.SetFlagValue(flag, (playerFile.GetFlagValue(flag) + 1));
                        //    ArchipelagoConsole.LogMessage(playerFile.GetFlagValue(flag).ToString());
                        //}
                        //else
                        //{
                        //    if (playerFile.GetFlagValue(flag) == -1)
                        //    {
                        //        playerFile.SetFlagValue(flag, 0);
                        //    }
                        //}
                        if (playerFile.GetFlagValue(flag) == -1)
                        {
                            playerFile.SetFlagValue(flag, 0);
                        }



                    }
                    else
                    {
                        b = false;
                    }
                }*/

                Util.archflagprocess(playerFile);

                for (int p = 1; p < playerFile.girls.Count; p++)
                {
                    if (playerFile.girls[p].playerMet)
                    {
                        playerFile.SetFlagValue("wardrobe_girl_id", playerFile.girls[p].girlDefinition.id);
                        break;
                    }
                }

                for (int g = 1; g < 13; g++)
                {
                    PlayerFileGirl gl = playerFile.GetPlayerFileGirl(Game.Data.Girls.Get(g));
                    int bflag = ((g - 1) * 3) + 69420189;
                    List<ItemDefinition> deflist = new List<ItemDefinition>();
                    if (ArchipelagoClient.alist.hasitem(bflag)) { deflist.Add(gl.girlDefinition.baggageItemDefs[0]); }
                    else { deflist.Add(Util.baggagestuff()); }
                    if (ArchipelagoClient.alist.hasitem(bflag+1)) { deflist.Add(gl.girlDefinition.baggageItemDefs[1]); }
                    else { deflist.Add(Util.baggagestuff()); }
                    if (ArchipelagoClient.alist.hasitem(bflag+2)) { deflist.Add(gl.girlDefinition.baggageItemDefs[2]); }
                    else { deflist.Add(Util.baggagestuff()); }

                    gl.girlDefinition.baggageItemDefs = deflist;

                    gl.stylesOnDates = true;
                }

                playerFile.finderSlots = Util.genfinder(playerFile);
                playerFile.storeProducts = Util.genStore(playerFile);

                playerFile.storyProgress = 7;
                playerFile.daytimeElapsed = 10;
                playerFile.finderRestockTime = 10;
                playerFile.storeRestockDay = 2;

                playerFile.locationDefinition = Game.Data.Locations.Get(21);

                Game.Persistence.Apply(savindex);
                Game.Persistence.SaveGame();

                ____titleCanvas.LoadGame(savindex, "MainScene");
            }
            if (Game.Persistence.playerData.files[3].started)
            {
                ///MAKE PLAYERS ERASE THE GAME SO NO CORRUPTION OCCURS 
                Util.cellerror("PLEASE ERASE THE BOTTOM RIGHT SAVE FILE TO START A GAME", __instance.cellphone);
            }
            else if (!ArchipelagoClient.Authenticated)
            {
                Util.cellerror("PLEASE CONNECT TO ARCHIPELAGO SERVER(Top Right) TO START GAME", __instance.cellphone);
            }
            return false;
        }
        /*
        [HarmonyPatch(typeof(PlayerFile), "ReadData")]
        [HarmonyPostfix]
        public static void loadoverite(PlayerFile __instance)
        {
            if (!ArchipelagoClient.Authenticated) { return; }
            for (int g = 1; g < 13; g++)
            {
                PlayerFileGirl gl = __instance.GetPlayerFileGirl(Game.Data.Girls.Get(g));
                int bflag = ((g - 1) * 3) + 69420189;
                List<ItemDefinition> deflist = new List<ItemDefinition>();
                if (ArchipelagoClient.alist.hasitem(bflag)) { deflist[0] = gl.girlDefinition.baggageItemDefs[0]; }
                else { deflist[0] = Util.baggagestuff(); }
                if (ArchipelagoClient.alist.hasitem(bflag + 1)) { deflist[1] = gl.girlDefinition.baggageItemDefs[1]; }
                else { deflist[1] = Util.baggagestuff(); }
                if (ArchipelagoClient.alist.hasitem(bflag + 2)) { deflist[2] = gl.girlDefinition.baggageItemDefs[2]; }
                else { deflist[2] = Util.baggagestuff(); }

                gl.girlDefinition.baggageItemDefs = deflist;

            }
        }*/

        /// <summary>
        /// OVERIDE THE STORE POPULATION AND POPULATE IT WITH WHAT WE WANT
        /// </summary>
        /// <returns>RETURNS FALSE TO SKIP ORIGINAL METHOD</returns>
        [HarmonyPatch(typeof(PlayerFile), "PopulateStoreProducts")]
        [HarmonyPrefix]
        public static bool shoppopulate(PlayerFile __instance)
        {
            List<int> pairints = new List<int>();

            for (int i = 0; i < __instance.metGirlPairs.Count; i++) { pairints.Add(__instance.metGirlPairs[i].id); }

            __instance.storeProducts = Util.genStore(__instance);

            return false;
        }



        /// <summary>
        /// PROCESS THE ITEMS IN THE ARCH QUEUE AND SAVE THEM TO FLAGS
        /// </summary>
        [HarmonyPatch(typeof(LocationManager), "Depart")]
        [HarmonyPostfix]
        public static void processarch()
        {
            PlayerFile file = Game.Persistence.playerFile;
            if (file.storyProgress >= 12)
            {
                ArchipelagoClient.complete();
            }

            if (ArchipelagoClient.alist.needprocessing())
            {
                Util.archflagprocess(file);
            }

            using (StreamWriter archfile = File.CreateText(Application.persistentDataPath + "/archdata"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(archfile, ArchipelagoClient.alist);
            }

        }

        /// <summary>
        /// DEBUG PATCH TO ADD CODES THAT DO STUFF
        /// </summary>
        [HarmonyPatch(typeof(UiCellphoneAppCode), "OnSubmitButtonPressed")]
        [HarmonyPrefix]
        public static void codestuff(UiCellphoneAppCode __instance)
        {
            string input = __instance.inputField.text.ToUpper().Trim();
            if (input == "TEST")
            {
                List<ItemDefinition> list = Game.Data.Items.GetAll();
                for (int i = 0; i < list.Count ; i++)
                {
                    ArchipelagoConsole.LogMessage(list[i].id.ToString());
                }
                //ArchipelagoConsole.LogMessage("ID:"+ j.ToString()+" : "+ Game.Data.Items.GetAllOfTypes(ItemType.BAGGAGE)[j].itemName);


            }
            if (input == "RESETITEMS")
            {
                if (!ArchipelagoClient.Authenticated)
                {
                    if (File.Exists(Application.persistentDataPath + "/archdata"))
                    {
                        File.Delete(Application.persistentDataPath + "/archdata");
                    }
                }
                else
                {
                    ArchipelagoClient.resetlist();
                }
            }

            if (input == "DEBUGDATA")
            {
                ArchipelagoConsole.LogMessage("-------DEBUG DATA-------");
                ArchipelagoConsole.LogMessage("List size: " + ArchipelagoClient.alist.list.Count.ToString());
                ArchipelagoConsole.LogMessage("------------------------");
                for (int i = 0; i < ArchipelagoClient.alist.list.Count; i++)
                {
                    ArchipelagoConsole.LogMessage("#" + i.ToString());
                    ArchipelagoConsole.LogMessage("ID:" + ArchipelagoClient.alist.list[i].item.ItemId.ToString());
                    ArchipelagoConsole.LogMessage("NAME:" + ArchipelagoClient.alist.list[i].item.ItemName);
                    ArchipelagoConsole.LogMessage("PROCESSED:" + ArchipelagoClient.alist.list[i].processed.ToString());
                    ArchipelagoConsole.LogMessage("PUTINSHOP:" + ArchipelagoClient.alist.list[i].putinshop.ToString());
                    ArchipelagoConsole.LogMessage("------------------------");
                }
                ArchipelagoConsole.LogMessage("-----END DEBUG DATA-----");
            }
        }

        /// <summary>
        /// OVERRIDE THE BAGGAGE CHECK TO RETURN TRUE FOR THE TEMP BAGGAGE AS WELL AS THE NORMAL GIRLS BAGGAGE
        /// </summary>
        /// <returns>RETURNS FALSE TO SKIP ORIGINAL METHOD</returns>
        [HarmonyPatch(typeof(PlayerFileGirl), "HasBaggage")]
        [HarmonyPrefix]
        public static bool hasbaggageoverite(ItemDefinition baggageDef, PlayerFileGirl __instance, ref bool __result)
        {
            __result = (Game.Data.Girls.Get(__instance.girlDefinition.id).baggageItemDefs.Contains(baggageDef) || (baggageDef == Util.baggagestuff()));
            return false;
        }

        /// <summary>
        /// MAKES SURE THAT WHEN A SAVE FILE IS RESET THAT ALL BAGGAGE IS UNLOCKED
        /// </summary>
        [HarmonyPatch(typeof(SaveFileGirl), "Reset")]
        [HarmonyPostfix]
        public static void savfilegirlresetmod(SaveFileGirl __instance)
        {
            __instance.activeBaggageIndex = UnityEngine.Random.Range(0, 2);
            __instance.learnedBaggage.Add(0);
            __instance.learnedBaggage.Add(1);
            __instance.learnedBaggage.Add(2);
        }

        /// <summary>
        /// ALLOWS FOR GIFT ITEM TO BE TRASHED NOT BE COMPLETELY LOST BE PUT IN STORE
        /// </summary>
        [HarmonyPatch(typeof(UiCellphoneTrashZone), "OnDrop")]
        [HarmonyPrefix]
        public static void uniquetrash(Draggable draggable, UiCellphoneTrashZone __instance)
        {
            //ArchipelagoConsole.LogMessage("ITEM TRASHED");
            if (draggable.type == DraggableType.INVENTORY_SLOT && (draggable.GetItemDefinition().itemType == ItemType.UNIQUE_GIFT || draggable.GetItemDefinition().itemType == ItemType.SHOES))
            {
                //ArchipelagoConsole.LogMessage("GIFT ITEM TRASHED with ID:" + draggable.GetItemDefinition().id.ToString());
                //Game.Persistence.playerFile.SetFlagValue(Util.idtoflag(draggable.GetItemDefinition().id).ToString(), 2);
                int flag = Util.idtoflag(draggable.GetItemDefinition().id);
                for (int i=0; i < ArchipelagoClient.alist.list.Count; i++)
                {
                    if (ArchipelagoClient.alist.list[i].item.ItemId == flag && ArchipelagoClient.alist.list[i].processed) 
                    {
                        ArchipelagoClient.alist.list[i].putinshop = true;
                    }
                }

            }
        }

        /// <summary>
        /// ALLOWS FOR AN ITEM YOU NEED THAT WAS TRASHED TO BE BOUGHT ONLY ONCE
        /// </summary>
        [HarmonyPatch(typeof(UiCellphoneAppStore), "OnProductPurchased")]
        [HarmonyPrefix]
        public static void giftpurchase(ItemSlotBehavior itemSlotBehavior )
        {
            if (itemSlotBehavior.itemDefinition.itemType == ItemType.UNIQUE_GIFT || itemSlotBehavior.itemDefinition.itemType == ItemType.SHOES)
            {
                Game.Persistence.playerFile.SetFlagValue(Util.idtoflag(itemSlotBehavior.itemDefinition.id).ToString(), 1);
            }
            if (itemSlotBehavior.itemDefinition.itemType == ItemType.FRUIT)
            {
                ArchipelagoConsole.LogMessage(itemSlotBehavior.itemDefinition.id.ToString());
                

                ArchipelagoClient.sendloc(69420505 + itemSlotBehavior.itemDefinition.id - 400);
                Game.Persistence.playerFile.SetFlagValue("shopslot" + (itemSlotBehavior.itemDefinition.id - 400).ToString(), 1);
            }
        }


        /// <summary>
        /// SENDS LOCATION COMPLETE FOR GETTING A RELATIONSHIP TO ATTRACTED/LOVERS
        /// AND ADDS 1 TO THE RELATIONSHIP UP COUNT SO TO NOT BREAK STUFF
        /// </summary>
        /// <returns>RETURNS FALSE TO SKIP ORIGINAL METHOD</returns>
        [HarmonyPatch(typeof(PlayerFileGirlPair), "RelationshipLevelUp")]
        [HarmonyPrefix]
        public static bool relationshiplvup(PlayerFileGirlPair __instance)
        {
            if (__instance.relationshipType == GirlPairRelationshipType.COMPATIBLE)
            {
                __instance.relationshipType++;
                Archipelago.ArchipelagoClient.sendloc(69420000 + __instance.girlPairDefinition.id);
                Game.Persistence.playerFile.GetPlayerFileGirl(__instance.girlPairDefinition.girlDefinitionOne).relationshipUpCount++;
                Game.Persistence.playerFile.GetPlayerFileGirl(__instance.girlPairDefinition.girlDefinitionTwo).relationshipUpCount++;
                Game.Persistence.playerFile.relationshipUpCount++;
            }
            else if (__instance.relationshipType == GirlPairRelationshipType.ATTRACTED)
            {
                __instance.relationshipType++;
                Archipelago.ArchipelagoClient.sendloc(69420024 + __instance.girlPairDefinition.id);
                Game.Persistence.playerFile.GetPlayerFileGirl(__instance.girlPairDefinition.girlDefinitionOne).relationshipUpCount++;
                Game.Persistence.playerFile.GetPlayerFileGirl(__instance.girlPairDefinition.girlDefinitionTwo).relationshipUpCount++;
                Game.Persistence.playerFile.relationshipUpCount++;
                if( Game.Persistence.playerFile.GetFlagValue("loversinsteadwings") == 1) { Game.Persistence.playerFile.completedGirlPairs.Add(__instance.girlPairDefinition); }
            }
            return false;
        }

        /// <summary>
        /// SEND LOCATION COMPLETE FOR GETTING A FAVROUTE QUESTION ANSWER
        /// </summary>
        [HarmonyPatch(typeof(PlayerFileGirl), "LearnFavAnswer")]
        [HarmonyPostfix]
        public static void questioncheck(QuestionDefinition questionDef, bool __result, PlayerFileGirl __instance)
        {
            if (__result == false) { return; }
            if (Game.Persistence.playerFile.GetFlagValue("questions_skiped") == 0)
            {
                if (Game.Persistence.playerFile.GetFlagValue("question:" + __instance.girlDefinition.id + ":" + questionDef.id) != 1)
                {
                    Archipelago.ArchipelagoClient.sendloc(69420144 + (__instance.girlDefinition.id - 1) * 20 + questionDef.id);
                    Game.Persistence.playerFile.SetFlagValue("question:" + __instance.girlDefinition.id + ":" + questionDef.id, 1);
                }
            }
        }

        /// <summary>
        /// SENDS LOCATION COMPLETE FOR GIFTING A UNIQUE GIFT THAT IS ACCEPTED
        /// </summary>
        [HarmonyPatch(typeof(PlayerFileGirl), "ReceiveUnique")]
        [HarmonyPrefix]
        public static bool uniquecheck(ItemDefinition uniqueDef,ref bool __result, PlayerFileGirl __instance, ref List<int> ____receivedUniques)
        {
            if (!__instance.girlDefinition.uniqueItemDefs.Contains(uniqueDef))
            {
                __result = false;
                return false;
            }

            if (____receivedUniques.Contains(__instance.girlDefinition.uniqueItemDefs.IndexOf(uniqueDef)))
            {
                __result = true;
                return false;
            }

            ____receivedUniques.Add(__instance.girlDefinition.uniqueItemDefs.IndexOf(uniqueDef));
            ArchipelagoClient.sendloc(Util.idtoflag(uniqueDef.id) - 44);
            
            __result = true;
            return false;
        }

        /// <summary>
        /// SENDS LOCATION COMPLETE FOR GIFTING A SHOE GIFT THAT IS ACCEPTED
        /// </summary>
        [HarmonyPatch(typeof(PlayerFileGirl), "ReceiveShoes")]
        [HarmonyPrefix]
        public static bool shoecheck(ItemDefinition shoesDef,ref bool __result, PlayerFileGirl __instance, ref List<int> ____receivedShoes)
        {
            if (!__instance.girlDefinition.shoesItemDefs.Contains(shoesDef))
            {
                __result = false;
                return false;
            }

            if (____receivedShoes.Contains(__instance.girlDefinition.shoesItemDefs.IndexOf(shoesDef)))
            {
                __result = true;
                return false;
            }

            ____receivedShoes.Add(__instance.girlDefinition.shoesItemDefs.IndexOf(shoesDef));
            ArchipelagoClient.sendloc(Util.idtoflag(shoesDef.id) - 44);

            __result = true;
            return false;
        }

        /// <summary>
        /// OVERWRITES THE METHOD FOR CALCULATION AFFECTION TOKEN EXP SO THAT WE CAN HAVE ARCHIPELAGO ITEMS FOR IT
        /// </summary>
        /// <returns>RETURNS FALSE TO SKIP ORIGINAL METHOD</returns>
        [HarmonyPatch(typeof(PlayerFile), "GetAffectionLevelExp")]
        [HarmonyPrefix]
        public static bool affectionexp(PuzzleAffectionType affectionType, bool ofLevel, PlayerFile __instance, ref int __result)
        { 
            int exp = 0;
            int flag = 69420025;
            if (affectionType == PuzzleAffectionType.ROMANCE) { flag += 4; }
            else if (affectionType == PuzzleAffectionType.FLIRTATION) { flag += 8; }
            else if (affectionType == PuzzleAffectionType.SEXUALITY) { flag += 12; }
            else { flag += 0; }

            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }
            flag++;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }
            flag++;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }
            flag++;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }

            if (ofLevel){__result = 0;}
            else{__result = exp;}
            return false;
        }

        /// <summary>
        /// OVERWRITES THE METHOD FOR CALCULATION PASSION EXP SO THAT WE CAN HAVE ARCHIPELAGO ITEMS FOR IT
        /// </summary>
        /// <returns>RETURNS FALSE TO SKIP ORIGINAL METHOD</returns>
        [HarmonyPatch(typeof(PlayerFile), "GetPassionLevelExp")]
        [HarmonyPrefix]
        public static bool passionexp(bool ofLevel, PlayerFile __instance, ref int __result)
        {
            int exp = 0;
            int flag = 69420041;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }
            flag++;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }
            flag++;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }
            flag++;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }
            flag++;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }
            flag++;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }
            flag++;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }
            flag++;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }

            if (ofLevel) { __result = 0; }
            else { __result = exp; }
            return false;
        }

        /// <summary>
        /// OVERWRITES THE METHOD FOR CALCULATION STYLE EXP SO THAT WE CAN HAVE ARCHIPELAGO ITEMS FOR IT
        /// </summary>
        /// <returns>RETURNS FALSE TO SKIP ORIGINAL METHOD</returns>
        [HarmonyPatch(typeof(PlayerFile), "GetStyleLevelExp")]
        [HarmonyPrefix]
        public static bool styleexp(bool ofLevel, PlayerFile __instance, ref int __result)
        {
            int exp = 0;
            int flag = 69420049;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }
            flag++;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }
            flag++;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }
            flag++;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }
            flag++;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }
            flag++;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }
            flag++;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }
            flag++;
            if (ArchipelagoClient.alist.hasitem(flag)) { exp += 6; }

            if (ofLevel) { __result = 0; }
            else { __result = exp; }
            return false;
        }

        [HarmonyPatch(typeof(ItemData), "Get")]
        [HarmonyPrefix]
        public static bool itemid(int id, ref ItemDefinition __result)
        {
            if (id > 400){
                __result = Util.genarchitem(id-400);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PuzzleStatus), "NextRound")]
        [HarmonyPostfix]
        public static void affectionoverite(PuzzleStatus __instance, ref int ____affectionGoal, ref int ____affection)
        {
            if (!__instance.bonusRound)
            {
                if (__instance.statusType == PuzzleStatusType.NORMAL)
                {
                    ____affectionGoal = Game.Persistence.playerFile.GetFlagValue("affection base") + (Game.Persistence.playerFile.GetFlagValue("affection addition") * Mathf.Min(Game.Persistence.playerFile.relationshipUpCount, 47));
                }
                else if (__instance.statusType == PuzzleStatusType.BOSS)
                {
                    ____affectionGoal = Game.Persistence.playerFile.GetFlagValue("affection boss");
                }
                ____affection = 0;
            }
        }

        [HarmonyPatch(typeof(PuzzleStatus), "get_initialMovesRemaining")]
        [HarmonyPrefix]
        public static bool moveoverite(ref int __result, PuzzleStatus __instance)
        {
            if (__instance.statusType == PuzzleStatusType.BOSS)
            {
                __result = Mathf.Min((Game.Persistence.playerFile.GetFlagValue("starting moves") * 4), 999);
            }
            else
            {
                __result = Game.Persistence.playerFile.GetFlagValue("starting moves");
            }
            return false;
        }

        [HarmonyPatch(typeof(PuzzleStatus), "get_maxMovesRemaining")]
        [HarmonyPrefix]
        public static bool maxmoveoverite(ref int __result)
        {
            __result = 999;
            return false;
        }

        [HarmonyPatch(typeof(CutsceneStepSpecialPostRewards), "Start")]
        [HarmonyPostfix]
        public static void test1(ref PuzzleStatus ____puzzleStatus)
        {
            //ArchipelagoConsole.LogMessage("hi");
            if (Game.Persistence.playerFile.GetFlagValue((____puzzleStatus.girlStatusLeft.playerFileGirl.girlDefinition.id.ToString() + ":" + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusLeft.girlDefinition).currentOutfitIndex.ToString())) == -1)
            {
                Game.Persistence.playerFile.SetFlagValue((____puzzleStatus.girlStatusLeft.playerFileGirl.girlDefinition.id.ToString() + ":" + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusLeft.girlDefinition).currentOutfitIndex.ToString()), 1);
                ArchipelagoClient.sendloc(69420385 + ((____puzzleStatus.girlStatusLeft.girlDefinition.id - 1) * 10) + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusLeft.girlDefinition).currentOutfitIndex);
                //ArchipelagoConsole.LogMessage("leftoutfit: " + (69420385 + ((____puzzleStatus.girlStatusLeft.girlDefinition.id - 1) * 10) + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusLeft.girlDefinition).currentOutfitIndex).ToString());
            }
            if (Game.Persistence.playerFile.GetFlagValue((____puzzleStatus.girlStatusRight.playerFileGirl.girlDefinition.id.ToString() + ":" + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusRight.girlDefinition).currentOutfitIndex.ToString())) == -1)
            {
                Game.Persistence.playerFile.SetFlagValue((____puzzleStatus.girlStatusRight.playerFileGirl.girlDefinition.id.ToString() + ":" + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusRight.girlDefinition).currentOutfitIndex.ToString()), 1);
                ArchipelagoClient.sendloc(69420385 + ((____puzzleStatus.girlStatusRight.girlDefinition.id - 1) * 10) + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusRight.girlDefinition).currentOutfitIndex);
                //ArchipelagoConsole.LogMessage("rightoutfit: " + (69420385 + ((____puzzleStatus.girlStatusRight.girlDefinition.id - 1) * 10) + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusRight.girlDefinition).currentOutfitIndex).ToString());
            }
        }

        [HarmonyPatch(typeof(CutsceneStepSpecialRoundClear), "Start")]
        [HarmonyPostfix]
        public static void test2(ref PuzzleStatus ____puzzleStatus)
        {
            if (Game.Persistence.playerFile.GetFlagValue((____puzzleStatus.girlStatusLeft.playerFileGirl.girlDefinition.id.ToString() + ":" + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusLeft.girlDefinition).currentOutfitIndex.ToString())) == -1)
            {
                Game.Persistence.playerFile.SetFlagValue((____puzzleStatus.girlStatusLeft.playerFileGirl.girlDefinition.id.ToString() + ":" + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusLeft.girlDefinition).currentOutfitIndex.ToString()), 1);
                ArchipelagoClient.sendloc(69420385 + ((____puzzleStatus.girlStatusLeft.girlDefinition.id - 1) * 10) + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusLeft.girlDefinition).currentOutfitIndex);
            }
            if (Game.Persistence.playerFile.GetFlagValue((____puzzleStatus.girlStatusRight.playerFileGirl.girlDefinition.id.ToString() + ":" + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusRight.girlDefinition).currentOutfitIndex.ToString())) == -1)
            {
                Game.Persistence.playerFile.SetFlagValue((____puzzleStatus.girlStatusRight.playerFileGirl.girlDefinition.id.ToString() + ":" + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusRight.girlDefinition).currentOutfitIndex.ToString()), 1);
                ArchipelagoClient.sendloc(69420385 + ((____puzzleStatus.girlStatusRight.girlDefinition.id - 1) * 10) + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusRight.girlDefinition).currentOutfitIndex);
            }
        }

        [HarmonyPatch(typeof(PlayerFileGirl), "UnlockHairstyle")]
        [HarmonyPrefix]
        public static bool hairoverite(int styleIndex, ref bool __result)
        {
            __result = false;
            return false;
        }

        [HarmonyPatch(typeof(PlayerFileGirl), "UnlockOutfit")]
        [HarmonyPrefix]
        public static bool outfitoverite(int styleIndex, ref bool __result)
        {
            __result = false;
            return false;
        }

        [HarmonyPatch(typeof(UiAppSelectListItem), "Populate")]
        [HarmonyPostfix]
        public static void thing(ref bool ____hidden)
        {
            ____hidden = false;
        }

        [HarmonyPatch(typeof(UiCellphoneAppWardrobe), "OnCheckBoxChanged")]
        [HarmonyPrefix]
        public static bool styletoggledisable()
        {
            return false;
        }
        //
        //[HarmonyPatch(typeof(UiAppStyleSelectList), "OnBuyButtonPressed")]
        //[HarmonyPrefix]
        //public static bool waudrobebuy(ButtonBehavior buttonBehavior, UiAppStyleSelectList __instance, ref PlayerFileGirl ____playerFileGirl, ref UiAppSelectListItem ____purchaseListItem)
        //{
        //    if (____purchaseListItem==null) { return false; }
        //
        //    PlayerFile file = Game.Persistence.playerFile;
        //    FruitCategoryInfo fruitCategoryInfo = Game.Session.Gift.GetFruitCategoryInfo((!__instance.alternative) ? ____playerFileGirl.girlDefinition.leastFavoriteAffectionType : ____playerFileGirl.girlDefinition.favoriteAffectionType);
        //    int cost = 0;
        //    int baseflag = 0;
        //    if (____purchaseListItem == __instance.listItems[6]) { cost = 10; baseflag = 69420391; }
        //    else if (____purchaseListItem == __instance.listItems[7]) { cost = 20; baseflag = 69420392; }
        //    else if (____purchaseListItem == __instance.listItems[8]) { cost = 30; baseflag = 69420393; }
        //    else if (____purchaseListItem == __instance.listItems[9]) { cost = 40; baseflag = 69420394; }
        //
        //    if (file.GetFruitCount(fruitCategoryInfo.affectionType) < cost) { return false; }
        //
        //    if (file.GetFlagValue("loc_" + (baseflag + ((____playerFileGirl.girlDefinition.id - 1) * 10)).ToString()) == -1)
        //    {
        //        Game.Persistence.playerFile.AddFruitCount(fruitCategoryInfo.affectionType, -cost);
        //        file.SetFlagValue("loc_" + (baseflag + ((____playerFileGirl.girlDefinition.id - 1) * 10)).ToString(), 1);
        //        ArchipelagoClient.sendloc(baseflag + ((____playerFileGirl.girlDefinition.id - 1) * 10));
        //    }
        //
        //    __instance.Populate(____playerFileGirl);
        //
        //    return false;
        //}

        [HarmonyPatch(typeof(TalkManager), "TalkStep")]
        [HarmonyPrefix]
        public static bool question(TalkManager __instance, ref int ____talkStepIndex, ref List<QuestionDefinition> ____questionPool, ref UiDoll ____targetDoll)
        {
            if (__instance.talkType == TalkWithType.FAVORITE_QUESTION)
            {
                if (____talkStepIndex == 0)
                {
                    List<QuestionDefinition> pool = new List<QuestionDefinition>();
                    List<QuestionDefinition> badpool = new List<QuestionDefinition>();
                    List<QuestionDefinition> goodpool = new List<QuestionDefinition>();

                    for (int i = 0; i < 20; i++)
                    {
                        if (Game.Persistence.playerFile.GetFlagValue("question:" + ____targetDoll.girlDefinition.id + ":" + i+1) != 1)
                        {
                            goodpool.Add(Game.Session.Talk.favQuestionDefinitions[i]);
                        }
                        else
                        {
                            badpool.Add(Game.Session.Talk.favQuestionDefinitions[i]);
                        }
                    }

                    for (int j = 1; j < 4; j++)
                    {
                        if (goodpool.Count > 0)
                        {
                            int index = UnityEngine.Random.Range(0, goodpool.Count);
                            pool.Add(goodpool[index]);
                            goodpool.RemoveAt(index);
                        }
                        else
                        {

                            int index = UnityEngine.Random.Range(0, badpool.Count);
                            pool.Add(badpool[index]);
                            badpool.RemoveAt(index);
                        }
                    }
                    ____questionPool = pool;
                }
            }
            return true;
        }


        [HarmonyPatch(typeof(PlayerFile), "PopulateFinderSlots")]
        [HarmonyPrefix]
        public static bool finder(PlayerFile __instance)
        {
            for (int i=0; i<__instance.finderSlots.Count; i++)
            {
                __instance.finderSlots[i].Clear();
            }

            __instance.finderSlots = Util.genfinder(__instance);

            return false;
        }


        /// IL CODE

        /// <summary>
        /// INJECT NOP(NO OPPERATION) INSTRUCTIONS IN THE 1ST FOR LOOP LOOKIN FOR UNKNOWN PAIRS SO THAT ITS ALWAYS SKIPED,
        /// CHANGE THE 2ND FOR LOOP THAT LOOK FOR UNKNOWN PAIRS TO LOOK FOR ALL PAIRS THAT ARE NOT UNKOWN
        /// </summary>
        //[HarmonyPatch(typeof(PlayerFile), "PopulateFinderSlots")]
        //[HarmonyILManipulator]
        //public static void finderslotmodificaions(ILContext ctx, MethodBase orig)
        //{
        //    for (int i = 0; i < ctx.Instrs.Count; i++)
        //    {
        //        if (ctx.Instrs[i].Offset >= 378 && ctx.Instrs[i].Offset <= 537)
        //        {
        //            ctx.Instrs[i].OpCode = OpCodes.Nop;
        //            ctx.Instrs[i].Operand = null;
        //        }
        //        if (ctx.Instrs[i].Offset == 563) { ctx.Instrs[i].OpCode = OpCodes.Brfalse; }
        //
        //    }
        //
        //}

        [HarmonyPatch(typeof(UiAppStyleSelectList), "Refresh")]
        [HarmonyILManipulator]
        public static void waudrobeoverite(ILContext ctx, MethodBase orig)
        {
            sbyte opp7 = 7;
            sbyte opp1 = 1;
            bool b = true;
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                if (ctx.Instrs[i].OpCode == OpCodes.Ldc_I4_S && ctx.Instrs[i].Operand.ToString() == "14") { ctx.Instrs[i].Operand = opp7; }
                if (b && ctx.Instrs[i].OpCode == OpCodes.Ldloc_S && ctx.Instrs[i].Operand.ToString() == "V_4")
                {
                    ctx.Instrs[i].OpCode = OpCodes.Ldc_I4_S;
                    ctx.Instrs[i].Operand = opp1;
                    b = false;
                }
            }
        }

        [HarmonyPatch(typeof(UiCellphoneAppWardrobe), "Start")]
        [HarmonyILManipulator]
        public static void waudrobeoverite2(ILContext ctx, MethodBase orig)
        {
            sbyte opp = 7;
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                if (ctx.Instrs[i].OpCode == OpCodes.Ldc_I4_S && ctx.Instrs[i].Operand.ToString() == "14") { ctx.Instrs[i].Operand = opp; }
                //break;
            }
        }

        [HarmonyPatch(typeof(TalkManager), "TalkStep")]
        [HarmonyILManipulator]
        public static void questionil(ILContext ctx, MethodBase orig)
        {
            int s = 0;
            int r = 0;

            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                if (s == 3)
                {
                    if (r == 2)
                    {
                        if (ctx.Instrs[i].OpCode == OpCodes.Newobj)
                        {
                            break;
                        }
                        ctx.Instrs[i].OpCode = OpCodes.Nop;
                        ctx.Instrs[i].Operand = null;
                    }
                    else
                    {
                        if (ctx.Instrs[i].OpCode == OpCodes.Ret)
                        {
                            r++;
                        }
                    }
                }
                else
                {
                    if (ctx.Instrs[i].OpCode == OpCodes.Switch) 
                    {
                        s++;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(LocationManager), "ResetDolls")]
        [HarmonyILManipulator]
        public static void dateoutfitoveride(ILContext ctx, MethodBase orig)
        {
            for (int i=0; i<ctx.Instrs.Count; i++)
            {
                if (ctx.Instrs[i].OpCode == OpCodes.Ldc_I4_2) 
                { 
                    ctx.Instrs[i].OpCode = OpCodes.Ldc_I4_0;
                    break;
                }
            }
        }

    }

    /// <summary>
    /// UTILITY CLASS FOR PATCHES (USUALLY STUFF THAT IS IN MORE THAN 1 PATCH)
    /// </summary>
    public class Util
    {

        /// <summary>
        /// METHOD TO SHOW AN ERROR POP UP ON SCREEN
        /// </summary>
        public static async void cellerror(string s, UiCellphone c)
        {
            c.phoneErrorMsg.ShowMessage(s);
            await Task.Delay(5000);
            c.phoneErrorMsg.ClearMessage();
        }

        /// <summary>
        /// GENERATE THE INITAL FINDER SLOTS WHEN SETTING UP A NEW GAME
        /// </summary>
        public static List<PlayerFileFinderSlot> genfinder(PlayerFile file)
        {

            List<GirlPairDefinition> pair = new List<GirlPairDefinition>();

            for (int i = 0; i < file.girlPairs.Count; i++)
            {
                if (file.girlPairs[i].girlPairDefinition.girlDefinitionOne.id >= 13 || file.girlPairs[i].girlPairDefinition.girlDefinitionTwo.id >= 13) { continue; }
                if (file.girlPairs[i].relationshipType != GirlPairRelationshipType.UNKNOWN) 
                { 
                    if (file.GetPlayerFileGirl(file.girlPairs[i].girlPairDefinition.girlDefinitionOne).playerMet && file.GetPlayerFileGirl(file.girlPairs[i].girlPairDefinition.girlDefinitionTwo).playerMet)
                    {
                        pair.Add(file.girlPairs[i].girlPairDefinition);
                    }
                }
            }


            int initalpaircount = pair.Count;

            List<int> areas = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };
            List<PlayerFileFinderSlot> finder = new List<PlayerFileFinderSlot>();

            for (int i = 0; i < 8; i++)
            {
                if (pair.Count == 0) { break; }
                int p;
                if (pair.Count == 1) { p = 1; } else { p = UnityEngine.Random.Range(0, pair.Count - 1); }
                int a = UnityEngine.Random.Range(0, areas.Count - 1);

                p = Math.Min(p, pair.Count - 1);
                a = Math.Min(a, areas.Count - 1);

                PlayerFileFinderSlot findSlot = new PlayerFileFinderSlot();
                findSlot.locationDefinition = Game.Data.Locations.Get(areas[a]);
                findSlot.girlPairDefinition = pair[p];
                findSlot.sidesFlipped = false;

                finder.Add(findSlot);

                areas.RemoveAt(a);
                pair.RemoveAt(p);

                if (i < 2) { continue; }

                if (UnityEngine.Random.Range(0, 48) <= initalpaircount) { break; }

            }

            return finder;
        }

        /// <summary>
        /// METHOD TO GENERATE STORE ITEMS
        /// </summary>
        public static List<PlayerFileStoreProduct> genStore(PlayerFile file)
        {

            List<PlayerFileStoreProduct> store = new List<PlayerFileStoreProduct>();
            List<ItemDefinition> girlgifts = new List<ItemDefinition>();
            List<ItemDefinition> food = Game.Data.Items.GetAllOfTypes(ItemType.FOOD);
            List<ItemDefinition> date = Game.Data.Items.GetAllOfTypes(ItemType.DATE_GIFT);

            List<int> architems = new List<int>();
            for (int s = 0; true; s++)
            {
                if (file.GetFlagValue("shopslot" + s.ToString()) == 0)
                {
                    architems.Add(s+1);
                }
                else if((file.GetFlagValue("shopslot" + s.ToString()) == -1))
                {
                    break;
                }
            }


            for (int f = 0; f < ArchipelagoClient.alist.list.Count; f++)
            {
                if (ArchipelagoClient.alist.list[f].putinshop)
                {
                    ItemDefinition def = Game.Data.Items.Get(Util.flagtoid((int)ArchipelagoClient.alist.list[f].item.ItemId));
                    girlgifts.Add(def);
                }
            }

            for (int i = 0; i < 32; i++)
            {
                int ran = UnityEngine.Random.Range(0, 100);
                if (ran%4==0)
                {
                    int num = UnityEngine.Random.Range(0, date.Count - 1);
                    store.Add(genproduct(i, date[num], UnityEngine.Random.Range(1, 5)));
                    date.RemoveAt(num);
                }
                else if((ran % 4 == 1) && girlgifts.Count > 0)
                {
                    int num = UnityEngine.Random.Range(0, girlgifts.Count - 1);
                    store.Add(genproduct(i, girlgifts[num], UnityEngine.Random.Range(1, 5)));
                    girlgifts.RemoveAt(num);
                }
                else if ((ran % 4 == 2) && architems.Count > 0)
                {
                    int num = UnityEngine.Random.Range(0, architems.Count - 1);                    
                    store.Add(genproduct(i, Util.genarchitem(architems[num]), UnityEngine.Random.Range(10, 20)));
                    architems.RemoveAt(num);
                }
                else
                {
                    int num = UnityEngine.Random.Range(0, food.Count - 1);
                    store.Add(genproduct(i, food[num], UnityEngine.Random.Range(1, 5)));
                    food.RemoveAt(num);
                }
            }

            return store;
        }

        /// <summary>
        /// GENERATES AN ARCH STORE PRODUCT TO SELL IN STORE
        /// </summary>
        public static ItemDefinition genarchitem(int i)
        {
            ScoutedItemInfo architem = ArchipelagoClient.getshopitem(i);

            if (architem == null) { return null; }

            ItemDefinition tmp = Game.Data.Items.Get(314);
            ItemDefinition item = new ItemDefinition();

            if ((int)architem.Flags == 1 || (int)architem.Flags == 3 || (int)architem.Flags == 5 || (int)architem.Flags == 4)
            {
                item.itemName = architem.Player.Name + " Progression item";
            }
            else if ((int)architem.Flags == 2 || (int)architem.Flags == 6)
            {
                item.itemName = architem.Player.Name + " Useful item";
            }
            else
            {
                item.itemName = architem.Player.Name + " Filler item";
            }
            item.id = i+400;
            item.itemDescription = "Buy this item to send it to "+ architem.Player.Name + ", Trash after buying, shop slot #"+i.ToString();
            item.itemType = ItemType.FRUIT;
            item.itemSprite = tmp.itemSprite;
            item.energyDefinition = tmp.energyDefinition;
            //ArchipelagoConsole.LogMessage((i + 1).ToString());
            return item;

        }

        /// <summary>
        /// GENERATES A STORE PRODUCT TO SELL IN STORE
        /// </summary>
        public static PlayerFileStoreProduct genproduct(int i, ItemDefinition item, int c)
        {
            PlayerFileStoreProduct product = new PlayerFileStoreProduct();
            product.productIndex = i;
            product.itemDefinition = item;
            product.itemCost = c;
            return product;

        }

        /// <summary>
        /// METHOD TO OVERITE BAGGAE WITH A TEMP ONE
        /// </summary>
        public static ItemDefinition baggagestuff()
        {
            ItemDefinition newbagage = Game.Data.Items.Get(103);

            newbagage.itemDescription = "temp baggage that does nothing so you can have access to all date gift slots and can give all uniques and shoes";
            newbagage.ailmentDefinition.enableType = AilmentEnableType.NONE;
            newbagage.baggageGirl = EditorDialogTriggerTab.KYU;
            newbagage.itemName = "TEMP BAGGAGE";
            for (int i = newbagage.ailmentDefinition.triggers.Count - 1; i >= 0; i--)
            {
                newbagage.ailmentDefinition.triggers.RemoveAt(i);
            }

            return newbagage;

        }

        /// <summary>
        /// METHOD TO PROCESS THE ARCH ITEMS THAT ARE STORED IN FLAGS ON THE PLAYRE FILE
        /// </summary>
        public static void archflagprocess(PlayerFile file)
        {
            ArchipelagoConsole.LogMessage("PROCESSING ITEMS");
            //ArchipelagoConsole.LogMessage(ArchipelagoClient.itemstoprocess.Dequeue().ToString());
            for (int i=0; i<ArchipelagoClient.alist.list.Count; i++)
            {
                if (ArchipelagoClient.alist.list[i].processed) { continue; }
                ArchipelagoConsole.LogMessage("PROCESSING ITEM ID: " + ArchipelagoClient.alist.list[i].item.ItemId);

                if (ArchipelagoClient.alist.list[i].item.ItemId > 69420000 && ArchipelagoClient.alist.list[i].item.ItemId < 69420025) 
                {
                    //WINGS
                    GirlPairDefinition def = Game.Data.GirlPairs.Get((int)ArchipelagoClient.alist.list[i].item.ItemId - 69420000);
                    if (!file.completedGirlPairs.Contains(def))
                    {
                        file.completedGirlPairs.Add(def);
                        ArchipelagoConsole.LogMessage(def.name + " WING ITEM PROCESSED PROCESSED");
                        ArchipelagoClient.alist.list[i].processed = true;
                    }
                }
                else if (ArchipelagoClient.alist.list[i].item.ItemId > 69420024 && ArchipelagoClient.alist.list[i].item.ItemId < 69420057) 
                {
                    //TOKEN LV-UP
                    ArchipelagoConsole.LogMessage("TOKEN LV-UP PROCESSED");
                    ArchipelagoClient.alist.list[i].processed = true;
                }
                else if (ArchipelagoClient.alist.list[i].item.ItemId > 69420056 && ArchipelagoClient.alist.list[i].item.ItemId < 69420069) 
                {
                    //GIRL UNLOCK
                    GirlDefinition def = Game.Data.Girls.Get((int)ArchipelagoClient.alist.list[i].item.ItemId - 69420056);
                    file.GetPlayerFileGirl(def).playerMet = true;
                    ArchipelagoClient.alist.list[i].processed = true;
                    ArchipelagoConsole.LogMessage(def.girlName + " IS UNLOCKED");
                    /*
                    for (int j = 0; j < file.girls.Count; j++)
                    {
                        if (file.girls[j].girlDefinition.name == def.name && file.girls[j].playerMet == false)
                        {
                            file.girls[j].playerMet = true;
                            ArchipelagoConsole.LogMessage(def.girlName + " IS UNLOCKED");
                            ArchipelagoClient.alist.list[i].processed = true;
                        }

                    }*/
                }
                else if (ArchipelagoClient.alist.list[i].item.ItemId > 69420068 && ArchipelagoClient.alist.list[i].item.ItemId < 69420093) 
                {
                    //PAIR UNLOCK
                    GirlPairDefinition def = Game.Data.GirlPairs.Get((int)ArchipelagoClient.alist.list[i].item.ItemId - 69420068);
                    for (int j = 0; j < file.girlPairs.Count; ++j)
                    {
                        if (file.girlPairs[j].girlPairDefinition.id == def.id && file.girlPairs[j].relationshipType == GirlPairRelationshipType.UNKNOWN)
                        {
                            file.girlPairs[j].relationshipType = GirlPairRelationshipType.COMPATIBLE;
                            file.metGirlPairs.Add(file.girlPairs[j].girlPairDefinition);
                            ArchipelagoConsole.LogMessage(def.name + " UNLOCKED PAIR");
                            ArchipelagoClient.alist.list[i].processed = true;
                        }
                    }
                }
                else if (ArchipelagoClient.alist.list[i].item.ItemId > 69420092 && ArchipelagoClient.alist.list[i].item.ItemId < 69420141) 
                {
                    //UNIQUE GIFT
                    if (!file.IsInventoryFull())
                    {
                        ItemDefinition def = Game.Data.Items.Get(Util.flagtoid((int)ArchipelagoClient.alist.list[i].item.ItemId));
                        file.AddInventoryItem(def);
                        ArchipelagoConsole.LogMessage(def.itemName + " UNIQUE GIFT OBTAINED");
                        ArchipelagoClient.alist.list[i].processed = true;
                    }
                }
                else if (ArchipelagoClient.alist.list[i].item.ItemId > 69420140 && ArchipelagoClient.alist.list[i].item.ItemId < 69420189) 
                {
                    //SHOE GIFT
                    if (!file.IsInventoryFull())
                    {
                        ItemDefinition def = Game.Data.Items.Get(Util.flagtoid((int)ArchipelagoClient.alist.list[i].item.ItemId));
                        file.AddInventoryItem(def);
                        ArchipelagoConsole.LogMessage(def.itemName + " SHOES GIFT PROCESSED");
                        ArchipelagoClient.alist.list[i].processed = true;
                    }
                }
                else if (ArchipelagoClient.alist.list[i].item.ItemId > 69420188 && ArchipelagoClient.alist.list[i].item.ItemId < 69420225) 
                {
                    //BAGGAGE

                    GirlDefinition def = Game.Data.Girls.Get((((int)ArchipelagoClient.alist.list[i].item.ItemId - 69420189) / 3) + 1);
                    ItemDefinition bagdef = Game.Data.Items.GetAllOfTypes(ItemType.BAGGAGE)[(int)ArchipelagoClient.alist.list[i].item.ItemId - 69420189];
                    file.GetPlayerFileGirl(def).girlDefinition.baggageItemDefs[(((int)ArchipelagoClient.alist.list[i].item.ItemId - 69420189) % 3)] = bagdef;
                    ArchipelagoConsole.LogMessage(def.girlName + " OBTAINED NEW BAGGAGGE");
                    ArchipelagoClient.alist.list[i].processed = true;

                    /*
                    int u = (int)ArchipelagoClient.alist.list[i].item.ItemId - 69420189;
                    for (int b = 0; b < file.girls.Count; b++)
                    {
                        if (file.girls[b].girlDefinition.id == (u / 3) + 1)
                        {
                            ItemDefinition def = Game.Data.Items.GetAllOfTypes(ItemType.BAGGAGE)[u];
                            for (int g = 0; g < file.girls[b].girlDefinition.baggageItemDefs.Count; g++)
                            {
                                if (file.girls[u / 3].girlDefinition.baggageItemDefs[g].itemName == "TEMP BAGGAGE")
                                {
                                    file.girls[b].girlDefinition.baggageItemDefs.RemoveAt(g);
                                    file.girls[b].girlDefinition.baggageItemDefs.Add(def);
                                    ArchipelagoConsole.LogMessage(file.girls[b].girlDefinition.girlName + " OBTAINED NEW BAGGAGGE");
                                    ArchipelagoClient.alist.list[i].processed = true;
                                    break;
                                }
                            }
                        }
                    }*/
                }
                else if (ArchipelagoClient.alist.list[i].item.ItemId > 69420224 && ArchipelagoClient.alist.list[i].item.ItemId < 69420345) 
                {
                    //OUTFITS
                    int u = (int)ArchipelagoClient.alist.list[i].item.ItemId - 69420225;
                    int girlid = (u / 10) + 1;
                    int styleid = u % 10;

                    GirlDefinition def = Game.Data.Girls.Get(girlid);
                    if (!file.GetPlayerFileGirl(Game.Data.Girls.Get(girlid)).unlockedOutfits.Contains(styleid))
                    {
                        ArchipelagoConsole.LogMessage("OBTAINED " + file.GetPlayerFileGirl(Game.Data.Girls.Get(girlid)).girlDefinition.girlName + " OUTFIT #" + (styleid + 1));
                        file.GetPlayerFileGirl(Game.Data.Girls.Get(girlid)).unlockedOutfits.Add(styleid);
                        file.GetPlayerFileGirl(Game.Data.Girls.Get(girlid)).unlockedHairstyles.Add(styleid);
                    }
                    /*
                    for (int b = 0; b < file.girls.Count; b++)
                    {
                        if (file.girls[b].girlDefinition.id == girlid)
                        {
                            if (!file.girls[b].unlockedOutfits.Contains(styleid))
                            {
                                ArchipelagoConsole.LogMessage("OBTAINED " + file.girls[b].girlDefinition.girlName + " OUTFIT #" + (styleid + 1));
                                file.girls[b].unlockedOutfits.Add(styleid);
                                file.girls[b].unlockedHairstyles.Add(styleid);
                            }
                        }
                    }*/
                    ArchipelagoClient.alist.list[i].processed = true;
                }
                else if (ArchipelagoClient.alist.list[i].item.ItemId > 69420344 && ArchipelagoClient.alist.list[i].item.ItemId < 69420422) 
                {
                    //FILLER ITEMS
                    if ((int)ArchipelagoClient.alist.list[i].item.ItemId == 69420345)
                    {
                        //ArchipelagoConsole.LogMessage("nothing");
                        ArchipelagoConsole.LogMessage("OBTAINED NOTHING");
                        ArchipelagoClient.alist.list[i].processed = true;
                    }
                    else if ((int)ArchipelagoClient.alist.list[i].item.ItemId == 69420421)
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
                        ArchipelagoConsole.LogMessage("OBTAINED SEEDS: " + b.ToString() + " Blue, " + g.ToString() + " Green, " + o.ToString() + " Orange, " + r.ToString() + " Red");
                        ArchipelagoClient.alist.list[i].processed = true;
                    }
                    else
                    {                        
                        if (!file.IsInventoryFull())
                        {
                            ArchipelagoConsole.LogMessage(Game.Data.Items.Get(Util.itemflagtoid((int)ArchipelagoClient.alist.list[i].item.ItemId)).name);
                            //file.AddInventoryItem(Game.Data.Items.Get(Util.itemflagtoid((int)ArchipelagoClient.alist.list[i].item.ItemId)));

                            for (int k = 0; k < 35; k++)
                            {
                                PlayerFileInventorySlot playerFileInventorySlot = file.GetPlayerFileInventorySlot(k);
                                if (playerFileInventorySlot.itemDefinition == null)
                                {
                                    playerFileInventorySlot.itemDefinition = Game.Data.Items.Get(Util.itemflagtoid((int)ArchipelagoClient.alist.list[i].item.ItemId));
                                    playerFileInventorySlot.daytimeStamp = 0;
                                    break;
                                }
                            }

                            ArchipelagoConsole.LogMessage("OBTAINED " + Game.Data.Items.Get(Util.itemflagtoid((int)ArchipelagoClient.alist.list[i].item.ItemId)).name + " ITEM");
                            ArchipelagoClient.alist.list[i].processed = true;
                        }
                        else
                        {
                            ArchipelagoConsole.LogMessage("INVENTORY FULL COUDNT PROCESS ITEM");
                        }
                    }
                }

            }

            /*
            for (int i = 0; i < file.flags.Count; i++)
            {
                int.TryParse(file.flags[i].flagName, out int flagint);
                if (flagint > 69420000 && file.flags[i].flagValue == 0)
                {
                    //ArchipelagoConsole.LogMessage("PROCESSING FLAG");
                    if (flagint > 69420000 && flagint < 69420025)
                    {
                        //WING STUFF
                        //ArchipelagoConsole.LogMessage("PROCESSING WING");
                        GirlPairDefinition def = Game.Data.GirlPairs.Get(flagint - 69420000);
                        if (!file.completedGirlPairs.Contains(def))
                        {
                            file.completedGirlPairs.Add(def);
                            ArchipelagoConsole.LogMessage(def.name + " WING ITEM PROCESSED PROCESSED");
                            file.flags[i].flagValue = 1;
                        }
                    }
                    else if (flagint > 69420024 && flagint < 69420057)
                    {
                        //TOKEN LVUP STUFF
                        //ArchipelagoConsole.LogMessage("PROCESSING TOKEN LVUP");
                        ArchipelagoConsole.LogMessage("TOKEN LV-UP PROCESSED");
                        file.flags[i].flagValue = 1;
                    }
                    else if (flagint > 69420056 && flagint < 69420069)
                    {
                        //GIRL UNLOCK STUFF
                        //ArchipelagoConsole.LogMessage("PROCESSING GIRL UNLOCK");
                        GirlDefinition def = Game.Data.Girls.Get(flagint - 69420056);
                        for (int j = 0; j < file.girls.Count; j++)
                        {
                            if (file.girls[j].girlDefinition.name == def.name && file.girls[j].playerMet == false)
                            {
                                file.girls[j].playerMet = true;
                                ArchipelagoConsole.LogMessage(def.girlName + " IS UNLOCKED");
                                file.flags[i].flagValue = 1;

                            }
                        }
                    }
                    else if (flagint > 69420068 && flagint < 69420093)
                    {
                        //PAIR UNLOCK STUFF
                        //ArchipelagoConsole.LogMessage("PAIR GIRL UNLOCK");
                        GirlPairDefinition def = Game.Data.GirlPairs.Get(flagint - 69420068);
                        for (int j = 0; j < file.girlPairs.Count; ++j)
                        {
                            if (file.girlPairs[j].girlPairDefinition.id == def.id && file.girlPairs[j].relationshipType == GirlPairRelationshipType.UNKNOWN)
                            {
                                file.girlPairs[j].relationshipType = GirlPairRelationshipType.COMPATIBLE;
                                file.metGirlPairs.Add(file.girlPairs[j].girlPairDefinition);
                                ArchipelagoConsole.LogMessage(def.name + " UNLOCKED PAIR");
                                file.flags[i].flagValue = 1;
                            }
                        }

                    }
                    else if (flagint > 69420092 && flagint < 69420141)
                    {
                        //GIFT UNIQUE STUFF
                        //ArchipelagoConsole.LogMessage("PROCESSING GIFT UNIQUE");
                        if (!file.IsInventoryFull())
                        {
                            ItemDefinition def = Game.Data.Items.Get(Util.flagtoid(flagint));
                            file.AddInventoryItem(def);
                            ArchipelagoConsole.LogMessage(def.itemName + " UNIQUE GIFT OBTAINED");
                            file.flags[i].flagValue = 1;

                        }


                    }
                    else if (flagint > 69420140 && flagint < 69420189)
                    {
                        //GIFT SHOE STUFF
                        //ArchipelagoConsole.LogMessage("PROCESSING GIFT SHOE");
                        if (!file.IsInventoryFull())
                        {
                            ItemDefinition def = Game.Data.Items.Get(Util.flagtoid(flagint));
                            file.AddInventoryItem(def);
                            ArchipelagoConsole.LogMessage(def.itemName + " SHOES GIFT PROCESSED");
                            file.flags[i].flagValue = 1;

                        }
                    }
                    else if (flagint > 69420188 && flagint < 69420225)
                    {
                        //BAGGAGE STUFF
                        //ArchipelagoConsole.LogMessage("PROCESSING BAGGAGE");
                        int u = flagint - 69420189;
                        for (int b = 0; b < file.girls.Count; b++)
                        {
                            if (file.girls[b].girlDefinition.id == (u / 3) + 1)
                            {
                                ItemDefinition def = Game.Data.Items.GetAllOfTypes(ItemType.BAGGAGE)[u];
                                for (int g = 0; g < file.girls[b].girlDefinition.baggageItemDefs.Count; g++)
                                {
                                    if (file.girls[u / 3].girlDefinition.baggageItemDefs[g].itemName == "TEMP BAGGAGE")
                                    {
                                        file.girls[b].girlDefinition.baggageItemDefs.RemoveAt(g);
                                        file.girls[b].girlDefinition.baggageItemDefs.Add(def);
                                        ArchipelagoConsole.LogMessage(file.girls[b].girlDefinition.girlName + " OBTAINED NEW BAGGAGGE");
                                        file.flags[i].flagValue = 1;
                                        break;
                                    }
                                }
                            }

                        }

                    }
                    else if (flagint > 69420224 && flagint < 69420345)
                    {
                        //OUTFIT STUFF
                        //ArchipelagoConsole.LogMessage("PROCESSING OUTFIT");
                        //ArchipelagoConsole.LogMessage(file.flags[i].flagName + " IS OUTFIT PROCESSED");

                        int u = flagint - 69420225;
                        int girlid = (u / 10) + 1;
                        int styleid = u % 10;

                        for (int b = 0; b < file.girls.Count; b++)
                        {
                            if (file.girls[b].girlDefinition.id == girlid)
                            {
                                if (!file.girls[b].unlockedOutfits.Contains(styleid))
                                {
                                    ArchipelagoConsole.LogMessage("OBTAINED " + file.girls[b].girlDefinition.girlName + " OUTFIT #" + (styleid+1));
                                    file.girls[b].unlockedOutfits.Add(styleid);
                                    file.girls[b].unlockedHairstyles.Add(styleid);
                                }
                            }
                        }

                        file.flags[i].flagValue = 1;
                    }
                    else if (flagint > 69420344 && flagint < 69420422)
                    {
                        //ArchipelagoConsole.LogMessage("OHI");
                        //ArchipelagoConsole.LogMessage(flagint.ToString());
                        if (flagint == 69420345)
                        {
                            //ArchipelagoConsole.LogMessage("nothing");
                            //ArchipelagoConsole.LogMessage("OBTAINED NOTHING");
                        }
                        else if (flagint == 69420421)
                        {
                            ArchipelagoConsole.LogMessage("tokens");

                            int pr = -1;
                            PlayerFileFlag fl = new PlayerFileFlag();
                            for (int j=0; j < file.flags.Count; j++)
                            {
                                if (file.flags[j].flagName == (flagint.ToString() + "processed"))
                                {
                                    pr=j; break;
                                }
                            }

                            if (pr == -1)
                            {
                                ArchipelagoConsole.LogMessage("no processed flag");
                                fl.flagName = (flagint.ToString() + "processed");
                                fl.flagValue = 0;

                                while (file.flags[i].flagValue > fl.flagValue)
                                {
                                    fl.flagValue++;
                                    int b = UnityEngine.Random.Range(0, 20);
                                    int g = UnityEngine.Random.Range(0, 20);
                                    int o = UnityEngine.Random.Range(0, 20);
                                    int r = UnityEngine.Random.Range(0, 20);

                                    file.AddFruitCount(PuzzleAffectionType.TALENT, b);
                                    file.AddFruitCount(PuzzleAffectionType.FLIRTATION, g);
                                    file.AddFruitCount(PuzzleAffectionType.ROMANCE, o);
                                    file.AddFruitCount(PuzzleAffectionType.SEXUALITY, r);
                                    ArchipelagoConsole.LogMessage("OBTAINED SEEDS: " + b.ToString() + " Blue, " + g.ToString() + " Green, " + o.ToString() + " Orange, " + r.ToString() + " Red");
                                }
                                file.flags.Add(fl);

                            }
                            else
                            {
                                ArchipelagoConsole.LogMessage("processed flag");
                                while (file.flags[i].flagValue > file.flags[pr].flagValue)
                                {
                                    file.flags[pr].flagValue++;
                                    int b = UnityEngine.Random.Range(0, 20);
                                    int g = UnityEngine.Random.Range(0, 20);
                                    int o = UnityEngine.Random.Range(0, 20);
                                    int r = UnityEngine.Random.Range(0, 20);

                                    file.AddFruitCount(PuzzleAffectionType.TALENT, b);
                                    file.AddFruitCount(PuzzleAffectionType.FLIRTATION, g);
                                    file.AddFruitCount(PuzzleAffectionType.ROMANCE, o);
                                    file.AddFruitCount(PuzzleAffectionType.SEXUALITY, r);
                                    ArchipelagoConsole.LogMessage("OBTAINED SEEDS: " + b.ToString() + " Blue, " + g.ToString() + " Green, " + o.ToString() + " Orange, " + r.ToString() + " Red");
                                }
                            }
                        }
                        else
                        {
                            ArchipelagoConsole.LogMessage("item");
                            int pr = -1;
                            PlayerFileFlag fl = new PlayerFileFlag();
                            for (int j = 0; j < file.flags.Count; j++)
                            {
                                if (file.flags[j].flagName == (flagint.ToString() + "processed"))
                                {
                                    pr = j; break;
                                }
                            }

                            if (pr == -1)
                            {
                                ArchipelagoConsole.LogMessage("no processed flag");
                                fl.flagName = (flagint.ToString() + "processed");
                                fl.flagValue = 0;

                                while (file.flags[i].flagValue > fl.flagValue)
                                {
                                    ArchipelagoConsole.LogMessage("itemtoprocess");
                                    if (file.IsInventoryFull())
                                    {
                                        ArchipelagoConsole.LogMessage("INVENTORY FULL COUDNT PROCESS ITEM");
                                        break;
                                    }
                                    fl.flagValue++;
                                    ArchipelagoConsole.LogMessage("additem");
                                    file.AddInventoryItem(Game.Data.Items.Get(Util.itemflagtoid(flagint)));
                                    ArchipelagoConsole.LogMessage("OBTAINED " + Game.Data.Items.Get(flagint).itemName + " ITEM");

                                }
                                file.flags.Add(fl);

                            }
                            else
                            {
                                ArchipelagoConsole.LogMessage("processed flag");
                                while (file.flags[i].flagValue > file.flags[pr].flagValue)
                                {
                                    if (file.IsInventoryFull())
                                    {
                                        ArchipelagoConsole.LogMessage("INVENTORY FULL COUDNT PROCESS ITEM");
                                        break;
                                    }
                                    file.flags[pr].flagValue++;
                                    ArchipelagoConsole.LogMessage("additem");
                                    file.AddInventoryItem(Game.Data.Items.Get(Util.itemflagtoid(flagint)));
                                    ArchipelagoConsole.LogMessage("OBTAINED " + Game.Data.Items.Get(flagint).itemName + " ITEM");

                                }

                            }

                        }
            

                    }
                }
            }
            */
        }

        /// <summary>
        /// HELPER METHOD TO CONVERT AN ITEM ID TO A FLAG/ARCHIPELAGO ID SINCE THERE ARE 5 UNIQUE/SHOE GIFT ITEMS
        /// IN THE GAME BUT YOU CAN ONLY GIVE 4 TO A GIRL 
        /// </summary>
        public static int idtoflag(int id)
        {
            if (id == 130) { return 69420093; }
            else if (id == 131) { return 69420094; }
            else if (id == 132) { return 69420095; }
            else if (id == 133) { return 69420096; }
            else if (id == 189) { return 69420141; }
            else if (id == 190) { return 69420142; }
            else if (id == 191) { return 69420143; }
            else if (id == 192) { return 69420144; }
             
            else if (id == 134) { return 69420097; }
            else if (id == 135) { return 69420098; }
            else if (id == 136) { return 69420099; }
            else if (id == 137) { return 69420100; }
            else if (id == 195) { return 69420145; }
            else if (id == 196) { return 69420146; }
            else if (id == 197) { return 69420147; }
            else if (id == 198) { return 69420148; }
             
            else if (id == 139) { return 69420101; }
            else if (id == 140) { return 69420102; }
            else if (id == 141) { return 69420103; }
            else if (id == 142) { return 69420104; }
            else if (id == 199) { return 69420149; }
            else if (id == 200) { return 69420150; }
            else if (id == 201) { return 69420151; }
            else if (id == 203) { return 69420152; }
             
            else if (id == 144) { return 69420105; }
            else if (id == 145) { return 69420106; }
            else if (id == 147) { return 69420107; }
            else if (id == 148) { return 69420108; }
            else if (id == 204) { return 69420153; }
            else if (id == 205) { return 69420154; }
            else if (id == 206) { return 69420155; }
            else if (id == 207) { return 69420156; }
             
            else if (id == 149) { return 69420109; }
            else if (id == 150) { return 69420110; }
            else if (id == 151) { return 69420111; }
            else if (id == 152) { return 69420112; }
            else if (id == 209) { return 69420157; }
            else if (id == 210) { return 69420158; }
            else if (id == 212) { return 69420159; }
            else if (id == 213) { return 69420160; }
             
            else if (id == 154) { return 69420113; }
            else if (id == 155) { return 69420114; }
            else if (id == 156) { return 69420115; }
            else if (id == 157) { return 69420116; }
            else if (id == 215) { return 69420161; }
            else if (id == 216) { return 69420162; }
            else if (id == 217) { return 69420163; }
            else if (id == 218) { return 69420164; }
             
            else if (id == 159) { return 69420117; }
            else if (id == 160) { return 69420118; }
            else if (id == 162) { return 69420119; }
            else if (id == 163) { return 69420120; }
            else if (id == 219) { return 69420165; }
            else if (id == 221) { return 69420166; }
            else if (id == 222) { return 69420167; }
            else if (id == 223) { return 69420168; }
             
            else if (id == 164) { return 69420121; }
            else if (id == 166) { return 69420122; }
            else if (id == 167) { return 69420123; }
            else if (id == 168) { return 69420124; }
            else if (id == 225) { return 69420169; }
            else if (id == 226) { return 69420170; }
            else if (id == 227) { return 69420171; }
            else if (id == 228) { return 69420172; }
             
            else if (id == 169) { return 69420125; }
            else if (id == 170) { return 69420126; }
            else if (id == 171) { return 69420127; }
            else if (id == 173) { return 69420128; }
            else if (id == 230) { return 69420173; }
            else if (id == 231) { return 69420174; }
            else if (id == 232) { return 69420175; }
            else if (id == 233) { return 69420176; }
             
            else if (id == 174) { return 69420129; }
            else if (id == 175) { return 69420130; }
            else if (id == 177) { return 69420131; }
            else if (id == 178) { return 69420132; }
            else if (id == 234) { return 69420177; }
            else if (id == 235) { return 69420178; }
            else if (id == 236) { return 69420179; }
            else if (id == 237) { return 69420180; }
            
            else if (id == 179) { return 69420133; }
            else if (id == 180) { return 69420134; }
            else if (id == 181) { return 69420135; }
            else if (id == 182) { return 69420136; }
            else if (id == 239) { return 69420181; }
            else if (id == 240) { return 69420182; }
            else if (id == 241) { return 69420183; }
            else if (id == 243) { return 69420184; }
            
            else if (id == 184) { return 69420137; }
            else if (id == 185) { return 69420138; }
            else if (id == 186) { return 69420139; }
            else if (id == 187) { return 69420140; }
            else if (id == 244) { return 69420185; }
            else if (id == 245) { return 69420186; }
            else if (id == 246) { return 69420187; }
            else if (id == 247) { return 69420188; }
            else { return -1; }
        }

        /// <summary>
        /// HELPER METHOD TO CONVERT A FLAG/ARCHIPELAGO ID TO AN ITEM ID SINCE THERE ARE 5 UNIQUE/SHOE GIFT ITEMS
        /// IN THE GAME BUT YOU CAN ONLY GIVE 4 TO A GIRL 
        /// </summary>
        public static int flagtoid(int flag)
        {
            if (flag == 69420093) { return 130; }
            else if (flag == 69420094) { return 131; }
            else if (flag == 69420095) { return 132; }
            else if (flag == 69420096) { return 133; }
            else if (flag == 69420141) { return 189; }
            else if (flag == 69420142) { return 190; }
            else if (flag == 69420143) { return 191; }
            else if (flag == 69420144) { return 192; }

            else if (flag == 69420097) { return 134; }
            else if (flag == 69420098) { return 135; }
            else if (flag == 69420099) { return 136; }
            else if (flag == 69420100) { return 137; }
            else if (flag == 69420145) { return 195; }
            else if (flag == 69420146) { return 196; }
            else if (flag == 69420147) { return 197; }
            else if (flag == 69420148) { return 198; }

            else if (flag == 69420101) { return 139; }
            else if (flag == 69420102) { return 140; }
            else if (flag == 69420103) { return 141; }
            else if (flag == 69420104) { return 142; }
            else if (flag == 69420149) { return 199; }
            else if (flag == 69420150) { return 200; }
            else if (flag == 69420151) { return 201; }
            else if (flag == 69420152) { return 203; }

            else if (flag == 69420105) { return 144; }
            else if (flag == 69420106) { return 145; }
            else if (flag == 69420107) { return 147; }
            else if (flag == 69420108) { return 148; }
            else if (flag == 69420153) { return 204; }
            else if (flag == 69420154) { return 205; }
            else if (flag == 69420155) { return 206; }
            else if (flag == 69420156) { return 207; }

            else if (flag == 69420109) { return 149; }
            else if (flag == 69420110) { return 150; }
            else if (flag == 69420111) { return 151; }
            else if (flag == 69420112) { return 152; }
            else if (flag == 69420157) { return 209; }
            else if (flag == 69420158) { return 210; }
            else if (flag == 69420159) { return 212; }
            else if (flag == 69420160) { return 213; }

            else if (flag == 69420113) { return 154; }
            else if (flag == 69420114) { return 155; }
            else if (flag == 69420115) { return 156; }
            else if (flag == 69420116) { return 157; }
            else if (flag == 69420161) { return 215; }
            else if (flag == 69420162) { return 216; }
            else if (flag == 69420163) { return 217; }
            else if (flag == 69420164) { return 218; }

            else if (flag == 69420117) { return 159; }
            else if (flag == 69420118) { return 160; }
            else if (flag == 69420119) { return 162; }
            else if (flag == 69420120) { return 163; }
            else if (flag == 69420165) { return 219; }
            else if (flag == 69420166) { return 221; }
            else if (flag == 69420167) { return 222; }
            else if (flag == 69420168) { return 223; }

            else if (flag == 69420121) { return 164; }
            else if (flag == 69420122) { return 166; }
            else if (flag == 69420123) { return 167; }
            else if (flag == 69420124) { return 168; }
            else if (flag == 69420169) { return 225; }
            else if (flag == 69420170) { return 226; }
            else if (flag == 69420171) { return 227; }
            else if (flag == 69420172) { return 228; }

            else if (flag == 69420125) { return 169; }
            else if (flag == 69420126) { return 170; }
            else if (flag == 69420127) { return 171; }
            else if (flag == 69420128) { return 173; }
            else if (flag == 69420173) { return 230; }
            else if (flag == 69420174) { return 231; }
            else if (flag == 69420175) { return 232; }
            else if (flag == 69420176) { return 233; }

            else if (flag == 69420129) { return 174; }
            else if (flag == 69420130) { return 175; }
            else if (flag == 69420131) { return 177; }
            else if (flag == 69420132) { return 178; }
            else if (flag == 69420177) { return 234; }
            else if (flag == 69420178) { return 235; }
            else if (flag == 69420179) { return 236; }
            else if (flag == 69420180) { return 237; }

            else if (flag == 69420133) { return 179; }
            else if (flag == 69420134) { return 180; }
            else if (flag == 69420135) { return 181; }
            else if (flag == 69420136) { return 182; }
            else if (flag == 69420181) { return 239; }
            else if (flag == 69420182) { return 240; }
            else if (flag == 69420183) { return 241; }
            else if (flag == 69420184) { return 243; }

            else if (flag == 69420137) { return 184; }
            else if (flag == 69420138) { return 185; }
            else if (flag == 69420139) { return 186; }
            else if (flag == 69420140) { return 187; }
            else if (flag == 69420185) { return 244; }
            else if (flag == 69420186) { return 245; }
            else if (flag == 69420187) { return 246; }
            else if (flag == 69420188) { return 247; }
            else { return -1; }

        }


        /// <summary>
        /// HELPER METHOD TO CONVERT A ITEM FLAG ID TO AN ITEM ID
        /// </summary>
        public static int itemflagtoid(int flag)
        {
            if (flag == 69420346) { return 250; }
            else if (flag == 69420347) { return 251; }
            else if (flag == 69420348) { return 252; }
            else if (flag == 69420349) { return 253; }
            else if (flag == 69420350) { return 254; }
            else if (flag == 69420351) { return 255; }
            else if (flag == 69420352) { return 256; }
            else if (flag == 69420353) { return 257; }
            else if (flag == 69420354) { return 258; }
            else if (flag == 69420355) { return 259; }
            else if (flag == 69420356) { return 261; }
            else if (flag == 69420357) { return 262; }
            else if (flag == 69420358) { return 263; }
            else if (flag == 69420359) { return 264; }
            else if (flag == 69420360) { return 265; }
            else if (flag == 69420361) { return 266; }
            else if (flag == 69420362) { return 268; }
            else if (flag == 69420363) { return 25; }
            else if (flag == 69420364) { return 26; }
            else if (flag == 69420365) { return 27; }
            else if (flag == 69420366) { return 28; }
            else if (flag == 69420367) { return 29; }
            else if (flag == 69420368) { return 30; }
            else if (flag == 69420369) { return 32; }
            else if (flag == 69420370) { return 31; }
            else if (flag == 69420371) { return 33; }
            else if (flag == 69420372) { return 284; }
            else if (flag == 69420373) { return 285; }
            else if (flag == 69420374) { return 286; }
            else if (flag == 69420375) { return 287; }
            else if (flag == 69420376) { return 288; }
            else if (flag == 69420377) { return 289; }
            else if (flag == 69420378) { return 34; }
            else if (flag == 69420379) { return 35; }
            else if (flag == 69420380) { return 36; }
            else if (flag == 69420381) { return 37; }
            else if (flag == 69420382) { return 38; }
            else if (flag == 69420383) { return 39; }
            else if (flag == 69420384) { return 41; }
            else if (flag == 69420385) { return 40; }
            else if (flag == 69420386) { return 42; }
            else if (flag == 69420387) { return 43; }
            else if (flag == 69420388) { return 44; }
            else if (flag == 69420389) { return 45; }
            else if (flag == 69420390) { return 46; }
            else if (flag == 69420391) { return 47; }
            else if (flag == 69420392) { return 48; }
            else if (flag == 69420393) { return 50; }
            else if (flag == 69420394) { return 49; }
            else if (flag == 69420395) { return 51; }
            else if (flag == 69420396) { return 52; }
            else if (flag == 69420397) { return 249; }
            else if (flag == 69420398) { return 294; }
            else if (flag == 69420399) { return 295; }
            else if (flag == 69420400) { return 296; }
            else if (flag == 69420401) { return 297; }
            else if (flag == 69420402) { return 298; }
            else if (flag == 69420403) { return 299; }
            else if (flag == 69420404) { return 300; }
            else if (flag == 69420405) { return 301; }
            else if (flag == 69420406) { return 269; }
            else if (flag == 69420407) { return 270; }
            else if (flag == 69420408) { return 271; }
            else if (flag == 69420409) { return 272; }
            else if (flag == 69420410) { return 273; }
            else if (flag == 69420411) { return 274; }
            else if (flag == 69420412) { return 275; }
            else if (flag == 69420413) { return 276; }
            else if (flag == 69420414) { return 277; }
            else if (flag == 69420415) { return 278; }
            else if (flag == 69420416) { return 279; }
            else if (flag == 69420417) { return 280; }
            else if (flag == 69420418) { return 281; }
            else if (flag == 69420419) { return 282; }
            else if (flag == 69420420) { return 283; }

            return 0;
        }

    }
}
