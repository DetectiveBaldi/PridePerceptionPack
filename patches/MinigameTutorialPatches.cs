using HarmonyLib;
using MTM101BaldAPI.Reflection;
using PridePerception.core;
using UnityEngine;
using UnityEngine.UI;

namespace PridePerception.patches
{
    [HarmonyPatch(typeof(MinigameTutorial))]
    public class MinigameTutorialPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("Initialize")]
        public static void Initialize(MinigameTutorial __instance)
        {
            if (__instance.name.Contains("MinigameTutorial_CampfireFrenzy"))
            {
                GameObject page = ((GameObject[]) ReflectionHelpers.ReflectionGetVariable(__instance, "screen"))[^1];

                page.GetComponentsInChildren<Image>()[8].rectTransform.anchoredPosition += Vector2.left * 25.0f;

                GameObject gameObject = new();

                gameObject.transform.SetParent(page.transform, false);

                Image _image = gameObject.AddComponent<Image>();

                _image.sprite = Plugin.current.assets.Get<Sprite>("Images/patches/Minigame_CampfirePatches/3dflagpride");

                RectTransform rectTransform = _image.GetComponent<RectTransform>();

                rectTransform.sizeDelta = new(64.0f, 64.0f);

                rectTransform.anchorMin = new(1.0f, 0.5f);

                rectTransform.anchorMax = new(1.0f, 0.5f);

                rectTransform.pivot = new(1.0f, 0.5f);

                rectTransform.anchoredPosition = new(-75.0f, -50.0f);
            }
        }
    }
}