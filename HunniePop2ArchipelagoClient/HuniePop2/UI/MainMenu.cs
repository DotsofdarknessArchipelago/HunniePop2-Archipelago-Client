using HarmonyLib;
using UnityEngine;

namespace HunniePop2ArchipelagoClient.HuniePop2.UI
{

    [HarmonyPatch]
    public class MainMenuPatches
    {
        [HarmonyPatch(typeof(UiCellphone), "LoadApp")]
        [HarmonyPrefix]
        public static bool appload(UiCellphone __instance,ref UiCellphoneApp ____currentApp,ref int ____currentAppIndex, int appIndex)
        {
            //ArchipelagoConsole.LogMessage($"UICELLPHONE LOADING APP ID{appIndex}");
            if (appIndex >= 3 || (GameObject.Find("Canvas").GetComponent<UiTitleCanvas>() == null))
            {
                Plugin.test = false;
                return true; 
            }
            //ArchipelagoConsole.LogMessage($"APP ID{appIndex} SKIPED");
            if (____currentApp != null)
            {
                if (__instance.phoneErrorMsg != null)
                {
                    __instance.phoneErrorMsg.ClearMessage();
                }
                if (__instance.cellphoneButtons.Count > 0 && ____currentApp.phoneButtonIndex >= 0)
                {
                    __instance.cellphoneButtons[____currentApp.phoneButtonIndex].buttonBehavior.Enable();
                }
                UnityEngine.Object.Destroy(____currentApp.gameObject);
            }
            ____currentApp = null;
            ____currentAppIndex = appIndex;
            Plugin.test = true;
            return false;
        }

    }

    public class MainMenuStuff
    {
        public static int[] SCREEN_RES_WIDTHS = new int[] { 1024, 1152, 1280, 1366, 1600, 1920 };//4.45, //1.81
        public static int[] SCREEN_RES_HEIGHTS = new int[] { 576, 648, 720, 768, 900, 1080 };//4.9 // 1.9

    }

}
