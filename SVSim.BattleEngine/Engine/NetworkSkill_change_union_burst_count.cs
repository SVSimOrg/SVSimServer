using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_change_union_burst_count : Skill_change_union_burst_count
{
	public NetworkSkill_change_union_burst_count(NetworkBattleManagerBase battleManager, SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		Func<SkillBase, List<BattleCardBase>, SkillConditionCheckerOption, SkillProcessor, VfxBase> value = delegate(SkillBase localSkill, List<BattleCardBase> localCards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
		{
			if (!(localSkill.ApplyingTargetFilter is SkillTargetHandSelfFilter) && NetworkBattleGenericTool.IsUnapprovedTarget(localSkill) && localCards.Count() > 0)
			{
				int gainUnionBurstCount = ((Skill_change_union_burst_count)localSkill).GetGainUnionBurstCount();
				RegisterChangeUnionBurstCount data = new RegisterChangeUnionBurstCount(localCards, localSkill, gainUnionBurstCount);
				battleManager.RegisterActionManager.Add(data);
			}
			return NullVfx.GetInstance();
		};
		base.OnSkillEnd -= value;
		base.OnSkillEnd += value;
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		List<BattleCardBase> source = parameter.targetCards.Where((BattleCardBase t) => t.HasUnionBurst && (t.IsPlayer || SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch)).ToList();
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnChangeUnionBurstAndSkyboundArt(source.ToList());
		return result;
	}
}
