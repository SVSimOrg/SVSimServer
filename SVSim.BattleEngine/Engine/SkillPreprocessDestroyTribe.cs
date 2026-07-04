using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessDestroyTribe : SkillPreprocessBase
{
	private readonly CardBasePrm.TribeType _tribe;

	private bool IsAllDestroy;

	private int _count = 1;

	private BattleCardBase _ownerCard;

	public bool IsWhiteRitual => _tribe == CardBasePrm.TribeType.WHITE_RITUAL;

	public SkillPreprocessDestroyTribe(BattleCardBase card, string tribe)
	{
		_ownerCard = card;
		string[] array = tribe.Split(':');
		if (array.Count() > 1)
		{
			tribe = array[0];
			_count = int.Parse(array[1]);
		}
		string strEnum = tribe;
		if (tribe.Contains("_all"))
		{
			IsAllDestroy = true;
			strEnum = tribe.Replace("_all", "");
		}
		_tribe = SkillFilterCreator.ParseEnum(strEnum, CardBasePrm.TribeType.MAX);
	}

	public CardBasePrm.TribeType GetDestroyTribe()
	{
		return _tribe;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		if (IsAllDestroy)
		{
			if (IsWhiteRitual)
			{
				return playerInfoPair.ReadOnlySelf.SkillInfoInPlayCards.Any((IReadOnlyBattleCardInfo c) => c.IsTribe(_tribe) && (c.IsField || c.IsChantField));
			}
			return playerInfoPair.ReadOnlySelf.SkillInfoInPlayCards.Any((IReadOnlyBattleCardInfo c) => c.IsTribe(_tribe));
		}
		if (IsWhiteRitual)
		{
			return playerInfoPair.ReadOnlySelf.SkillInfoInPlayCards.Where((IReadOnlyBattleCardInfo c) => (c.IsField || c.IsChantField) && c.IsTribe(CardBasePrm.TribeType.WHITE_RITUAL)).Sum((IReadOnlyBattleCardInfo c) => c.SkillApplyInformation.WhiteRitualCount) >= _count;
		}
		return playerInfoPair.ReadOnlySelf.SkillInfoInPlayCards.Where((IReadOnlyBattleCardInfo c) => c.IsTribe(_tribe)).Count() >= _count;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		List<BattleCardBase> list = (IsWhiteRitual ? playerPair.Self.InPlayCards.Where((BattleCardBase c) => c.IsTribe(_tribe) && (c.IsField || c.IsChantField)).ToList() : playerPair.Self.InPlayCards.Where((BattleCardBase c) => c.IsTribe(_tribe)).ToList());
		if (!list.IsNotNullOrEmpty())
		{
			return NullVfx.GetInstance();
		}
		playerPair.Self.LastInplayWhiteRitualStack = list.Sum((BattleCardBase c) => c.SkillApplyInformation.WhiteRitualCount);
		if (IsWhiteRitual)
		{
			int num = (IsAllDestroy ? list[0].SkillApplyInformation.WhiteRitualCount : Math.Min(list[0].SkillApplyInformation.WhiteRitualCount, _count));
			skill.SkillPrm.selfBattlePlayer.CallOnChangeWhiteRitualStack(list[0], -num, list[0].SkillApplyInformation.WhiteRitualCount == num);
		}
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		int num2 = (IsAllDestroy ? list.Count : _count);
		int num3 = 0;
		int num4 = 0;
		for (int num5 = 0; num5 < num2; num5++)
		{
			BattleCardBase battleCardBase = list[num3];
			if (IsWhiteRitual && battleCardBase.IsTribe(CardBasePrm.TribeType.WHITE_RITUAL))
			{
				if (IsAllDestroy)
				{
					battleCardBase.SelfBattlePlayer.GameUsedWhiteRitualCount += battleCardBase.SkillApplyInformation.WhiteRitualCount;
					checkerOption.LastUsedWhiteRitualStackCount += battleCardBase.SkillApplyInformation.WhiteRitualCount;
					battleCardBase.SkillApplyInformation.FourceDepriveWhiteRitualCount();
				}
				else
				{
					battleCardBase.SelfBattlePlayer.GameUsedWhiteRitualCount++;
					checkerOption.LastUsedWhiteRitualStackCount++;
					num4++;
					int value = 1;
					battleCardBase.SkillApplyInformation.DepriveWhiteRitualCount(value);
				}
				if (battleCardBase.SkillApplyInformation.WhiteRitualCount > 0)
				{
					continue;
				}
				parallelVfxPlayer.Register(battleCardBase.BattleCardView.UpdateStackWhiteRitualIconNumber());
				battleCardBase.DeathTypeInfo.MysteriesDestroy = true;
				if (skill.IsWhenDestroySkill)
				{
					skill.SkillPrm.ownerCard.IsExecutedEarthRite = true;
				}
			}
			if (num3 == 0)
			{
				skill.SkillPrm.selfBattlePlayer.CallOnSkillDestroyOrBanish(_ownerCard);
			}
			battleCardBase.FlagCardAsDestroyedBySkill();
			VfxBase vfx = battleCardBase.SelfBattlePlayer.CardManagement(battleCardBase, skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.DESTROY, isRandom: false);
			parallelVfxPlayer.Register(vfx);
			if (!skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsVirtualBattle)
			{
				BattleLogManager.GetInstance().AddLogSkillDeath(new List<BattleCardBase> { battleCardBase }, skill);
			}
			num3++;
			num4 = 0;
		}
		if (IsWhiteRitual)
		{
			if (num4 != 0)
			{
				parallelVfxPlayer.Register(NullVfx.GetInstance());
				BattleLogManager.GetInstance().AddLogDepriveWhiteRitualStack(-num4, list[num3], skill);
			}
			if (skill.SkillPrm.selfBattlePlayer.BattleMgr is NetworkBattleManagerBase networkBattleManagerBase)
			{
				networkBattleManagerBase.RegisterInplayWhiteRitualStack(skill.SkillPrm.selfBattlePlayer);
			}
			skill.SkillPrm.selfBattlePlayer.StartSkillWhenUseWhiteRitualStack(skillProcessor, checkerOption);
		}
		return parallelVfxPlayer;
	}
}
