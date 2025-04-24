using HarmonyLib;
using HunniePop2ArchipelagoClient.Archipelago;
using System.Collections.Generic;


//need to override the baggage since you need to be able to give all gifts without having the baggage stop it
//otherwise randomiser logic gets really messy
namespace HunniePop2ArchipelagoClient.HuniePop2.Girls
{
    [HarmonyPatch]
    public class Baggage
    {
        /// <summary>
        /// makes sure that when you are checking if a girl has baggage it counts the temp baggage
        /// </summary>
        /// <returns>RETURNS FALSE TO SKIP ORIGINAL METHOD</returns>
        [HarmonyPatch(typeof(PlayerFileGirl), "HasBaggage")]
        [HarmonyPrefix]
        public static bool hasbaggageoverite(ItemDefinition baggageDef, PlayerFileGirl __instance, ref bool __result)
        {
            __result = (Game.Data.Girls.Get(__instance.girlDefinition.id).baggageItemDefs.Contains(baggageDef) || (baggageDef == Baggage.baggagestuff()));
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
            List<int> list = new List<int>();
            list.Add(0);
            list.Add(1);
            list.Add(2);
            __instance.learnedBaggage = list;
        }

        /// <summary>
        /// CHECK THAT PLAYERFILE GIRL LOADS PROPERLY
        /// </summary>
        /// <returns>RETURNS FALSE TO SKIP ORIGINAL METHOD</returns>
        [HarmonyPatch(typeof(PlayerFileGirl), "ReadData")]
        [HarmonyPostfix]
        public static void Playergirlchack(PlayerFileGirl __instance, ref List<int> ____learnedBaggage)
        {
            //make sure all baggage slots have been learnt
            if (____learnedBaggage.Count != 3)
            {
                List<int> temp = new List<int>();
                temp.Add(0);
                temp.Add(1);
                temp.Add(2);
                ____learnedBaggage = temp;
            }

            //fill all baggages slots with temp baggage if not all slots have baggage in them
            int id = __instance.girlDefinition.id;
            if (__instance.girlDefinition.baggageItemDefs.Count != 3)
            {
                List<ItemDefinition> newlist = new List<ItemDefinition>();
                newlist.Add(baggagestuff());
                newlist.Add(baggagestuff());
                newlist.Add(baggagestuff());
                __instance.girlDefinition.baggageItemDefs = newlist;
            }

            //check if the 1st baggage item has been obtained and put it in the 1st slot otherwise put a temp baggage instead
            if (ArchipelagoClient.alist.hasitem(69420189 + ((id - 1) * 3)) && __instance.girlDefinition.baggageItemDefs[0] != Game.Data.Items.Get(((id - 1) * 3) + 93))
            {
                __instance.girlDefinition.baggageItemDefs[0] = Game.Data.Items.Get(((id - 1) * 3) + 93);
            }
            else if (__instance.girlDefinition.baggageItemDefs[0] != baggagestuff() && !ArchipelagoClient.alist.hasitem(69420189 + ((id - 1) * 3)))
            {
                __instance.girlDefinition.baggageItemDefs[0] = baggagestuff();
            }

            //check if the 2nd baggage item has been obtained and put it in the 1st slot otherwise put a temp baggage instead
            if (ArchipelagoClient.alist.hasitem(69420190 + ((id - 1) * 3)) && __instance.girlDefinition.baggageItemDefs[1] != Game.Data.Items.Get(((id - 1) * 3) + 94))
            {
                __instance.girlDefinition.baggageItemDefs[1] = Game.Data.Items.Get(((id - 1) * 3) + 94);
            }
            else if (__instance.girlDefinition.baggageItemDefs[1] != baggagestuff() && !ArchipelagoClient.alist.hasitem(69420190 + ((id - 1) * 3)))
            {
                __instance.girlDefinition.baggageItemDefs[1] = baggagestuff();
            }

            //check if the 3rd baggage item has been obtained and put it in the 1st slot otherwise put a temp baggage instead
            if (ArchipelagoClient.alist.hasitem(69420191 + ((id - 1) * 3)) && __instance.girlDefinition.baggageItemDefs[2] != Game.Data.Items.Get(((id - 1) * 3) + 95))
            {
                __instance.girlDefinition.baggageItemDefs[2] = Game.Data.Items.Get(((id - 1) * 3) + 95);
            }
            else if (__instance.girlDefinition.baggageItemDefs[2] != baggagestuff() && !ArchipelagoClient.alist.hasitem(69420191 + ((id - 1) * 3)))
            {
                __instance.girlDefinition.baggageItemDefs[2] = baggagestuff();
            }
        }

        /// <summary>
        /// method to generate a temp baggage for each girl to have
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

    }
}