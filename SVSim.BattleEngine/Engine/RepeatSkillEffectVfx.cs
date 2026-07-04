using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class RepeatSkillEffectVfx : SequentialVfxPlayer
{

	public RepeatSkillEffectVfx(BattleManagerBase mgr, IBattleCardView cardView, string repeatTiming, bool isSelf)
	{
		if (cardView != null && !(cardView.GameObject == null) && repeatTiming == "when_destroy")
		{
			Register(VfxWithLoadingSequential.Create());
			Register(WaitVfx.Create(1f));
			mgr.OperateMgr.CallOnShowRepeatSkillEffect(isSelf);
		}
	}
}
