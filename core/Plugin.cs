using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using PridePerception.compat;
using PridePerception.npcs;
using UnityEngine;

namespace PridePerception.core
{
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")]
    [BepInDependency("mtm101.rulerp.baldiplus.leveleditor", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("detectivebaldi.pluspacks.prideperception", "Pride Perception Pack", "1.2.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin current;

        public AssetManager assets;

        public void Awake()
        {
            current = this;

            Harmony harmony = new("detectivebaldi.pluspacks.prideperception");

            harmony.PatchAllConditionals();

            assets = new();

            LoadingEvents.RegisterOnAssetsLoaded(Info, runCallbacks, false);

            GeneratorManagement.Register(this, GenerationModType.Addend, sceneGenerated);
        }

        public void runCallbacks()
        {
            registerAssets();

            updateAssets();

            registerNPCs();

            registerCompatibilities();
        }

        public void registerAssets()
        {
            assets.Add<Sprite>("Images/compat/LevelEditorCompat/bezzLevelEditorPortrait",
                AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Images/compat/LevelEditorCompat/bezzLevelEditorPortrait.png"),
                    10.0f));

            assets.Add<Sprite>("Images/npcs/Bezz/bezzIdle", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Images/npcs/Bezz/bezzIdle.png"), 30.0f));

            for (int i = 0; i < 12.0f; i++)
                assets.Add<Sprite>("Images/npcs/Bezz/bezzWalk" + i, AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Images/npcs/Bezz/bezzWalk" + i + ".png"), 30.0f));

            for (int i = 0; i < 2.0f; i++)
                assets.Add<Sprite>("Images/npcs/Bezz/bezzHappyTalk" + i, AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Images/npcs/Bezz/bezzHappyTalk" + i + ".png"), 30.0f));

            for (int i = 0; i < 2.0f; i++)
                assets.Add<Sprite>("Images/npcs/Bezz/bezzUpsetTalk" + i, AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Images/npcs/Bezz/bezzUpsetTalk" + i + ".png"), 30.0f));

            for (int i = 0; i < 5.0f; i++)
            {
                assets.Add<Sprite>("Images/npcs/Bezz/Flag/" + i + "0", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Images/npcs/Bezz/Flag/" + i + "0.png"), 30.0f));

                assets.Add<Sprite>("Images/npcs/Bezz/Flag/" + i + "1", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Images/npcs/Bezz/Flag/" + i + "1.png"), 30.0f));

                assets.Add<Sprite>("Images/npcs/Bezz/Flag/3dflag" + i, AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Images/npcs/Bezz/Flag/3dflag" + i + ".png"), 10.0f));
            }

            assets.Add<Sprite>("Images/npcs/Bezz/Flag/mapMarker", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Images/npcs/Bezz/Flag/mapMarker.png"), 15.0f));

            assets.Add<Sprite>("Images/npcs/Bezz/Flag/mapIcon", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Images/npcs/Bezz/Flag/mapIcon.png"), 16.0f));

            assets.Add<Sprite>("Images/patches/Minigame_CampfirePatches/3dflagpride", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Images/patches/Minigame_CampfirePatches/3dflagpride.png"), 10.0f));

            assets.Add<Texture2D>("Images/patches/Minigame_CampfirePatches/CampfireFrenzy_TutPic_04Replacement", AssetLoader.TextureFromMod(this, "Images/patches/Minigame_CampfirePatches/CampfireFrenzy_TutPic_04Replacement.png"));

            assets.Add<Sprite>("Images/patches/Minigame_PicnicPatches/brownie", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Images/patches/Minigame_PicnicPatches/brownie.png"), 50.0f));

            assets.Add<Texture2D>("Images/PosterObject/poster0", AssetLoader.TextureFromMod(this, "Images/PosterObject/poster0.png"));

            AssetLoader.LocalizationFromMod(this);

            assets.Add<string>("Midis/npcs/Bezz/FlagCatch0", AssetLoader.MidiFromMod("Midis/npcs/Bezz/FlagCatch0", this, "Midis", "npcs", "Bezz", "FlagCatch0.mid"));

            assets.Add<string>("Midis/npcs/Bezz/FlagCatch1", AssetLoader.MidiFromMod("Midis/npcs/Bezz/FlagCatch1", this, "Midis", "npcs", "Bezz", "FlagCatch1.mid"));

            assets.Add<string>("Midis/npcs/Bezz/FlagCatchEnd0", AssetLoader.MidiFromMod("Midis/npcs/Bezz/FlagCatchEnd0", this, "Midis", "npcs", "Bezz", "FlagCatchEnd0.mid"));

            assets.Add<string>("Midis/npcs/Bezz/FlagCatchEnd1", AssetLoader.MidiFromMod("Midis/npcs/Bezz/FlagCatchEnd1", this, "Midis", "npcs", "Bezz", "FlagCatchEnd1.mid"));

            assets.Add<SoundObject>("Sounds/npcs/Bezz/bezzTarget0", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "Sounds", "npcs", "Bezz", "bezzTarget0.wav"), "bezzTarget0", SoundType.Voice, new Color(1.0f, 0.5f, 0.0f)));

            assets.Add<SoundObject>("Sounds/npcs/Bezz/bezzTarget1", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "Sounds", "npcs", "Bezz", "bezzTarget1.wav"), "bezzTarget1", SoundType.Voice, new Color(1.0f, 0.5f, 0.0f)));

            assets.Add<SoundObject>("Sounds/npcs/Bezz/bezzPrompt0", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "Sounds", "npcs", "Bezz", "bezzPrompt0.wav"), "bezzPrompt0", SoundType.Voice, new Color(1.0f, 0.5f, 0.0f)));

            assets.Add<SoundObject>("Sounds/npcs/Bezz/bezzPrompt1", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "Sounds", "npcs", "Bezz", "bezzPrompt1.wav"), "bezzPrompt1", SoundType.Voice, new Color(1.0f, 0.5f, 0.0f)));

            assets.Add<SoundObject>("Sounds/npcs/Bezz/bezzActivityWin0", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "Sounds", "npcs", "Bezz", "bezzActivityWin0.wav"), "bezzActivityWin0", SoundType.Voice, new Color(1.0f, 0.5f, 0.0f)));

            assets.Add<SoundObject>("Sounds/npcs/Bezz/bezzActivityWin1", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "Sounds", "npcs", "Bezz", "bezzActivityWin1.wav"), "bezzActivityWin1", SoundType.Voice, new Color(1.0f, 0.5f, 0.0f)));

            assets.Add<SoundObject>("Sounds/npcs/Bezz/bezzActivityWin1Alt", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "Sounds", "npcs", "Bezz", "bezzActivityWin1Alt.wav"), "bezzActivityWin1Alt", SoundType.Voice, new Color(1.0f, 0.5f, 0.0f)));

