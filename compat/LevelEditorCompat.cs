using BaldiLevelEditor;
using PlusLevelLoader;
using PridePerception.core;
using UnityEngine;

namespace PridePerception.compat
{
    public class LevelEditorCompat
    {
        public static void Awake()
        {
            BaldiLevelEditorPlugin.characterObjects.Add("npcs/Bezz", BaldiLevelEditorPlugin.StripAllScripts(Plugin.current.assets.Get<NPC>("npcs/Bezz").gameObject, true));

            BaldiLevelEditorPlugin.Instance.assetMan.Add("UI/NPC_npcs/Bezz",
                Plugin.current.assets.Get<Sprite>("Images/compat/LevelEditorCompat/bezzLevelEditorPortrait"));

            PlusLevelLoaderPlugin.Instance.npcAliases.Add("npcs/Bezz", Plugin.current.assets.Get<NPC>("npcs/Bezz"));
        }
    }
}