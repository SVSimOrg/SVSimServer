using UnityEngine;

// PASS-8/Phase-1 STUB: 776-line client-side battle-result UI. Held on BattleManagerBase
// as `public BattleResultUIController BattleResultControl` and carried into 6 NextSceneSelector
// variants (Arena/Colosseum/FreeMatch/NetworkMatch/Practice/Null) + 3 ResultAnimationAgent
// subclasses. Headless never runs the result screen; all instance-method calls happen on the
// null field (mgr.BattleResultControl stays null in headless). Reduced to the compile-time
// surface the ctor-params + external touches require. The 3 static color constants and 6
// externally-touched members (TitleWin, TitleLose, IsResultOn, AlreadyResultRecovery,
// StartUI, SetSpecialResultTypeText) are kept as no-op/default returns.
public class BattleResultUIController : MonoBehaviour
{

    public UISprite TitleWin;
    public UISprite TitleLose;

    public bool IsResultOn { get; private set; }
    public bool AlreadyResultRecovery { get; set; }

    public void StartUI(bool win, BattleCamera battleCamera) { }
    public void SetSpecialResultTypeText(string text) { }
}
