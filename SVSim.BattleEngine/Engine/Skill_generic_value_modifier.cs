using System.Linq;
using Wizard.Battle.View.Vfx;

public class Skill_generic_value_modifier : SkillBase
{
	protected string _setOptionText = "_OPT_NULL_";

	public override bool ShowSideLog => false;

	public Skill_generic_value_modifier(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		_setOptionText = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.set, "_OPT_NULL_");
	}

	protected int[] GetGenericArray(string text)
	{
		string[] array = text.Split(':');
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = base.OptionValue.ParseInt(array[i]);
		}
		return array2;
	}

	public virtual void InsertTargetInfo(CallParameter parameter)
	{
		int[] skillGenericArray = parameter.calledSkillResultInfo.SelfLastTargetCards[0].Select((BattleCardBase v) => v.CardId).ToArray();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			targetCard.SkillApplyInformation.SetSkillGenericArray(skillGenericArray);
		}
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		ParallelVfxPlayer vfxToRegister = ParallelVfxPlayer.Create();
		string option = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.id, "_OPT_NULL_");
		if (_setOptionText != "_OPT_NULL_")
		{
			if (option != "_OPT_NULL_")
			{
				int value = GetGenericArray(_setOptionText)[0];
				foreach (BattleCardBase targetCard in parameter.targetCards)
				{
					targetCard.SkillApplyInformation.SetSkillGenericKeyAndValue(option, value);
				}
			}
			else
			{
				int[] genericArray = GetGenericArray(_setOptionText);
				foreach (BattleCardBase targetCard2 in parameter.targetCards)
				{
					targetCard2.SkillApplyInformation.SetSkillGenericArray(genericArray);
				}
			}
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		vfxWithLoadingSequential.RegisterToMainVfx(vfxToRegister);
		return vfxWithLoadingSequential;
	}
}
