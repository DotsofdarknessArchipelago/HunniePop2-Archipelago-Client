﻿using Archipelago.MultiClient.Net;
using BepInEx;
using HarmonyLib;
using HunniePop2ArchipelagoClient.Archipelago;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace HunniePop2ArchipelagoClient.Utils
{
    public class Patches
    {

        public static ArchipelagoClient arch;
        public static void patch(ArchipelagoClient a)
        {
            arch = a;
            //ArchipelagoConsole.LogMessage("DOING PATCHES");

            Harmony.CreateAndPatchAll(typeof(Patches));
        }


        [HarmonyPatch(typeof(PuzzleStatus), "AddPuzzleReward")]
        [HarmonyPrefix]
        public static void puzzleautocomplete(PuzzleStatus __instance)
        {
            //if (__instance.bonusRound) { return; }
            //__instance.AddResourceValue(PuzzleResourceType.AFFECTION, 100000, false);
        }


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

        [HarmonyPatch(typeof(UiCellphoneAppNew), "OnStartButtonPressed")]
        [HarmonyPrefix]
        public static bool Newsavefileoverite(UiCellphoneAppNew __instance, ref UiTitleCanvas ____titleCanvas, ref int ____newSaveFileIndex, ref UiAppFileIconSlot ____selectedFileIconSlot)
        {

            //ArchipelagoConsole.LogMessage("testing");
            if (ArchipelagoClient.Authenticated && !Game.Persistence.playerData.files[3].started)
            {
                int savindex = 3;//____newSaveFileIndex;
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
                playerFile.SetFlagValue("skip_tutorial", 1); ////

                playerFile.SetFlagValue(ArchipelagoClient.seed(), 1);

                for (int g=1; g<13; g++)
                {
                    PlayerFileGirl gl =playerFile.GetPlayerFileGirl(Game.Data.Girls.Get(g));
                    for (int g2=0; g2<gl.girlDefinition.baggageItemDefs.Count(); g2++)
                    {
                        if (gl.girlDefinition.baggageItemDefs[g2] != Util.baggagestuff())
                        {
                            gl.girlDefinition.baggageItemDefs[g2] = Util.baggagestuff();
                        }
                    }
                }
                for (int p = 1; p < 25; p++)
                {
                    playerFile.GetPlayerFileGirlPair(Game.Data.GirlPairs.Get(p));
                }

                //playerFile = Util.removebaggage(playerFile);

                //playerFile.GetPlayerFileGirl(Game.Data.Girls.Get(1)).playerMet = true;
                //playerFile.GetPlayerFileGirl(Game.Data.Girls.Get(6)).playerMet = true;
                //playerFile.GetPlayerFileGirl(Game.Data.Girls.Get(11)).playerMet = true;
                //playerFile.GetPlayerFileGirl(Game.Data.Girls.Get(10)).playerMet = true;
                //playerFile.GetPlayerFileGirl(Game.Data.Girls.Get(12)).playerMet = true;

                //playerFile.GetPlayerFileGirlPair(Game.Data.GirlPairs.Get(1)).relationshipType = GirlPairRelationshipType.COMPATIBLE;
                //playerFile.GetPlayerFileGirlPair(Game.Data.GirlPairs.Get(4)).relationshipType = GirlPairRelationshipType.COMPATIBLE;
                //playerFile.GetPlayerFileGirlPair(Game.Data.GirlPairs.Get(26)).relationshipType = GirlPairRelationshipType.LOVERS;

                //playerFile.metGirlPairs.Add(Game.Data.GirlPairs.Get(1));
                //playerFile.metGirlPairs.Add(Game.Data.GirlPairs.Get(4));



                int i;
                bool b = true;
                ArchipelagoConsole.LogMessage(ArchipelagoClient.itemstoprocess.Count.ToString());
                while (b){
                    if (ArchipelagoClient.itemstoprocess.Count > 0)
                    {
                        ArchipelagoConsole.LogMessage("PROCESSING ITEM");
                        playerFile.SetFlagValue(ArchipelagoClient.itemstoprocess.Dequeue().ToString(), 0);
                    }
                    else
                    {
                        b = false;
                    }
                }

                Util.archflagprocess(playerFile);




                playerFile.finderSlots = Util.genfinder(playerFile.girlPairs);
                playerFile.storeProducts = Util.genStore(playerFile);

                playerFile.storyProgress = 7;
                playerFile.daytimeElapsed = 10;
                playerFile.finderRestockTime = 10;
                playerFile.storeRestockDay = 2;

                playerFile.fruitCounts[0] = 25;
                playerFile.fruitCounts[1] = 25;
                playerFile.fruitCounts[2] = 25;
                playerFile.fruitCounts[3] = 25;

                playerFile.locationDefinition = Game.Data.Locations.Get(21); ////

                Game.Persistence.Apply(savindex);
                Game.Persistence.SaveGame();
                ____titleCanvas.LoadGame(savindex, "MainScene");
            }
            if (Game.Persistence.playerData.files[3].started)
            {
                Util.cellerror("PLEASE ERASE THE BOTTOM RIGHT SAVE FILE TO START A GAME", __instance.cellphone);
            }
            else if (!ArchipelagoClient.Authenticated)
            {
                Util.cellerror("PLEASE CONNECT TO ARCHIPELAGO SERVER(Top Right) TO START GAME", __instance.cellphone);
            }



            return false;
        }

        [HarmonyPatch(typeof(PlayerFile), "PopulateStoreProducts")]
        [HarmonyPrefix]
        public static bool shoppopulate(PlayerFile __instance)
        {
            List<int> pairints = new List<int>();

            for (int i = 0; i < __instance.metGirlPairs.Count; i++) { pairints.Add(__instance.metGirlPairs[i].id); }

            __instance.storeProducts = Util.genStore(__instance);

            return false;
        }


        [HarmonyPatch(typeof(PlayerFile), "PopulateFinderSlots")]
        [HarmonyILManipulator]
        public static void finderslotmodificaions(ILContext ctx, MethodBase orig)
        {
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                if (ctx.Instrs[i].Offset >= 378 && ctx.Instrs[i].Offset <= 537)
                {
                    //ArchipelagoConsole.LogMessage("OLD INSTRUCTION@" + i.ToString() + " :: " + ctx.Instrs[i].Offset.ToString() + " :: " + ctx.Instrs[i].Offset.ToString("X") + " :: " + ctx.Instrs[i].OpCode.ToString());
                    ctx.Instrs[i].OpCode = OpCodes.Nop;
                    ctx.Instrs[i].Operand = null;
                    //ArchipelagoConsole.LogMessage("NEW INSTRUCTION@" + i.ToString() + " :: " + ctx.Instrs[i].Offset.ToString() + " :: " + ctx.Instrs[i].Offset.ToString("X") + " :: " + ctx.Instrs[i].OpCode.ToString());
                }
                if (ctx.Instrs[i].Offset == 563) { ctx.Instrs[i].OpCode = OpCodes.Brfalse; }

            }

        }


        [HarmonyPatch(typeof(LocationManager), "Depart")]
        [HarmonyPostfix]
        public static void processarch()
        {
            if (Game.Persistence.playerFile.storyProgress >= 12)
            {
                ArchipelagoClient.complete();
            }

            bool b = true;
            while (b)
            {
                if (ArchipelagoClient.itemstoprocess.Count != 0)
                {
                    Game.Persistence.playerFile.SetFlagValue(ArchipelagoClient.itemstoprocess.Dequeue().ToString(), 0);
                }
                else
                {
                    b=false;
                }
            }

            Util.archflagprocess(Game.Persistence.playerFile);

        }

        [HarmonyPatch(typeof(UiCellphoneAppCode), "OnSubmitButtonPressed")]
        [HarmonyPrefix]
        public static bool codestuff(UiCellphoneAppCode __instance)
        {
            string input = __instance.inputField.text.ToUpper().Trim();
            if (input == "TEST")
            {
                for (int j = 0; j < Game.Data.Items.GetAllOfTypes(ItemType.BAGGAGE).Count; j++)
                {

                    //ArchipelagoConsole.LogMessage("ID:"+ j.ToString()+" : "+ Game.Data.Items.GetAllOfTypes(ItemType.BAGGAGE)[j].itemName);

                }
            }
            if (input == "CONNECT")
            {
                //ArchipelagoConsole.LogMessage("NEW GAME");
            }

            if (input == "NEWGAME")
            {
                //ArchipelagoConsole.LogMessage("NEW GAME");
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerFileGirl), "HasBaggage")]
        [HarmonyPrefix]
        public static bool hasbaggageoverite(ItemDefinition baggageDef, PlayerFileGirl __instance, ref bool __result)
        {
            __result = (Game.Data.Girls.Get(__instance.girlDefinition.id).baggageItemDefs.Contains(baggageDef) || (baggageDef == Util.baggagestuff()));
            return false;
        }

        [HarmonyPatch(typeof(SaveFileGirl), "Reset")]
        [HarmonyPostfix]
        public static void savfilegirlresetmod(SaveFileGirl __instance)
        {
            __instance.activeBaggageIndex = UnityEngine.Random.Range(0, 2);
            __instance.learnedBaggage.Add(0);
            __instance.learnedBaggage.Add(1);
            __instance.learnedBaggage.Add(2);
        }

        [HarmonyPatch(typeof(UiCellphoneTrashZone), "OnDrop")]
        [HarmonyPrefix]
        public static void uniquetrash(Draggable draggable, UiCellphoneTrashZone __instance)
        {
            //ArchipelagoConsole.LogMessage("ITEM TRASHED");
            if (draggable.type == DraggableType.INVENTORY_SLOT && (draggable.GetItemDefinition().itemType == ItemType.UNIQUE_GIFT || draggable.GetItemDefinition().itemType == ItemType.SHOES))
            {
                //ArchipelagoConsole.LogMessage("GIFT ITEM TRASHED with ID:" + draggable.GetItemDefinition().id.ToString());
                Game.Persistence.playerFile.SetFlagValue(Util.idtoflag(draggable.GetItemDefinition().id).ToString(), 2);
            }
        }

        [HarmonyPatch(typeof(UiCellphoneAppStore), "OnProductPurchased")]
        [HarmonyPrefix]
        public static void giftpurchase(ItemSlotBehavior itemSlotBehavior )
        {
            if (itemSlotBehavior.itemDefinition.itemType == ItemType.UNIQUE_GIFT || itemSlotBehavior.itemDefinition.itemType == ItemType.SHOES)
            {
                Game.Persistence.playerFile.SetFlagValue(Util.idtoflag(itemSlotBehavior.itemDefinition.id).ToString(), 1);
            }
        }


        // ARCH CHECKS AREA
        [HarmonyPatch(typeof(PlayerFileGirlPair), "RelationshipLevelUp")]
        [HarmonyPrefix]
        public static bool relationshiplvup(PlayerFileGirlPair __instance)
        {
            if (__instance.relationshipType == GirlPairRelationshipType.COMPATIBLE)
            {
                __instance.relationshipType++;
                Archipelago.ArchipelagoClient.sendloc(69420000 + __instance.girlPairDefinition.id);
               // ArchipelagoConsole.LogMessage("RELATIONSHIP LV UP TO ATTRACTED: PAIR ID : " + __instance.girlPairDefinition.id.ToString());
                Game.Persistence.playerFile.GetPlayerFileGirl(__instance.girlPairDefinition.girlDefinitionOne).relationshipUpCount++;
                Game.Persistence.playerFile.GetPlayerFileGirl(__instance.girlPairDefinition.girlDefinitionTwo).relationshipUpCount++;
                Game.Persistence.playerFile.relationshipUpCount++;
            }
            else if (__instance.relationshipType == GirlPairRelationshipType.ATTRACTED)
            {
                __instance.relationshipType++;
                Archipelago.ArchipelagoClient.sendloc(69420024 + __instance.girlPairDefinition.id);
                //ArchipelagoConsole.LogMessage("RELATIONSHIP LV UP TO LOVERS: PAIR ID : " + __instance.girlPairDefinition.id.ToString());
                Game.Persistence.playerFile.GetPlayerFileGirl(__instance.girlPairDefinition.girlDefinitionOne).relationshipUpCount++;
                Game.Persistence.playerFile.GetPlayerFileGirl(__instance.girlPairDefinition.girlDefinitionTwo).relationshipUpCount++;
                Game.Persistence.playerFile.relationshipUpCount++;
            }
            return false;
        }

        [HarmonyPatch(typeof(PlayerFileGirl), "LearnFavAnswer")]
        [HarmonyPostfix]
        public static void questioncheck(QuestionDefinition questionDef, bool __result, PlayerFileGirl __instance)
        {
            if (__result == false) { return; }

            Archipelago.ArchipelagoClient.sendloc(69420144 + (__instance.girlDefinition.id-1)*20 + questionDef.id);
            //ArchipelagoConsole.LogMessage("LEARNED FAV QUESTION FROM : " + __instance.girlDefinition.name + " : QUESTION NAME Name : " + questionDef.name + " : locationID : " + (69420144 + (__instance.girlDefinition.id - 1) * 20 + questionDef.id));
        }

        [HarmonyPatch(typeof(PlayerFileGirl), "ReceiveUnique")]
        [HarmonyPostfix]
        public static void uniquecheck(ItemDefinition uniqueDef, bool __result, PlayerFileGirl __instance)
        {
            if (__result == false) { return; }
            //ArchipelagoConsole.LogMessage("RECIEVED UNIQUE ITEM : " + __instance.girlDefinition.name + " : QUESTION NAME IS : " + uniqueDef.name + " : QUESTION ID IS : " + uniqueDef.id);
        }

        [HarmonyPatch(typeof(PlayerFileGirl), "ReceiveShoes")]
        [HarmonyPostfix]
        public static void shoecheck(ItemDefinition shoesDef, bool __result, PlayerFileGirl __instance)
        {
            if (__result == false) { return; }
            //ArchipelagoConsole.LogMessage("RECIEVED SHOE : " + __instance.girlDefinition.name + " : QUESTION NAME IS : " + shoesDef.name + " : QUESTION ID IS : " + shoesDef.id);
        }

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

            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }
            flag++;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }
            flag++;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }
            flag++;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }

            if (ofLevel){__result = 0;}
            else{__result = exp;}
            return false;
        }

        [HarmonyPatch(typeof(PlayerFile), "GetPassionLevelExp")]
        [HarmonyPrefix]
        public static bool passionexp(bool ofLevel, PlayerFile __instance, ref int __result)
        {
            int exp = 0;
            int flag = 69420041;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }
            flag++;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }
            flag++;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }
            flag++;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }
            flag++;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }
            flag++;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }
            flag++;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }
            flag++;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }

            if (ofLevel) { __result = 0; }
            else { __result = exp; }
            return false;
        }

        [HarmonyPatch(typeof(PlayerFile), "GetStyleLevelExp")]
        [HarmonyPrefix]
        public static bool styleexp(bool ofLevel, PlayerFile __instance, ref int __result)
        {
            int exp = 0;
            int flag = 69420049;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }
            flag++;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }
            flag++;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }
            flag++;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }
            flag++;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }
            flag++;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }
            flag++;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }
            flag++;
            if (__instance.GetFlagValue(flag.ToString()) == 1) { exp += 6; }

            if (ofLevel) { __result = 0; }
            else { __result = exp; }
            return false;
        }


    }

    public class Util
    {

        public static async void cellerror(string s, UiCellphone c)
        {
            c.phoneErrorMsg.ShowMessage(s);
            await Task.Delay(5000);
            c.phoneErrorMsg.ClearMessage();
        }

        public static List<PlayerFileFinderSlot> genfinder(List<PlayerFileGirlPair> filepair)
        {


            List<GirlPairDefinition> pair = new List<GirlPairDefinition>();

            for (int i = 0; i < filepair.Count; i++)
            {
                if (filepair[i].girlPairDefinition.girlDefinitionOne.id == 13 || filepair[i].girlPairDefinition.girlDefinitionTwo.id == 13) { continue; }
                if (filepair[i].relationshipType != GirlPairRelationshipType.UNKNOWN) { pair.Add(filepair[i].girlPairDefinition); }
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
                a = Math.Min(a, pair.Count - 1);

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

        public static List<PlayerFileStoreProduct> genStore(PlayerFile file)
        {

            List<PlayerFileStoreProduct> store = new List<PlayerFileStoreProduct>();
            List<ItemDefinition> girlgifts = new List<ItemDefinition>();
            List<ItemDefinition> food = Game.Data.Items.GetAllOfTypes(ItemType.FOOD);
            List<ItemDefinition> date = Game.Data.Items.GetAllOfTypes(ItemType.DATE_GIFT);
            /*
            for (int i = 0; i < file.girls.Count; i++)
            {
                if (file.girls[i].playerMet)
                {
                    List<ItemDefinition> tmpunique = file.girls[i].girlDefinition.uniqueItemDefs;
                    List<ItemDefinition> tmpshoe = file.girls[i].girlDefinition.shoesItemDefs;

                    for (int j = tmpshoe.Count - 1; j >= 0; j--)
                    {
                        if (file.girls[i].HasShoes(tmpshoe[j]) || file.IsItemInInventory(tmpshoe[j], true)) { tmpshoe.RemoveAt(j); }
                    }
                    for (int k = tmpunique.Count - 1; k >= 0; k--)
                    {
                        if (file.girls[i].HasUnique(tmpunique[k]) || file.IsItemInInventory(tmpunique[k], true)) { tmpunique.RemoveAt(k); }
                    }

                    girlgifts.AddRange(tmpshoe);
                    girlgifts.AddRange(tmpunique);

                }
            }
            
            if (girlgifts.Count >= 1)
            {
                int num = UnityEngine.Random.Range(0, girlgifts.Count - 1);
                store.Add(genproduct(0, girlgifts[num], UnityEngine.Random.Range(1, 5)));
                girlgifts.RemoveAt(num);
            }
            else
            {
                int num = UnityEngine.Random.Range(0, food.Count - 1);
                store.Add(genproduct(0, food[num], UnityEngine.Random.Range(1, 5)));
                food.RemoveAt(num);
            }
            if (girlgifts.Count >= 1)
            {
                int num = UnityEngine.Random.Range(0, girlgifts.Count - 1);
                store.Add(genproduct(1, girlgifts[num], UnityEngine.Random.Range(1, 5)));
                girlgifts.RemoveAt(num);
            }
            else
            {
                int num = UnityEngine.Random.Range(0, date.Count - 1);
                store.Add(genproduct(1, date[num], UnityEngine.Random.Range(1, 5)));
                date.RemoveAt(num);
            }
            for (int i = 2; i < 32; i++)
            {
                int ran = UnityEngine.Random.Range(0, 100);
                if (girlgifts.Count >= 1)
                {
                    if (ran % 2 == 0)
                    {
                        int num = UnityEngine.Random.Range(0, girlgifts.Count - 1);
                        store.Add(genproduct(i, girlgifts[num], UnityEngine.Random.Range(1, 5)));
                        girlgifts.RemoveAt(num);
                    }
                    else if (ran >= 50)
                    {
                        int num = UnityEngine.Random.Range(0, food.Count - 1);
                        store.Add(genproduct(i, food[num], UnityEngine.Random.Range(1, 5)));
                        food.RemoveAt(num);
                    }
                    else
                    {
                        int num = UnityEngine.Random.Range(0, date.Count - 1);
                        store.Add(genproduct(i, date[num], UnityEngine.Random.Range(1, 5)));
                        date.RemoveAt(num);
                    }
                }
                else if (ran >= 50)
                {
                    int num = UnityEngine.Random.Range(0, date.Count - 1);
                    store.Add(genproduct(i, date[num], UnityEngine.Random.Range(1, 5)));
                    date.RemoveAt(num);
                }
                else
                {
                    int num = UnityEngine.Random.Range(0, food.Count - 1);
                    store.Add(genproduct(i, food[num], UnityEngine.Random.Range(1, 5)));
                    food.RemoveAt(num);
                }
            }*/

            for (int f = 0; f < file.flags.Count; f++)
            {
                int.TryParse(file.flags[f].flagName, out int flagint);
                if (flagint > 69420092 && flagint < 69420189 && file.flags[f].flagValue == 2)
                {
                    ItemDefinition def = Game.Data.Items.Get(Util.flagtoid(flagint));
                    girlgifts.Add(def);
                }
            }

            for (int i = 0; i < 32; i++)
            {
                if (i % 7 == 0 && girlgifts.Count != 0)
                {
                    int num = UnityEngine.Random.Range(0, girlgifts.Count - 1);
                    store.Add(genproduct(i, girlgifts[num], UnityEngine.Random.Range(1, 5)));
                    girlgifts.RemoveAt(num);
                    continue;
                }

                int ran = UnityEngine.Random.Range(0, 100);
                if (ran >= 50)
                {
                    int num = UnityEngine.Random.Range(0, date.Count - 1);
                    store.Add(genproduct(i, date[num], UnityEngine.Random.Range(1, 5)));
                    date.RemoveAt(num);
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

        public static PlayerFileStoreProduct genproduct(int i, ItemDefinition item, int c)
        {
            PlayerFileStoreProduct product = new PlayerFileStoreProduct();
            product.productIndex = i;
            product.itemDefinition = item;
            product.itemCost = c;
            return product;

        }

        public static ItemDefinition baggagestuff()
        {
            ItemDefinition newbagage = Game.Data.Items.Get(103);

            newbagage.itemDescription = "temp baggage that does nothing so you can have access to all date gift slots and can give all uniques and shoes";
            newbagage.ailmentDefinition.enableType = AilmentEnableType.NONE;
            newbagage.baggageGirl = EditorDialogTriggerTab.KYU;
            newbagage.itemName = "TEMP BAGGAGE";
            for (int i = newbagage.ailmentDefinition.triggers.Count() - 1; i >= 0; i--)
            {
                newbagage.ailmentDefinition.triggers.RemoveAt(i);
            }
            //newbagage.itemSprite = new Sprite();

            return newbagage;

        }

        public static PlayerFile removebaggage(PlayerFile orig)
        {
            PlayerFile edit = orig;
            for (int i = 0; i < edit.girls.Count(); i++)
            {
                for (int j = 0; j < edit.girls[i].girlDefinition.baggageItemDefs.Count(); j++)
                {
                    edit.girls[i].girlDefinition.baggageItemDefs[j].ailmentDefinition = null;
                }
            }

            return edit;
        }

        public static void archflagprocess(PlayerFile file)
        {
            //ArchipelagoConsole.LogMessage(ArchipelagoClient.itemstoprocess.Dequeue().ToString());
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
                    else if (flagint > 69420225 && flagint < 69420345)
                    {
                        //OUTFIT STUFF
                        //ArchipelagoConsole.LogMessage("PROCESSING OUTFIT");
                        //ArchipelagoConsole.LogMessage(file.flags[i].flagName + " IS OUTFIT PROCESSED");
                        file.flags[i].flagValue = 1;
                    }
                    else
                    {

                    }
                }
            }
        }

        public static int idtoflag(int id)
        {
            if (id == 129) { return 69420093; }
            else if(id == 130) { return 69420094; }
            else if (id == 131) { return 69420095; }
            else if (id == 132) { return 69420096; }
            else if (id == 189) { return 69420141; }
            else if (id == 190) { return 69420142; }
            else if (id == 191) { return 69420143; }
            else if (id == 192) { return 69420144; }
             
            else if (id == 134) { return 69420097; }
            else if (id == 135) { return 69420098; }
            else if (id == 136) { return 69420099; }
            else if (id == 137) { return 69420100; }
            else if (id == 194) { return 69420145; }
            else if (id == 195) { return 69420146; }
            else if (id == 196) { return 69420147; }
            else if (id == 197) { return 69420148; }
             
            else if (id == 139) { return 69420101; }
            else if (id == 140) { return 69420102; }
            else if (id == 141) { return 69420103; }
            else if (id == 142) { return 69420104; }
            else if (id == 199) { return 69420149; }
            else if (id == 200) { return 69420150; }
            else if (id == 201) { return 69420151; }
            else if (id == 202) { return 69420152; }
             
            else if (id == 144) { return 69420105; }
            else if (id == 145) { return 69420106; }
            else if (id == 146) { return 69420107; }
            else if (id == 147) { return 69420108; }
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
            else if (id == 211) { return 69420159; }
            else if (id == 212) { return 69420160; }
             
            else if (id == 154) { return 69420113; }
            else if (id == 155) { return 69420114; }
            else if (id == 156) { return 69420115; }
            else if (id == 157) { return 69420116; }
            else if (id == 214) { return 69420161; }
            else if (id == 215) { return 69420162; }
            else if (id == 216) { return 69420163; }
            else if (id == 217) { return 69420164; }
             
            else if (id == 159) { return 69420117; }
            else if (id == 160) { return 69420118; }
            else if (id == 161) { return 69420119; }
            else if (id == 162) { return 69420120; }
            else if (id == 219) { return 69420165; }
            else if (id == 220) { return 69420166; }
            else if (id == 221) { return 69420167; }
            else if (id == 222) { return 69420168; }
             
            else if (id == 164) { return 69420121; }
            else if (id == 165) { return 69420122; }
            else if (id == 166) { return 69420123; }
            else if (id == 167) { return 69420124; }
            else if (id == 224) { return 69420169; }
            else if (id == 225) { return 69420170; }
            else if (id == 226) { return 69420171; }
            else if (id == 227) { return 69420172; }
             
            else if (id == 169) { return 69420125; }
            else if (id == 170) { return 69420126; }
            else if (id == 171) { return 69420127; }
            else if (id == 172) { return 69420128; }
            else if (id == 229) { return 69420173; }
            else if (id == 230) { return 69420174; }
            else if (id == 231) { return 69420175; }
            else if (id == 232) { return 69420176; }
             
            else if (id == 174) { return 69420129; }
            else if (id == 175) { return 69420130; }
            else if (id == 176) { return 69420131; }
            else if (id == 177) { return 69420132; }
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
            else if (id == 242) { return 69420184; }
            
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
        public static int flagtoid(int flag)
        {
            if (flag == 69420093) { return 129; }
            else if (flag == 69420094) { return 130; }
            else if (flag == 69420095) { return 131; }
            else if (flag == 69420096) { return 132; }
            else if (flag == 69420141) { return 189; }
            else if (flag == 69420142) { return 190; }
            else if (flag == 69420143) { return 191; }
            else if (flag == 69420144) { return 192; }

            else if (flag == 69420097) { return 134; }
            else if (flag == 69420098) { return 135; }
            else if (flag == 69420099) { return 136; }
            else if (flag == 69420100) { return 137; }
            else if (flag == 69420145) { return 194; }
            else if (flag == 69420146) { return 195; }
            else if (flag == 69420147) { return 196; }
            else if (flag == 69420148) { return 197; }

            else if (flag == 69420101) { return 139; }
            else if (flag == 69420102) { return 140; }
            else if (flag == 69420103) { return 141; }
            else if (flag == 69420104) { return 142; }
            else if (flag == 69420149) { return 199; }
            else if (flag == 69420150) { return 200; }
            else if (flag == 69420151) { return 201; }
            else if (flag == 69420152) { return 202; }

            else if (flag == 69420105) { return 144; }
            else if (flag == 69420106) { return 145; }
            else if (flag == 69420107) { return 146; }
            else if (flag == 69420108) { return 147; }
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
            else if (flag == 69420159) { return 211; }
            else if (flag == 69420160) { return 212; }

            else if (flag == 69420113) { return 154; }
            else if (flag == 69420114) { return 155; }
            else if (flag == 69420115) { return 156; }
            else if (flag == 69420116) { return 157; }
            else if (flag == 69420161) { return 214; }
            else if (flag == 69420162) { return 215; }
            else if (flag == 69420163) { return 216; }
            else if (flag == 69420164) { return 217; }

            else if (flag == 69420117) { return 159; }
            else if (flag == 69420118) { return 160; }
            else if (flag == 69420119) { return 161; }
            else if (flag == 69420120) { return 162; }
            else if (flag == 69420165) { return 219; }
            else if (flag == 69420166) { return 220; }
            else if (flag == 69420167) { return 221; }
            else if (flag == 69420168) { return 222; }

            else if (flag == 69420121) { return 164; }
            else if (flag == 69420122) { return 165; }
            else if (flag == 69420123) { return 166; }
            else if (flag == 69420124) { return 167; }
            else if (flag == 69420169) { return 224; }
            else if (flag == 69420170) { return 225; }
            else if (flag == 69420171) { return 226; }
            else if (flag == 69420172) { return 227; }

            else if (flag == 69420125) { return 169; }
            else if (flag == 69420126) { return 170; }
            else if (flag == 69420127) { return 171; }
            else if (flag == 69420128) { return 172; }
            else if (flag == 69420173) { return 229; }
            else if (flag == 69420174) { return 230; }
            else if (flag == 69420175) { return 231; }
            else if (flag == 69420176) { return 232; }

            else if (flag == 69420129) { return 174; }
            else if (flag == 69420130) { return 175; }
            else if (flag == 69420131) { return 176; }
            else if (flag == 69420132) { return 177; }
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
            else if (flag == 69420184) { return 242; }

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


    }
}