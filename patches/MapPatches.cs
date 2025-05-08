using HarmonyLib;
using MTM101BaldAPI.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace PridePerception.patches
{
    [HarmonyPatch(typeof(Map))]
    public class MapPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("DestroyMarker")]
        public static void DestroyMarker(Map __instance)
        {
            GameObject fullIndicator = (GameObject) ReflectionHelpers.ReflectionGetVariable(__instance, "fullIndicator");

            List<MapMarker> markers = (List<MapMarker>) ReflectionHelpers.ReflectionGetVariable(__instance, "markers");

            fullIndicator.SetActive(markers.Count >= 32.0f);
        }
    }
}