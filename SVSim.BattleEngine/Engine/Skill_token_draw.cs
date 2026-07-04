using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_token_draw : SkillBase
{

	protected IEnumerable<int> _tokenIds;

	protected BattlePlayerBase _playerSide;

	protected bool _isPlayerSideBoth;

	protected BattlePlayerBase _selfBattlePlayer;

	protected int _randomCount = -1;

	public override bool IsTargetIndicate => false;

	public override bool IsAllowDestroyTarget => true;

	public override bool IsVisibleTarget { get; protected set; }

	public List<int> TokenModifierList { get; protected set; }

	private List<int> _tokenModifierIndexList { get; set; }

	private List<int> _targetCardsIndexList { get; set; }

	public string IsOpen { get; private set; }

	protected bool IsOpponentHandCopy
	{
		get
		{
			if (base.ApplyingTargetFilter is SkillTargetHandFilter)
			{
				return base.ApplyBattlePlayerFilter is OpponentBattlePlayerFilter;
			}
			return false;
		}
	}

	public Skill_token_draw(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		TokenModifierList = new List<int>();
		_tokenModifierIndexList = new List<int>();
		_targetCardsIndexList = ((parameter.targetCards.Count() > 0) ? parameter.targetCards.Select((BattleCardBase c) => c.Index).ToList() : new List<int> { -1 });
		if (!CreateTokenInfo(parameter.targetCards))
		{
			parameter.calledSkillResultInfo.drawCards = new List<IReadOnlyBattleCardInfo>();
			return NullVfxWithLoading.GetInstance();
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		bool isCopy = parameter.targetCards.Count() > 0 && !(base.ApplyBattlePlayerFilter is OpponentBattlePlayerFilter) && (base.ApplyingTargetFilter is SkillTargetHandOtherSelfFilter || base.ApplyingTargetFilter is SkillTargetHandFilter || base.ApplyingTargetFilter is SkillTargetDeckFilter || base.ApplyingTargetFilter is SkillTargetLastTargetFilter || base.ApplyingTargetFilter is SkillTargetDiscardThisTurnCardListFilter || IsTargetHandOtherSelfFilter() || IsHaveApplicableTargetFilter<SkillTargetFusionIngredientCardsFilter>() || IsHaveApplicableTargetFilter<SkillTargetDiscardCardListFilter>());
		List<BattleCardBase> drawList = CreateTokenObjectAndView(_playerSide, vfxWithLoadingSequential, isCopy);
		CreateTokenDrawVfx(parameter, drawList, vfxWithLoadingSequential, _playerSide);
		if (_isPlayerSideBoth)
		{
			drawList = CreateTokenObjectAndView(base.SkillPrm.ownerCard.OpponentBattlePlayer, vfxWithLoadingSequential, isCopy);
			return CreateTokenDrawVfx(parameter, drawList, vfxWithLoadingSequential, base.SkillPrm.ownerCard.OpponentBattlePlayer);
		}
		return vfxWithLoadingSequential;
	}

	private List<BattleCardBase> CreateTokenObjectAndView(BattlePlayerBase playerSide, VfxWithLoadingSequential vfxWithLoading, bool isCopy)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		for (int i = 0; i < _tokenIds.Count(); i++)
		{
			int num = _tokenIds.ElementAt(i);
			int id = num;
			CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(num);
			if (IsMakeFoil)
			{
				id = cardParameterFromId.FoilCardId;
			}
			int index = ((_targetCardsIndexList.Count() < _tokenIds.Count()) ? _targetCardsIndexList.First() : _targetCardsIndexList[i]);
			BattleCardBase battleCardBase = CreateTokenCard(playerSide, id, index, NetworkBattleDefine.NetworkCardPlaceState.Hand, !_tokenModifierIndexList.Contains(i) && isCopy);
			battleCardBase.SetOnDraw(draw: true);
			list.Add(battleCardBase);
		}
		vfxWithLoading.RegisterToLoadingVfx(base.SkillPrm.selfBattlePlayer.BattleMgr.LoadCardResources(list));
		return list;
	}

	protected bool CreateTokenInfo(IEnumerable<BattleCardBase> targetCards, bool isReserve = false)
	{
		_selfBattlePlayer = base.SkillPrm.selfBattlePlayer;
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.repeat_count, -1);
		string text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.token_draw, "_OPT_NULL_");
		bool allowDuplication = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.duplication, "false") == "true";
		string option = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.player_side, SkillFilterCreator.ContentKeyword.me.ToStringCustom());
		IsOpen = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.is_open, "_OPT_NULL_");
		bool flag = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.type, "_OPT_NULL_") == "oldest";
		bool flag2 = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.except_fusion_card_id) == "true";
		bool flag3 = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.except_game_play_card_id) == "true";
		int tokenCardBaseCost = -1;
		if (option == SkillFilterCreator.ContentKeyword.both.ToStringCustom())
		{
			_isPlayerSideBoth = true;
			_playerSide = base.SkillPrm.ownerCard.SelfBattlePlayer;
		}
		else
		{
			_playerSide = ((option == SkillFilterCreator.ContentKeyword.me.ToStringCustom()) ? base.SkillPrm.ownerCard.SelfBattlePlayer : base.SkillPrm.ownerCard.OpponentBattlePlayer);
		}
		bool flag4 = false;
		if ("_OPT_NULL_" != text && text.Contains("Gleaming_Gem_v2_"))
		{
			string[] array = text.Replace("Gleaming_Gem_v2_", "").Split('?', ':');
			int classId = int.Parse(array[0]);
			_tokenIds = Data.Master.GetGleamingGemListV2Master(classId);
			_randomCount = base.OptionValue.ParseInt(array[1]);
			if (array.Length > 2)
			{
				tokenCardBaseCost = base.OptionValue.ParseInt(array[2]);
			}
		}
		else if ("_OPT_NULL_" != text && text.Contains("Radiant_Crystal_v2_"))
		{
			string[] array2 = text.Replace("Radiant_Crystal_v2_", "").Split('?');
			int classId2 = int.Parse(array2[0]);
			_tokenIds = Data.Master.GetRadiantCrystalListV2Master(classId2);
			_randomCount = base.OptionValue.ParseInt(array2[1]);
			if (array2.Length > 2)
			{
				tokenCardBaseCost = base.OptionValue.ParseInt(array2[2]);
			}
		}
		else if ("_OPT_NULL_" != text && text.Contains("Gleaming_Gem_"))
		{
			string[] array3 = text.Replace("Gleaming_Gem_", "").Split('?');
			int classId3 = int.Parse(array3[0]);
			_tokenIds = Data.Master.GetGleamingGemList(classId3);
			_randomCount = int.Parse(array3[1]);
		}
		else if ("_OPT_NULL_" != text && text.Contains("Radiant_Crystal_"))
		{
			string[] array4 = text.Replace("Radiant_Crystal_", "").Split('?');
			int classId4 = int.Parse(array4[0]);
			_tokenIds = Data.Master.GetRadiantCrystalList(classId4);
			_randomCount = int.Parse(array4[1]);
		}
		else if ("_OPT_NULL_" != text && text.Contains("basic_card"))
		{
			string[] source = text.Replace(")", "").Replace("(", "").Split(':');
			int classType = 0;
			DataMgr dataMgr = SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.GetDataMgr();
			foreach (BattleCardBase targetCard in targetCards)
			{
				classType = (targetCard.IsPlayer ? dataMgr.GetPlayerClassId() : dataMgr.GetEnemyClassId());
			}
			_tokenIds = ClassBasicCardList.GetRandomBasicCardId((CardBasePrm.ClanType)classType).AsEnumerable();
			_randomCount = int.Parse(source.Last());
			flag4 = true;
		}
		else if (targetCards.Count() > 0)
		{
			_tokenIds = GetTargetId(targetCards);
			flag4 = true;
		}
		else if ("_OPT_NULL_" != text)
		{
			_tokenIds = SkillOptionValue.ParseOptionTokenID(text);
			string[] source2 = text.Replace(")", "").Replace("(", "").Split('?');
			_randomCount = ((source2.Count() >= 2) ? base.OptionValue.ParseInt(source2.Last()) : (-1));
			flag4 = true;
		}
		if (tokenCardBaseCost != -1)
		{
			_tokenIds = _tokenIds.Where((int id) => CardMaster.GetInstanceForBattle().GetCardParameterFromId(id).Cost == tokenCardBaseCost);
		}
		if (flag2)
		{
			List<int> second = base.SkillPrm.ownerCard.FusionIngredients.Select((BattleCardBase card) => card.CardId).ToList();
			_tokenIds = _tokenIds.Except(second);
		}
		if (flag3)
		{
			List<int> list = base.SkillPrm.ownerCard.SelfBattlePlayer.GamePlayCards.Select((BattleCardBase card) => card.BaseParameter.BaseCardId).ToList();
			list.AddRange(base.SkillPrm.ownerCard.SelfBattlePlayer.GamePlayCards.Select((BattleCardBase card) => card.BaseParameter.FoilCardId).ToList());
			_tokenIds = _tokenIds.Except(list);
		}
		if (_randomCount != -1)
		{
			bool isRandomDistinct = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.is_random_distinct, "null") == "true";
			_tokenIds = GetRandomSelect(_tokenIds, _randomCount, allowDuplication, isRandomDistinct);
			flag4 = true;
		}
		if (flag && _tokenIds != null)
		{
			int num2 = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.limit_upper_count, -1);
			if (num2 != -1)
			{
				int count = Mathf.Min(num2, _tokenIds.Count());
				_tokenIds = _tokenIds.Take(count);
			}
		}
		if (num != -1 && _tokenIds != null)
		{
			if (num == 0)
			{
				return false;
			}
			List<int> collection = _tokenIds.ToList();
			List<int> list2 = _tokenIds.ToList();
			for (int num3 = 1; num3 < num; num3++)
			{
				list2.AddRange(collection);
			}
			_tokenIds = list2;
			flag4 = true;
		}
		if (!flag4)
		{
			return false;
		}
		if (!isReserve)
		{
			List<int> list3 = new List<int>();
			List<int> list4 = new List<int>();
			for (int num4 = 0; num4 < _tokenIds.Count(); num4++)
			{
				int num5 = _tokenIds.ElementAt(num4);
				list3.Add(num5);
				list4.Add((_targetCardsIndexList.Count() < _tokenIds.Count()) ? _targetCardsIndexList.First() : _targetCardsIndexList[num4]);
				TokenDrawModifier tokenDrawModifier = _selfBattlePlayer.Class.SkillApplyInformation.GetTokenDrawModifier(num5);
				if (tokenDrawModifier != null)
				{
					TokenModifierList.Add(num5);
					_tokenModifierIndexList.AddRange(Enumerable.Range(list3.Count, tokenDrawModifier.MultiplyCount - 1));
					list3.AddRange(Enumerable.Repeat(num5, tokenDrawModifier.MultiplyCount - 1));
					list4.AddRange(Enumerable.Repeat(-1, tokenDrawModifier.MultiplyCount - 1));
				}
			}
			if (list3.Count > 0)
			{
				_tokenIds = list3;
				_targetCardsIndexList = list4;
			}
		}
		if (_tokenIds.Count() == 0)
		{
			return false;
		}
		return true;
	}

	protected virtual VfxWithLoading CreateTokenDrawVfx(CallParameter parameter, List<BattleCardBase> drawList, VfxWithLoadingSequential vfxWithLoading, BattlePlayerBase playerSide, bool isReservation = false)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		VfxWith<IEnumerable<BattleCardBase>> vfxWith = playerSide.DrawCards(drawList, parameter.skillProcessor, !isReservation, isMulligan: false, isToken: true, isSkillDraw: true, this, isReservation, parameter.calledSkillResultInfo, base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.turn_token_draw_skill_id, -1));
		drawList = vfxWith.Value.ToList();
		parallelVfxPlayer.Register(vfxWith.Vfx);
		if (parameter.targetCards.Count() == 0)
		{
			AddLastTarget(parameter, drawList);
		}
		parameter.calledSkillResultInfo.drawCards = BattlePlayerBase.ConvertToSkillInfoCollection(drawList);
		if (!PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SHOW_BATTLE_EFFECT) && !string.IsNullOrEmpty(base.SkillPrm.buildInfo._effectPath))
		{
			vfxWithLoading.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		}
		VfxWithLoading vfxWithLoading2 = CreateTokenSpawnVfx(this, drawList.First());
		IsVisibleTarget = IsVisibleDrawSkillTarget(_selfBattlePlayer, parameter);
		if (IsVisibleTarget && IsOpen == "false" && !drawList.First().IsPlayer)
		{
			IsVisibleTarget = false;
		}
		if (IsOpponentHandCopy || IsOpen == "true")
		{
			IsVisibleTarget = true;
		}
		vfxWithLoading.RegisterToMainVfx(NullVfx.GetInstance());
		vfxWithLoading.RegisterToLoadingVfx(vfxWithLoading2.LoadingVfx);
		vfxWithLoading.RegisterToMainVfx(InstantVfx.Create(delegate
		{
			base.SkillPrm.selfBattlePlayer.UpdateHandCardsPlayability();
		}));
		List<BattleCardBase> list = drawList.Where((BattleCardBase s) => s.IsInHand).ToList();
		for (int num = 0; num < list.Count(); num++)
		{
			base.SkillPrm.ownerCard.SkillApplyInformation.AddSkillDrewCard(list.ElementAt(num));
			BattleLogManager.GetInstance().UpdateFusionedCardSkillDrewCard(base.SkillPrm.ownerCard);
		}
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillDrawToken(list, this, IsVisibleTarget);
			BattleLogManager.GetInstance().AddLogSkillDrawToken(drawList.Where((BattleCardBase s) => !s.IsInHand).ToList(), this, IsVisibleTarget, isOverDraw: true);
		}
		if (_randomCount > 0)
		{
			for (int num2 = 0; num2 < drawList.Count; num2++)
			{
				base.SkillPrm.ownerCard.SkillApplyInformation.AddRandomSelectedCard(drawList[num2]);
			}
		}
		return vfxWithLoading;
	}

	protected virtual BattleCardBase CreateTokenCard(BattlePlayerBase player, int id, int index, NetworkBattleDefine.NetworkCardPlaceState toState, bool isCopy = false)
	{
		return player.CreateNextIndexCard(id);
	}

	public virtual bool IsInvisibleTarget()
	{
		if (!(base.ApplyingTargetFilter is SkillTargetHandOtherSelfFilter) && !(base.ApplyingTargetFilter is SkillTargetHandFilter) && !(base.ApplyingTargetFilter is SkillTargetDeckFilter) && !(base.ApplyingTargetFilter is SkillTargetChosenCardsFilter) && !(base.ApplyingTargetFilter is SkillTargetDiscardThisTurnCardListFilter) && !IsHaveApplicableTargetFilter<SkillTargetDiscardCardListFilter>())
		{
			return IsTargetHandOtherSelfFilter();
		}
		return true;
	}

	public virtual bool IsVisibleDrawSkillTarget(BattlePlayerBase selfBattlePlayer, CallParameter parameter)
	{
		if (!selfBattlePlayer.IsPlayer && !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch)
		{
			if (parameter.targetCards.Count() > 0)
			{
				if (!(base.ApplyingTargetFilter is SkillTargetHandOtherSelfFilter) && !(base.ApplyingTargetFilter is SkillTargetHandFilter) && !(base.ApplyingTargetFilter is SkillTargetDeckFilter) && !(base.ApplyingTargetFilter is SkillTargetChosenCardsFilter) && !(base.ApplyingTargetFilter is SkillTargetDiscardThisTurnCardListFilter) && !IsTargetHandOtherSelfFilter())
				{
					return !IsHaveApplicableTargetFilter<SkillTargetDiscardCardListFilter>();
				}
				return false;
			}
			return true;
		}
		return true;
	}

	protected bool IsTargetHandOtherSelfFilter()
	{
		for (int i = 0; i < base.ApplyAndFilter.Count; i++)
		{
			if (base.ApplyAndFilter[i].TargetFilter is SkillTargetHandOtherSelfFilter)
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerable<int> GetTargetId(IEnumerable<BattleCardBase> targetCards)
	{
		return targetCards.Select((BattleCardBase card) => (!card.IsChoiceEvolutionCard) ? card.BaseParameter.NormalCardId : card.BaseParameter.BaseCardId);
	}

	private IEnumerable<int> GetRandomSelect(IEnumerable<int> ids, int randomCount, bool allowDuplication, bool isRandomDistinct)
	{
		List<int> list = ids.ToList();
		List<int> list2 = new List<int>();
		int num = (allowDuplication ? randomCount : Math.Min(randomCount, list.Count));
		if (isRandomDistinct)
		{
			list = list.Where((int num3) => !base.SkillPrm.ownerCard.SkillApplyInformation.RandomSelectedCardList.Any((BattleCardBase c) => c.BaseParameter.BaseCardId == num3)).ToList();
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

	public static VfxWithLoading CreateTokenSpawnVfx(SkillBase skill, BattleCardBase firstToken)
	{
		// Static helper with `skill` param — route through skill.SkillPrm rather than instance SkillPrm.
		if (skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsRecovery)
		{
			return NullVfxWithLoading.GetInstance();
		}
		float animationTime = 0f;
		Color color = firstToken.Clan switch
		{
			CardBasePrm.ClanType.MIN => Global.EFFECT_COLOR_ELF, 
			CardBasePrm.ClanType.ROYAL => Global.EFFECT_COLOR_ROYAL, 
			CardBasePrm.ClanType.WITCH => Global.EFFECT_COLOR_WITCH_1, 
			CardBasePrm.ClanType.DRAGON => Global.EFFECT_COLOR_DRAGON, 
			CardBasePrm.ClanType.NECRO => Global.EFFECT_COLOR_NECROMANCER, 
			CardBasePrm.ClanType.VAMPIRE => Global.EFFECT_COLOR_VANPIRE, 
			CardBasePrm.ClanType.BISHOP => Global.EFFECT_COLOR_BISHOP, 
			CardBasePrm.ClanType.NEMESIS => Global.EFFECT_COLOR_NEMESIS, 
			_ => Color.clear, 
		};
		string text = "cmn_token_draw_1";
		DataMgr.SpecialBattleSetting specialBattleSettingInfo = skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.GetDataMgr().SpecialBattleSettingInfo;
		if (specialBattleSettingInfo != null && specialBattleSettingInfo.TokenDrawOverrideEffectPair.ContainsKey(firstToken.CardId))
		{
			text = specialBattleSettingInfo.TokenDrawOverrideEffectPair[firstToken.CardId];
			if (text.Contains(":"))
			{
				string[] array = text.Split(':');
				text = array[0];
				animationTime = float.Parse(array[1]);
			}
			color = Color.clear;
		}
		Func<Vector3> func = () => firstToken.BattleCardView.GameObject.transform.position;
		return skill.CreateSkillEffectFromPath(text, "se_" + text, skill.SkillPrm.resourceMgr, EffectMgr.EngineType.SHURIKEN, EffectMgr.MoveType.DIRECT, func, func, animationTime, color);
	}

	public override void AddIndividualIdSkillBuffLog(Skill_attach_skill attachSkill, BattleCardBase target)
	{
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(base.SkillPrm.selfBattlePlayer, base.SkillPrm.opponentBattlePlayer);
		SkillCollectionBase.SetupOptionValue(base.OptionValue, playerInfoPair, base.SkillPrm.ownerCard, this);
		int num = SkillOptionValue.ParseOptionTokenID(base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.token_draw, "_OPT_NULL_")).FirstOrDefault();
		BuffInfo buffInfo = new BuffInfo(num, num, this);
		buffInfo.IsReserveTokenDrawSkill = true;
		target.AddBuffInfo(buffInfo);
		attachSkill.OnIndividualIdSkillStop = (Action)Delegate.Combine(attachSkill.OnIndividualIdSkillStop, (Action)delegate
		{
			target.RemoveBuffInfo(buffInfo);
			if (target.IsClass)
			{
				UpdateClassBuffIfActive(target);
			}
		});
	}
}
