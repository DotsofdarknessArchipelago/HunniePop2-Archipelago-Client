using HarmonyLib;
using System;
using System.Collections.Generic;

namespace HunniePop2ArchipelagoClient.HuniePop2.Gameplay
{
    [HarmonyPatch]
    public class Finder
    {

        /// <summary>
        /// clears all te finder slots tehn generates them using our logic
        /// TODO probally colapse the external method if replacing the UI dosent pan out
        /// </summary>
        [HarmonyPatch(typeof(PlayerFile), "PopulateFinderSlots")]
        [HarmonyPrefix]
        public static bool finder(PlayerFile __instance)
        {
            for (int i = 0; i < __instance.finderSlots.Count; i++)
            {
                __instance.finderSlots[i].Clear();
            }

            __instance.finderSlots = genfinder(__instance);

            return false;
        }


        /// <summary>
        /// Generate and overwirite the finder slots
        /// </summary>
        public static List<PlayerFileFinderSlot> genfinder(PlayerFile file)
        {

            List<GirlPairDefinition> pair = new List<GirlPairDefinition>();

            //iterate over the number of girl pairs
            for (int i = 0; i < file.girlPairs.Count; i++)
            {
                //if girl pair is not the standard gameplay pairs skip over them
                if (file.girlPairs[i].girlPairDefinition.girlDefinitionOne.id >= 13 || file.girlPairs[i].girlPairDefinition.girlDefinitionTwo.id >= 13) { continue; }
                //check if the pair is able to be met yet
                if (file.girlPairs[i].relationshipType != GirlPairRelationshipType.UNKNOWN)
                {
                    //check if each girl can be met yet
                    if (file.GetPlayerFileGirl(file.girlPairs[i].girlPairDefinition.girlDefinitionOne).playerMet && file.GetPlayerFileGirl(file.girlPairs[i].girlPairDefinition.girlDefinitionTwo).playerMet)
                    {
                        //add to list
                        pair.Add(file.girlPairs[i].girlPairDefinition);
                    }
                }
            }


            int initalpaircount = pair.Count;

            List<LocationDefinition> areas = new List<LocationDefinition>();
            //itterate over locations for pairs to be found natually
            for (int j = 1; j < 9; j++)
            {
                LocationDefinition a = Game.Data.Locations.Get(j);
                if (file.locationDefinition != a)
                {
                    areas.Add(a);
                }
            }

            List<PlayerFileFinderSlot> finder = new List<PlayerFileFinderSlot>();

            for (int i = 0; i < 8; i++)
            {
                //if we run out of pairs avaliable or locations return
                if (pair.Count == 0 || areas.Count == 0) { break; }
                //get a random pair and location index from list
                int p;
                if (pair.Count == 1) { p = 0; } else { p = UnityEngine.Random.Range(0, pair.Count); }
                int a = UnityEngine.Random.Range(0, areas.Count);

                //make sure that the index is valid
                p = Math.Min(p, pair.Count - 1);
                a = Math.Min(a, areas.Count - 1);

                //generate a new PlayerFileFinderSlot based on index generated
                PlayerFileFinderSlot findSlot = new PlayerFileFinderSlot();
                findSlot.locationDefinition = areas[a];
                findSlot.girlPairDefinition = pair[p];
                //randomise if the girls are flipped or not
                if ((UnityEngine.Random.Range(0, 100) % 2) == 0)
                {
                    findSlot.sidesFlipped = false;
                }
                else
                {
                    findSlot.sidesFlipped = true;
                }

                //add finder slot to list and remove pair and locations from their list
                finder.Add(findSlot);
                areas.RemoveAt(a);
                pair.RemoveAt(p);

            }
            //return finder list
            return finder;
        }

    }
}