using System;
using UnityEngine;
using Wizard;
using Wizard.Battle.Resource;

public class SkillParameter
{
	public BattlePlayerBase selfBattlePlayer { get; set; }

	public BattlePlayerBase opponentBattlePlayer { get; set; }

	public BattleCardBase ownerCard { get; set; }

	public SkillCreator.SkillBuildInfo buildInfo { get; set; }

	public IBattleResourceMgr resourceMgr { get; set; }

	public Func<Vector3> afterFallPos { get; set; }

	public BattlePlayerReadOnlyInfoPair CreateInfoPair()
	{
		return new BattlePlayerReadOnlyInfoPair(selfBattlePlayer, opponentBattlePlayer);
	}
}
