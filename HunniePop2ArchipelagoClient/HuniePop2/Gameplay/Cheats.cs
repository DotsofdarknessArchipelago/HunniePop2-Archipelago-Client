using HarmonyLib;


/// GENERAL CLASS FOR ADDING CHEATS FOR TESTING
namespace HunniePop2ArchipelagoClient.HuniePop2.Gameplay
{
    [HarmonyPatch]
    public class Cheats
    {
        public static bool disable = true;

        /// <summary>
        /// CHEAT TO MAKE PUZZLES COMPLETE IN 1 MOVE
        /// </summary>
        [HarmonyPatch(typeof(PuzzleStatus), "AddPuzzleReward")]
        [HarmonyPrefix]
        public static void puzzleautocomplete(PuzzleStatus __instance)
        {
            if (__instance.bonusRound || disable) { return; }
            __instance.AddResourceValue(PuzzleResourceType.AFFECTION, 100000, false);
        }
    }
}
