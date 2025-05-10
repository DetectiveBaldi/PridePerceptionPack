using HarmonyLib;
using MidiPlayerTK;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using PridePerception.core;
using PridePerception.util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PridePerception.npcs
{
    public class Bezz : NPC
    {
        public AudioManager audio;

        public CustomSpriteAnimator animator;

        public float timeBeforeTargeting;

        public int flagType;

        public List<Flag> flags;

        public IEnumerator collectionPending;

        public HudGauge timerGauge;

        public override void Initialize()
        {
            base.Initialize();

            audio = GetComponent<AudioManager>();

            LoadFrames();

            behaviorStateMachine.ChangeState(new BezzWanderState(this, new NavigationState_WanderRandom(this, 0)));

            flagType = -1;

            flags = [];

            collectionPending = WaitForCollection();
        }

        public override void Despawn()
        {
            base.Despawn();

            MusicManager musicManager = Singleton<MusicManager>.Instance;

            MidiFilePlayer midiFilePlayer = (MidiFilePlayer)ReflectionHelpers.ReflectionGetVariable(musicManager, "midiPlayer");

            if (midiFilePlayer.MPTK_MidiName.Contains("FlagCatch"))
                musicManager.StopMidi();

            flagType = -1;

            DisposeFlags();

            StopCoroutine(collectionPending);
        }

        public void OnDestroy()
        {
            if (flagType != -1.0f)
            {
                CoreGameManager coregm = Singleton<CoreGameManager>.Instance;

                if (coregm.GetHud(0) != null)
                    coregm.AddPoints(-100, 0, true);

                DisposeFlags();
            }
        }

        public void LoadFrames()
        {
            animator = gameObject.AddComponent<CustomSpriteAnimator>();

            animator.spriteRenderer = spriteRenderer[0];

            Sprite[] idleSprites = [Plugin.current.assets.Get<Sprite>("Images/npcs/Bezz/bezzIdle")];

            animator.animations.Add("idle", new(idleSprites, 1.0f));

            Sprite[] walkSprites = [];

            for (int i = 0; i < 12.0f; i++)
                walkSprites = walkSprites.AddToArray(Plugin.current.assets.Get<Sprite>("Images/npcs/Bezz/bezzWalk" + i));

            animator.animations.Add("walk", new(walkSprites, 1.0f));

            Sprite[] happyTalkSprites = [];

            for (int i = 0; i < 2.0f; i++)
                happyTalkSprites = happyTalkSprites.AddToArray(Plugin.current.assets.Get<Sprite>("Images/npcs/Bezz/bezzHappyTalk" + i));

            animator.animations.Add("happyTalk", new(happyTalkSprites, 0.1f));

            Sprite[] upsetTalkSprites = [];

            for (int i = 0; i < 2.0f; i++)
                upsetTalkSprites = upsetTalkSprites.AddToArray(Plugin.current.assets.Get<Sprite>("Images/npcs/Bezz/bezzUpsetTalk" + i));

            animator.animations.Add("upsetTalk", new(upsetTalkSprites, 0.1f));

            animator.Play("idle", 1.0f);

            animator.SetDefaultAnimation("idle", 1.0f);
        }

        public IEnumerator WaitForCollection()
        {
            MusicManager musicManager = Singleton<MusicManager>.Instance;

            MidiFilePlayer midiFilePlayer = (MidiFilePlayer)ReflectionHelpers.ReflectionGetVariable(musicManager, "midiPlayer");

            float totalTime = Mathf.Ceil(midiFilePlayer.MPTK_DurationMS * 0.001f);

            float timeLeft = totalTime;

            CoreGameManager coregm = Singleton<CoreGameManager>.Instance;

            timerGauge = coregm.GetHud(0).gaugeManager.ActivateNewGauge(Plugin.current.assets.Get<Sprite>("Images/npcs/Bezz/Flag/3dflag" + flagType), totalTime);

            while (timeLeft > 0.0f)
            {
                timeLeft -= Time.deltaTime * TimeScale;

                timerGauge.SetValue(totalTime, timeLeft);

                yield return null;
            }

            LossSequence();

            timeBeforeTargeting = 120.0f;

            flagType = -1;

            for (int i = 0; i < flags.Count; i++)
            {
                Flag flag = flags[i];

                MapMarkerUtil.Dispose(ec.map, flag.mapMarker);

                flag.tweenOut();
            }

            timerGauge.Deactivate();
        }

        public Flag LoadFlag(Cell cell, int _flagType)
        {
            GameObject gameObject = new();

            Flag flag = gameObject.AddComponent<Flag>().Initialize(this, cell, _flagType);

            flags.Add(flag);

            return flag;
        }

        public void DisposeFlag(Flag flag)
        {
            flags.Remove(flag);

            UnityEngine.Object.Destroy(flag);

            MapMarkerUtil.Dispose(ec.map, flag.mapMarker);

            UnityEngine.Object.Destroy(flag.renderer);
        }

        public void DisposeFlags()
        {
            for (int i = flags.Count - 1; i >= 0.0f; i--)
                DisposeFlag(flags[i]);
        }

        public void CheckFlag(Flag flag)
        {
            if (flagType == flag.flagType)
                WinSequence();
            else
                LossSequence();

            timeBeforeTargeting = 120.0f;

            flagType = -1;

            for (int i = 0; i < flags.Count; i++)
            {
                Flag _flag = flags[i];

                MapMarkerUtil.Dispose(ec.map, _flag.mapMarker);

                _flag.tweenOut();
            }

            timerGauge.Deactivate();

            StopCoroutine(collectionPending);
        }

        public void WinSequence()
        {
            behaviorStateMachine.ChangeState(new BezzCheckState(this, new NavigationState_DoNothing(this, 62), true));

            CoreGameManager coregm = Singleton<CoreGameManager>.Instance;

            PlayerManager pm = coregm.GetPlayer(0);

            bool spawnItem = pm.itm.Has(Items.None);

            coregm.AddPoints(spawnItem ? 50 : 75, 0, true);

            MusicManager musicManager = Singleton<MusicManager>.Instance;

            MidiFilePlayer midiFilePlayer = (MidiFilePlayer) ReflectionHelpers.ReflectionGetVariable(musicManager, "midiPlayer");

            if (midiFilePlayer.MPTK_MidiName.Contains("FlagCatch"))
            {
                musicManager.StopMidi();

                musicManager.PlayMidi(Plugin.current.assets.Get<string>("Midis/npcs/Bezz/FlagCatchEnd0"), false);
            }

            if (spawnItem)
            {
                ItemMetaStorage itemMetaStorage = ItemMetaStorage.Instance;

                List<WeightedItemObject> foodItems =
                [
                    new() { selection = itemMetaStorage.FindByEnum(Items.ZestyBar).value , weight = 100 },

                    new() { selection = itemMetaStorage.FindByEnum(Items.NanaPeel).value , weight = 75 },

                    new() { selection = itemMetaStorage.FindByEnum(Items.DietBsoda).value , weight = 45 },

                    new() { selection = itemMetaStorage.FindByEnum(Items.Bsoda).value , weight = 25 },

                    new() { selection = itemMetaStorage.FindByEnum(Items.Apple).value , weight = 5 }
                ];

                Vector3 lPosition = pm.transform.localPosition;

                ec.CreateItem(ec.CellFromPosition(lPosition).room, WeightedSelection<ItemObject>.RandomSelection([.. foodItems]), new(lPosition.x, lPosition.z));
            }
        }

        public void LossSequence()
        {
            behaviorStateMachine.ChangeState(new BezzCheckState(this, new NavigationState_DoNothing(this, 62), false));

            Singleton<CoreGameManager>.Instance.AddPoints(-100, 0, true);

            MusicManager musicManager = Singleton<MusicManager>.Instance;

            MidiFilePlayer midiFilePlayer = (MidiFilePlayer)ReflectionHelpers.ReflectionGetVariable(musicManager, "midiPlayer");

            if (midiFilePlayer.MPTK_MidiName.Contains("FlagCatch"))
            {
                musicManager.StopMidi();

                musicManager.PlayMidi(Plugin.current.assets.Get<string>("Midis/npcs/Bezz/FlagCatchEnd1"), false);
            }
        }
    }

    public class BezzBaseState(Bezz _bezz, NavigationState _navigationState) : NpcState(_bezz)
    {
        public Bezz bezz = _bezz;

        public NavigationState navigationState = _navigationState;

        public override void Enter()
        {
            base.Enter();

            ChangeNavigationState(navigationState);
        }
    }

    public class BezzWanderState(Bezz _bezz, NavigationState _navigationState) : BezzBaseState(_bezz, _navigationState)
    {
        public override void Enter()
        {
            base.Enter();

            bezz.Navigator.SetSpeed(22.5f);

            bezz.Navigator.maxSpeed = 22.5f;

            bezz.animator.ChangeSpeed(1.0f);

            bezz.animator.Play("walk", 1.0f);

            bezz.animator.SetDefaultAnimation("walk", 1.0f);
        }

        public override void Update()
        {
            base.Update();

            if (bezz.timeBeforeTargeting > 0.0f)
                bezz.timeBeforeTargeting -= Time.deltaTime * bezz.TimeScale;
        }

        public override void PlayerInSight(PlayerManager pm)
        {
            base.PlayerInSight(pm);

            if (bezz.timeBeforeTargeting <= 0.0f && !pm.Tagged)
                bezz.behaviorStateMachine.ChangeState(new BezzTargetState(bezz, new NavigationState_TargetPlayer(bezz, 62, pm.transform.position, true), pm));
        }

        public override void Exit()
        {
            base.Exit();

            bezz.animator.ChangeSpeed(1.0f);
        }
    }

    public class BezzTargetState(Bezz _bezz, NavigationState _navigationState, PlayerManager _pm) : BezzBaseState(_bezz, _navigationState)
    {
        public PlayerManager pm = _pm;

        public override void Enter()
        {
            base.Enter();

            bezz.Navigator.SetSpeed(30.0f);

            bezz.Navigator.maxSpeed = 30.0f;

            bezz.audio.FlushQueue(true);

            bezz.audio.QueueAudio(Plugin.current.assets.Get<SoundObject>("Sounds/npcs/Bezz/bezzTarget0"));

            bezz.audio.QueueAudio(Plugin.current.assets.Get<SoundObject>("Sounds/npcs/Bezz/bezzTarget1"));

            bezz.animator.ChangeSpeed(1.325f);

            bezz.animator.Play("walk", 1.325f);

            bezz.animator.SetDefaultAnimation("walk", 1.325f);
        }

        public override void PlayerSighted(PlayerManager _pm)
        {
            base.PlayerSighted(pm);

            if (!_pm.tagged)
            {
                bezz.audio.FlushQueue(true);

                bezz.audio.QueueAudio(Plugin.current.assets.Get<SoundObject>("Sounds/npcs/Bezz/bezzTarget0"));

                bezz.audio.QueueAudio(Plugin.current.assets.Get<SoundObject>("Sounds/npcs/Bezz/bezzTarget1"));
            }
        }

        public override void PlayerInSight(PlayerManager _pm)
        {
            base.PlayerInSight(_pm);

            if (!_pm.Tagged)
                currentNavigationState.UpdatePosition(_pm.transform.position);
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();

            bezz.behaviorStateMachine.ChangeState(new BezzWanderState(bezz, new NavigationState_WanderRandom(bezz, 62)));
        }

        public override void OnStateTriggerStay(Collider collider)
        {
            base.OnStateTriggerStay(collider);

            if (collider.CompareTag("Player"))
            {
                PlayerManager _pm = collider.GetComponent<PlayerManager>();

                if (!_pm.Tagged)
                    bezz.behaviorStateMachine.ChangeState(new BezzPromptState(bezz, new NavigationState_DoNothing(bezz, 62, true), _pm));
            }
        }

        public override void Exit()
        {
            base.Exit();

            bezz.animator.ChangeSpeed(1.0f);
        }
    }

    public class BezzPromptState(Bezz _bezz, NavigationState _navigationState, PlayerManager _pm) : BezzBaseState(_bezz, _navigationState)
    {
        public PlayerManager pm = _pm;

        public override void Enter()
        {
            base.Enter();

            bezz.Navigator.SetSpeed(22.5f);

            bezz.Navigator.maxSpeed = 22.5f;

            bezz.Navigator.Entity.AddForce(new(bezz.transform.position - pm.transform.position, 20.0f, -60.0f));

            bezz.audio.FlushQueue(true);

            bezz.audio.QueueAudio(Plugin.current.assets.Get<SoundObject>("Sounds/npcs/Bezz/bezzPrompt0"));

            bezz.audio.QueueAudio(Plugin.current.assets.Get<SoundObject>("Sounds/npcs/Bezz/bezzPrompt1"));

            bezz.animator.ChangeSpeed(1.0f);

            bezz.animator.Play("happyTalk", 1.0f);

            bezz.animator.SetDefaultAnimation("happyTalk", 1.0f);

            bezz.flagType = UnityEngine.Random.Range(0, 5);

            List<Cell> cells = [.. bezz.ec.AllTilesNoGarbage(false, false).Where(cell => cell.room.category == RoomCategory.Hall)];

            CoreGameManager coregm = Singleton<CoreGameManager>.Instance;

            int multiplier = Math.Max(coregm.sceneObject.levelNo, 0) + 1;

            for (int i = 0; i < 15.0f * multiplier; i++)
            {
                if (cells.Count <= multiplier)
                    break;

                Cell cell = cells[UnityEngine.Random.Range(0, cells.Count)];

                cells.Remove(cell);

                int flagType = UnityEngine.Random.Range(0, 5);

                while (bezz.flagType == flagType)
                    flagType = UnityEngine.Random.Range(0, 5);

                Flag flag = bezz.LoadFlag(cell, flagType);
            }

            for (int i = 0; i < multiplier; i++)
            {
                if (cells.Count == 0.0)
                    break;

                Cell cell = cells[UnityEngine.Random.Range(0, cells.Count)];

                cells.Remove(cell);

                Flag flag = bezz.LoadFlag(cell, bezz.flagType);
            }

            coregm.audMan.PlaySingle(SoundObjectUtil.fromName("CashBell"));

            MusicManager musicManager = Singleton<MusicManager>.Instance;

            musicManager.StopMidi();

            musicManager.PlayMidi(Plugin.current.assets.Get<string>("Midis/npcs/Bezz/FlagCatch" + UnityEngine.Random.Range(0, 2)), false);

            bezz.collectionPending = bezz.WaitForCollection();

            bezz.StartCoroutine(bezz.collectionPending);
        }

        public override void Update()
        {
            base.Update();

            if (!bezz.audio.AnyAudioIsPlaying && bezz.animator.currentAnimationName == "happyTalk")
            {
                bezz.animator.ChangeSpeed(1.0f);

                bezz.animator.Play("idle", 1.0f);

                bezz.animator.SetDefaultAnimation("idle", 1.0f);
            }
        }

        public override void Exit()
        {
            base.Exit();

            bezz.audio.FlushQueue(true);

            bezz.animator.ChangeSpeed(1.0f);
        }
    }

    public class BezzCheckState(Bezz _bezz, NavigationState _navigationState, bool _win) : BezzBaseState(_bezz, _navigationState)
    {
        public bool win = _win;

        public override void Enter()
        {
            base.Enter();

            bezz.Navigator.SetSpeed(22.5f);

            bezz.Navigator.maxSpeed = 22.5f;

            bezz.audio.FlushQueue(true);

            if (win)
            {
                bezz.audio.QueueAudio(Plugin.current.assets.Get<SoundObject>("Sounds/npcs/Bezz/bezzActivityWin0"));

                CoreGameManager coregm = Singleton<CoreGameManager>.Instance;

                PlayerManager pm = coregm.GetPlayer(0);

                bool spawnItem = pm.itm.Has(Items.None);

                bezz.audio.QueueAudio(Plugin.current.assets.Get<SoundObject>("Sounds/npcs/Bezz/bezzActivityWin1" + (spawnItem ? "" : "Alt")));
            }
            else
            {
                bezz.audio.QueueAudio(Plugin.current.assets.Get<SoundObject>("Sounds/npcs/Bezz/bezzActivityLoss0"));

                bezz.audio.QueueAudio(Plugin.current.assets.Get<SoundObject>("Sounds/npcs/Bezz/bezzActivityLoss1"));

                bezz.audio.QueueAudio(Plugin.current.assets.Get<SoundObject>("Sounds/npcs/Bezz/bezzActivityLoss2"));
            }

            string animatorPrefix = win ? "happy" : "upset";

            bezz.animator.ChangeSpeed(1.0f);

            bezz.animator.Play(animatorPrefix + "Talk", 1.0f);

            bezz.animator.SetDefaultAnimation(animatorPrefix + "Talk", 1.0f);
        }

        public override void Update()
        {
            base.Update();

            if (!bezz.audio.AnyAudioIsPlaying)
                bezz.behaviorStateMachine.ChangeState(new BezzWanderState(bezz, new NavigationState_WanderRandom(bezz, 62)));
        }
    }

    public class Flag : MonoBehaviour, IClickable<int>
    {
        public Bezz bezz;

        public Cell cell;

        public int flagType;

        public MapMarker mapMarker;

        public GameObject renderer;

        public IEnumerator tween;

        public Action onTweenFinished;

        public Flag Initialize(Bezz _bezz, Cell _cell, int _flagType)
        {
            bezz = _bezz;

            cell = _cell;

            flagType = _flagType;

            mapMarker = MapMarkerUtil.Initialize(bezz.ec.map, cell, Plugin.current.assets.Get<Sprite>("Images/npcs/Bezz/Flag/mapMarker"), Plugin.current.assets.Get<Sprite>("Images/npcs/Bezz/Flag/mapIcon"));

            gameObject.transform.localPosition = cell.TileTransform.localPosition;

            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();

            boxCollider.isTrigger = true;

            boxCollider.size = new(3.5f, 10.0f, 3.5f);

            ((SpriteRenderer)ReflectionHelpers.ReflectionGetVariable(mapMarker, "environmentMarker")).transform.localPosition += Vector3.up * 4.105f;

            renderer = new();

            renderer.layer = LayerMask.NameToLayer("Billboard");

            renderer.transform.localPosition = gameObject.transform.localPosition;

            SpriteRenderer spriteRenderer = renderer.AddComponent<SpriteRenderer>();

            spriteRenderer.sprite = Plugin.current.assets.Get<Sprite>("Images/npcs/Bezz/Flag/" + flagType + "0");

            spriteRenderer.material = ObjectCreators.SpriteMaterial;

            CustomSpriteAnimator animator = renderer.AddComponent<CustomSpriteAnimator>();

            animator.spriteRenderer = spriteRenderer;

            Sprite[] idleSprites = [];

            for (int i = 0; i < 2.0f; i++)
                idleSprites = idleSprites.AddToArray(Plugin.current.assets.Get<Sprite>("Images/npcs/Bezz/Flag/" + flagType + "" + i));

            animator.animations.Add("idle", new([.. idleSprites], 1.0f));

            animator.Play("idle", 1.0f);

            animator.SetDefaultAnimation("idle", 1.0f);

            tweenIn();

            return this;
        }

        public void Update()
        {
            if (mapMarker != null)
                mapMarker.ShowMarker(Singleton<CoreGameManager>.Instance.GetCamera(0).QuickMapAvailable);
        }

        public void Clicked(int pm)
        {
            if (tween == null)
                bezz.CheckFlag(this);
        }

        public void ClickableSighted(int pm)
        {

        }

        public void ClickableUnsighted(int pm)
        {

        }

        public bool ClickableHidden()
        {
            return tween != null;
        }

        public bool ClickableRequiresNormalHeight()
        {
            return false;
        }

        public void resetTweenCoroutine()
        {
            if (tween != null)
            {
                StopCoroutine(tween);

                tween = null;
            }

            onTweenFinished = null;

            tween = startTween();

            StartCoroutine(tween);
        }

        public IEnumerator startTween()
        {
            while (Vector3.Distance(renderer.transform.localPosition, gameObject.transform.localPosition) != 0.0f)
            {
                renderer.transform.localPosition = Vector3.MoveTowards(renderer.transform.localPosition, gameObject.transform.localPosition,
                    10.0f * Time.deltaTime);

                yield return null;
            }

            tween = null;

            if (onTweenFinished != null)
            {
                onTweenFinished.Invoke();

                onTweenFinished = null;
            }
        }

        public void tweenIn()
        {
            gameObject.transform.localPosition += Vector3.up * 3.85f;

            resetTweenCoroutine();
        }

        public void tweenOut()
        {
            transform.localPosition += Vector3.down * 7.85f;

            resetTweenCoroutine();

            onTweenFinished = () => bezz.DisposeFlag(this);
        }
    }
}