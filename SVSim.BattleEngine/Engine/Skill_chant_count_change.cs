using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_chant_count_change : SkillBase
{
	public static readonly int CHANT_NONE = -1;

	private int setChant;

	protected int gainChant;

	protected int addChant;

	public bool IsSelfChantSkill;

	public Skill_chant_count_change(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		setChant = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.set_chant, CHANT_NONE);
		gainChant = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.gain_chant, 0);
		addChant = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_chant, 0);
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards, isFollowInHand: false, addToLastOperation: true));
		int num = 0;
		int num2 = 0;
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		CallOnChantCountChange(parameter.targetCards.ToList());
		foreach (BattleCardBase targetCard2 in parameter.targetCards)
		{
			BattleCardBase targetCard = targetCard2;
			targetCard.OnRemoveFromInPlayAfterOneTime += (bool flg, SkillProcessor skillProcessor) => NullVfx.GetInstance();
			num = targetCard.ChantCount;
			if (setChant != CHANT_NONE)
			{
				ChantCountSet(targetCard, setChant);
				vfxWithLoadingSequential.RegisterToMainVfx(NullVfx.GetInstance());
			}
			else if (gainChant > 0)
			{
				ChantCountGain(targetCard, gainChant);
				vfxWithLoadingSequential.RegisterVfxWithLoading(NullVfxWithLoading.GetInstance());
				BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(targetCard2.SelfBattlePlayer, targetCard2.OpponentBattlePlayer);
				if (!IsSelfChantSkill && base.SkillPrm.ownerCard.IsPlayer == targetCard.IsPlayer)
				{
					parameter.skillProcessor.Register(targetCard2.Skills.CreateWhenChantCountChangeInfo(parameter.skillProcessor, playerInfoPair));
				}
				parameter.skillProcessor.Register(targetCard2.Skills.CreateWhenChantCountGainInfo(parameter.skillProcessor, playerInfoPair));
			}
			else if (addChant > 0)
			{
				ChantCountAdd(targetCard, addChant);
				vfxWithLoadingSequential.RegisterVfxWithLoading(NullVfxWithLoading.GetInstance());
			}
			parallelVfxPlayer.Register(CheckChantCountDestroy(targetCard, parameter.skillProcessor));
			num2 = targetCard.ChantCount;
			if (IsBattleLog)
			{
				if (num > 0)
				{
					BattleLogManager.GetInstance().AddLogSkillChangeChantCount(num2 - num, targetCard, this);
				}
				if (num2 <= 0)
				{
					BattleLogManager.GetInstance().AddLogDeath(targetCard);
				}
			}
		}
		if (gainChant > 0)
		{
			RegisterChantCountChangeTriggerSkill(parameter.skillProcessor, parameter.targetCards);
		}
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		return vfxWithLoadingSequential;
	}

	private void ChantCountGain(BattleCardBase card, int gain)
	{
		if (gain > 0)
		{
			card.SkillApplyInformation.GiveChantCount(new ChantCountAddModifier(-gain));
		}
	}

	private void ChantCountAdd(BattleCardBase card, int add)
	{
		card.SkillApplyInformation.GiveChantCount(new ChantCountAddModifier(add));
	}

	private void ChantCountSet(BattleCardBase card, int set)
	{
		card.SkillApplyInformation.GiveChantCount(new ChantCountSetModifier(set));
	}

	protected virtual VfxBase CheckChantCountDestroy(BattleCardBase card, SkillProcessor skillProcessor)
	{
		if (card.ChantCount > 0)
		{
			return NullVfx.GetInstance();
		}
		card.FlagCardAsDestroyedBySkill();
		return card.SelfBattlePlayer.CardManagement(card, skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.DESTROY, base.UsedRandom, null, null, this);
	}

	private void RegisterChantCountChangeTriggerSkill(SkillProcessor skillProcessor, IEnumerable<BattleCardBase> targets)
	{
		List<BattleCardBase> list = base.SkillPrm.selfBattlePlayer.ClassAndInPlayCardList.Where((BattleCardBase c) => c.Skills.Any((SkillBase s) => s.OnWhenChantCountGainSelfAndOther != 0)).ToList();
		list.AddRange(base.SkillPrm.opponentBattlePlayer.ClassAndInPlayCardList.Where((BattleCardBase c) => c.Skills.Any((SkillBase s) => s.OnWhenChantCountGainSelfAndOther != 0)));
		for (int num = 0; num < list.Count; num++)
		{
			if (list[num].Skills.Any((SkillBase s) => s.OnWhenChantCountGainSelfAndOther != 0))
			{
				skillProcessor.Register(list[num].Skills.CreateWhenChantCountGainSelfAndOtherInfo(targets.ToList(), skillProcessor, new BattlePlayerReadOnlyInfoPair(list[num].SelfBattlePlayer, list[num].OpponentBattlePlayer)));
			}
		}
	}

	protected virtual void CallOnChantCountChange(List<BattleCardBase> targetCards)
	{
	}
}
