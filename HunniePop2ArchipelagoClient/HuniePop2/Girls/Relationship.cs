using HarmonyLib;

namespace HunniePop2ArchipelagoClient.HuniePop2.Girls
{
    [HarmonyPatch]
    public class Relationship
    {

        /// <summary>
        /// sends relevent location when progressing relationship between pairs
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
                Archipelago.ArchipelagoClient.sendloc(69420000 + __instance.girlPairDefinition.id);
                Archipelago.ArchipelagoClient.sendloc(69420024 + __instance.girlPairDefinition.id);
                Game.Persistence.playerFile.GetPlayerFileGirl(__instance.girlPairDefinition.girlDefinitionOne).relationshipUpCount++;
                Game.Persistence.playerFile.GetPlayerFileGirl(__instance.girlPairDefinition.girlDefinitionTwo).relationshipUpCount++;
                Game.Persistence.playerFile.relationshipUpCount++;
                //if (Game.Persistence.playerFile.GetFlagValue("loversinsteadwings") == 1) { Game.Persistence.playerFile.completedGirlPairs.Add(__instance.girlPairDefinition); }
            }
            return false;
        }
    }
}