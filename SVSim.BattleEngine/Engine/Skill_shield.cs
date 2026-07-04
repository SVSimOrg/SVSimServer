using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_shield : SkillBase
{
	private class ShieldInfoContainer : BuffInfoContainer
	{
		public ShieldInfo ShieldInfo { get; private set; }

		public ShieldInfoContainer(BattleCardBase card, BuffInfo info, string shieldType, ShieldInfo shieldInfo)
			: base(card, info, -1, shieldType, null, 0L)
		{
			ShieldInfo = shieldInfo;
		}
	}

	private SkillGainType _gainType;

	public Skill_shield(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		string text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.type, "all");
		_gainType = base.OptionValue.GetShieldSkillGainType(SkillFilterCreator.ContentKeyword.type);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			BattleCardBase battleCardBase = targetCard;
			ShieldInfo shieldInfo = new ShieldInfo(base.SkillPrm.ownerCard, text);
			BuffInfo buffInfo = AddBuffInfoIfNeeded(targetCard);
			if (buffInfo != null && base.SkillPrm.ownerCard.IsClass && base.SkillPrm.ownerCard == targetCard)
			{
				buffInfo.IsHiddenClassLogSkill = true;
			}
			ShieldInfoContainer shieldInfoContainer = new ShieldInfoContainer(battleCardBase, buffInfo, text, shieldInfo);
			VfxBase vfx = targetCard.SkillApplyInformation.GiveShield(shieldInfoContainer.ShieldInfo);
			if (targetCard.IsClass)
			{
				UpdateClassBuffIfActive(targetCard);
			}
			buffInfoContainer.Add(shieldInfoContainer);
			SetOnLoseEvent(battleCardBase, buffInfo, shieldInfoContainer);
			parallelVfxPlayer.Register(vfx);
		}
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillGain(parameter.targetCards.ToList(), this, _gainType);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		List<BattleCardBase> list = new List<BattleCardBase>();
		foreach (ShieldInfoContainer item in buffInfoContainer)
		{
			VfxBase vfx = item._targetCard.SkillApplyInformation.DepriveShield(item.ShieldInfo);
			list.Add(item._targetCard);
			item._targetCard.RemoveBuffInfo(item._buffInfo);
			if (item._targetCard.IsClass)
			{
				UpdateClassBuffIfActive(item._targetCard);
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
			return (container is ShieldInfoContainer shieldInfoContainer) ? card.SkillApplyInformation.FourceDepriveShield(shieldInfoContainer.ShieldInfo.Type) : NullVfx.GetInstance();
		};
	}
}
