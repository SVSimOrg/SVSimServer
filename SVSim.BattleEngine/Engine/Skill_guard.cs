using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_guard : SkillBase
{
	private class GuardInfoContainer : BuffInfoContainer
	{
		public GuardInfo GuardInfo { get; private set; }

		public GuardInfoContainer(BattleCardBase card, BuffInfo info, GuardInfo guardInfo)
			: base(card, info, -1, "", null, 0L)
		{
			GuardInfo = guardInfo;
		}
	}

	protected List<BattleCardBase> _targetCards;

	public Skill_guard(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		SetDuplicateBanSkillNum();
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		_targetCards = parameter.targetCards.ToList();
		if (base.DuplicateBanSkillNum != string.Empty)
		{
			_targetCards = _targetCards.Where((BattleCardBase c) => !c.SkillApplyInformation.GuardInfo.Any((GuardInfo g) => (!base.IsDuplicateBanSelfSkill || g.OwnerCard == base.SkillPrm.ownerCard) && g.DuplicateBanSkillNum == base.DuplicateBanSkillNum)).ToList();
		}
		foreach (BattleCardBase targetCard in _targetCards)
		{
			GuardInfo guardInfo = new GuardInfo(base.SkillPrm.ownerCard, base.DuplicateBanSkillNum);
			VfxBase vfx = targetCard.SkillApplyInformation.GiveGuard(guardInfo);
			BattleCardBase battleCardBase = targetCard;
			BuffInfo buffInfo = AddBuffInfoIfNeeded(targetCard);
			GuardInfoContainer guardInfoContainer = new GuardInfoContainer(battleCardBase, buffInfo, guardInfo);
			buffInfoContainer.Add(guardInfoContainer);
			SetOnLoseEvent(battleCardBase, buffInfo, guardInfoContainer);
			List<BattleCardBase> list = new List<BattleCardBase>();
			list.Add(targetCard);
			BattlePlayerPair playerInfoPair = new BattlePlayerPair(battleCardBase.SelfBattlePlayer, battleCardBase.OpponentBattlePlayer);
			parameter.skillProcessor.Register(battleCardBase.Skills.CreateWhenAttachAbilityInfo(parameter.skillProcessor, playerInfoPair, this, BattlePlayerBase.ConvertToSkillInfoCollection(list)));
			parallelVfxPlayer.Register(vfx);
		}
		if (IsBattleLog && _targetCards.Count() > 0)
		{
			BattleLogManager.GetInstance().AddLogSkillGain(_targetCards.ToList(), this, SkillGainType.Guard);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, _targetCards));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		List<BattleCardBase> list = new List<BattleCardBase>();
		foreach (GuardInfoContainer item in buffInfoContainer)
		{
			VfxBase vfx = item._targetCard.SkillApplyInformation.DepriveGuard(item.GuardInfo);
			list.Add(item._targetCard);
			item._targetCard.RemoveBuffInfo(item._buffInfo);
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
			return card.SkillApplyInformation.ForceDepriveGuard();
		};
	}
}
