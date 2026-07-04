using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_token_draw_modifier : SkillBase
{
	private List<TokenDrawModifier> _tokenDrawModifierList;

	public Skill_token_draw_modifier(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		_tokenDrawModifierList = CreateTokenDrawModifierList();
		List<BattleCardBase> list = parameter.targetCards.ToList();
		for (int i = 0; i < parameter.targetCards.Count(); i++)
		{
			BuffInfo buffInfo = AddBuffInfoIfNeeded(list[i]);
			for (int j = 0; j < _tokenDrawModifierList.Count; j++)
			{
				list[i].SkillApplyInformation.AddTokenDrawModifier(_tokenDrawModifierList[j]);
			}
			buffInfoContainer.Add(new BuffInfoContainer(list[i], buffInfo, -1, "", null, 0L));
		}
		VfxWithLoading result = CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards);
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddTokenDrawModifier(list, this);
		}
		SetOnLoseEvent(base.SkillPrm.ownerCard, null, null);
		return result;
	}

	private List<TokenDrawModifier> CreateTokenDrawModifierList()
	{
		string[] array = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.card_id).Split(':');
		int multiplyCount = int.Parse(base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.multiply_count));
		List<TokenDrawModifier> list = new List<TokenDrawModifier>();
		for (int i = 0; i < array.Count(); i++)
		{
			list.Add(new TokenDrawModifier(int.Parse(array[i]), multiplyCount));
		}
		return list;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		for (int i = 0; i < buffInfoContainer.Count; i++)
		{
			for (int j = 0; j < _tokenDrawModifierList.Count; j++)
			{
				buffInfoContainer[i]._targetCard.SkillApplyInformation.RemoveTokenDrawModifier(_tokenDrawModifierList[j]);
				buffInfoContainer[i]._targetCard.RemoveBuffInfo(buffInfoContainer[i]._buffInfo);
			}
		}
		buffInfoContainer.Clear();
		return base.Stop(skillProcessor);
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += (SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card) => Stop(skillProcessor);
	}
}
