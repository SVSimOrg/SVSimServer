// AUTHORED SHIM (not copied). Non-battle game types (settings widgets, error-dialog
// data, login-bonus, deck-builder, story chapter-selection, room-match, socket.io)
// swept into the copy closure but never driven headless. Stubbed in their ORIGINAL
// namespaces so the copied engine resolves their type references. Enums are replicated
// VERBATIM from decomp (integer values can be cast); classes are empty until the loop
// demands a member.
using UnityEngine;

namespace Wizard.Dialog.Setting
{
    // Real base of ItemButton/ItemToggle/... (copied). Abstract methods the copied
    // subclasses override -- without this base they fall back to the unrelated
    // Wizard.Item data class and CS0115 ("no method to override").
    public abstract class Item : MonoBehaviour
    {
        public abstract void AddChangeCallback(EventDelegate.Callback callback);
        public abstract void SetActive_SeparatorLine(bool isActive);
    }
}

namespace Wizard.ErrorDialog
{
    // Real Wizard.ErrorDialog.Data; without it Dialog.cs's unqualified `Data` falls
    // back to the static Wizard.Data god-object (CS0718/0722/0723 + missing ButtonType).
    public class Data
    {
        public enum ContactDisplayType { }
        public enum ButtonType { _NONE_, OK, リトライ, タイトルへ戻る, ホームへ戻る, アプリ終了, バージョンアップ, 推奨端末一覧}

        public string TitleId { get; private set; }
        public string BodyId { get; private set; }
        public bool IsDisplayContact { get; private set; }
        public ButtonType MainButton { get; private set; }
        public ButtonType SubButton { get; private set; }
        public int PanelDepth { get; private set; }

        public Data(string id, string titleId, string bodyId, string contactDisplay,
                    string mainButton, string subButton, string panelDepth) { }
    }
}

namespace Wizard.Battle.UI
{
    public enum CantAttackType { Null, All, Class, NotHasGuard, Unit}
}

namespace Wizard.Battle.View
{
    // Decomp bases (dropped by the hand stub): both derive from BattleCardView, which
    // carries the IBattleCardView impl — so they convert to IBattleCardView via it.
    // The decomp ClassBattleCardViewBase also implements IClassBattleCardView; the resolution
    // path casts the created view to it (ClassBattleCardBase.Setup), so re-attach the dropped
    // interface here with no-op members (the leaf PlayerClassBattleCardView inherits them).
    public abstract class ClassBattleCardViewBase : BattleCardView, IClassBattleCardView
    {
        // HEADLESS-FIX (M-HC-4a): forward the BuildInfo to the BattleCardView base so a class (leader)
        // view's CardInfo resolves to its backing card — the receive ATTACK path reads CardInfo.IsClass
        // (true for a leader) via AttackSelectControl.IsCardTranslatable when an attack targets the leader.
        protected ClassBattleCardViewBase() { }
        protected ClassBattleCardViewBase(BuildInfo buildInfo) : base(buildInfo) { }
        public virtual Wizard.Battle.Player.ClassCharacter.IClassCharacter ClassCharacter => null;
        public virtual void StartOutFrame() { }
        public virtual void StartIntoFrame() { }
        public virtual float GetCurrentClipTime() => 0f;
        public virtual bool GetCurrentClipIsName(global::ClassCharaPrm.MotionType motionType) => false;
        public virtual void ClearSpineObject() { }
    }
    public class NullBattleCardView : BattleCardView { public NullBattleCardView() { } public NullBattleCardView(BuildInfo buildInfo) : base(buildInfo) { } public static void ReleaseSharedDummy() { } } // HEADLESS-FIX (M-HC-4a): chain BuildInfo so a null-view card's CardInfo still resolves

    // The decomp NullClassBattleCardView is `: NullBattleCardView, IClassBattleCardView, IBattleCardView`;
    // base-clause recovery kept only the base class. IBattleCardView is satisfied via the BattleCardView
    // base, but IClassBattleCardView was dropped. The generated NullClassBattleCardView stub already
    // provides that interface's members (public no-ops), so just re-attach the dropped interface here.
    // The resolution path's VirtualClone (createNullView) -> ClassBattleCardBase.Setup casts the null
    // view to IClassBattleCardView, which throws InvalidCastException at runtime without this (M3,
    // fixed-damage spell: Skill_damage.TakeDamageSingle clones the leader before applying damage).
    // Compiles fine without it (it's a cast, not a member call), so the M1 loop never surfaced it.
    public partial class NullClassBattleCardView : IClassBattleCardView { }
}

namespace Wizard.Battle.View.Vfx
{
    // Base of the copied PuzzleBattleManager.PuzzleOpeningVfx (ctor + abstract override).
    public abstract partial class OpeningVfx : SequentialVfxPlayer
    {
        protected OpeningVfx(BackGroundBase backGround) { }
        public abstract void RegisterOpeningVfx(ClassBattleCardBase playerClass, ClassBattleCardBase enemyClass);

        public static string OpenningLogStep = "";
        public static VfxBase ShowBattleUIImmediatelyVfx(BattlePlayerBase battlePlayerBase, bool fixDirection = false, bool isNewReplay = false, bool isBanmenkun = false) => NullVfx.GetInstance();

        public class WaitVoiceEndVfx : VfxBase { }
        public class OpeningShowCharacterPanelVfx : SequentialVfxPlayer { }
    }
}

namespace AnimationOrTween
{
    public enum DisableCondition { DoNotDisable}
    public enum EnableCondition { EnableThenPlay, IgnoreDisabledState }
}

namespace Wizard.UI.LoginBonus
{
}

namespace DeckBuilder
{
    public partial class GenerateDeckCode { }
}

namespace Wizard.RoomMatch
{
}

namespace Cute
{
}

namespace Wizard.Story
{
    public enum StoryApiType { None, MainStory, LimitedStory, EventStory }
    public partial class SelectedStoryInfo { }
    public partial class StoryWorldDataManager { }
}

namespace Wizard.Title
{
}

namespace BestHTTP.SocketIO
{
}

// ---- namespace anchors (referenced via `using`/qualified path; no type used yet) ----
namespace Wizard.Scripts.Network.Task { }
namespace Wizard.Scripts.Network.Task.Arena { }
namespace Wizard.Scripts.Network.Task.Arena.TwoPick { }
namespace BestHTTP.Decompression.Zlib { }
