using HarmonyLib;
using HunniePop2ArchipelagoClient.Archipelago;
using System;

namespace HunniePop2ArchipelagoClient.HuniePop2.Gameplay
{
    [HarmonyPatch]
    public class Boss
    {

        /// <summary>
        /// method to override the number of wings to start the boss fight
        /// </summary>
        [HarmonyPatch(typeof(HubManager), "HubStep")]
        [HarmonyPrefix]
        public static void winggiving(HubManager __instance)
        {
            //make sure we are in the wing check convisation
            if (__instance.hubStepType == HubStepType.WINGS)
            {
                // if we have equal to or more than 24 wings or the boss_wing_requirement add all girl pairs to completed pairs list to allow access to boss fight
                if (Game.Persistence.playerFile.completedGirlPairs.Count >= 24) { return; }
                if (Game.Persistence.playerFile.completedGirlPairs.Count >= Convert.ToInt32(ArchipelagoClient.ServerData.slotData["boss_wing_requirement"]))
                {
                    ArchipelagoConsole.LogMessage("WING THRESHOLD REACHED GIVING REST OF WINGS TO ACCESS BOSS FIGHT");
                    for (int j = 1; j < 25; j++)
                    {
                        if (!Game.Persistence.playerFile.completedGirlPairs.Contains(Game.Data.GirlPairs.Get(j)))
                        {
                            Game.Persistence.playerFile.completedGirlPairs.Add(Game.Data.GirlPairs.Get(j));
                            Game.Persistence.SaveGame();
                        }
                    }
                }
                //otherwise output the wings required to access the boss fight
                else
                {
                    ArchipelagoConsole.LogMessage("WING THRESHOLD OF " + Convert.ToInt32(ArchipelagoClient.ServerData.slotData["boss_wing_requirement"]) + " NOT REACHED YET");
                }
            }
        }
    }
}