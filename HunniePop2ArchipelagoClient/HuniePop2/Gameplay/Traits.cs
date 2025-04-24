using HarmonyLib;
using HunniePop2ArchipelagoClient.Archipelago;

namespace HunniePop2ArchipelagoClient.HuniePop2.Gameplay
{
    [HarmonyPatch]
    public class Traits
    {

        /// <summary>
        /// overwrite the caculation for affection lv for each type
        /// </summary>
        /// <returns>RETURNS FALSE TO SKIP ORIGINAL METHOD</returns>
        [HarmonyPatch(typeof(PlayerFile), "GetAffectionLevelExp")]
        [HarmonyPrefix]
        public static bool affectionexp(PuzzleAffectionType affectionType, bool ofLevel, PlayerFile __instance, ref int __result)
        {
            int exp = 0;
            
            //sets the base flag id based on the affection type
            int flag = 69420025;
            if (affectionType == PuzzleAffectionType.ROMANCE) { flag += 4; }
            else if (affectionType == PuzzleAffectionType.FLIRTATION) { flag += 8; }
            else if (affectionType == PuzzleAffectionType.SEXUALITY) { flag += 12; }
            else { flag += 0; }

            //checks the recieved item list based on the flag if the item has been found and adds 6 to the total exp
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
        /// overwrite the caculation for passion lv
        /// </summary>
        /// <returns>RETURNS FALSE TO SKIP ORIGINAL METHOD</returns>
        [HarmonyPatch(typeof(PlayerFile), "GetPassionLevelExp")]
        [HarmonyPrefix]
        public static bool passionexp(bool ofLevel, PlayerFile __instance, ref int __result)
        {
            //checks the recieved item list based on the flag value and adds 6 exp if the item has been recieved
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
        /// overwrite the caculation for style lv
        /// </summary>
        /// <returns>RETURNS FALSE TO SKIP ORIGINAL METHOD</returns>
        [HarmonyPatch(typeof(PlayerFile), "GetStyleLevelExp")]
        [HarmonyPrefix]
        public static bool styleexp(bool ofLevel, PlayerFile __instance, ref int __result)
        {
            //checks the recieved item list based on the flag value and adds 6 exp if the item has been recieved
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
    }
}