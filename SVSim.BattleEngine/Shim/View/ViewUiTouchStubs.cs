// AUTHORED SHIM (not copied). The battle View / UI / Touch / Replay / RoomMatch
// presentation tree the engine holds references to but never drives headless
// (IsForecast suppresses VFX; we never pump input or rendering). Stubbed in their
// ORIGINAL namespaces so the copied engine's type references resolve. Members grow
// only as the compile loop demands a specific call. Most are referenced as field/
// parameter types only, so empty stubs suffice.

namespace Wizard.Battle.View
{
    public partial interface IReadOnlyVoiceInfo { }
    public partial class BattleCardView
    {
        // BuildInfo (14-arg ctor + members) provided by Generated/BattleCardView_BuildInfo.g.cs
        // Parameterless ctor lets the no-op subclass hand stubs (ClassBattleCardViewBase,
        // NullBattleCardView) and any non-chaining stub satisfy their implicit base() call.
        public BattleCardView() { }
        public BattleCardView(BuildInfo buildInfo) { _buildInfo = buildInfo; }

        // HEADLESS-FIX (M-HC-4a): the receive ATTACK path reads BattleCardView.CardInfo (the backing
        // card) and BattleCardView._inPlayFrameEffect on the resolve path (InPlayCardReflection /
        // ActionProcessor.Attack). The interface getters in Generated/_IfaceImpl.g.cs surface these two
        // fields. CardInfo comes from the stored BuildInfo (cardInfo == the card, IReadOnlyBattleCardInfo,
        // so IsClass etc. are authentic); _inPlayFrameEffect is a non-null no-op frame-effect control
        // whose HideFrameEffect/UpdateCanAttackEffect are empty (Generated/InPlayCardFrameEffectControl.g.cs).
        internal BuildInfo _buildInfo;
        internal IReadOnlyBattleCardInfo HeadlessCardInfo => _buildInfo?.cardInfo;
        internal InPlayCardFrameEffectControl _headlessInPlayFrameEffect =
            new InPlayCardFrameEffectControl(null, null, null);

        // AttackTargetSelectInfo provided by Generated/BattleCardView_AttackTargetSelectInfo.g.cs
        //
        // HEADLESS-FIX: lazily non-null GameObject so unguarded Unity touches on the IsRecovery
        // path resolve as no-ops instead of NRE-ing on the shim's null default. Matches the
        // existing Component.gameObject lazy pattern (UnityShim.cs:94). The IsRecovery card-create
        // delegate (NetworkBattleManagerBase.cs:379) passes null for cardGameObject, which left
        // BattleCardView.GameObject null and caused Skill_metamorphose.cs:147 (the in-play
        // metamorphose branch — Petrification etc.) to NRE on
        // `metamorphosedCard.BattleCardView.GameObject.transform.rotation = Quaternion.identity`,
        // a purely cosmetic transform reset; making it a no-op preserves the surrounding state
        // mutations (ReplaceInPlay, SetUpInplay, FlagCardAsDestroyedBySkill, RemoveFromInPlay).
        // Live regression: bid 283192092460, A's Petrification on B's in-play card idx 1.
        private UnityEngine.GameObject _gameObject;
        public virtual UnityEngine.GameObject GameObject
        {
            get => _gameObject ??= new UnityEngine.GameObject();
            protected set => _gameObject = value;
        }
    }
    public partial class NonDialogPopup : UnityEngine.MonoBehaviour { }  // Close() in Generated/NonDialogPopup.g.cs
    public abstract class BattlePlayerViewBase
    {
        public enum BattleDialogItem { Menu, Retire }
        public bool IsSelecting { get; set; }
    }
    public partial class InPlayCardFrameEffectControl { }
}

namespace Wizard.Battle.UI
{
    public partial class BattleLogItem : UnityEngine.MonoBehaviour { }
    public partial class BattleLogManager { }
    public partial class BattleLogWindow : UnityEngine.MonoBehaviour
    {
        public enum BattleLogType { Battle, PlayCardLog, Destruction}
    }
    public partial class EvolutionConfirmation { }
}

namespace Wizard.Battle.Touch
{
    public partial class SkillTargetSelectTouchProcessor { }
    public partial class EvolutionTouchProcessor
    {
        // events dropped by m1_stub_gen (generator does not capture `event` decls)
        public event global::System.Func<BattleCardBase, global::Wizard.Battle.View.Vfx.VfxBase> OnFocusTarget;
        public event global::System.Func<BattleCardBase, global::Wizard.Battle.View.Vfx.VfxBase> OnUnfocusTarget;
        public event global::System.Func<BattleCardBase, global::Wizard.Battle.View.Vfx.VfxBase> OnSelectTarget;
        public event global::System.Action OnNotSelectTarget;
    }
    public partial class SetCardProcessor { }
    public partial class EvolutionSimpleProcessor { }
    public partial class EmotionTouchProcessor { }
    public partial class DetailPanelTouchProcessor { }
    public partial class ClassBuffTouchProcessor { }
}

namespace Wizard.Battle.Replay
{
    public interface IReplayRecordManager
    {
        void SetupRecording(BattleManagerBase battleMgr);
        void SetupBattleInfoFilter();
        void SetupOperateMgrEvents(BattleManagerBase battleMgr);
    }
}

namespace Wizard.Replay
{
}

namespace Wizard.RoomMatch
{
    public partial class WatchDataHandler { }
    // RoomConnectController (members + BattleRule/PositionMode enums + InitializeParameter)
    // provided by Generated/RoomConnectController*.g.cs
}

namespace Wizard.Story
{
    public class StoryRecoveryData
    {
        public StoryRecoveryData(LitJson.JsonData jsonData) { }
        public StoryRecoveryData(SelectedStoryInfo data) { }
        public int ChapterCharaId { get; }
        public LitJson.JsonData ToJsonData() => default!;
    }
}

namespace Wizard.UI.Common
{
}

namespace Wizard.UI.Dialog.ImageSelection
{
    public partial class ImageSelection : UnityEngine.MonoBehaviour { }
}
