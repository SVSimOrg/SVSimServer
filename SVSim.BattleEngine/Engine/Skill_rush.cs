using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_rush : SkillBase
{
	private class RushInfoContainer : BuffInfoContainer
	{
		public RushInfo RushInfo { get; private set; }

		public RushInfoContainer(BattleCardBase card, BuffInfo info, RushInfo rushInfo)
			: base(card, info, -1, "", null, 0L)
		{
			RushInfo = rushInfo;
		}
	}

	protected List<BattleCardBase> _targetCards;

	public Skill_rush(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		SetDuplicateBanSkillNum();
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		_targetCards = parameter.targetCards.ToList();
		if (base.DuplicateBanSkillNum != string.Empty)
		{
			_targetCards = _targetCards.Where((BattleCardBase c) => !c.SkillApplyInformation.RushInfo.Any((RushInfo g) => (!base.IsDuplicateBanSelfSkill || g.OwnerCard == base.SkillPrm.ownerCard) && g.DuplicateBanSkillNum == base.DuplicateBanSkillNum)).ToList();
		}
		foreach (BattleCardBase targetCard in _targetCards)
		{
			RushInfo rushInfo = new RushInfo(base.SkillPrm.ownerCard, base.DuplicateBanSkillNum);
			VfxBase vfxToRegister = targetCard.SkillApplyInformation.GiveRush(rushInfo);
			BattleCardBase battleCardBase = targetCard;
			BuffInfo buffInfo = AddBuffInfoIfNeeded(targetCard);
			RushInfoContainer rushInfoContainer = new RushInfoContainer(battleCardBase, buffInfo, rushInfo);
			buffInfoContainer.Add(rushInfoContainer);
			SetOnLoseEvent(battleCardBase, buffInfo, rushInfoContainer);
			List<BattleCardBase> list = new List<BattleCardBase>();
			list.Add(targetCard);
			BattlePlayerPair playerInfoPair = new BattlePlayerPair(battleCardBase.SelfBattlePlayer, battleCardBase.OpponentBattlePlayer);
			parameter.skillProcessor.Register(battleCardBase.Skills.CreateWhenAttachAbilityInfo(parameter.skillProcessor, playerInfoPair, this, BattlePlayerBase.ConvertToSkillInfoCollection(list)));
			vfxWithLoadingSequential.RegisterToMainVfx(vfxToRegister);
		}
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, _targetCards));
		if (IsBattleLog && _targetCards.Count() > 0)
		{
			BattleLogManager.GetInstance().AddLogSkillGain(_targetCards.ToList(), this, SkillGainType.Rush);
		}
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		List<BattleCardBase> list = new List<BattleCardBase>();
		foreach (RushInfoContainer item in buffInfoContainer)
		{
			parallelVfxPlayer.Register(item._targetCard.SkillApplyInformation.DepriveRush(item.RushInfo));
			list.Add(item._targetCard);
			item._targetCard.RemoveBuffInfo(item._buffInfo);
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
			return card.SkillApplyInformation.ForceDepriveRush();
		};
	}
}
