using System.Linq;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_turn_start_fixed_pp : SkillBase
{
	public Skill_turn_start_fixed_pp(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			targetCard.SkillApplyInformation.GiveTurnStartFixedPP();
			CardParameter baseParameter = base.SkillPrm.ownerCard.BaseParameter;
			BuffInfo buffInfo = new BuffInfo(baseParameter.BaseCardId, baseParameter.NormalCardId, this);
			targetCard.AddBuffInfo(buffInfo);
			buffInfoContainer.Add(new BuffInfoContainer(targetCard, buffInfo, -1, "", null, 0L));
			if (targetCard.IsClass)
			{
				UpdateClassBuffIfActive(targetCard);
			}
		}
		if (IsBattleLog && parameter.targetCards.Count() > 0)
		{
			BattleLogManager.GetInstance().AddLogSkillGain(parameter.targetCards.ToList(), this, SkillGainType.TurnStartFixedPP);
		}
		return NullVfxWithLoading.GetInstance();
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		foreach (BuffInfoContainer item in buffInfoContainer)
		{
			item._targetCard.SkillApplyInformation.DepriveTurnStartFixedPP();
			item._targetCard.RemoveBuffInfo(item._buffInfo);
			if (item._targetCard.IsClass)
			{
				UpdateClassBuffIfActive(item._targetCard);
			}
		}
		return NullVfxWithLoading.GetInstance();
	}
}
