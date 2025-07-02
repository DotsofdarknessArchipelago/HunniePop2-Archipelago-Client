using HarmonyLib;
using HunniePop2ArchipelagoClient.Archipelago;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HunniePop2ArchipelagoClient.HuniePop2.Gameplay
{
    [HarmonyPatch]
    public class Puzzle
    {

        /// <summary>
        /// sets the affection goal to one that we want
        /// </summary>
        [HarmonyPatch(typeof(PuzzleStatus), "NextRound")]
        [HarmonyPostfix]
        public static void affectionoverite(PuzzleStatus __instance, ref int ____affectionGoal, ref int ____affection)
        {
            //make sure it isnt a bonus round since we dont really care about that
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
                //make sure that the starting affection is 0
                ____affection = 0;
            }
        }


        /// <summary>
        /// set the amount of puzzle moves to one we want
        /// </summary>
        [HarmonyPatch(typeof(PuzzleStatus), "initialMovesRemaining", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool moveoverite(ref int __result, PuzzleStatus __instance)
        {
            //if the puzzle is the boss fight set the moves to be 4x their value caped at 999
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


        /// <summary>
        /// sets the maximum amount of moves to 999 so that when we set the starting moves it doent get cut off
        /// </summary>
        [HarmonyPatch(typeof(PuzzleStatus), "maxMovesRemaining", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool maxmoveoverite(ref int __result)
        {
            __result = 999;
            return false;
        }


        /// <summary>
        /// after the puzzle game is done check to see if outfit locations are done
        /// </summary>
        [HarmonyPatch(typeof(CutsceneStepSpecialPostRewards), "Start")]
        [HarmonyPostfix]
        public static void test1(ref PuzzleStatus ____puzzleStatus, ref bool ____puzzleFailure)
        {
            //check to see if we have outfit locations in logic
            if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["outfit_date_complete"]) && ____puzzleFailure)
            {
                ArchipelagoConsole.LogMessage("DATE NOT SUCCESSFUL OUTFIT LOCATIONS WILL NOT BE SENT PLZ TRY AGAIN");
                return;
            }

            //check left girls outfit to see if location is not already checked otherwise send location
            if (Game.Persistence.playerFile.GetFlagValue((____puzzleStatus.girlStatusLeft.playerFileGirl.girlDefinition.id.ToString() + ":" + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusLeft.girlDefinition).currentOutfitIndex.ToString())) == -1)
            {
                //set flag
                Game.Persistence.playerFile.SetFlagValue((____puzzleStatus.girlStatusLeft.playerFileGirl.girlDefinition.id.ToString() + ":" + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusLeft.girlDefinition).currentOutfitIndex.ToString()), 1);
                //send location
                ArchipelagoClient.sendloc(69420385 + ((____puzzleStatus.girlStatusLeft.girlDefinition.id - 1) * 10) + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusLeft.girlDefinition).currentOutfitIndex);
            }

            //check right girls outfit to see if location is not already checked otherwise send location
            if (Game.Persistence.playerFile.GetFlagValue((____puzzleStatus.girlStatusRight.playerFileGirl.girlDefinition.id.ToString() + ":" + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusRight.girlDefinition).currentOutfitIndex.ToString())) == -1)
            {
                //set flag
                Game.Persistence.playerFile.SetFlagValue((____puzzleStatus.girlStatusRight.playerFileGirl.girlDefinition.id.ToString() + ":" + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusRight.girlDefinition).currentOutfitIndex.ToString()), 1);
                //send location
                ArchipelagoClient.sendloc(69420385 + ((____puzzleStatus.girlStatusRight.girlDefinition.id - 1) * 10) + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusRight.girlDefinition).currentOutfitIndex);
            }
        }

        /// <summary>
        /// set the amount of puzzle moves to one we want
        /// </summary>
        [HarmonyPatch(typeof(PuzzleStatus), "Reset", [typeof(List<GirlDefinition>), typeof(bool)])]
        [HarmonyPrefix]
        public static void bosspairs(PuzzleStatus __instance, ref List<GirlDefinition> girlList)
        {
            if (girlList != null && girlList.Count == 10)
            {
                //ArchipelagoConsole.LogMessage($"PUZZLE RESET GIRL LIST COUNT:{girlList.Count}");
                foreach (GirlDefinition girl in girlList)
                {
                    //ArchipelagoConsole.LogMessage($"{girl.name}");
                }
                List<GirlDefinition> allBySpecial = Game.Data.Girls.GetAllBySpecial(false);

                if (ArchipelagoClient.ServerData.slotData["lola"].ToString() == 1.ToString())
                {
                    allBySpecial.Remove(Game.Data.Girls.Get(1));
                }
                if (ArchipelagoClient.ServerData.slotData["jessie"].ToString() == 1.ToString())
                {
                    allBySpecial.Remove(Game.Data.Girls.Get(2));
                }
                if (ArchipelagoClient.ServerData.slotData["lillian"].ToString() == 1.ToString())
                {
                    allBySpecial.Remove(Game.Data.Girls.Get(3));
                }
                if (ArchipelagoClient.ServerData.slotData["zoey"].ToString() == 1.ToString())
                {
                    allBySpecial.Remove(Game.Data.Girls.Get(4));
                }
                if (ArchipelagoClient.ServerData.slotData["sarah"].ToString() == 1.ToString())
                {
                    allBySpecial.Remove(Game.Data.Girls.Get(5));
                }
                if (ArchipelagoClient.ServerData.slotData["lailani"].ToString() == 1.ToString())
                {
                    allBySpecial.Remove(Game.Data.Girls.Get(6));
                }
                if (ArchipelagoClient.ServerData.slotData["candace"].ToString() == 1.ToString())
                {
                    allBySpecial.Remove(Game.Data.Girls.Get(7));
                }
                if (ArchipelagoClient.ServerData.slotData["nora"].ToString() == 1.ToString())
                {
                    allBySpecial.Remove(Game.Data.Girls.Get(8));
                }
                if (ArchipelagoClient.ServerData.slotData["brooke"].ToString() == 1.ToString())
                {
                    allBySpecial.Remove(Game.Data.Girls.Get(9));
                }
                if (ArchipelagoClient.ServerData.slotData["ashley"].ToString() == 1.ToString())
                {
                    allBySpecial.Remove(Game.Data.Girls.Get(10));
                }
                if (ArchipelagoClient.ServerData.slotData["abia"].ToString() == 1.ToString())
                {
                    allBySpecial.Remove(Game.Data.Girls.Get(11));
                }
                if (ArchipelagoClient.ServerData.slotData["polly"].ToString() == 1.ToString())
                {
                    allBySpecial.Remove(Game.Data.Girls.Get(12));
                }

                if (allBySpecial.Count < 8)
                {
                    while (allBySpecial.Count < 8)
                    {
                        ListUtils.ShuffleList<GirlDefinition>(allBySpecial);
                        allBySpecial.Add(allBySpecial[0]);
                    }
                }
                ListUtils.ShuffleList<GirlDefinition>(allBySpecial);
                while (allBySpecial.Count > 8)
                {
                    allBySpecial.RemoveAt(allBySpecial.Count - 1);
                }
                allBySpecial.Add(Game.Data.Girls.Get(14));
                allBySpecial.Add(Game.Data.Girls.Get(15));
                girlList = allBySpecial;
                //ArchipelagoConsole.LogMessage($"--------------------");
                //foreach (GirlDefinition girl in girlList)
                //{
                //    ArchipelagoConsole.LogMessage($"{girl.name}");
                //}
            }
        }

        ///// <summary>
        ///// after the puzzle game is done check to see if outfit locations are done
        ///// duplicated just in case the other isnt triggered
        ///// </summary>
        //[HarmonyPatch(typeof(CutsceneStepSpecialRoundClear), "Start")]
        //[HarmonyPostfix]
        //public static void test2(ref PuzzleStatus ____puzzleStatus)
        //{
        //    //check to see if we have outfit locations in logic
        //    if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["outfit_date_complete"]))
        //    {
        //        ArchipelagoConsole.LogMessage("hello");
        //        return;
        //    }
        //
        //    //check left girls outfit to see if location is not already checked otherwise send location
        //    if (Game.Persistence.playerFile.GetFlagValue((____puzzleStatus.girlStatusLeft.playerFileGirl.girlDefinition.id.ToString() + ":" + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusLeft.girlDefinition).currentOutfitIndex.ToString())) == -1)
        //    {
        //        //set flag
        //        Game.Persistence.playerFile.SetFlagValue((____puzzleStatus.girlStatusLeft.playerFileGirl.girlDefinition.id.ToString() + ":" + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusLeft.girlDefinition).currentOutfitIndex.ToString()), 1);
        //        //send location
        //        ArchipelagoClient.sendloc(69420385 + ((____puzzleStatus.girlStatusLeft.girlDefinition.id - 1) * 10) + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusLeft.girlDefinition).currentOutfitIndex);
        //    }
        //
        //    //check right girls outfit to see if location is not already checked otherwise send location
        //    if (Game.Persistence.playerFile.GetFlagValue((____puzzleStatus.girlStatusRight.playerFileGirl.girlDefinition.id.ToString() + ":" + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusRight.girlDefinition).currentOutfitIndex.ToString())) == -1)
        //    {
        //        //set flag
        //        Game.Persistence.playerFile.SetFlagValue((____puzzleStatus.girlStatusRight.playerFileGirl.girlDefinition.id.ToString() + ":" + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusRight.girlDefinition).currentOutfitIndex.ToString()), 1);
        //        //send location
        //        ArchipelagoClient.sendloc(69420385 + ((____puzzleStatus.girlStatusRight.girlDefinition.id - 1) * 10) + Game.Session.gameCanvas.GetDoll(____puzzleStatus.girlStatusRight.girlDefinition).currentOutfitIndex);
        //    }
        //}
    }
}