            assets.Add<SoundObject>("Sounds/npcs/Bezz/bezzActivityLoss0", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "Sounds", "npcs", "Bezz", "bezzActivityLoss0.wav"), "bezzActivityLoss0", SoundType.Voice, new Color(1.0f, 0.5f, 0.0f)));

            assets.Add<SoundObject>("Sounds/npcs/Bezz/bezzActivityLoss1", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "Sounds", "npcs", "Bezz", "bezzActivityLoss1.wav"), "bezzActivityLoss1", SoundType.Voice, new Color(1.0f, 0.5f, 0.0f)));

            assets.Add<SoundObject>("Sounds/npcs/Bezz/bezzActivityLoss2", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "Sounds", "npcs", "Bezz", "bezzActivityLoss2.wav"), "bezzActivityLoss2", SoundType.Voice, new Color(1.0f, 0.5f, 0.0f)));
        }

        public void updateAssets()
        {
            AssetLoader.ReplaceTexture("CampfireFrenzy_TutPic_04", AssetLoader.AttemptConvertTo(assets.Get<Texture2D>("Images/patches/Minigame_CampfirePatches/CampfireFrenzy_TutPic_04Replacement"), TextureFormat.RGB24));
        }

        public void registerNPCs()
        {
            NPCBuilder<Bezz> bezzBuilder = new(Info);

            bezzBuilder.SetName("Bezz");

            bezzBuilder.SetEnum("npcs/Bezz");

            bezzBuilder.SetPoster(AssetLoader.TextureFromMod(this, "Images/npcs/Bezz/bezzPriPoster.png"), "bezzPriPosterTitle", "bezzPriPosterDesc");

            bezzBuilder.AddLooker();

            bezzBuilder.AddTrigger();

            bezzBuilder.AddSpawnableRoomCategories(RoomCategory.Hall);

            Bezz bezz = bezzBuilder.Build();

            bezz.spriteRenderer[0].sprite = Plugin.current.assets.Get<Sprite>("Images/npcs/Bezz/bezzIdle");

            bezz.spriteRenderer[0].transform.localPosition += Vector3.up;

            assets.Add<NPC>("npcs/Bezz", bezz);
        }

        public void registerCompatibilities()
        {
            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.leveleditor"))
                LevelEditorCompat.Awake();
        }

        public void sceneGenerated(string sceneTitle, int sceneIndex, SceneObject sceneObject)
        {
            sceneObject.MarkAsNeverUnload();

            CustomLevelObject[] levelObjects = sceneObject.GetCustomLevelObjects();

            for (int i = 0; i < levelObjects.Length; i++)
            {
                if (!sceneTitle.StartsWith("F"))
                    continue;

                CustomLevelObject levelObject = levelObjects[i];

                if (sceneIndex > 0.0)
                {
                    int weight = 75;

                    if (sceneIndex > 1.0)
                        weight = 125;

                    sceneObject.potentialNPCs.Add(new() { selection = assets.Get<NPC>("npcs/Bezz"), weight = weight });
                }

                levelObject.posters = levelObject.posters.AddToArray<WeightedPosterObject>(new() { selection = ObjectCreators.CreatePosterObject(assets.Get<Texture2D>("Images/PosterObject/poster0"), []), weight = 25 });
            }
        }
    }
}