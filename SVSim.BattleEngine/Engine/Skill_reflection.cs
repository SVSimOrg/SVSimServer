using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class Skill_reflection : SkillBase
{
	private class ReflectionBuffContainer : BuffInfoContainer
	{
		public ReflectionInfo ReflectionInfo { get; private set; }

		public ReflectionBuffContainer(BattleCardBase card, BuffInfo info, ReflectionInfo reflectionInfo)
			: base(card, info, -1, "", null, 0L)
		{
			ReflectionInfo = reflectionInfo;
		}
	}

	public Skill_reflection(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter callParameter)
	{
		string text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.target, string.Empty);
		string type = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.type, string.Empty);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if (text == "")
		{
			return NullVfxWithLoading.GetInstance();
		}
		ReflectionInfo reflectionInfo = new ReflectionInfo(text, type);
		for (int i = 0; i < callParameter.targetCards.Count(); i++)
		{
			BattleCardBase battleCardBase = callParameter.targetCards.ElementAt(i);
			VfxBase vfx = battleCardBase.SkillApplyInformation.GiveReflection(reflectionInfo);
			parallelVfxPlayer.Register(vfx);
			BuffInfo buffInfo = AddBuffInfoIfNeeded(battleCardBase);
			if (battleCardBase.IsClass)
			{
				UpdateClassBuffIfActive(battleCardBase);
			}
			ReflectionBuffContainer reflectionBuffContainer = new ReflectionBuffContainer(battleCardBase, buffInfo, reflectionInfo);
			buffInfoContainer.Add(reflectionBuffContainer);
			SetOnLoseEvent(battleCardBase, buffInfo, reflectionBuffContainer);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, callParameter.targetCards));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		List<BattleCardBase> list = new List<BattleCardBase>();
		for (int i = 0; i < buffInfoContainer.Count; i++)
		{
			ReflectionBuffContainer reflectionBuffContainer = buffInfoContainer[i] as ReflectionBuffContainer;
			VfxBase vfx = reflectionBuffContainer._targetCard.SkillApplyInformation.DepriveReflection(reflectionBuffContainer.ReflectionInfo);
			list.Add(reflectionBuffContainer._targetCard);
			reflectionBuffContainer._targetCard.RemoveBuffInfo(reflectionBuffContainer._buffInfo);
			if (reflectionBuffContainer._targetCard.IsClass)
			{
				UpdateClassBuffIfActive(reflectionBuffContainer._targetCard);
			}
			parallelVfxPlayer.Register(vfx);
		}
		CallOnUpdateSkillEffect(list);
		buffInfoContainer.Clear();
		return VfxWithLoading.Create(parallelVfxPlayer);
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			card.RemoveBuffInfo(buff);
			buffInfoContainer.Remove(container);
			return card.SkillApplyInformation.ForceDepriveReflection();
		};
	}
}
