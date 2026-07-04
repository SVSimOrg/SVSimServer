using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using LitJson;
using Wizard.AutoTest;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Operation;

public abstract class SkillOperationCommandBase : IOperationCommand
{
	protected AutoTestBattleMgr.CardInfo _cardInfo;

	protected readonly List<SkillTargetInfo> _skillTargetInfoList = new List<SkillTargetInfo>();

	public SkillOperationCommandBase(JsonData actionJsonData)
	{
		_cardInfo = parseSkillInfo(actionJsonData);
		if (!actionJsonData.HasKey("reaction"))
		{
			return;
		}
		JsonData jsonData = actionJsonData["reaction"];
		if (jsonData.IsObject)
		{
			parseSkillInfo(jsonData);
			return;
		}
		foreach (JsonData item in jsonData.ToJsonDataCollection("reaction"))
		{
			parseSkillInfo(item);
		}
	}

	private AutoTestBattleMgr.CardInfo parseSkillInfo(JsonData jsonData)
	{
		string cardName = jsonData["index"].ToString();
		int cardId = jsonData.ToIntOrDefault("id", 0);
		int cost = jsonData.ToIntOrDefault("cost", -1);
		AutoTestBattleMgr.CardInfo cardInfo = new AutoTestBattleMgr.CardInfo(cardName, cardId, cost);
		IEnumerable<JsonData> enumerable = jsonData.ToJsonDataCollection("skill_target");
		if (enumerable != null)
		{
			foreach (JsonData item in enumerable)
			{
				_skillTargetInfoList.Add(new SkillTargetInfo(cardInfo, item.ToString()));
			}
		}
		return cardInfo;
	}

	protected void SetupSkillSummon()
	{
	}

	public abstract void Operation(BattleManagerBase battleMgr);

	protected VfxWith<List<BattleCardBase>> GetSkillSelectedCardsWithVfx(BattleCardBase card, bool isEvolution, Func<bool, VfxBase> func = null)
	{
		BattleManagerBase ins = card.GetBuildInfo.BattleMgr; // Pre-Phase-5b: mgr via card's build info
		List<SkillBase> list = card.GetSelectTypeSkill(isEvolution).ToList();
		SkillCollectionBase source = (isEvolution ? card.EvolutionSkills : card.NormalSkills);
		VfxBase vfx = NullVfx.GetInstance();
		if (list.Count == 0 && !isEvolution && source.Any(delegate(SkillBase s)
		{
			if (!(s is Skill_pp_fixeduse skill_pp_fixeduse))
			{
				return false;
			}
			return skill_pp_fixeduse.IsMutationFixedUseCost && s.CheckCondition(new BattlePlayerReadOnlyInfoPair(s.SkillPrm.selfBattlePlayer, s.SkillPrm.opponentBattlePlayer), new SkillConditionCheckerOption(), isPrePlay: true);
		}) && source.Any((SkillBase s) => s is Skill_transform))
		{
			Skill_transform accelerateOrCrystallizeTransformSkill = card.GetAccelerateOrCrystallizeTransformSkill();
			if (accelerateOrCrystallizeTransformSkill != null)
			{
				list = ins.CreateTransformCardRegisterVfx(accelerateOrCrystallizeTransformSkill.SkillPrm.ownerCard, accelerateOrCrystallizeTransformSkill.TransformId, accelerateOrCrystallizeTransformSkill.SkillPrm.ownerCard.IsPlayer).GetSelectTypeSkill(isEvolution).ToList();
			}
		}
		bool flag = list.Count > 0 && _skillTargetInfoList.Any();
		if (func != null)
		{
			vfx = func.Call(flag);
		}
		IEnumerable<SkillBase> enumerable = list.Where((SkillBase s) => s.IsChoiceType);
		bool flag2 = enumerable != null && enumerable.Count() > 0;
		int num = 0;
		foreach (SkillBase item3 in enumerable)
		{
			num = ((!(item3.ApplySelectFilter is SkillChoiceSelectFilter)) ? (num + 1) : (num + ((SkillChoiceSelectFilter)item3.ApplySelectFilter).Count));
		}
		List<BattleCardBase> list2 = new List<BattleCardBase>();
		if (flag)
		{
			foreach (SkillTargetInfo skillTargetInfo in _skillTargetInfoList)
			{
				if (flag2 && num > 0)
				{
					BattleCardBase item = ins.CreateTransformCardRegisterVfx(card, skillTargetInfo.TargetCard.Index, _cardInfo.IsPlayer);
					list2.Add(item);
					num--;
					continue;
				}
				AutoTestBattleMgr.CardInfo targetCardInfo = skillTargetInfo.TargetCard;
				BattleCardBase item2 = ins.GetBattlePlayer(targetCardInfo.IsPlayer).AllCards.Single((BattleCardBase c) => c.Index == targetCardInfo.Index);
				list2.Add(item2);
			}
		}
		return new VfxWith<List<BattleCardBase>>(vfx, list2);
	}
}
