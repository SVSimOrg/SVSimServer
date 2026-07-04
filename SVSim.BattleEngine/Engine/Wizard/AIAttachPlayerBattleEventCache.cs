using System;

namespace Wizard;

public class AIAttachPlayerBattleEventCache
{
	public Action<BattleCardBase, BattlePlayerBase.CEMETERY_TYPE, bool, SkillBase> AllyAddCemeteryEvent;

	public Action<BattleCardBase, BattlePlayerBase.CEMETERY_TYPE, bool, SkillBase> OpponentAddCemeteryEvent;

	public Action OpponentTurnStartCompleteEvent;

	public Action AllyTurnStartCompleteEvent;

	public Action AllySelfTurnEndEvent;

	public Action OpponentSelfTurnEndEvent;
}
