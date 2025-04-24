using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;

namespace HunniePop2ArchipelagoClient.HuniePop2.Girls
{
    [HarmonyPatch]
    public class Wardrobe
    {
        /// <summary>
        /// disables buying hairstyles in wardrobe
        /// </summary>
        [HarmonyPatch(typeof(PlayerFileGirl), "UnlockHairstyle")]
        [HarmonyPrefix]
        public static bool hairoverite(int styleIndex, ref bool __result)
        {
            __result = false;
            return false;
        }

        /// <summary>
        /// disables buying outfits in wardrobe
        /// </summary>
        [HarmonyPatch(typeof(PlayerFileGirl), "UnlockOutfit")]
        [HarmonyPrefix]
        public static bool outfitoverite(int styleIndex, ref bool __result)
        {
            __result = false;
            return false;
        }

        /// <summary>
        /// make it so all wardrobe items are shown
        /// </summary>
        [HarmonyPatch(typeof(UiAppSelectListItem), "Populate")]
        [HarmonyPostfix]
        public static void thing(ref bool ____hidden)
        {
            ____hidden = false;
        }

        /// <summary>
        /// make it so that you cant toggle the style on dates option(its always on)
        /// </summary>
        [HarmonyPatch(typeof(UiCellphoneAppWardrobe), "OnCheckBoxChanged")]
        [HarmonyPrefix]
        public static bool styletoggledisable()
        {
            return false;
        }




        //IL CODE

        /// <summary>
        /// enable stuff for when you completed the game to show
        /// disable tooltip showing you can buy an outfit/style
        /// </summary>
        [HarmonyPatch(typeof(UiAppStyleSelectList), "Refresh")]
        [HarmonyILManipulator]
        public static void waudrobeoverite(ILContext ctx, MethodBase orig)
        {
            sbyte opp7 = 7;
            sbyte opp1 = 1;
            bool b = true;
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                if (ctx.Instrs[i].OpCode == OpCodes.Ldc_I4_S && ctx.Instrs[i].Operand.ToString() == "14") { ctx.Instrs[i].Operand = opp7; }
                if (b && ctx.Instrs[i].OpCode == OpCodes.Ldloc_S && ctx.Instrs[i].Operand.ToString() == "V_4")
                {
                    ctx.Instrs[i].OpCode = OpCodes.Ldc_I4_S;
                    ctx.Instrs[i].Operand = opp1;
                    b = false;
                }
            }
        }

        /// <summary>
        /// something to do with the wear outfits on date button
        /// </summary>
        [HarmonyPatch(typeof(UiCellphoneAppWardrobe), "Start")]
        [HarmonyILManipulator]
        public static void waudrobeoverite2(ILContext ctx, MethodBase orig)
        {
            sbyte opp = 7;
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                if (ctx.Instrs[i].OpCode == OpCodes.Ldc_I4_S && ctx.Instrs[i].Operand.ToString() == "14") { ctx.Instrs[i].Operand = opp; }
                //break;
            }
        }


        /// <summary>
        /// disable the automatic sex outfit/style change during dates
        /// </summary>
        [HarmonyPatch(typeof(LocationManager), "ResetDolls")]
        [HarmonyILManipulator]
        public static void dateoutfitoveride(ILContext ctx, MethodBase orig)
        {
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                if (ctx.Instrs[i].OpCode == OpCodes.Ldc_I4_2)
                {
                    ctx.Instrs[i].OpCode = OpCodes.Ldc_I4_0;
                    break;
                }
            }
        }
    }
}