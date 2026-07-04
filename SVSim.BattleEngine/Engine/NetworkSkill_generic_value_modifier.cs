using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Wizard;

public class NetworkSkill_generic_value_modifier : Skill_generic_value_modifier
{
	private List<BattleCardBase> genericValueCards;

	private List<ApplySkillTargetFilterCollection> _filters;

	private bool IsNeedExtract => _setOptionText.Contains("last_target.charge_count");

	public NetworkSkill_generic_value_modifier(NetworkBattleManagerBase battleManager, SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		NetworkSkill_generic_value_modifier networkSkill_generic_value_modifier = this;
		genericValueCards = new List<BattleCardBase>();
		_filters = new List<ApplySkillTargetFilterCollection>();
		string text = _setOptionText.Split(':')[0];
		ConvertAndSetConditionsToTargetText(text);
		if (base.SkillPrm.ownerCard.IsPlayer)
		{
			return;
		}
		base.OnSkillStart += delegate(SkillBase skill, List<BattleCardBase> targetsCard, SkillConditionCheckerOption checkeroption)
		{
			networkSkill_generic_value_modifier.genericValueCards = networkSkill_generic_value_modifier.GetGenericValueCards(checkeroption);
			if (networkSkill_generic_value_modifier.IsNeedExtract && networkSkill_generic_value_modifier.genericValueCards.Count > 0)
			{
				RegisterExtract register = new RegisterExtract(networkSkill_generic_value_modifier.SkillPrm.ownerCard.IsPlayer, networkSkill_generic_value_modifier.genericValueCards, RegisterActionBase.ActionBaseParameter.cost.ToString(), isBase: true);
				battleManager._networkBattleSetupCardEventBase.AddRegisterActionManager(register);
			}
		};
	}

	public override void InsertTargetInfo(CallParameter parameter)
	{
		if (base.SkillPrm.ownerCard.IsPlayer && !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsWatchBattle)
		{
			base.InsertTargetInfo(parameter);
			return;
		}
		int[] skillGenericArray = new int[0];
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			targetCard.SkillApplyInformation.SetSkillGenericArray(skillGenericArray);
		}
	}

	private void ConvertAndSetConditionsToTargetText(string text)
	{
		if (text.Contains('{'))
		{
			string[] array = text.Split('{');
			for (int i = 1; i < array.Length; i++)
			{
				ConvertAndSetConditionToTargetText(array[i].Split('}')[0]);
			}
		}
		else
		{
			ConvertAndSetConditionToTargetText(text);
		}
	}

	private void ConvertAndSetConditionToTargetText(string text)
	{
		string[] array = text.Split('.');
		ApplySkillTargetFilterCollection applySkillTargetFilterCollection = new ApplySkillTargetFilterCollection();
		for (int i = 0; i < array.Count(); i++)
		{
			SkillFilterCreator.ContentKeyword keyword = SkillFilterCreator.Str2ContentKeyword(array[i]);
			if (SkillFilterCreator.PLAYER_FILTER_NAMES.Contains(array[i]))
			{
				applySkillTargetFilterCollection.BattlePlayerFilter = SkillFilterCreator.CreateBattlePlayerFilter(keyword);
				continue;
			}
			if (SkillFilterCreator.PLAYER_TARGET_FILTER_NAMES.Contains(array[i]))
			{
				SkillFilterCreator.ParseContentInfo(array[i], out var retParsedInfo);
				applySkillTargetFilterCollection.TargetFilter = SkillFilterCreator.CreatePlayerTargetFilter(retParsedInfo, keyword, base.SkillPrm.ownerCard, this);
			}
			if (SkillFilterCreator.CARD_TYPE_FILTER_NAMES.Contains(text))
			{
				applySkillTargetFilterCollection.CardFilterList.Add(SkillFilterCreator.CreateCardTypeFilter(keyword));
			}
			else if (SkillFilterCreator.CARD_CONDITION_FILTER_NAMES.Any(text.StartsWith))
			{
				applySkillTargetFilterCollection.CardFilterList.Add(SkillFilterCreator.CreateCardConditionFilter(text, base.SkillPrm.ownerCard));
			}
			else if (SkillFilterCreator.CARD_SELECT_FILTER_NAMES.Any(text.StartsWith))
			{
				applySkillTargetFilterCollection.SelectFilter = SkillFilterCreator.CreateCardSelectFilter(text);
			}
			else if (SkillFilterCreator.CARD_PARAMETER_COMPARE_FILTER_NAMES.Any((string n) => Regex.IsMatch(text, "^" + n + "[<>!:=]=?")))
			{
				applySkillTargetFilterCollection.CardFilterList.Add(SkillFilterCreator.CreateCardParameterCompareFilter(text, base.SkillPrm.ownerCard));
			}
		}
		_filters.Add(applySkillTargetFilterCollection);
	}

	private List<BattleCardBase> GetGenericValueCards(SkillConditionCheckerOption option)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		for (int i = 0; i < _filters.Count; i++)
		{
			IEnumerable<BattleCardBase> source = _filters[i].Filtering(new BattlePlayerReadOnlyInfoPair(base.SkillPrm.selfBattlePlayer, base.SkillPrm.opponentBattlePlayer), option, null).Cast<BattleCardBase>();
			for (int j = 0; j < source.Count(); j++)
			{
				if (!list.Contains(source.ElementAt(j)))
				{
					list.Add(source.ElementAt(j));
				}
			}
		}
		return list;
	}
}
