using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;

namespace HunniePop2ArchipelagoClient.HuniePop2.Girls
{
    [HarmonyPatch]
    public class Kyu
    {

        [HarmonyPatch(typeof(LocationManager), "ResetDolls")]
        [HarmonyILManipulator]
        public static void kyurandom(ILContext ctx, MethodBase orig)
        {
            for (int i = 10; i < ctx.Instrs.Count; i++)
            {
                if (ctx.Instrs[i].OpCode == OpCodes.Bgt_Un_S) { 
                    ctx.Instrs[i-17].OpCode = OpCodes.Nop;
                    ctx.Instrs[i-16].OpCode = OpCodes.Nop;
                    ctx.Instrs[i-15].OpCode = OpCodes.Nop;
                    ctx.Instrs[i-14].OpCode = OpCodes.Nop;
                    ctx.Instrs[i-13].OpCode = OpCodes.Nop;
                    ctx.Instrs[i-12].OpCode = OpCodes.Nop;
                    ctx.Instrs[i-11].OpCode = OpCodes.Nop;
                    ctx.Instrs[i-10].OpCode = OpCodes.Nop;
                    ctx.Instrs[i-9].OpCode = OpCodes.Nop;
                    ctx.Instrs[i-8].OpCode = OpCodes.Nop;
                    ctx.Instrs[i-7].OpCode = OpCodes.Nop;
                    ctx.Instrs[i-6].OpCode = OpCodes.Nop;
                    ctx.Instrs[i-5].OpCode = OpCodes.Nop;
                    ctx.Instrs[i-4].OpCode = OpCodes.Nop;
                    ctx.Instrs[i-3].OpCode = OpCodes.Nop;
                    ctx.Instrs[i-2].OpCode = OpCodes.Nop;
                    ctx.Instrs[i-1].OpCode = OpCodes.Nop;
                    ctx.Instrs[i].OpCode = OpCodes.Nop;
                }
            }
        }
    }
}
