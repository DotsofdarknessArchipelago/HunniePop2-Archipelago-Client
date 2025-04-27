using HarmonyLib;
using HunniePop2ArchipelagoClient.Archipelago;
using System;
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