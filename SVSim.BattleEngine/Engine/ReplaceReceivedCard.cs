using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class ReplaceReceivedCard
{
	public class CardIdAndIndex
	{
		public int CardId { get; private set; }

		public int CardIndex { get; private set; }

		public bool IsPlayer { get; private set; }

		public int Cost { get; private set; }

		public CardIdAndIndex(int cardId, int index, bool isPlayer, int cost = -1)
		{
			CardId = cardId;
			CardIndex = index;
			IsPlayer = isPlayer;
			Cost = cost;
		}
	}

	protected readonly NetworkBattleManagerBase _networkBattleMgr;

	protected int CardIdx;

	protected int CardId;

	private int _playCardCost = -1;

	protected int? _buffAddAtk;

	protected int? _buffSetAtk;

	protected int? _buffSetLife;

	protected int? _buffAddLife;

	protected int _clan;

	protected List<CardBasePrm.TribeType> _tribe;

	protected string _attachTarget = "";

	protected NetworkBattleDefine.NetworkCardPlaceState _fromState;

	protected List<NetworkBattleDefine.NetworkCardPlaceState> _toStateList;

	protected int _skillCardIndex;

	protected int _spellboost = -1;

	protected int? _addChantCount;

	protected int? _setChantCount;

	protected int _unionBurstCount = -1;

	protected int _skyboundArtCount = -1;

	protected List<int> _fusionIngredientList;

	protected bool _isOpen;

	private bool _isReserved;

	private bool _isCemetary;

	private bool _isNecromanceZone;

	protected BattleCardBase _originalDummyCard;

	private readonly int[] _machineGoddessCardIdList = new int[3] { 900841110, 900841120, 900841130 };

	public ReplaceReceivedCard(NetworkBattleManagerBase battleMgrBase, CardDataModel cardData)
	{
		_networkBattleMgr = battleMgrBase;
		CardIdx = cardData.Index;
		CardId = cardData.CardId;
		_playCardCost = cardData.playCardCost;
		_buffAddAtk = cardData.AddAtk;
		_buffSetAtk = cardData.SetAtk;
		_buffAddLife = cardData.AddLife;
		_buffSetLife = cardData.SetLife;
		_clan = cardData.Clan;
		_tribe = ((cardData.Tribe != "NONE") ? CardParameter.CreateTribeList(cardData.Tribe) : null);
		_attachTarget = cardData.AttachTarget;
		_fromState = cardData.fromState;
		_toStateList = cardData.ToStateList;
		_skillCardIndex = cardData.skillCardIndex;
		_spellboost = cardData.Spellboost;
		_addChantCount = cardData.AddChantCount;
		_setChantCount = cardData.SetChantCount;
		_unionBurstCount = cardData.UnionBurstCount;
		_skyboundArtCount = cardData.SkyboundArtCount;
		_fusionIngredientList = cardData.FusionIngredientList;
		_isOpen = cardData.IsOpen;
		_isReserved = false;
		_isCemetary = false;
		_isNecromanceZone = false;
	}

	public BattleCardBase ReplaceCard(BattlePlayerBase battlePlayer)
	{
		bool isCardInDeck = SearchForDummyCardInHandAndDeck(battlePlayer);
		CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(CardId);
		bool flag = cardParameterFromId.SkillTiming.Contains("when_draw");
		bool flag2 = _machineGoddessCardIdList.Contains(cardParameterFromId.BaseCardId);
		if (_originalDummyCard == null)
		{
			if (!_networkBattleMgr.GameMgr.IsWatchBattle && (flag || flag2))
			{
				_networkBattleMgr.AddNotReplaceCardList(new CardIdAndIndex(CardId, CardIdx, battlePlayer.IsPlayer, _playCardCost));
				return null;
			}
			return null;
		}
		BattleCardBase battleCardBase = CreateActualCard(battlePlayer, isCardInDeck, flag);
		if (battleCardBase is NullBattleCard)
		{
			return null;
		}
		ReplaceCardInList(battlePlayer, battleCardBase, isCardInDeck);
		return battleCardBase;
	}

	protected bool SearchForDummyCardInHandAndDeck(BattlePlayerBase battlePlayer)
	{
		bool result = true;
		_originalDummyCard = battlePlayer.DeckCardList.SingleOrDefault((BattleCardBase c) => c.Index == CardIdx);
		if (_originalDummyCard == null)
		{
			_originalDummyCard = battlePlayer.HandCardList.SingleOrDefault((BattleCardBase c) => c.Index == CardIdx);
			result = false;
		}
		if (_originalDummyCard == null)
		{
			_originalDummyCard = battlePlayer.CemeteryList.FirstOrDefault((BattleCardBase c) => c.Index == CardIdx);
			_isCemetary = _originalDummyCard != null;
		}
		if (_originalDummyCard == null)
		{
			_originalDummyCard = battlePlayer.NecromanceZoneList.FirstOrDefault((BattleCardBase c) => c.Index == CardIdx);
			_isNecromanceZone = _originalDummyCard != null;
		}
		if (_originalDummyCard == null)
		{
			_originalDummyCard = battlePlayer.ReservedCardList.FirstOrDefault((BattleCardBase c) => c.Index == CardIdx);
			_isReserved = true;
		}
		return result;
	}

	protected virtual BattleCardBase CreateActualCard(BattlePlayerBase battlePlayer, bool isCardInDeck, bool isOpenDrawSkill)
	{
		BattleCardBase battleCardBase = _networkBattleMgr.CreateBattleCardWithGameObject(new BattleManagerBase.CardCreateInfo(CardId, battlePlayer.IsPlayer, isChoice: false, NetworkBattleDefine.NetworkCardPlaceState.None), new BattleManagerBase.IndexInfo(CardIdx), -1, isVirtual: false, isActualCard: true);
		if (!_networkBattleMgr.GameMgr.IsWatchBattle && _isOpen && _fromState == NetworkBattleDefine.NetworkCardPlaceState.Deck && _toStateList.Contains(NetworkBattleDefine.NetworkCardPlaceState.Banish) && battleCardBase.Skills.Any((SkillBase s) => s is Skill_banish && s.OnWhenDraw != 0 && s.ApplyingTargetFilter is SkillTargetSelfFilter))
		{
			return CardCreatorBase.GetDummyInstance();
		}
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(CreateReplaceDummyCardVfx(battleCardBase, battlePlayer, isCardInDeck, isOpenDrawSkill));
		InheritedCardData(battleCardBase);
		if (!(battleCardBase is SpellBattleCard))
		{
			battleCardBase.Skills.InitTimingInfo();
			battleCardBase.BattleCardView.BattleCardIconAnimations.Initialize(battleCardBase, battleCardBase.Skills);
		}
		ReplaceBuffInfoList(battleCardBase, _originalDummyCard);
		battleCardBase.ShallowCopyBuffInfoList(_originalDummyCard);
		_networkBattleMgr.VfxMgr.RegisterSequentialVfx(sequentialVfxPlayer);
		SettingTargetDeckSelfCardAddDeckSkillCardList(battleCardBase, isCardInDeck);
		RemoveOpenCardRemoveAfterActionSkills(battleCardBase);
		return battleCardBase;
	}

	protected void InheritedCardData(BattleCardBase receivedCard)
	{
		NetworkBattleReceiver.ReceiveData receiveData = _networkBattleMgr.networkBattleData.GetReceiveData();
		if (receiveData != null && (!receiveData.IsTransformChoice || receiveData.playCardIndex != receivedCard.Index || receiveData.keyActionType.Exists((SendKeyActionDataManager.KeyActionType x) => x == SendKeyActionDataManager.KeyActionType.HaveBeforeSkillChoice)))
		{
			CopyDataToActualCard(receivedCard);
			AddAttachedSkill(receivedCard);
			InheritedAddParameter(receivedCard);
			InheritedAffiliation(receivedCard);
			InheritedSkillValue(receivedCard);
		}
	}

	protected void AddAttachedSkill(BattleCardBase receivedCard)
	{
		if (_attachTarget == null)
		{
			return;
		}
		string[] array = _attachTarget.Split(',');
		for (int i = 0; i < array.Count(); i++)
		{
			string text = array[i];
			if (text.Contains('|') || !(text != string.Empty))
			{
				continue;
			}
			if (!int.TryParse(text, out var count))
			{
				LocalLog.AccumulateTraceLog("#690983 incorrect attachTarget:" + _attachTarget);
				break;
			}
			SkillBase skillBase = receivedCard.SelfBattlePlayer.BattleMgr.PublishedSkillList.SingleOrDefault((SkillBase s) => s.PublishedActiveSkillCount == count);
			if (skillBase == null)
			{
				continue;
			}
			if (skillBase is Skill_attach_skill)
			{
				skillBase.IsNotAssignPublishedActiveSkillCount = true;
				SkillBase skill = Skill_attach_skill.CreateAndAttachSkill(receivedCard, skillBase, Skill_attach_skill.CreateAttachSkillBuildInfo(skillBase.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.skill)));
				skillBase.ReplaceBuffInfoTargetCard(_originalDummyCard, receivedCard);
				if (skillBase.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessTurnEndStop))
				{
					skillBase.ReplaceBuffInfoSkill(receivedCard, skill);
				}
				skillBase.IsNotAssignPublishedActiveSkillCount = false;
			}
			else if (skillBase is Skill_powerup)
			{
				receivedCard.SkillApplyInformation.GiveBuff(isReplace: true);
				if ((skillBase as Skill_powerup).GetAddLife() > 0)
				{
					receivedCard.SkillApplyInformation.GiveBuffLife();
				}
				receivedCard.OnRemoveFromInPlayAfterOneTime += delegate
				{
					receivedCard.SkillApplyInformation.DepriveBuff();
					return NullVfx.GetInstance();
				};
			}
			else if (skillBase is Skill_attack_by_life)
			{
				receivedCard.SkillApplyInformation.GiveAttackByLife((skillBase as Skill_attack_by_life).type);
				skillBase.ReplaceBuffInfoTargetCard(_originalDummyCard, receivedCard);
			}
		}
	}

	protected void InheritedAddParameter(BattleCardBase receivedCard)
	{
		AddSetModifier(receivedCard);
		InheritedAddParameterBuff(receivedCard);
	}

	private void InheritedAddParameterBuff(BattleCardBase receivedCard)
	{
		ICardOffenseModifier cardOffenseModifier = null;
		ICardLifeModifier cardLifeModifier = null;
		if (_buffAddAtk.HasValue)
		{
			cardOffenseModifier = new OffenseAddModifier(_buffAddAtk.Value);
		}
		if (_buffAddLife.HasValue)
		{
			cardLifeModifier = new LifeAddModifier(_buffAddLife.Value);
		}
		if (cardOffenseModifier != null || cardLifeModifier != null)
		{
			receivedCard.SkillApplyInformation.GiveCombatValueModifier(cardOffenseModifier, cardLifeModifier, new SkillProcessor());
		}
	}

	private void AddSetModifier(BattleCardBase receivedCard)
	{
		ICardOffenseModifier cardOffenseModifier = null;
		ICardLifeModifier cardLifeModifier = null;
		if (_buffSetAtk.HasValue)
		{
			cardOffenseModifier = new OffenseSetModifier(_buffSetAtk.Value);
		}
		if (_buffSetLife.HasValue)
		{
			cardLifeModifier = new LifeSetModifier(_buffSetLife.Value);
		}
		if (cardOffenseModifier != null || cardLifeModifier != null)
		{
			receivedCard.SkillApplyInformation.GiveCombatValueModifier(cardOffenseModifier, cardLifeModifier, new SkillProcessor());
		}
	}

	protected void InheritedAffiliation(BattleCardBase receivedCard)
	{
		if (_clan == -1 && (_tribe == null || _tribe.Count <= 0) && _attachTarget == null)
		{
			return;
		}
		CardBasePrm.ClanType clan = receivedCard.Clan;
		if (_clan != -1)
		{
			clan = (CardBasePrm.ClanType)_clan;
		}
		CardBasePrm.TribeInfo tribeInfo = null;
		if (_tribe != null && _tribe.Count > 0)
		{
			tribeInfo = new CardBasePrm.TribeInfo(_tribe, CardBasePrm.TribeChangeType.CHANGE);
		}
		receivedCard.SkillApplyInformation.GiveChangeAffiliation(clan, tribeInfo, showEffect: false);
		if (_attachTarget == null)
		{
			return;
		}
		string[] array = _attachTarget.Split(',');
		for (int i = 0; i < array.Count(); i++)
		{
			string text = array[i];
			if (text.Contains('|') || !(text != string.Empty))
			{
				continue;
			}
			if (!int.TryParse(text, out var count))
			{
				LocalLog.AccumulateTraceLog("#690983 incorrect attachTarget:" + _attachTarget);
				break;
			}
			Skill_change_affiliation skill = receivedCard.SelfBattlePlayer.BattleMgr.PublishedSkillList.SingleOrDefault((SkillBase s) => s.PublishedActiveSkillCount == count) as Skill_change_affiliation;
			if (skill != null)
			{
				skill.ReplaceBuffInfoTargetCard(_originalDummyCard, receivedCard);
				receivedCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
				{
					skill.RegisterStop(card);
					return card.SkillApplyInformation.ForceDepriveChangeAffiliation();
				};
			}
		}
	}

	protected void InheritedSkillValue(BattleCardBase receivedCard)
	{
		if (_unionBurstCount != -1)
		{
			ICardUnionBurstCountModifier unionBurstCountModifier = new UnionBurstCountAddModifier(-_unionBurstCount);
			receivedCard.SkillApplyInformation.GiveUnionBurstCount(unionBurstCountModifier);
		}
		if (_skyboundArtCount != -1)
		{
			ICardSkyboundArtCountModifier skyboundArtCountModifier = new SkyboundArtCountAddModifier(-_skyboundArtCount);
			receivedCard.SkillApplyInformation.GiveSkyboundArtCount(skyboundArtCountModifier);
			ICardSuperSkyboundArtCountModifier superSkyboundArtCountModifier = new SuperSkyboundArtCountAddModifier(-_skyboundArtCount);
			receivedCard.SkillApplyInformation.GiveSuperSkyboundArtCount(superSkyboundArtCountModifier);
		}
		if (receivedCard.BaseParameter.BaseCardId == _originalDummyCard.BaseParameter.BaseCardId)
		{
			receivedCard.SetSkillActivatedCount(_originalDummyCard.SkillActivatedCount);
		}
		if (_originalDummyCard.SkillApplyInformation.SkillRandomArray != null)
		{
			receivedCard.SkillApplyInformation.GiveSkillRandomArray(_originalDummyCard.SkillApplyInformation.SkillRandomArray);
		}
		if (_originalDummyCard.SkillApplyInformation.RandomSelectedCardList != null)
		{
			receivedCard.SkillApplyInformation.RandomSelectedCardList.AddRange(_originalDummyCard.SkillApplyInformation.RandomSelectedCardList);
		}
		if (_originalDummyCard.SkillApplyInformation.SkillDrewCardList != null)
		{
			receivedCard.SkillApplyInformation.SkillDrewCardList.AddRange(_originalDummyCard.SkillApplyInformation.SkillDrewCardList);
		}
		for (int i = 0; i < Mathf.Min(_originalDummyCard.Skills.Count(), receivedCard.Skills.Count()); i++)
		{
			SkillPreprocessTimesPerGame skillPreprocessTimesPerGame = _originalDummyCard.Skills.ElementAt(i).PreprocessList.FirstOrDefault((SkillPreprocessBase p) => p is SkillPreprocessTimesPerGame) as SkillPreprocessTimesPerGame;
			SkillPreprocessTimesPerGame skillPreprocessTimesPerGame2 = receivedCard.Skills.ElementAt(i).PreprocessList.FirstOrDefault((SkillPreprocessBase p) => p is SkillPreprocessTimesPerGame) as SkillPreprocessTimesPerGame;
			if (skillPreprocessTimesPerGame != null)
			{
				skillPreprocessTimesPerGame2?.Clone(skillPreprocessTimesPerGame, _originalDummyCard.Skills.ElementAt(i));
			}
		}
	}

	protected void ReplaceBuffInfoList(BattleCardBase receivedCard, BattleCardBase dummyCard)
	{
		if (_attachTarget == null)
		{
			dummyCard.BuffInfoList.Clear();
			return;
		}
		List<SkillBase> list = new List<SkillBase>();
		List<int> list2 = new List<int>();
		string[] array = _attachTarget.Split(',');
		int num = 0;
		bool flag = false;
		for (int i = 0; i < array.Count(); i++)
		{
			string text = array[i];
			if (text.Contains('|'))
			{
				string[] array2 = array[i].Split('|');
				int num2 = 0;
				int num3 = 0;
				int skillIndex = 0;
				bool isEvol = false;
				BattlePlayerBase selfBattlePlayer = receivedCard.SelfBattlePlayer;
				for (int j = 0; j < array2.Count(); j++)
				{
					int result = 0;
					if (!int.TryParse(array2[j], out result))
					{
						return;
					}
					switch (num2)
					{
					case 0:
						num3 = result;
						list2.Add(num3);
						break;
					case 1:
						skillIndex = result;
						break;
					case 2:
						isEvol = result == 1;
						break;
					}
					if (num2 == 2)
					{
						try
						{
							CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(num3);
							BattleCardBase battleCardBase = _networkBattleMgr.CreateBattleCard(num3, selfBattlePlayer.IsPlayer, null, cardParameterFromId, selfBattlePlayer, CardIdx);
							SkillBase skillBase = NetworkBattleGenericTool.SearchCardSkillIndex(battleCardBase, skillIndex, isEvol);
							list.Add(skillBase);
							if (skillBase.OnWhenPlayOtherStart != 0 || skillBase.OnWhenEvolveOtherStart != 0 || skillBase.OnWhenEvolveSelfAndOtherStart != 0 || skillBase.OnWhenBanishOther != 0 || skillBase.OnWhenSpellChargeStart != 0 || ((skillBase.OnSelfTurnEndStart != 0 || skillBase.OnWhenDraw != 0) && skillBase.ApplyingTargetFilter is SkillTargetHandSelfFilter))
							{
								if (skillBase is Skill_powerup)
								{
									Skill_powerup skill_powerup = skillBase as Skill_powerup;
									skill_powerup.OptionValue = new SkillOptionValue(skill_powerup.SkillPrm.buildInfo._parsedOption);
									BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(_networkBattleMgr.BattlePlayer, _networkBattleMgr.BattleEnemy);
									SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
									skillConditionCheckerOption.AddChargeCount = _spellboost;
									SkillCollectionBase.SetupOptionValue(skill_powerup.OptionValue, playerInfoPair, skillBase.SkillPrm.ownerCard, skillBase, skillConditionCheckerOption);
									if (SkillOptionValue.IsVariableValue(skill_powerup.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.add_offense, "0")) || SkillOptionValue.IsVariableValue(skill_powerup.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.add_life, "0")))
									{
										bool flag2 = skill_powerup.IsRefVariable(SkillFilterCreator.ContentKeyword.add_offense.ToString());
										bool flag3 = skill_powerup.IsRefVariable(SkillFilterCreator.ContentKeyword.add_life.ToString());
										skill_powerup.SettingPowerUpData(flag2 ? 1 : 0, flag3 ? 1 : 0);
									}
									else
									{
										int offense = skill_powerup.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_offense, 0);
										int life = skill_powerup.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_life, 0);
										skill_powerup.SettingPowerUpData(offense, life);
									}
									skillBase.InsertBuffInfoIfNeeded(dummyCard, i);
									receivedCard.SkillApplyInformation.GiveBuff(isReplace: true);
									if ((skillBase as Skill_powerup).GetAddLife() > 0)
									{
										receivedCard.SkillApplyInformation.GiveBuffLife();
									}
								}
								else if (skillBase is Skill_attach_skill)
								{
									Skill_attach_skill skill_attach_skill = skillBase as Skill_attach_skill;
									CreateAndAttachSkill(receivedCard, skill_attach_skill);
									flag = true;
									skill_attach_skill.InsertBuffInfoIfNeeded(dummyCard, i);
									list.Add(skill_attach_skill);
								}
								else if (skillBase is Skill_generic_value_modifier)
								{
									if (skillBase.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.set).Contains(SkillFilterCreator.ContentKeyword.fixed_generic_value_initial.ToString()))
									{
										IEnumerable<SkillBase> source = battleCardBase.Skills.Where((SkillBase s) => s is Skill_attach_skill && s.SkillPrm.buildInfo._condition.Contains("{me.hand_self.count}>0"));
										Skill_attach_skill skill_attach_skill2 = source.ElementAt(num % source.Count()) as Skill_attach_skill;
										CreateAndAttachSkill(receivedCard, skill_attach_skill2);
										num++;
										skill_attach_skill2.InsertBuffInfoIfNeeded(dummyCard, i);
										list.Add(skill_attach_skill2);
										flag = true;
									}
								}
								else
								{
									Debug.LogError("UnknownBuff" + skillBase);
								}
							}
						}
						catch
						{
						}
						num2 = 0;
						skillIndex = 0;
					}
					else
					{
						num2++;
					}
				}
			}
			else
			{
				if (!(text != string.Empty))
				{
					continue;
				}
				if (!int.TryParse(text, out var count))
				{
					LocalLog.AccumulateTraceLog("#690983 incorrect attachTarget:" + _attachTarget);
					return;
				}
				SkillBase value = receivedCard.SelfBattlePlayer.BattleMgr.PublishedSkillList.SingleOrDefault((SkillBase s) => s.PublishedActiveSkillCount == count);
				if (value != null)
				{
					list2.Add(value.SkillPrm.ownerCard.BaseParameter.BaseCardId);
					list.Add(value);
					SkillBase skillBase2 = receivedCard.Skills.FirstOrDefault((SkillBase s) => s.GetAttachSkill == value);
					if (skillBase2 != null && flag)
					{
						receivedCard.Skills.Remove(skillBase2);
						receivedCard.Skills.Add(skillBase2);
						receivedCard.Skills.Complete();
					}
				}
			}
		}
		for (int num4 = 0; num4 < list.Count; num4++)
		{
			SkillBase skill = list[num4];
			if (skill == null)
			{
				LocalLog.AccumulateTraceLog("#690983 incorrect attachTarget:" + _attachTarget);
			}
			else if (IsBuffServerInfo(skill))
			{
				BuffInfo buffInfo = dummyCard.BuffInfoList.FirstOrDefault((BuffInfo b) => b.SkillFrom == skill);
				if (buffInfo == null)
				{
					buffInfo = skill.InsertBuffInfoIfNeeded(dummyCard, num4, isBaseCardId: true);
				}
				skill.SetEventAfterReplace(receivedCard, buffInfo);
			}
		}
		List<BuffInfo> list3 = new List<BuffInfo>();
		for (int num5 = 0; num5 < dummyCard.BuffInfoList.Count(); num5++)
		{
			BuffInfo buff = dummyCard.BuffInfoList[num5];
			try
			{
				if (list2.Count == 0)
				{
					list3.Add(buff);
					continue;
				}
				SkillBase skillBase3 = list.Find((SkillBase x) => x.ToString() == buff.SkillFrom.ToString());
				if (!list2.Contains(buff.BaseCardIDFrom) || skillBase3 == null)
				{
					list3.Add(buff);
					continue;
				}
				list2.Remove(buff.BaseCardIDFrom);
				list.Remove(skillBase3);
			}
			catch
			{
			}
		}
		for (int num6 = 0; num6 < list3.Count; num6++)
		{
			dummyCard.BuffInfoList.Remove(list3[num6]);
		}
	}

	private static bool IsBuffServerInfo(SkillBase skill)
	{
		if (!RegisterFilter.IsBothTurnFilterSkill(skill))
		{
			if ((!(skill is Skill_cost_change) && !(skill is Skill_powerup)) || !(skill.ApplyingTargetFilter is SkillTargetDeckFilter))
			{
				return skill.ApplyingTargetFilter is SkillTargetSkillUpdateDeckCardFilter;
			}
			return true;
		}
		return true;
	}

	private void CreateAndAttachSkill(BattleCardBase receivedCard, Skill_attach_skill attachSkill)
	{
		attachSkill.IsNotAssignPublishedActiveSkillCount = true;
		SkillBase skillBase = Skill_attach_skill.CreateAndAttachSkill(receivedCard, attachSkill, Skill_attach_skill.CreateAttachSkillBuildInfo(attachSkill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.skill)));
		BuffInfo buffInfo = skillBase.AddBuffInfoIfNeeded(_originalDummyCard);
		attachSkill.AddBuffInfo(new SkillBase.BuffInfoContainer(receivedCard, buffInfo, attachSkill.SkillPrm.ownerCard.Skills.IndexOf(attachSkill), "", skillBase, 0L));
		attachSkill.ReplaceBuffInfoTargetCard(_originalDummyCard, receivedCard);
		attachSkill.IsNotAssignPublishedActiveSkillCount = false;
	}

	protected void SettingTargetDeckSelfCardAddDeckSkillCardList(BattleCardBase receivedCard, bool isCardInDeck)
	{
		if (_networkBattleMgr.networkBattleData != null && _networkBattleMgr.networkBattleData.GetReceiveData() != null && _networkBattleMgr.networkBattleData.GetReceiveData().unapprovedList != null)
		{
			bool flag = _networkBattleMgr.networkBattleData.GetReceiveData().unapprovedList.Exists((CardDataModel x) => x.skillCardIndex == receivedCard.Index) && receivedCard.Skills.Any((SkillBase s) => s.ConditionTargetFilter is SkillTargetDeckSelfFilter);
			bool flag2 = receivedCard.Skills.Any((SkillBase s) => RegisterValidate.IsDeckParamVariable(s)) && _toStateList.Contains(NetworkBattleDefine.NetworkCardPlaceState.Field);
			if (isCardInDeck && _fromState == NetworkBattleDefine.NetworkCardPlaceState.Deck && _networkBattleMgr.networkBattleData.GetReceiveData().unapprovedList.Exists((CardDataModel x) => x.skillCardIndex == receivedCard.Index) && (flag || flag2))
			{
				receivedCard.SelfBattlePlayer.RemoveOriginalAndAddDeckSkillCard(receivedCard);
			}
		}
	}

	protected void CopyDataToActualCard(BattleCardBase receivedCard)
	{
		receivedCard.SetSpellChargeCount(_spellboost);
		if (_setChantCount.HasValue)
		{
			receivedCard.SkillApplyInformation.GiveChantCount(new ChantCountSetModifier(_setChantCount.Value));
		}
		if (_addChantCount.HasValue)
		{
			receivedCard.SkillApplyInformation.GiveChantCount(new ChantCountAddModifier(_addChantCount.Value));
		}
		if (_playCardCost > -1)
		{
			receivedCard.AddCostModifier(new CostSetModifier(_playCardCost), null, eventCall: false);
		}
		receivedCard.SkillApplyInformation.AddFusionIngredients(_originalDummyCard.SkillApplyInformation.FusionIngredients);
	}

	public void SetPrivateCardSpellboost(BattlePlayerBase battlePlayer)
	{
		if (CardIdx == 0)
		{
			return;
		}
		_originalDummyCard = battlePlayer.DeckCardList.SingleOrDefault((BattleCardBase c) => c.Index == CardIdx);
		if (_originalDummyCard == null)
		{
			_originalDummyCard = battlePlayer.HandCardList.SingleOrDefault((BattleCardBase c) => c.Index == CardIdx);
		}
		if (_originalDummyCard != null)
		{
			_originalDummyCard.SetSpellChargeCount(_spellboost);
		}
	}

	protected void ReplaceCardInList(BattlePlayerBase battlePlayer, BattleCardBase receivedCard, bool isCardInDeck)
	{
		List<BattleCardBase> list = (_isReserved ? battlePlayer.ReservedCardList : (_isCemetary ? battlePlayer.CemeteryList : (_isNecromanceZone ? battlePlayer.NecromanceZoneList : ((!isCardInDeck) ? battlePlayer.HandCardList : battlePlayer.DeckCardList))));
		int index = list.IndexOf(_originalDummyCard);
		list[index] = receivedCard;
	}

	private VfxBase CreateReplaceDummyCardVfx(BattleCardBase receivedCard, BattlePlayerBase battlePlayer, bool isCardInDeck, bool isOpenDrawSkill)
	{
		int cost = receivedCard.Cost;
		BattleCardBase cardBeforeChangeParameter = receivedCard.VirtualClone(receivedCard.SelfBattlePlayer, receivedCard.OpponentBattlePlayer);
		IBattleCardView dummyCardView = _originalDummyCard.BattleCardView;
		bool isCemetary = _isCemetary;
		bool isNecromanceZone = _isNecromanceZone;
		return InstantVfx.Create(delegate
		{
			if (!isCardInDeck)
			{
				receivedCard.BattleCardView.GameObject.transform.parent = dummyCardView.GameObject.transform.parent;
				receivedCard.BattleCardView.GameObject.transform.localPosition = dummyCardView.GameObject.transform.localPosition;
				receivedCard.BattleCardView.GameObject.transform.localScale = dummyCardView.GameObject.transform.localScale;
				receivedCard.BattleCardView.GameObject.transform.localRotation = dummyCardView.GameObject.transform.localRotation;
				receivedCard.BattleCardView.UpdateParameterView(receivedCard.Atk, receivedCard.Life, (receivedCard.FixedUseCost != -1) ? receivedCard.FixedUseCost : receivedCard.Cost, receivedCard.BaseParameter.CardName, receivedCard.IsInplay);
				if (!_isReserved && !(isCemetary || isNecromanceZone))
				{
					battlePlayer.BattleView.HandView.ReplaceCardInViewWithoutRearrange(dummyCardView, receivedCard.BattleCardView);
					receivedCard.BattleCardView.GameObject.SetActive(value: true);
					receivedCard.BattleCardView.GameObject.GetComponent<CardTemplate>().CardNormalTemp.gameObject.SetActive(value: true);
				}
			}
			else if (isOpenDrawSkill || (_isOpen && isCardInDeck))
			{
				InheritedAddParameter(cardBeforeChangeParameter);
				receivedCard.BattleCardView.UpdateParameterView(cardBeforeChangeParameter.Atk, cardBeforeChangeParameter.Life, (receivedCard.FixedUseCost != -1) ? receivedCard.FixedUseCost : cost, receivedCard.BaseParameter.CardName, receivedCard.IsInplay, isRecovery: false, useNormalCost: true);
			}
			if (_networkBattleMgr.InstanceNetworkAgent != null && !_networkBattleMgr.IsBattleEnd)
			{
				_networkBattleMgr.InstanceNetworkAgent.StartCoroutine(WaitToReady(receivedCard, dummyCardView.GameObject));
			}
			else
			{
				Object.DestroyImmediate(dummyCardView.GameObject);
			}
		});
	}

	private IEnumerator WaitToReady(BattleCardBase receivedCard, GameObject dummyGameObject)
	{
		do
		{
			yield return null;
		}
		while (!CheckReadyFinishCard(receivedCard));
		Object.DestroyImmediate(dummyGameObject);
	}

	private bool CheckReadyFinishCard(BattleCardBase receivedCard)
	{
		if (receivedCard != null && receivedCard.IsTokenLoad)
		{
			return true;
		}
		return false;
	}

	protected void RemoveOpenCardRemoveAfterActionSkills(BattleCardBase receivedCard)
	{
		if (_fromState != NetworkBattleDefine.NetworkCardPlaceState.Hand || (!_toStateList.Contains(NetworkBattleDefine.NetworkCardPlaceState.Hand) && !_toStateList.Contains(NetworkBattleDefine.NetworkCardPlaceState.Field) && !_toStateList.Contains(NetworkBattleDefine.NetworkCardPlaceState.Banish)) || _originalDummyCard.CardId != receivedCard.CardId)
		{
			return;
		}
		IEnumerable<SkillBase> source = receivedCard.Skills.Where((SkillBase s) => s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessOpenCard) && s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessRemoveAfterAction) && s.OnSelfTurnEndStart != 0);
		if (source.Count() <= 0)
		{
			return;
		}
		List<int> originalDummyCardSkillIndexList = new List<int>();
		IEnumerable<SkillBase> source2 = _originalDummyCard.Skills.Where((SkillBase s) => s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessOpenCard) && s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessRemoveAfterAction) && s.OnSelfTurnEndStart != 0);
		for (int num = 0; num < source2.Count(); num++)
		{
			originalDummyCardSkillIndexList.Add(_originalDummyCard.NormalSkillBuildInfos.IndexOf(source2.ElementAt(num).SkillPrm.buildInfo));
		}
		List<SkillBase> list = source.Where((SkillBase s) => !originalDummyCardSkillIndexList.Contains(receivedCard.Skills.IndexOf(s))).ToList();
		for (int num2 = 0; num2 < list.Count(); num2++)
		{
			receivedCard.NormalSkills.Remove(list[num2]);
		}
	}
}
