using HarmonyLib;
using HunniePop2ArchipelagoClient.Archipelago;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;

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
                PlayerFile file = Game.Persistence.playerFile;
                if (ArchipelagoClient.ServerData.slotData["lovers_instead_wings"].ToString() == 1.ToString())
                {
                    //the goal is lovers
                    bool goal = true;
                    int r = 0;
                    int o = 0;
                    foreach (GirlPairDefinition pair in Game.Data.GirlPairs.GetAll())
                    {
                        if (pair.specialPair) { continue; }
                        if (ArchipelagoClient.ServerData.slotData[pair.girlDefinitionOne.girlName.ToLower()].ToString() == 0.ToString() && ArchipelagoClient.ServerData.slotData[pair.girlDefinitionTwo.girlName.ToLower()].ToString() == 0.ToString())
                        {
                            goal = goal && file.GetPlayerFileGirlPair(pair).relationshipType == GirlPairRelationshipType.LOVERS;
                            if (file.GetPlayerFileGirlPair(pair).relationshipType == GirlPairRelationshipType.LOVERS) { o++; }
                            r++;
                        }
                    }
                    if (goal)
                    {
                        ArchipelagoConsole.LogMessage("LOVER THRESHOLD REACHED GIVING PLAYER ALL WINGS TO ACCESS BOSS FIGHT");
                        for (int j = 1; j < 26; j++)
                        {
                            if (!Game.Persistence.playerFile.completedGirlPairs.Contains(Game.Data.GirlPairs.Get(j)))
                            {
                                Game.Persistence.playerFile.completedGirlPairs.Add(Game.Data.GirlPairs.Get(j));
                                Game.Persistence.SaveGame();
                            }
                        }
                    }
                    else
                    {
                        ArchipelagoConsole.LogMessage($"{o} OUT OF {r} REQUIRED PAIRS ARE LOVERS");
                    }

                }
                else
                {
                    //the goal is wings
                    if (Game.Persistence.playerFile.completedGirlPairs.Count >= Convert.ToInt32(ArchipelagoClient.ServerData.slotData["boss_wing_requirement"]))
                    {
                        ArchipelagoConsole.LogMessage("WING THRESHOLD REACHED GIVING REST OF WINGS TO ACCESS BOSS FIGHT");
                        for (int j = 1; j < 26; j++)
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

        [HarmonyPatch(typeof(HubManager), "HubStep")]
        [HarmonyILManipulator]
        public static void wing25(ILContext ctx, MethodBase orig)
        {
            sbyte n = 25;
            for (int i = 10; i < ctx.Instrs.Count; i++)
            {
                if (ctx.Instrs[i].OpCode == OpCodes.Ldc_I4_S && ctx.Instrs[i].Operand.ToString() == "24" && ctx.Instrs[i-1].OpCode == OpCodes.Callvirt) { ctx.Instrs[i].Operand = n; }
            }
        }
    }
}