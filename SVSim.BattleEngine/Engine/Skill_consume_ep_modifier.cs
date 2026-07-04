using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_consume_ep_modifier : SkillBase
{
	private string _tribe = string.Empty;

	private string _option = string.Empty;

	public Skill_consume_ep_modifier(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		_option = option;
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		if (_option == "none")
		{
			foreach (BattleCardBase targetCard in parameter.targetCards)
			{
				SetupNotConsumeTarget(targetCard, targetCard, null);
			}
			if (IsBattleLog)
			{
				BattleLogManager.GetInstance().AddLogSkillGain(parameter.targetCards.ToList(), this, SkillGainType.DontConsumeEp);
			}
		}
		else
		{
			_tribe = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.tribe, "ALL");
			ValueWithOperator valueWithOperator = base.OptionValue.GetValueWithOperator(SkillFilterCreator.ContentKeyword.base_card_id);
			foreach (BattleCardBase targetCard2 in parameter.targetCards)
			{
				SetupNotConsumeTarget(targetCard2, null, valueWithOperator);
			}
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		return vfxWithLoadingSequential;
	}

	private void SetupNotConsumeTarget(BattleCardBase targetCard, BattleCardBase notConsumeCard, ValueWithOperator cardId)
	{
		NotConsumeEpModifierInfo notConsumeEpModifierInfo = new NotConsumeEpModifierInfo(_tribe, targetCard, this, notConsumeCard, cardId);
		BuffInfoContainer buffInfoContainer = new BuffInfoContainer(targetCard, null, -1, "", null, 0L, notConsumeEpModifierInfo);
		base.buffInfoContainer.Add(buffInfoContainer);
		targetCard.SkillApplyInformation.GiveNotConsumeEpModifier(notConsumeEpModifierInfo);
		SetOnLoseEvent(targetCard, null, buffInfoContainer);
		if (!targetCard.IsClass && IsBattleLog)
		{
			AddBuffInfoIfNeeded(targetCard);
		}
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		foreach (BuffInfoContainer item in buffInfoContainer)
		{
			item._targetCard.SkillApplyInformation.DepriveNotConsumeEpModifier(item._notConsumeEpModifierInfo);
		}
		buffInfoContainer.Clear();
		return NullVfxWithLoading.GetInstance();
	}

	private void ClearParameterModifier(BuffInfoContainer info)
	{
		info._targetCard.SkillApplyInformation.ForceDepriveNotConsumeEpModifier();
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate
		{
			ClearParameterModifier(container);
			buffInfoContainer.Remove(container);
			return NullVfx.GetInstance();
		};
	}
}
