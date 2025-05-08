using BaldiLevelEditor;
using HarmonyLib;
using MTM101BaldAPI;

namespace PridePerception.patches
{
    [ConditionalPatchMod("mtm101.rulerp.baldiplus.leveleditor")]
    [HarmonyPatch(typeof(PlusLevelEditor))]
    public class PlusLevelEditorPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("Initialize")]
        public static void Initialize(PlusLevelEditor __instance)
        {
            __instance.toolCats.Find(toolCategory => toolCategory.name == "characters").tools.Add(new NpcTool("npcs/Bezz"));
        }
    }
}