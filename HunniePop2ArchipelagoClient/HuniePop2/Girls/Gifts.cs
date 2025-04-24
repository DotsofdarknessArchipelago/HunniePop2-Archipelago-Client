using HarmonyLib;
using HunniePop2ArchipelagoClient.Archipelago;
using System.Collections.Generic;

namespace HunniePop2ArchipelagoClient.HuniePop2.Girls
{
    [HarmonyPatch]
    public class Gifts
    {
        /// <summary>
        /// sends relevent location based on the unique gift recieved
        /// </summary>
        [HarmonyPatch(typeof(PlayerFileGirl), "ReceiveUnique")]
        [HarmonyPrefix]
        public static bool uniquecheck(ItemDefinition uniqueDef, ref bool __result, PlayerFileGirl __instance, ref List<int> ____receivedUniques)
        {
            //if the gift isnt for the girl return false
            if (!__instance.girlDefinition.uniqueItemDefs.Contains(uniqueDef))
            {
                __result = false;
                return false;
            }

            //if the gift has already been given to the girl return true to get rid of the duplicate gift
            if (____receivedUniques.Contains(__instance.girlDefinition.uniqueItemDefs.IndexOf(uniqueDef)))
            {
                __result = true;
                return false;
            }

            //send location and add gift the recieved gifts list of the girl and return true
            ____receivedUniques.Add(__instance.girlDefinition.uniqueItemDefs.IndexOf(uniqueDef));
            ArchipelagoClient.sendloc(IDs.idtoflag(uniqueDef.id) - 44);

            __result = true;
            return false;
        }


        /// <summary>
        /// sends relevent location based on the unique gift recieved
        /// </summary>
        [HarmonyPatch(typeof(PlayerFileGirl), "ReceiveShoes")]
        [HarmonyPrefix]
        public static bool shoecheck(ItemDefinition shoesDef, ref bool __result, PlayerFileGirl __instance, ref List<int> ____receivedShoes)
        {
            //if the gift isnt for the girl return false
            if (!__instance.girlDefinition.shoesItemDefs.Contains(shoesDef))
            {
                __result = false;
                return false;
            }

            //if the gift has already been given to the girl return true to get rid of the duplicate gift
            if (____receivedShoes.Contains(__instance.girlDefinition.shoesItemDefs.IndexOf(shoesDef)))
            {
                __result = true;
                return false;
            }

            //send location and add gift the recieved gifts list of the girl and return true
            ____receivedShoes.Add(__instance.girlDefinition.shoesItemDefs.IndexOf(shoesDef));
            ArchipelagoClient.sendloc(IDs.idtoflag(shoesDef.id) - 44);

            __result = true;
            return false;
        }
    }
}