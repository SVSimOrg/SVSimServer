using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_attack_count : SkillBase
{
	public class CardPair
	{
		public BattleCardBase card { get; private set; }

		public int Sub { get; private set; }

		public BuffInfo buffInfo { get; private set; }

		public CardPair(BattleCardBase card, int sub, BuffInfo buffInfo)
		{
			this.card = card;
			Sub = sub;
			this.buffInfo = buffInfo;
		}
	}

	public bool IsRecovery => base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.attack_count) == "recovery";

	public Skill_attack_count(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public int GetSetAttackCount()
	{
		if (!IsRecovery)
		{
			return base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.attack_count, -1);
		}
		return -1;
	}

	private int GetAddAttackCount()
	{
		return base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_attack_count, -1);
	}

	public bool IsSetAttackCount()
	{
		return GetSetAttackCount() != -1;
	}

	public bool IsAddAttackCount()
	{
		return GetAddAttackCount() != -1;
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		bool isRecovery = IsRecovery;
		int num = (IsAddAttackCount() ? GetAddAttackCount() : GetSetAttackCount());
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		CallOnChangeMaxAttackableCount(parameter.targetCards.ToList());
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			BattleCardBase battleCardBase = targetCard;
			if (num != -1)
			{
				BuffInfo buffInfo = (IsBattleLog ? AddBuffInfoIfNeeded(targetCard) : null);
				int maxAttackableCount = targetCard.MaxAttackableCount;
				int intValue = (IsAddAttackCount() ? num : (num - maxAttackableCount));
				vfxWithLoadingSequential.RegisterToMainVfx(battleCardBase.SkillApplyInformation.GiveAttackCount(this, num));
				BuffInfoContainer buffInfoContainer = new BuffInfoContainer(battleCardBase, buffInfo, intValue, "", null, 0L);
				SetOnLoseEvent(battleCardBase, buffInfo, buffInfoContainer);
				base.buffInfoContainer.Add(buffInfoContainer);
			}
			if (isRecovery)
			{
				vfxWithLoadingSequential.RegisterToMainVfx(battleCardBase.RecoveryAttackCount());
			}
		}
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		if (IsBattleLog)
		{
			if (isRecovery)
			{
				BattleLogManager.GetInstance().AddLogSkillAttackCountRecovery(parameter.targetCards.ToList(), this);
			}
			else
			{
				BattleLogManager.GetInstance().AddLogSkillGain(parameter.targetCards.ToList(), this, SkillGainType.AttackCount, num);
			}
		}
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		parallelVfxPlayer.Register(base.Stop(skillProcessor));
		List<BattleCardBase> list = new List<BattleCardBase>();
		foreach (BuffInfoContainer item in buffInfoContainer)
		{
			list.Add(item._targetCard);
			item._targetCard.RemoveBuffInfo(item._buffInfo);
			parallelVfxPlayer.Register(item._targetCard.SkillApplyInformation.DepriveAttackCount(this));
		}
		CallOnUpdateSkillEffect(list, updateAttackEffect: true);
		buffInfoContainer.Clear();
		return VfxWithLoading.Create(parallelVfxPlayer);
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			card.RemoveBuffInfo(buff);
			buffInfoContainer.Remove(container);
			card.SkillApplyInformation.ForceDepriveAttackCount();
			return NullVfx.GetInstance();
		};
	}

	protected virtual void CallOnChangeMaxAttackableCount(List<BattleCardBase> targetCards)
	{
	}
}
