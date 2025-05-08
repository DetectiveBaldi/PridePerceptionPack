using HarmonyLib;
using MTM101BaldAPI.Reflection;
using PicnicPanic;
using PridePerception.core;
using UnityEngine;

namespace PridePerception.patches
{
    [HarmonyPatch(typeof(Minigame_Picnic))]
    public class Minigame_PicnicPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("Initialize")]
        public static void Initialize(Minigame_Picnic __instance)
        {
            bool endless = !(bool) __instance.ReflectionGetVariable("scoringMode");

            PicnicGroupGroup[] picnicGroupGroups = (PicnicGroupGroup[]) __instance.ReflectionGetVariable(endless ? 
                "normalGroup" : "scoringGroup");

            for (int i = 0; i < picnicGroupGroups.Length; i++)
            {
                PicnicGroupGroup picnicGroupGroup = picnicGroupGroups[i];

                for (int j = 0; j < picnicGroupGroup.groupObject.Length; j++)
                {
                    WeightedPicnicGroupObject weightedPicnicGroupObject = picnicGroupGroup.groupObject[j];

                    if (allHotdogs(weightedPicnicGroupObject.selection.sprite, weightedPicnicGroupObject.selection.desireExclusiveSprite) ||
                        allPizza(weightedPicnicGroupObject.selection.sprite, weightedPicnicGroupObject.selection.desireExclusiveSprite))
                        continue;

                    weightedPicnicGroupObject.selection.sprite = weightedPicnicGroupObject.selection.sprite.
                       AddToArray(Plugin.current.assets.Get<Sprite>("Images/patches/Minigame_PicnicPatches/brownie"));
                }
            }
        }

        public static bool allFoodOfType(Sprite[] sprite, Sprite[] desireExclusiveSprite, string foodType)
        {
            for (int i = 0; i < sprite.Length; i++)
                if (!sprite[i].name.Contains(foodType))
                    return false;

            for (int i = 0; i < desireExclusiveSprite.Length; i++)
                if (!desireExclusiveSprite[i].name.Contains(foodType))
                    return false;

            return true;
        }

        public static bool allHotdogs(Sprite[] sprite, Sprite[] desireExclusiveSprite)
        {
            return allFoodOfType(sprite, desireExclusiveSprite, "Hotdog");
        }

        public static bool allPizza(Sprite[] sprite, Sprite[] desireExclusiveSprite)
        {
            return allFoodOfType(sprite, desireExclusiveSprite, "Pizza");
        }
    }
}