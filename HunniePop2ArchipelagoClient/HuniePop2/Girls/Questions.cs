using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using System.Reflection;

namespace HunniePop2ArchipelagoClient.HuniePop2.Girls
{
    [HarmonyPatch]
    public class Questions
    {

        /// <summary>
        /// sends relevent location when learning girls details
        /// </summary>
        [HarmonyPatch(typeof(PlayerFileGirl), "LearnFavAnswer")]
        [HarmonyPostfix]
        public static void questioncheck(QuestionDefinition questionDef, bool __result, PlayerFileGirl __instance)
        {
            if (__result == false) { return; }
            //if questions arent in logic skip checking
            if (Game.Persistence.playerFile.GetFlagValue("questions_skiped") == 0)
            {
                if (Game.Persistence.playerFile.GetFlagValue("question:" + __instance.girlDefinition.id + ":" + questionDef.id) != 1)
                {
                    Archipelago.ArchipelagoClient.sendloc(69420144 + (__instance.girlDefinition.id - 1) * 20 + questionDef.id);
                    Game.Persistence.playerFile.SetFlagValue("question:" + __instance.girlDefinition.id + ":" + questionDef.id, 1);
                }
            }
        }

        /// <summary>
        /// overwrite the question list when a girl is asking what their favruote thing is since the nomal logic for this is terrible to get unanswered questions
        /// </summary>
        [HarmonyPatch(typeof(TalkManager), "TalkStep")]
        [HarmonyPrefix]
        public static bool question(TalkManager __instance, ref int ____talkStepIndex, ref List<QuestionDefinition> ____questionPool, ref UiDoll ____targetDoll)
        {
            //make sure we are getting asked about their favroute thing and that we are in the begining
            if (__instance.talkType == TalkWithType.FAVORITE_QUESTION)
            {
                if (____talkStepIndex == 0)
                {
                    //clear the question pool since we are replacing it anyways
                    ____questionPool.Clear();


                    List<QuestionDefinition> badpool = new List<QuestionDefinition>();
                    List<QuestionDefinition> goodpool = new List<QuestionDefinition>();

                    List<QuestionDefinition> questionlist = Game.Data.Questions.GetAll();

                    foreach (QuestionDefinition question in questionlist)
                    {
                        //add the question to the good pool if it hasnt been answered yet otherwise put it in the bad pool
                        if (Game.Persistence.playerFile.GetFlagValue("question:" + ____targetDoll.girlDefinition.id + ":" + question.id) != 1)
                        {
                            goodpool.Add(question);
                        }
                        else
                        {
                            badpool.Add(question);
                        }
                    }

                    //choose 4 questions to be asked pulling from the good pool untill its empty
                    for (int j = 1; j < 4; j++)
                    {
                        if (goodpool.Count > 0)
                        {
                            int index = UnityEngine.Random.Range(0, goodpool.Count);
                            ____questionPool.Add(goodpool[index]);
                            goodpool.RemoveAt(index);
                        }
                        else
                        {
                            int index = UnityEngine.Random.Range(0, badpool.Count);
                            ____questionPool.Add(badpool[index]);
                            badpool.RemoveAt(index);
                        }
                    }
                }
            }
            return true;
        }



        //IL CODE

        /// <summary>
        /// nop the logic for setting the question list
        /// </summary>
        [HarmonyPatch(typeof(TalkManager), "TalkStep")]
        [HarmonyILManipulator]
        public static void questionil(ILContext ctx, MethodBase orig)
        {
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                //skip the first 20 instrcts since what we want is not in them
                if (i < 20)
                {
                    continue;
                }

                //find where the question pool logic begins then nop it all
                if (ctx.Instrs[i].OpCode == OpCodes.Ldarg_0
                    && ctx.Instrs[i - 1].OpCode == OpCodes.Br
                    && ctx.Instrs[i - 2].OpCode == OpCodes.Callvirt
                    && ctx.Instrs[i - 3].OpCode == OpCodes.Newobj
                    && ctx.Instrs[i - 4].OpCode == OpCodes.Ldftn
                    && ctx.Instrs[i - 5].OpCode == OpCodes.Ldarg_0
                    && ctx.Instrs[i - 6].OpCode == OpCodes.Ldfld
                    && ctx.Instrs[i - 7].OpCode == OpCodes.Ldarg_0
                    && ctx.Instrs[i - 8].OpCode == OpCodes.Callvirt
                    && ctx.Instrs[i - 9].OpCode == OpCodes.Ldc_I4_1)
                {
                    for (int j = 0; j < 103; j++)
                    {
                        ctx.Instrs[i + j].OpCode = OpCodes.Nop;
                    }
                }
            }
        }
    }
}