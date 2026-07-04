using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_lose : SkillBase
{
	protected string _ability = string.Empty;

	public Skill_lose(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		GetBuffText();
		BuffInfo buffInfo = null;
		_ability = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.ability, string.Empty);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			BattleCardBase battleCardBase = targetCard;
			if (_ability == SkillFilterCreator.ContentKeyword.guard.ToString())
			{
				if (!battleCardBase.IsEvolution)
				{
					SkillBase selfGuardSkill = targetCard.Card.NormalSkills.FirstOrDefault((SkillBase s) => s is Skill_guard && s.ApplyingTargetFilter is SkillTargetSelfFilter);
					if (selfGuardSkill != null)
					{
						SkillBase skillBase = targetCard.Card.EvolutionSkills.FirstOrDefault((SkillBase s) => s.IsSameSkill(selfGuardSkill));
						if (skillBase != null)
						{
							targetCard.Card.EvolutionSkills.Remove(skillBase);
						}
					}
				}
				parallelVfxPlayer.Register(battleCardBase.SkillApplyInformation.ForceDepriveGuard());
				buffInfo = AddBuffInfoIfNeeded(battleCardBase);
				BuffInfoContainer buffInfoContainer = new BuffInfoContainer(battleCardBase, buffInfo, -1, "", null, 0L);
				base.buffInfoContainer.Add(buffInfoContainer);
				SetOnLoseEvent(battleCardBase, buffInfo, buffInfoContainer);
			}
			else if (_ability == SkillFilterCreator.ContentKeyword.cant_attack_all.ToString())
			{
				if (!battleCardBase.SkillApplyInformation.IsSkillCantAtkAll)
				{
					continue;
				}
				if (!battleCardBase.IsEvolution)
				{
					SkillBase selfCantAttackSkill = targetCard.Card.NormalSkills.FirstOrDefault((SkillBase s) => s is Skill_cant_attack && s.ApplyingTargetFilter is SkillTargetSelfFilter && s.Option == "cant_attack=all");
					if (selfCantAttackSkill != null)
					{
						SkillBase skillBase2 = targetCard.Card.EvolutionSkills.FirstOrDefault((SkillBase s) => s.IsSameSkill(selfCantAttackSkill));
						if (skillBase2 != null)
						{
							targetCard.Card.EvolutionSkills.Remove(skillBase2);
						}
					}
				}
				parallelVfxPlayer.Register(battleCardBase.SkillApplyInformation.ForceDepriveCantAttackAll());
				buffInfo = AddBuffInfoIfNeeded(battleCardBase);
				BuffInfoContainer buffInfoContainer2 = new BuffInfoContainer(battleCardBase, buffInfo, -1, "", null, 0L);
				base.buffInfoContainer.Add(buffInfoContainer2);
				SetOnLoseEvent(battleCardBase, buffInfo, buffInfoContainer2);
			}
			else
			{
				bool isEvolutionSkill = base.SkillPrm.ownerCard.EvolutionSkills != null && base.SkillPrm.ownerCard.EvolutionSkills.Any((SkillBase skill) => skill == this);
				parallelVfxPlayer.Register(battleCardBase.LoseSkill(this));
				buffInfo = AddBuffInfoIfNeeded(battleCardBase);
				buffInfo.IsEvolutionSkill = isEvolutionSkill;
				BuffInfoContainer buffInfoContainer3 = new BuffInfoContainer(battleCardBase, buffInfo, -1, "", null, 0L);
				base.buffInfoContainer.Add(buffInfoContainer3);
				SetOnLoseEvent(battleCardBase, buffInfo, buffInfoContainer3);
			}
		}
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogLose(parameter.targetCards.ToList(), this);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		ParallelVfxPlayer mainVfx = ParallelVfxPlayer.Create(base.Stop(skillProcessor));
		List<BattleCardBase> list = new List<BattleCardBase>();
		int i = 0;
		for (int count = base.buffInfoContainer.Count; i < count; i++)
		{
			BuffInfoContainer buffInfoContainer = base.buffInfoContainer[i];
			list.Add(base.buffInfoContainer[i]._targetCard);
			buffInfoContainer._targetCard.RemoveBuffInfo(buffInfoContainer._buffInfo);
		}
		CallOnUpdateSkillEffect(list);
		base.buffInfoContainer.Clear();
		return VfxWithLoading.Create(mainVfx);
	}

	private string GetBuffText()
	{
		if (!IsBattleLog)
		{
			return "";
		}
		return BattleLogUtility.BuildTextLose();
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			if (!(loseSkill is Skill_lose))
			{
				card.RemoveBuffInfo(buff);
				buffInfoContainer.Remove(container);
			}
			return NullVfx.GetInstance();
		};
	}
}
