using HarmonyLib;
using HunniePop2ArchipelagoClient.Archipelago;
using System.IO;
using UnityEngine;

namespace HunniePop2ArchipelagoClient.HuniePop2.Gameplay
{
    [HarmonyPatch]
    public class Codes
    {

        /// <summary>
        /// method to trigger various codes
        /// </summary>
        [HarmonyPatch(typeof(UiCellphoneAppCode), "OnSubmitButtonPressed")]
        [HarmonyPrefix]
        public static void codestuff(UiCellphoneAppCode __instance)
        {
            string input = __instance.inputField.text.ToUpper().Trim();
            //add code TEST to be triggered
            if (input == "TEST")
            {

            }

            //add code RESETITEMS to be triggered
            if (input == "RESETITEMS")
            {
                //delete saved archdata file and reset the list
                if (!ArchipelagoClient.Authenticated)
                {
                    if (File.Exists(Application.persistentDataPath + "/archdata"))
                    {
                        File.Delete(Application.persistentDataPath + "/archdata");
                    }
                }
                else
                {
                    ArchipelagoClient.resetlist();
                }
            }

            //add code DEBUGDATA to be triggered
            if (input == "DEBUGDATA")
            {
                //go through recieved item list and output state
                ArchipelagoConsole.LogMessage("-------DEBUG DATA-------");
                ArchipelagoConsole.LogMessage("List size: " + ArchipelagoClient.alist.list.Count.ToString());
                ArchipelagoConsole.LogMessage("------------------------");
                for (int i = 0; i < ArchipelagoClient.alist.list.Count; i++)
                {
                    ArchipelagoConsole.LogMessage("#" + i.ToString());
                    ArchipelagoConsole.LogMessage("ID:" + ArchipelagoClient.alist.list[i].Id.ToString());
                    ArchipelagoConsole.LogMessage("NAME:" + ArchipelagoClient.alist.list[i].ItemName);
                    ArchipelagoConsole.LogMessage("PROCESSED:" + ArchipelagoClient.alist.list[i].processed.ToString());
                    ArchipelagoConsole.LogMessage("PUTINSHOP:" + ArchipelagoClient.alist.list[i].putinshop.ToString());
                    ArchipelagoConsole.LogMessage("------------------------");
                }
                ArchipelagoConsole.LogMessage("-----END DEBUG DATA-----");
            }
        }
    }
}