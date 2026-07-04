using Wizard.Battle.Recovery;

// PASS-8/Phase-2 STUB: 1,285-line client-side story/area-selection UI reduced to the
// compile-time surface external callers touch. Held alive by story/area/quest navigation
// chain calling AreaSelectUI.SetRecoveryData from battle-recovery paths. One member
// preserved (SetRecoveryData); two additional members were initially in the stub —
// IsUseChapterListClearedMask and GetChapterTitleStory — but cascade round 1 auto-removed
// them when ChapterSelectButton.cs was deleted. See Task 2b of PASS8-PLAN.md.
public class AreaSelectUI : UIBase
{

    public static void SetRecoveryData(SetupConditionInfo setupInfo) { }
}
