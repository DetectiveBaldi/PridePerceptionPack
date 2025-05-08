using MTM101BaldAPI.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace PridePerception.util
{
    public class MapMarkerUtil
    {
        /**
         * Initializes a `MapMarker` object.
         * @warning To negate certain issues, the created marker is not automatically added to the `Map`'s `markers` `List`.
         * @warning You can either add it yourself, or call `ShowMarker` in your own `MonoBehaviour`!
         * @param map The `Map` instance to associate the `MapMarker` with.
         * @param cell The `Cell` instance to position the `MapMarker` at.
         * @param markerSprite The sprite that is displayed for the `MapMarker`.
         * @param mapIconSprite The sprite that is displayed for the `MapIcon`.
         * @see patches.MapPatches
         */
        public static MapMarker Initialize(Map map, Cell cell, Sprite markerSprite, Sprite mapIconSprite)
        {
            MapMarker[] markerPre = (MapMarker[]) ReflectionHelpers.ReflectionGetVariable(map, "markerPre");

            GameObject markersObject = (GameObject) ReflectionHelpers.ReflectionGetVariable(map, "markersObject");

            MapMarker mapMarker = UnityEngine.Object.Instantiate(markerPre[0], markersObject.transform);

            mapMarker.transform.localPosition = new(cell.position.x, cell.position.z, 0.0f);

            mapMarker.Initialize(map, mapMarker.transform.localPosition, 0);

            SpriteRenderer spriteRenderer = (SpriteRenderer) ReflectionHelpers.ReflectionGetVariable(mapMarker, "environmentMarker");

            spriteRenderer.sprite = markerSprite;

            mapMarker.Icon.spriteRenderer.sprite = mapIconSprite;

            mapMarker.Icon.UpdatePosition(map);

            List<MapIcon> icons = (List<MapIcon>) ReflectionHelpers.ReflectionGetVariable(map, "icons");

            icons.Add(mapMarker.Icon);

            return mapMarker;
        }

        /**
         * Disposes a `MapMarker`.
         * @param map The `Map` instance associated with the `MapMarker`.
         * @param mapMarker The `MapMarker` to dispose.
         * @see patches.MapPatches
         */
        public static void Dispose(Map map, MapMarker mapMarker)
        {
            List<MapIcon> icons = (List<MapIcon>) ReflectionHelpers.ReflectionGetVariable(map, "icons");

            icons.Remove(mapMarker.Icon);

            UnityEngine.Object.Destroy(mapMarker);

            UnityEngine.Object.Destroy(mapMarker.Icon);

            UnityEngine.Object.Destroy((SpriteRenderer) ReflectionHelpers.ReflectionGetVariable(mapMarker, "environmentMarker"));

            UnityEngine.Object.Destroy(mapMarker.Icon.spriteRenderer);
        }
    }
}
