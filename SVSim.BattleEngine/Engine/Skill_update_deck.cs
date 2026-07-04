using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_update_deck : SkillBase
{
	private class OptionResult
	{
		public List<int> _tokenIds;

		public int _repeatCount;

		public OptionResult(List<int> tokenIds, int repeatCount)
		{
			_tokenIds = tokenIds;
			_repeatCount = repeatCount;
		}
	}

	protected string _updateType = "change";

	protected bool _isReferenceOpponentCard;

	protected bool _isReferenceFusionedCard;

	private List<BattleCardBase> _addCards;

	private VfxWithLoadingSequential _vfxWithLoading;

	protected bool _isUsingTargetCards;

	public override bool IsTargetIndicate => false;

	public override bool IsAllowDestroyTarget => true;

	public bool IsOpen { get; protected set; } = true;

	public Skill_update_deck(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		IsOpen = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.is_open, "false") == "true";
	}

	private OptionResult ParseOption(CallParameter parameter)
	{
		_updateType = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.type, "change");
		string stringAllParse = base.OptionValue.GetStringAllParse(SkillFilterCreator.ContentKeyword.token_draw, ':', "_OPT_NULL_");
		bool allowDuplication = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.duplication, "false") == "true";
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.repeat_count, -1);
		int num2 = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.limit_upper_count, -1);
		_isReferenceOpponentCard = base.ApplyFilterCollection.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.BattlePlayerFilter is OpponentBattlePlayerFilter);
		_isReferenceFusionedCard = base.ApplyFilterCollection.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.TargetFilter is SkillTargetFusionIngredientCardsFilter);
		int num3 = -1;
		List<int> list = null;
		if (parameter.targetCards.Any())
		{
			list = GetTargetId(parameter.targetCards).ToList();
			_isUsingTargetCards = true;
		}
		else if ("_OPT_NULL_" != stringAllParse)
		{
			list = SkillOptionValue.ParseOptionTokenID(stringAllParse).ToList();
			num3 = SkillOptionValue.ParseTokenOption(stringAllParse);
		}
		if (num3 != -1)
		{
			bool isRandomDistinct = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.is_random_distinct, "null") == "true";
			list = GetRandomSelect(list, num3, allowDuplication, isRandomDistinct).ToList();
		}
		if (list == null || list.Count == 0)
		{
			return null;
		}
		if (IsMakeFoil)
		{
			CardMaster instanceForBattle = CardMaster.GetInstanceForBattle();
			for (int num4 = 0; num4 < list.Count; num4++)
			{
				if (list[num4] != -1)
				{
					list[num4] = instanceForBattle.GetCardParameterFromId(list[num4]).FoilCardId;
				}
			}
		}
		switch (num)
		{
		case 0:
			return null;
		default:
		{
			List<int> list2 = new List<int>();
			for (int num5 = 0; num5 < list.Count; num5++)
			{
				for (int num6 = 0; num6 < num; num6++)
				{
					list2.Add(list[num5]);
				}
			}
			list = list2;
			break;
		}
		case -1:
			break;
		}
		if (num2 != -1)
		{
			int count = Mathf.Min(num2, list.Count());
			list = list.Take(count).ToList();
		}
		return new OptionResult(list, num);
	}

	private void AddCardAndVfx(CallParameter parameter, List<int> tokenIds, int repeatCount, BattlePlayerBase targetPlayer)
	{
		for (int i = 0; i < tokenIds.Count(); i++)
		{
			if (tokenIds[i] != -1)
			{
				BattleCardBase item = CreateTokenCard(parameter, tokenIds[i], repeatCount, i, targetPlayer);
				_addCards.Add(item);
			}
		}
		if (_updateType != "change")
		{
			_vfxWithLoading.RegisterToLoadingVfx(base.SkillPrm.selfBattlePlayer.BattleMgr.LoadCardResources(_addCards));
		}
	}

	protected virtual BattleCardBase CreateTokenCard(CallParameter parameter, int tokenId, int repeatCount, int tokenIdIndex, BattlePlayerBase targetPlayer)
	{
		return targetPlayer.CreateNextIndexCard(tokenId);
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		bool flag = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.player_side, SkillFilterCreator.ContentKeyword.me.ToStringCustom()) == "me";
		BattlePlayerBase battlePlayerBase = (flag ? base.SkillPrm.selfBattlePlayer : base.SkillPrm.opponentBattlePlayer);
		_addCards = new List<BattleCardBase>();
		_vfxWithLoading = VfxWithLoadingSequential.Create();
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		int num = 0;
		string text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.option, string.Empty);
		if (!string.IsNullOrEmpty(text))
		{
			string pattern = "\\(.*?\\)";
			foreach (Match item in Regex.Matches(text, pattern))
			{
				string text2 = item.Value.Substring(1, item.Value.Length - 2);
				base.OptionValue.SetText(text2);
				OptionResult optionResult = ParseOption(parameter);
				if (optionResult != null)
				{
					AddCardAndVfx(parameter, optionResult._tokenIds, optionResult._repeatCount, battlePlayerBase);
					num += optionResult._tokenIds.Count;
				}
			}
			base.OptionValue.SetText(base.SkillPrm.buildInfo._option);
		}
		else
		{
			OptionResult optionResult2 = ParseOption(parameter);
			if (optionResult2 != null)
			{
				AddCardAndVfx(parameter, optionResult2._tokenIds, optionResult2._repeatCount, battlePlayerBase);
				num = optionResult2._tokenIds.Count;
			}
		}
		int count = battlePlayerBase.DeckCardList.Count;
		if (_addCards.Count == 0)
		{
			parameter.calledSkillResultInfo.UpdatedDeckCards = new List<IReadOnlyBattleCardInfo>();
			if (_updateType == "change")
			{
				_vfxWithLoading.RegisterToMainVfx(NullVfx.GetInstance());
			}
			return _vfxWithLoading;
		}
		bool flag2 = battlePlayerBase.IsPlayer || _isReferenceOpponentCard || _isReferenceFusionedCard || !_isUsingTargetCards || IsOpen;
		parallelVfxPlayer.Register(battlePlayerBase.AddDeckTokenCards(_addCards, parameter.skillProcessor, _updateType, this, flag2));
		parameter.calledSkillResultInfo.UpdatedDeckCards = BattlePlayerBase.ConvertToSkillInfoCollection(_addCards);
		if (flag && _updateType != "change")
		{
			base.SkillPrm.selfBattlePlayer.GameAddUpdateDeckCards.AddRange(_addCards);
			for (int i = 0; i < _addCards.Count(); i++)
			{
				base.SkillPrm.selfBattlePlayer.GameUpdateDeckMomentTribe.Add(new BattlePlayerBase.CardAndTribe(_addCards[i], _addCards[i].Tribe));
			}
		}
		battlePlayerBase.CallOnChangeDeckAfterEvent(count, parameter.skillProcessor, new List<BattleCardBase>());
		if (!_isUsingTargetCards)
		{
			AddLastTarget(parameter, _addCards);
		}
		if (!PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SHOW_BATTLE_EFFECT) && !string.IsNullOrEmpty(base.SkillPrm.buildInfo._effectPath))
		{
			_vfxWithLoading.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards, isFollowInHand: false, addToLastOperation: true));
		}
		if (_updateType != "change")
		{
			VfxWithLoading vfxWithLoading = Skill_token_draw.CreateTokenSpawnVfx(this, _addCards.First());
			_vfxWithLoading.RegisterToMainVfx(NullVfx.GetInstance());
			_vfxWithLoading.RegisterToLoadingVfx(vfxWithLoading.LoadingVfx);
		}
		if (_updateType != "change" && (_isUsingTargetCards || num > 0))
		{
			CreateWhenAddToDeckInfo(parameter, battlePlayerBase, flag ? base.SkillPrm.opponentBattlePlayer : base.SkillPrm.selfBattlePlayer);
		}
		_vfxWithLoading.RegisterToMainVfx(NullVfx.GetInstance());
		_vfxWithLoading.RegisterToMainVfx(NullVfx.GetInstance());
		if (IsBattleLog)
		{
			bool isAdminWatch = SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch;
			if ((!_isUsingTargetCards || base.SkillPrm.ownerCard.IsPlayer || _isReferenceOpponentCard || _isReferenceFusionedCard || isAdminWatch || IsOpen) && _updateType != "change")
			{
				BattleLogManager.GetInstance().AddLogSkillAddDeck(_addCards, this);
			}
			else
			{
				BattleLogManager.GetInstance().AddLogSkillChangeDeck(battlePlayerBase.Class, this);
			}
		}
		return _vfxWithLoading;
	}

	private IEnumerable<int> GetTargetId(IEnumerable<BattleCardBase> targetCards)
	{
		return targetCards.Select((BattleCardBase card) => (!card.IsChoiceEvolutionCard) ? card.BaseParameter.NormalCardId : card.BaseParameter.BaseCardId);
	}

	protected virtual void CreateWhenAddToDeckInfo(CallParameter parameter, BattlePlayerBase self, BattlePlayerBase opp)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		list.AddRange(self.HandCardList);
		if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr is SingleBattleMgr)
		{
			list.AddRange(self.ClassAndInPlayCardList);
		}
		else
		{
			list.AddRange(self.InPlayCards);
		}
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(self, opp);
		for (int i = 0; i < list.Count(); i++)
		{
			parameter.skillProcessor.Register(list[i].Skills.CreateWhenAddToDeck(parameter.skillProcessor, playerInfoPair));
		}
	}

	private IEnumerable<int> GetRandomSelect(IEnumerable<int> ids, int randomCount, bool allowDuplication, bool isRandomDistinct)
	{
		List<int> list = ids.ToList();
		List<int> list2 = new List<int>();
		int num = Math.Min(randomCount, list.Count);
		if (isRandomDistinct)
		{
			list = list.Where((int num3) => !base.SkillPrm.ownerCard.SkillApplyInformation.RandomSelectedCardList.Any((BattleCardBase c) => c.CardId == num3)).ToList();
			if (list.Count() == 0)
			{
				return list;
			}
		}
		for (int num2 = 0; num2 < num; num2++)
		{
			if (list.Count <= 0)
			{
				continue;
			}
			int index = (base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.InstanceIsRandomDraw ? base.SkillPrm.selfBattlePlayer.BattleMgr.StableRandom(list.Count) : num2);
			int id = list[index];
			if (!allowDuplication)
			{
				list = list.Where((int c) => c != id).ToList();
			}
			list2.Add(id);
		}
		return list2;
	}
}
