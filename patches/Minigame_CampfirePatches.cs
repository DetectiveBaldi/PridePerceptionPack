using CampfireFrenzy;
using HarmonyLib;
using PridePerception.core;
using UnityEngine;

namespace PridePerception.patches
{
    [HarmonyPatch(typeof(Minigame_Campfire))]
    public class Minigame_CampfirePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("Initialize")]
        public static void Initialize(Minigame_Campfire __instance)
        {
            for (int i = 0; i < 5.0; i++)
                __instance.incorrectSprite = __instance.incorrectSprite.AddToArray(new() { selection = Plugin.current.assets.Get<Sprite>
                    ("Images/npcs/Bezz/Flag/3dflag" + i), weight = 25 });
        }
    }
}