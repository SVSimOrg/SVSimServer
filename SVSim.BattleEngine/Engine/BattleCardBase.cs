using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.Card.InnerOptions;
using Wizard.Battle.Resource;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public abstract class BattleCardBase : IReadOnlyBattleCardInfo, IBattleCardUniqueID
{
	public class BuildInfo
	{
		public GameObject GameObject;

		public int CardId;

		public BattlePlayerBase SelfBattlePlayer;

		public BattlePlayerBase OpponentBattlePlayer;

		public IBattlePlayerReadOnlyInfo SelfBattlePlayerReadOnlyInfo;

		public List<SkillCreator.SkillBuildInfo> NormalSkillBuildInfos;

		public List<SkillCreator.SkillBuildInfo> EvolveSkillBuildInfos;

		public bool IsPlayer;

		public int BattleCardIndex;

		public CardInnerOptionsBase InnerOptions;

		public BattleManagerBase BattleMgr;

		public IBattleResourceMgr ResourceMgr;

		public BuildInfo(GameObject _gameObject, int _cardId, BattlePlayerBase _selfBattlePlayer, BattlePlayerBase _opponentBattlePlayer, IBattlePlayerReadOnlyInfo _selfBattlePlayerReadOnlyInfo, List<SkillCreator.SkillBuildInfo> _normalSkillBuildInfos, List<SkillCreator.SkillBuildInfo> _evolveSkillBuildInfos, bool _isPlayer, int _battleCardIndex, CardInnerOptionsBase _innerOptions, BattleManagerBase _battleMgr, IBattleResourceMgr _resourceMgr)
		{
			GameObject = _gameObject;
			CardId = _cardId;
			SelfBattlePlayer = _selfBattlePlayer;
			OpponentBattlePlayer = _opponentBattlePlayer;
			SelfBattlePlayerReadOnlyInfo = _selfBattlePlayerReadOnlyInfo;
			NormalSkillBuildInfos = _normalSkillBuildInfos;
			EvolveSkillBuildInfos = _evolveSkillBuildInfos;
			IsPlayer = _isPlayer;
			BattleCardIndex = _battleCardIndex;
			InnerOptions = _innerOptions;
			BattleMgr = _battleMgr;
			ResourceMgr = _resourceMgr;
		}

		public BuildInfo VirtualClone(BattlePlayerBase virtualSelfBattlePlayer, BattlePlayerBase virtualOpponentBattlePlayer)
		{
			return new BuildInfo(null, CardId, virtualSelfBattlePlayer, virtualOpponentBattlePlayer, virtualSelfBattlePlayer, new List<SkillCreator.SkillBuildInfo>(NormalSkillBuildInfos), new List<SkillCreator.SkillBuildInfo>(EvolveSkillBuildInfos), IsPlayer, BattleCardIndex, InnerOptions.VirtualClone(), BattleMgr, ResourceMgr);
		}
	}

	public class DeathTypeInformation
	{
		public bool WhenDestroy;

		public bool DestroyedByKiller;

		public bool ChantDestroy;

		public bool MysteriesDestroy;

		public bool BanishDestroy;

		public bool BurialRite;

		public bool UseFusionIngredient;

		public bool UseFusionMetamorphoseIngredient;

		public bool LeaveByGetOn;

		public DeathTypeInformation()
		{
			WhenDestroy = false;
			DestroyedByKiller = false;
			ChantDestroy = false;
			MysteriesDestroy = false;
			BanishDestroy = false;
			BurialRite = false;
			UseFusionIngredient = false;
			UseFusionMetamorphoseIngredient = false;
			LeaveByGetOn = false;
		}

		public DeathTypeInformation Clone()
		{
			return (DeathTypeInformation)MemberwiseClone();
		}
	}

	public class ParameterChangeInformation
	{
		public int CurrentAtk;

		public int BaseAtk;

		public int CurrentHealth;

		public int MaxHealth;

		public int BaseHealth;

		public ParameterChangeInformation(int currentAtk, int baseAtk, int currentHealth, int maxHealth, int baseHealth)
		{
			CurrentAtk = currentAtk;
			BaseAtk = baseAtk;
			CurrentHealth = currentHealth;
			MaxHealth = maxHealth;
			BaseHealth = baseHealth;
		}
	}

	public class ItWasDamagedCounter
	{
		public int SelfTurnDamage { get; private set; }

		public int OpponentTurnDamage { get; private set; }

		public ItWasDamagedCounter()
		{
			Clear();
		}

		public ItWasDamagedCounter(int selfTurnDamage, int opponentTurnDamage)
		{
			SelfTurnDamage = selfTurnDamage;
			OpponentTurnDamage = opponentTurnDamage;
		}

		public void Clear()
		{
			SelfTurnDamage = 0;
			OpponentTurnDamage = 0;
		}

		public void AddDamageCount(bool selfTurn)
		{
			if (selfTurn)
			{
				SelfTurnDamage++;
			}
			else
			{
				OpponentTurnDamage++;
			}
		}

		public int GetDamageCount(bool selfTurn)
		{
			if (selfTurn)
			{
				return SelfTurnDamage;
			}
			return OpponentTurnDamage;
		}
	}

	private struct DamageClipping
	{
		public int ClippingMax;

		public int GiveCount;

		public DamageClipping(int clippingMax, int count)
		{
			ClippingMax = clippingMax;
			GiveCount = count;
		}
	}

	public struct TransformInformation
	{
		public BattleCardBase OriginalCard { get; private set; }

		public TransformType Type { get; private set; }

		public TransformInformation(TransformType type, BattleCardBase card)
		{
			Type = type;
			OriginalCard = card;
		}
	}

	public struct SkillActivationInfo
	{
		public long SkillId { get; private set; }

		public SkillBase Skill { get; private set; }

		public SkillActivationInfo(long skillId, SkillBase skill)
		{
			SkillId = skillId;
			Skill = skill;
		}
	}

	public enum CHECK_CONDITION_MUTATIONSKILL_TYPE
	{
		NONE,
		NOT_HAVE_MUTATION_SKILL,
		SELECT_ACCELERATE_SKILL_NOT_ACTIVE,
		CRYSTALLIZE_SKILL_ACTIVE,
		SELECT_CRYSTALLIZE_SKILL_NOT_ACTIVE,
		NOT_PLAY,
		PLAY
	}

	public enum TransformType
	{
		None,
		Accelerate,
		Crystallize,
		Choice,
		Metamorphose
	}

	public class DestroyedBySkillInfo
	{
		public enum DestroyedBySkillAbility
		{
			None,
			WhenPlay,
			Accelerate,
			Crystallize,
			WhenDestroy
		}

		public DestroyedBySkillAbility Ability { get; private set; }

		public int BaseCardId { get; private set; }

		public string Player { get; private set; }

		public DestroyedBySkillInfo(DestroyedBySkillAbility ability, int baseCardId, bool isDestroyedBySelf)
		{
			Ability = ability;
			BaseCardId = baseCardId;
			Player = (isDestroyedBySelf ? "me" : "op");
		}
	}

	public class BanishInfo
	{
		public enum BanishPlace
		{
			None,
			Hand,
			Field,
			Deck
		}

		public int Turn { get; private set; }

		public bool IsSelfTurn { get; private set; }

		public BanishPlace Place { get; private set; }

		public BanishInfo(int turn, bool isSelfTurn, BanishPlace place)
		{
			Turn = turn;
			IsSelfTurn = isSelfTurn;
			Place = place;
		}
	}

	public class AttackCountInfo
	{
		public Skill_attack_count Skill { get; private set; }

		public int Count { get; private set; }

		public AttackCountInfo(Skill_attack_count skill, int count)
		{
			Skill = skill;
			Count = count;
		}

		public virtual int CalcAttackCount(int baseAttackCount)
		{
			return baseAttackCount;
		}
	}

	public class SetAttackCountInfo : AttackCountInfo
	{
		public SetAttackCountInfo(Skill_attack_count skill, int count)
			: base(skill, count)
		{
		}

		public override int CalcAttackCount(int baseAttackCount)
		{
			return base.Count;
		}
	}

	public class AddAttackCountInfo : AttackCountInfo
	{
		public AddAttackCountInfo(Skill_attack_count skill, int count)
			: base(skill, count)
		{
		}

		public override int CalcAttackCount(int baseAttackCount)
		{
			return baseAttackCount + base.Count;
		}
	}

	public class AttackOpponentResult
	{
		public VfxBase attackVfx { get; private set; }

		public VfxBase damageVfx { get; private set; }

		public DamageResult damageResult { get; private set; }

		public AttackOpponentResult(VfxBase _attackVfx, VfxBase _damageVfx, DamageResult _damageResult)
		{
			attackVfx = _attackVfx;
			damageVfx = _damageVfx;
			damageResult = _damageResult;
		}
	}

	public struct DamageParam
	{
		public int Damage;

		public BattleCardBase OwnerCard { get; private set; }

		public DamageParam(int damage, BattleCardBase card, string damageType = "_OPT_NULL_", CardBasePrm.ClanType damageClan = CardBasePrm.ClanType.NONE)
		{
			List<DamageModifier> list = card.SkillApplyInformation.AddDamageList.Where((DamageModifier m) => m.IsEffective(damageType, damageClan, isUseClass: false)).ToList();
			list.AddRange(card.SelfBattlePlayer.Class.SkillApplyInformation.AddDamageList.Where((DamageModifier m) => m.IsEffective(damageType, damageClan, isUseClass: true)));
			list.Sort((DamageModifier a, DamageModifier b) => a.OrderCount - b.OrderCount);
			for (int num = 0; num < list.Count; num++)
			{
				damage = list[num].Calc(damage);
			}
			Damage = damage;
			OwnerCard = card;
		}
	}

	public class DamageResult
	{
		public VfxBase Vfx { get; private set; }

		public VfxBase PreDamageVfx { get; private set; }

		public VfxBase PostDamageVfx { get; private set; }

		public int DamageApplied { get; private set; }

		public int GainLife { get; private set; }

		public bool IsReflectedDamage { get; private set; }

		public DamageResult(VfxBase _vfx, int _damageApplied, int _gainLife, VfxBase _preDamageVfx = null, VfxBase _postDamageVfx = null, bool isReflectedDamage = false)
		{
			Vfx = _vfx;
			DamageApplied = _damageApplied;
			GainLife = _gainLife;
			IsReflectedDamage = isReflectedDamage;
			PreDamageVfx = ((_preDamageVfx == null) ? NullVfx.GetInstance() : _preDamageVfx);
			PostDamageVfx = ((_postDamageVfx == null) ? NullVfx.GetInstance() : _postDamageVfx);
		}
	}

	public struct HealParam
	{
		public int HealAmount;

		public HealParam(int healAmount, BattleCardBase owner, BattleCardBase target, bool applyModifier = true)
		{
			if (applyModifier)
			{
				List<HealModifier> list = owner.SelfBattlePlayer.Class.SkillApplyInformation.HealModifierList.ToList();
				list.AddRange(owner.OpponentBattlePlayer.Class.SkillApplyInformation.HealModifierList);
				list.Sort((HealModifier a, HealModifier b) => a.OrderCount - b.OrderCount);
				for (int num = 0; num < list.Count; num++)
				{
					healAmount = list[num].Calc(healAmount, owner, target);
				}
			}
			HealAmount = healAmount;
		}
	}

	public class HealResult
	{
		public int HealAmount { get; private set; }

		public VfxBase HealVfx { get; private set; }

		public VfxBase PrehealVfxVfx { get; private set; }

		public VfxBase PosthealVfxVfx { get; private set; }

		public HealResult(int healAmount, VfxBase _healVfx, VfxBase _prehealVfxVfx = null, VfxBase _posthealVfxVfx = null)
		{
			HealAmount = healAmount;
			HealVfx = _healVfx;
			PrehealVfxVfx = ((_prehealVfxVfx == null) ? NullVfx.GetInstance() : _prehealVfxVfx);
			PosthealVfxVfx = ((_posthealVfxVfx == null) ? NullVfx.GetInstance() : _posthealVfxVfx);
		}
	}

	public class CopySkillInfo
	{
		public VfxBase Vfx { get; private set; }

		public bool IsEvolutionSkill { get; private set; }

		public SkillBaseCopy NewCopySkill { get; private set; }

		public List<SkillBase> CopiedSkillList { get; private set; }

		public List<BuffInfo> AttachBuffs { get; private set; }

		public CopySkillInfo(VfxBase vfx, bool isEvolutionSkill, SkillBaseCopy copySkill, List<SkillBase> copiedSkillList, List<BuffInfo> buffs)
		{
			Vfx = vfx;
			IsEvolutionSkill = isEvolutionSkill;
			NewCopySkill = copySkill;
			CopiedSkillList = copiedSkillList;
			AttachBuffs = buffs;
		}
	}

	private BattleCardBase _finalMetamorphoseCard;

	protected BuildInfo _buildInfo;

	protected SkillCollectionBase _normalSkillCollection;

	protected SkillCollectionBase _evolveSkillCollection;

	private CardParameter _baseParameter;

	private CardParameter _evolveToOtherCardBaseParameter;

	public readonly IBattlePlayerReadOnlyInfo SelfBattlePlayerReadOnlyInfo;

	private bool _isOndraw;

	private int _skillActivatedCountWrapValue = -1;

	private int _skillActivatedCount;

	private int _normalIndividualId = -1;

	private int _evolutionIndividualId = -1;

	private List<CardBasePrm.TribeType> _tribeCache;

	private List<CardBasePrm.TribeInfo> _lastTribeInfo = new List<CardBasePrm.TribeInfo>();

	private int _playedCost = -1;

	private int _lastCost = -1;

	public readonly List<ICardCostModifier> CostModifierList;

	public List<AttackCountInfo> attackCountinfo;

	private static StringBuilder _extractedText = new StringBuilder(512);

	public BattleCardBase Card
	{
		get
		{
			if (MetamorphoseCard != null)
			{
				return MetamorphoseCard.Card;
			}
			return this;
		}
	}

	public virtual bool IsClass => false;

	public virtual bool IsUnit => false;

	public virtual bool IsSpell => false;

	public virtual bool IsField => false;

	public virtual bool IsChantField => false;

	public virtual bool IsSpecialSkill => false;

	public BattleCardBase LastDrawOpenCard { get; set; }

	public BattleCardBase MetamorphoseCard { get; set; }

	public BattleCardBase FinalMetamorphoseCard
	{
		get
		{
			if (_finalMetamorphoseCard == null && MetamorphoseCard != null)
			{
				_finalMetamorphoseCard = MetamorphoseCard;
			}
			while (_finalMetamorphoseCard != null && _finalMetamorphoseCard.MetamorphoseCard != null)
			{
				_finalMetamorphoseCard = _finalMetamorphoseCard.MetamorphoseCard;
			}
			return _finalMetamorphoseCard;
		}
	}

	public int PlayedTurn { get; protected set; }

	public DeathTypeInformation DeathTypeInfo { get; protected set; }

	public BuildInfo GetBuildInfo => _buildInfo;

	public int UpdateBuildInfoBeforeCardId { get; private set; } = -1;

	public string UpdateBuildInfoBeforeCardName { get; private set; } = string.Empty;

	public List<SkillCreator.SkillBuildInfo> NormalSkillBuildInfos => _buildInfo.NormalSkillBuildInfos;

	public List<SkillCreator.SkillBuildInfo> EvolveSkillBuildInfos => _buildInfo.EvolveSkillBuildInfos;

	public IEnumerable<BattleCardBase> GetCopiedCardList
	{
		get
		{
			List<SkillCreator.SkillBuildInfo> list = new List<SkillCreator.SkillBuildInfo>();
			list.AddRange(NormalSkillBuildInfos);
			list.AddRange(EvolveSkillBuildInfos);
			return (from b in list
				where b._previousSkillOwner != null
				select b._previousSkillOwner).Distinct();
		}
	}

	public List<BuffInfo> BuffInfoList { get; private set; }

	protected CardInnerOptionsBase InnerOptions => _buildInfo.InnerOptions;

	public IBattleResourceMgr ResourceMgr => _buildInfo.ResourceMgr;

	public IBattleCardView BattleCardView { get; protected set; }

	public virtual int Index => _buildInfo.BattleCardIndex;

	public BattlePlayerBase SelfBattlePlayer { get; protected set; }

	public BattlePlayerBase OpponentBattlePlayer { get; protected set; }

	public CardParameter BaseParameter
	{
		get
		{
			if (_evolveToOtherCardBaseParameter != null)
			{
				return _evolveToOtherCardBaseParameter;
			}
			return _baseParameter;
		}
		private set
		{
			_baseParameter = value;
		}
	}

	public SkillCollectionBase Skills { get; protected set; }

	public SkillCollectionBase NormalSkills => _normalSkillCollection;

	public SkillCollectionBase EvolutionSkills => _evolveSkillCollection;

	public TransformInformation TransformInfo { get; set; }

	public ISkillApplyInformation SkillApplyInformation { get; protected set; }

	public bool HasAnySkill { get; private set; }

	public bool IsTokenLoad { get; set; }

	public bool IsPlayer { get; private set; }

	public bool IsFirstTurn { get; protected set; }

	public bool IsOnMove { get; private set; }

	public bool IsSelfTurn => SelfBattlePlayer.IsSelfTurn;

	public virtual bool IsOnDraw
	{
		get
		{
			return _isOndraw;
		}
		private set
		{
			_isOndraw = value;
			if (value)
			{
				OnStartDraw.Call();
			}
		}
	}

	public Action OnStartDraw { get; set; }

	public virtual bool IsActionCard => false;

	public virtual bool IsEvolution => false;

	public virtual bool IsEvolvedOnWhenLeave => false;

	public bool IsEvolDrunkenness
	{
		get
		{
			if (IsFirstTurn && IsSummonDrunkenness)
			{
				return !SkillApplyInformation.IsQuick;
			}
			return false;
		}
	}

	public virtual bool IsCantAttackClass
	{
		get
		{
			if (!_buildInfo.BattleMgr.GameMgr.IsNewReplayBattle)
			{
				if (!SelfBattlePlayer.Class.IsCantAttackClass && !SkillApplyInformation.IsSkillCantAtkClass && !IsEvolDrunkenness)
				{
					if (IsFirstTurn && SkillApplyInformation.IsRush)
					{
						return !SkillApplyInformation.IsQuick;
					}
					return false;
				}
				return true;
			}
			return IsCantAttackClassOnReplay;
		}
	}

	public bool IsCantAttackClassOnReplay { get; set; }

	public bool IsSummonDrunkenness { get; set; }

	public bool IsPreviousTurnAttacked { get; set; }

	public bool IsSelectedDuringSelectingBurialRiteTarget { get; set; }

	public bool IsCantAttack
	{
		get
		{
			if (IsCantAttackClass)
			{
				return SkillApplyInformation.IsSkillCantAtkUnit;
			}
			return false;
		}
	}

	public List<int> ReplaySkillDescriptionValueList { get; set; } = new List<int>();

	public List<int> ReplayEvoSkillDescriptionValueList { get; set; } = new List<int>();

	public List<int> ReplayBuffDetailSkillDescriptionValueList { get; set; } = new List<int>();

	public List<int> ReplayBuffDetailEvoSkillDescriptionValueList { get; set; } = new List<int>();

	public virtual bool IsCantActivateFanfare => false;

	public int SpellChargeCount { get; protected set; }

	public int ChantCount => SkillApplyInformation.GetChantCount(BaseParameter.ChantCount);

	public int ExecutedFixedUseCostIndex { get; set; }

	public bool IsExecutedEarthRite { get; set; }

	public bool IsSkillLost { get; set; }

	public bool IsReanimate { get; set; }

	public ItWasDamagedCounter DamagedCounter { get; private set; }

	public bool HasSkillActivatedCountWrapValue => _skillActivatedCountWrapValue != -1;

	public int SkillActivatedCount
	{
		get
		{
			if (_skillActivatedCountWrapValue == -1)
			{
				return _skillActivatedCount;
			}
			int num;
			for (num = _skillActivatedCount; num > _skillActivatedCountWrapValue; num -= _skillActivatedCountWrapValue)
			{
			}
			return num;
		}
		protected set
		{
			_skillActivatedCount = value;
		}
	}

	public int ThisTurnSkillActivatedCount { get; protected set; }

	public int NormalIndividualId
	{
		get
		{
			if (_normalIndividualId == -1)
			{
				return _normalSkillCollection.GetIndividualId();
			}
			return _normalIndividualId;
		}
		set
		{
			_normalIndividualId = value;
		}
	}

	public int EvolutionIndividualId
	{
		get
		{
			if (_evolutionIndividualId == -1)
			{
				return _evolveSkillCollection.GetIndividualId();
			}
			return _evolutionIndividualId;
		}
		set
		{
			_evolutionIndividualId = value;
		}
	}

	public List<SkillActivationInfo> SkillActivationList { get; set; }

	public bool AlreadyInactiveSkillActivateCountBySimultaneousDestroyedCardList { get; private set; }

	public bool ActiveSkillActivateCountBySimultaneousDestroyedCardList { get; set; }

	public bool AlreadyInactiveSkillActivateCountBySimultaneousBuffingCards { get; private set; }

	public bool ActiveSkillActivateCountBySimultaneousBuffingCards { get; set; }

	public bool AlreadyInactiveSkillActivateCountBySimultaneousSummonedCard { get; private set; }

	public bool ActiveSkillActivateCountBySimultaneousSummonedCard { get; set; }

	public bool HasSpellCharge
	{
		get
		{
			if (IsClass)
			{
				return false;
			}
			if (Skills.Any((SkillBase s) => s.OnWhenSpellChargeStart != 0))
			{
				return true;
			}
			if (Skills.Any((SkillBase s) => Regex.IsMatch(s.CallCountText, "CHARGE_COUNT")))
			{
				return true;
			}
			if (Skills.Any((SkillBase s) => s.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter t) => t.Lhs.Contains("charge_count"))))
			{
				return true;
			}
			if (BaseParameter.SkillTarget.Contains("self.charge_count"))
			{
				return true;
			}
			string oddChargeCountText = SkillFilterCreator.ContentKeyword.odd_charge_count.ToStringCustom();
			string evenChargeCountText = SkillFilterCreator.ContentKeyword.even_charge_count.ToStringCustom();
			return Skills.Any((SkillBase s) => s.IsRefVariable("CHARGE_COUNT") || s.IsRefVariable(oddChargeCountText) || s.IsRefVariable(evenChargeCountText));
		}
	}

	public bool IsFusionable
	{
		get
		{
			SkillBase skillBase = Skills.FirstOrDefault((SkillBase s) => s is Skill_fusion);
			if (skillBase == null)
			{
				return false;
			}
			if (IsAlreadyFusionInThisTurn)
			{
				return false;
			}
			BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(SelfBattlePlayer, OpponentBattlePlayer);
			SkillConditionCheckerOption option = new SkillConditionCheckerOption();
			if (skillBase.GetSelectableCards(playerInfoPair, option).Count() == 0)
			{
				return false;
			}
			if (!skillBase.CheckCondition(playerInfoPair, option, isPrePlay: true))
			{
				return false;
			}
			return true;
		}
	}

	public bool IsAlreadyFusionInThisTurn => SkillApplyInformation.FusionIngredients.Any((FusionIngredientInfo c) => c.FusionTurn == SelfBattlePlayer.Turn);

	public List<BattleCardBase> FusionIngredients => SkillApplyInformation.FusionIngredients.Select((FusionIngredientInfo f) => f.Card).ToList();

	public int FusionedTurn { get; protected set; }

	public List<BattleCardBase> GetOnCards => SkillApplyInformation.GetOnCards.ToList();

	public bool IsChoiceEvolutionCard => CardId / 1000000 == 910;

	public List<BattleCardBase> GetOffCards { get; set; }

	public bool CanPlayAsChoiceBraveCard
	{
		get
		{
			if (CardMaster.IsChoiceBraveCardCheck(CardId) && Cost <= SelfBattlePlayer.Bp)
			{
				if (Skills.Any((SkillBase s) => s.IsUserSelectType))
				{
					return Skills.CheckWhenPlaySelectTargetSkillCondition;
				}
				return true;
			}
			return false;
		}
	}

	public virtual CHECK_CONDITION_MUTATIONSKILL_TYPE IsCheckActiveMutationSkill
	{
		get
		{
			using (IEnumerator<SkillBase> enumerator = Skills.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					if (!(enumerator.Current is Skill_pp_fixeduse skill_pp_fixeduse))
					{
						return CHECK_CONDITION_MUTATIONSKILL_TYPE.NOT_HAVE_MUTATION_SKILL;
					}
					if (skill_pp_fixeduse.IsMutationFixedUseCost)
					{
						Skill_transform accelerateOrCrystallizeTransformSkill = GetAccelerateOrCrystallizeTransformSkill();
						if (accelerateOrCrystallizeTransformSkill != null)
						{
							BattleCardBase battleCardBase = SelfBattlePlayer.BattleMgr.CreateTransformCardRegisterVfx(this, accelerateOrCrystallizeTransformSkill.TransformId, accelerateOrCrystallizeTransformSkill.SkillPrm.ownerCard.IsPlayer);
							if (battleCardBase.Skills.Any((SkillBase t) => t.IsUserSelectType))
							{
								IEnumerable<SkillBase> selectTypeSkill = battleCardBase.GetSelectTypeSkill();
								if (selectTypeSkill != null && selectTypeSkill.Count() > 0 && (battleCardBase.BaseParameter.CharType != CardBasePrm.CharaType.SPELL || (battleCardBase as SpellBattleCard).IsSelectableSkillTarget()))
								{
									return (battleCardBase.BaseParameter.CharType == CardBasePrm.CharaType.SPELL) ? CHECK_CONDITION_MUTATIONSKILL_TYPE.PLAY : CHECK_CONDITION_MUTATIONSKILL_TYPE.CRYSTALLIZE_SKILL_ACTIVE;
								}
								return (battleCardBase.BaseParameter.CharType == CardBasePrm.CharaType.SPELL) ? CHECK_CONDITION_MUTATIONSKILL_TYPE.SELECT_ACCELERATE_SKILL_NOT_ACTIVE : CHECK_CONDITION_MUTATIONSKILL_TYPE.SELECT_CRYSTALLIZE_SKILL_NOT_ACTIVE;
							}
							return (battleCardBase.BaseParameter.CharType == CardBasePrm.CharaType.SPELL) ? CHECK_CONDITION_MUTATIONSKILL_TYPE.PLAY : CHECK_CONDITION_MUTATIONSKILL_TYPE.CRYSTALLIZE_SKILL_ACTIVE;
						}
					}
					return CHECK_CONDITION_MUTATIONSKILL_TYPE.NOT_PLAY;
				}
			}
			return CHECK_CONDITION_MUTATIONSKILL_TYPE.NONE;
		}
	}

	public virtual bool BaseMovable => Movable();

	public virtual bool IsInHand => SelfBattlePlayer.HandCardList.Contains(this);

	public virtual bool IsInDeck => SelfBattlePlayer.DeckCardList.Contains(this);

	public virtual bool IsInplay => SelfBattlePlayer.ClassAndInPlayCardList.Contains(this);

	public virtual bool IsInCemetery => SelfBattlePlayer.CemeteryList.Contains(this);

	public virtual bool IsInNecromanceZone => SelfBattlePlayer.NecromanceZoneList.Contains(this);

	public virtual bool IsFusionIngredient => SelfBattlePlayer.FusionIngredientList.Contains(this);

	public bool IsDestroyedByKiller { get; protected set; }

	public bool IsDestroyedBySkill { get; protected set; }

	public virtual bool IsDead
	{
		get
		{
			if (!IsLifeZeroDead && !IsDestroyedByKiller)
			{
				return IsDestroyedBySkill;
			}
			return true;
		}
	}

	public virtual bool IsLifeZeroDead => Life <= 0;

	public int DestroyedTurn { get; private set; }

	public bool IsDestroySelfTurn { get; private set; }

	public List<DestroyedBySkillInfo> DestroyedBySkillList { get; private set; }

	public BanishInfo BanishedInfo { get; private set; }

	public SkillBase DiscardedSkill { get; private set; }

	public SkillBase ReturnedSkill { get; private set; }

	public bool HasDeckSelfSkill => Skills.Any((SkillBase s) => s.IsDeckSelfSkill);

	public int AttackableCount { get; set; }

	public virtual bool Attackable
	{
		get
		{
			if (!_buildInfo.BattleMgr.GameMgr.IsNewReplayBattle)
			{
				if (AttackableCount <= 0 || (IsSummonDrunkenness && (!IsSummonDrunkenness || !IsEvolution)) || IsCantAttack)
				{
					if (SkillApplyInformation.IsInfiniteAttack && !IsSummonDrunkenness)
					{
						return !IsCantAttack;
					}
					return false;
				}
				return true;
			}
			return AttackableOnReplay;
		}
	}

	public bool AttackableOnReplay { get; set; }

	public virtual List<CardBasePrm.TribeType> Tribe
	{
		get
		{
			if (SkillApplyInformation == null || SkillApplyInformation.TribeSkinInfo.Count == 0)
			{
				return BaseParameter.Tribe;
			}
			if (_tribeCache != null && _lastTribeInfo != null && _lastTribeInfo.SequenceEqual(SkillApplyInformation.TribeSkinInfo))
			{
				return _tribeCache;
			}
			List<CardBasePrm.TribeType> list = new List<CardBasePrm.TribeType>(BaseParameter.Tribe);
			for (int i = 0; i < SkillApplyInformation.TribeSkinInfo.Count(); i++)
			{
				if (SkillApplyInformation.TribeSkinInfo[i].TribeTypeList != null)
				{
					switch (SkillApplyInformation.TribeSkinInfo[i].ChangeType)
					{
					case CardBasePrm.TribeChangeType.CHANGE:
						list = SkillApplyInformation.TribeSkinInfo[i].TribeTypeList;
						break;
					case CardBasePrm.TribeChangeType.ADD:
						list.AddRange(SkillApplyInformation.TribeSkinInfo[i].TribeTypeList);
						break;
					default:
						list = SkillApplyInformation.TribeSkinInfo[i].TribeTypeList;
						break;
					}
				}
			}
			_tribeCache = list.Distinct().ToList();
			if (_tribeCache.Count >= 2 && _tribeCache.Contains(CardBasePrm.TribeType.ALL))
			{
				_tribeCache.Remove(CardBasePrm.TribeType.ALL);
			}
			_lastTribeInfo.Clear();
			_lastTribeInfo.AddRange(SkillApplyInformation.TribeSkinInfo);
			return _tribeCache;
		}
	}

	public virtual CardBasePrm.ClanType Clan
	{
		get
		{
			if (SkillApplyInformation.ClanSkinInfo.Count <= 0)
			{
				return BaseParameter.Clan;
			}
			return SkillApplyInformation.ClanSkinInfo.Last();
		}
	}

	public int CardId => _buildInfo.CardId;

	public int Cost
	{
		get
		{
			int num = BaseParameter.Cost;
			if (IsChoiceBraveSkillCard)
			{
				return num;
			}
			for (int i = 0; i < CostModifierList.Count; i++)
			{
				ICardCostModifier cardCostModifier = CostModifierList[i];
				if (!(cardCostModifier is CostHalfModifier))
				{
					num = cardCostModifier.CalcCost(num);
				}
			}
			for (int j = 0; j < CostModifierList.Count; j++)
			{
				ICardCostModifier cardCostModifier2 = CostModifierList[j];
				if (cardCostModifier2 is CostHalfModifier)
				{
					num = cardCostModifier2.CalcCost(num);
				}
			}
			return Math.Max(0, num);
		}
	}

	public int BaseCost => BaseParameter.Cost;

	public int Atk => SkillApplyInformation.GetAtk();

	public int BaseAtk
	{
		get
		{
			if (!IsEvolution)
			{
				return BaseParameter.Atk;
			}
			return BaseParameter.EvoAtk;
		}
	}

	public int Life => SkillApplyInformation.GetLife();

	public int MaxLife => SkillApplyInformation.GetMaxLife();

	public virtual int BaseMaxLife
	{
		get
		{
			if (!IsEvolution)
			{
				return BaseParameter.Life;
			}
			return BaseParameter.EvoLife;
		}
	}

	public int PlayedCost => _playedCost;

	public int LastCost => _lastCost;

	public int MaxAttackableCount
	{
		get
		{
			int num = 1;
			for (int i = 0; i < attackCountinfo.Count; i++)
			{
				num = attackCountinfo[i].CalcAttackCount(num);
			}
			return num;
		}
	}

	public DamageParam DamageCalculationAtkTypeAttack => new DamageParam(SkillApplyInformation.IsAttackByLifeTypeAttack ? Life : Atk, this, SkillFilterCreator.ContentKeyword.unit.ToString(), Clan);

	public DamageParam DamageCalculationAtkTypeBeAttacked => new DamageParam(SkillApplyInformation.IsAttackByLifeTypeBeAttacked ? Life : Atk, this, SkillFilterCreator.ContentKeyword.unit.ToString(), Clan);

	public bool AreCanPlayConditionsFulfilled => this.OnCheckCanPlay.GetAllFuncCallResults().All((bool condition) => condition);

	public bool AreCanAttackConditionsFulfilled => this.OnCheckCanAttack.GetAllFuncCallResults().All((bool condition) => condition);

	public int FixedUseCost => CalcFixedUseCost(SelfBattlePlayer.Pp);

	public List<int> UseCostList
	{
		get
		{
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			int pp = SelfBattlePlayer.Pp;
			for (int i = 0; i < Skills.Count(); i++)
			{
				if (Skills.ElementAt(i) is Skill_pp_fixeduse skill_pp_fixeduse)
				{
					if (skill_pp_fixeduse.IsAccelerateOrCrystallize)
					{
						list2.Add(skill_pp_fixeduse._fixedUsePP);
					}
					else
					{
						list.Add(skill_pp_fixeduse._fixedUsePP);
					}
				}
			}
			List<int> list3 = new List<int>();
			if (!list.Any() && !list2.Any())
			{
				return list3;
			}
			if (IsSelectedDuringSelectingBurialRiteTarget)
			{
				return list3;
			}
			if (!IsCantActivateFanfare)
			{
				list3.AddRange(list.Where((int c) => c <= pp).Reverse());
			}
			if ((!list.Any((int c) => c <= Cost) || IsCantActivateFanfare) && Cost <= pp)
			{
				list3.Add(Cost);
			}
			list3.AddRange(list2.Where((int c) => pp >= c && c < Cost).Reverse());
			list3.AddRange(list2.Where((int c) => pp < c && c < Cost));
			if (!list.Any((int c) => c <= Cost) && pp < Cost)
			{
				list3.Add(Cost);
			}
			if (!IsCantActivateFanfare)
			{
				list3.AddRange(list.Where((int c) => pp < c));
			}
			list3.AddRange(list2.Where((int c) => Cost <= c));
			if (list.Any((int c) => c <= Cost))
			{
				list3.Add(Cost);
			}
			if (IsCantActivateFanfare)
			{
				list3.AddRange(list);
			}
			return list3;
		}
	}

	public bool HasSkillFixedUseCost => FixedUseCost != -1;

	public bool HasSkillAccelerate => Skills.Any((SkillBase s) => s.OnWhenAccelerate != 0);

	public bool HasSkillCrystallize => Skills.Any((SkillBase s) => s.OnWhenCrystallize != 0);

	public bool HasSkillEnhance => Skills.Any((SkillBase s) => s.IsEnhance());

	public bool HasSkillDestroyWhiteRitual => HasSkillDestroyTribe(CardBasePrm.TribeType.WHITE_RITUAL);

	public bool HasSkillStackWhiteRitual => Skills.Any((SkillBase s) => s is Skill_stack_white_ritual);

	public bool HasSkillBurialRite
	{
		get
		{
			if (!NormalSkills.Any((SkillBase s) => s.IsBurialRite))
			{
				return EvolutionSkills.Any((SkillBase s) => s.IsBurialRite);
			}
			return true;
		}
	}

	public bool HasSkillNecromance { get; private set; }

	public bool HasSkillWhenDestroy
	{
		get
		{
			if (IsInDeck)
			{
				if (!NormalSkills.Any((SkillBase s) => s.IsWhenDestroySkill))
				{
					return EvolutionSkills.Any((SkillBase s) => s.IsWhenDestroySkill);
				}
				return true;
			}
			return Skills.Any((SkillBase s) => s.IsWhenDestroySkill);
		}
	}

	public bool HasSkillReanimate
	{
		get
		{
			if (!NormalSkills.Any((SkillBase s) => s is Skill_summon_token && (s as Skill_summon_token).IsReanimate))
			{
				return EvolutionSkills.Any((SkillBase s) => s is Skill_summon_token && (s as Skill_summon_token).IsReanimate);
			}
			return true;
		}
	}

	public bool HasSkillFusion => NormalSkills.Any((SkillBase s) => s is Skill_fusion);

	public bool HasWhenAttack => Skills.Any((SkillBase s) => s.IsBeforAttackSkill);

	public bool HasWhenFight => Skills.Any((SkillBase s) => s.IsWhenFightSkill);

	public bool IsChoiceBraveSkillCard { get; set; }

	public bool HasSkillWhenEvolve => EvolutionSkills.Any((SkillBase s) => s.IsWhenEvolveSkill);

	public bool HasUnionBurst
	{
		get
		{
			string unionBurstString = SkillFilterCreator.ContentKeyword.union_burst_count.ToString();
			if (Skills.Any((SkillBase s) => s.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter t) => t.Lhs.Contains(unionBurstString))))
			{
				return true;
			}
			return false;
		}
	}

	public bool HasSkyboundArt
	{
		get
		{
			string skyboundArtString = SkillFilterCreator.ContentKeyword.skybound_art_count.ToString();
			return Skills.Any((SkillBase s) => s.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter t) => t.Lhs.Contains(skyboundArtString)));
		}
	}

	public bool HasSuperSkyboundArt
	{
		get
		{
			string superSkyboundArtString = SkillFilterCreator.ContentKeyword.super_skybound_art_count.ToString();
			return Skills.Any((SkillBase s) => s.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter t) => t.Lhs.Contains(superSkyboundArtString)));
		}
	}

	public int DrawTurn { get; set; }

	public bool IsHaveBurialRiteJudgeBothFlag
	{
		get
		{
			bool flag = false;
			bool flag2 = false;
			foreach (SkillBase skill in Skills)
			{
				List<SkillConditionBurialRite> list = skill.ConditionFilterCollection.ConditionCheckerFilterList.FindAll((ISkillConditionChecker x) => x is SkillConditionBurialRite).ConvertAll((ISkillConditionChecker x) => x as SkillConditionBurialRite);
				if (list == null)
				{
					continue;
				}
				if (!flag)
				{
					flag = list.Find((SkillConditionBurialRite x) => x.judgeFlg) != null;
				}
				if (!flag2)
				{
					flag2 = list.Find((SkillConditionBurialRite x) => !x.judgeFlg) != null;
				}
			}
			return flag && flag2;
		}
	}

	public bool IsBuffDetail
	{
		get
		{
			if (IsShowBuffDetail || IsRecordingBuffDetail)
			{
				return !IsRecordingExceptBuffDetail;
			}
			return false;
		}
	}

	public bool IsShowBuffDetail { get; set; }

	public bool IsRecordingBuffDetail { get; set; }

	public bool IsRecordingExceptBuffDetail { get; set; }

	public bool IsRecordingFusionInfo { get; set; }

	public event Func<bool, SkillProcessor, VfxBase> OnRemoveFromInPlayAfterOneTime;

	public event Func<SkillProcessor, VfxBase> OnBeforeEvolve;

	public event Action<bool> OnEvolveEvent;

	public event Func<VfxBase> OnPlay;

	public event Func<VfxBase> OnFinishWhenPlaySkill;

	public event Func<BattleCardBase, SkillProcessor, VfxBase> OnDestroy;

	public event Func<BattleCardBase, SkillProcessor, VfxBase> OnBanish;

	public event Func<BattleCardBase, SkillProcessor, VfxBase> OnReturnCard;

	public event Func<BattleCardBase, SkillProcessor, VfxBase> OnMetamorphose;

	public event Func<BattleCardBase, SkillProcessor, VfxBase> OnGetOn;

	public event Action<SkillBase, ICardCostModifier> OnAddCostState;

	public event Action<SkillBase, ICardCostModifier> OnRemoveCostState;

	public event Func<VfxBase> OnAfterAddDamage;

	public event Action<BattleCardBase, SkillBase> OnAttachSkill;

	public event Action<BattleCardBase> OnCopySkillComplete;

	public event Func<SkillBase, SkillProcessor, BattleCardBase, VfxBase> OnLoseSkillOneTime;

	public event Func<SkillProcessor, VfxBase> OnDamageAfter;

	public event Func<SkillProcessor, VfxBase> OnGiveDamage;

	public event Func<SkillProcessor, VfxBase> OnReflectionAfter;

	public event Action OnResetCardParameter;

	public event Action<List<BattleCardBase>> OnFusionEvent;

	public event Action OnTurnStart;

	public event Func<bool> OnCheckCanPlay;

	public event Func<bool> OnCheckCanAttack;

	public void SetPlayedTurnNow()
	{
		PlayedTurn = SelfBattlePlayer.Turn;
	}

	private void ResetUpdateBuildInfo()
	{
		UpdateBuildInfoBeforeCardId = -1;
		UpdateBuildInfoBeforeCardName = string.Empty;
	}

	public void UpdateBuildInfoAndSkillCollection(int cardId, bool isFoil, bool isNotUpdateAtkLife = false)
	{
		UpdateBuildInfoBeforeCardId = BaseParameter.NormalCardId;
		UpdateBuildInfoBeforeCardName = BaseParameter.CardName;
		SkillCreator.CardSkillsBuildInfo cardSkillsBuildInfo = SkillCreator.CreateBuildInfo(CardMaster.GetInstanceForBattle().GetCardParameterFromId(cardId));
		_buildInfo = new BuildInfo(_buildInfo.GameObject, isFoil ? (cardId + 1) : cardId, _buildInfo.SelfBattlePlayer, _buildInfo.OpponentBattlePlayer, _buildInfo.SelfBattlePlayer, cardSkillsBuildInfo.normalSkillBuildInfos, cardSkillsBuildInfo.evolveSkillBuildInfos, _buildInfo.IsPlayer, _buildInfo.BattleCardIndex, _buildInfo.InnerOptions, _buildInfo.BattleMgr, _buildInfo.ResourceMgr);
		int evoAtk = BaseParameter.EvoAtk;
		int evoLife = BaseParameter.EvoLife;
		_evolveToOtherCardBaseParameter = CardMaster.GetInstanceForBattle().GetCardParameterFromId(_buildInfo.CardId).Clone();
		InitSkillCollection();
		if (isNotUpdateAtkLife)
		{
			_evolveToOtherCardBaseParameter.UpdateEvoAtkLife(evoAtk, evoLife);
		}
	}

	public void ResetChoiceEvolutionCardBuildInfo()
	{
		UpdateBuildInfoAndSkillCollection(BaseParameter.BaseCardId, BaseParameter.IsFoil);
		if (!SelfBattlePlayer.BattleMgr.IsRecovery || IsPlayer)
		{
			BattleCardView.CardTemplate.NormalNameLabelTemp.text = BaseParameter.CardName;
			Global.SetRepositionNameLabel(BattleCardView.CardTemplate.NormalNameLabelTemp, BaseParameter.CardName, is2D: false);
			BattleCardView.InitializeVoiceInfo(CardId);
			for (int i = 0; i < NormalSkills.Count(); i++)
			{
				NormalSkills.ElementAt(i).SetInductionVoiceIndex();
			}
			for (int j = 0; j < EvolutionSkills.Count(); j++)
			{
				EvolutionSkills.ElementAt(j).SetInductionVoiceIndex();
			}
		}
	}

	public virtual string SkillDescription(BattlePlayerBase.SideLogInfo sideLogInfo = null, bool isSkipOption = false, BuffInfo buff = null, string divergenceId = "", List<int> skillDescriptionValueList = null, List<int> sideLogDescriptionValueList = null)
	{
		return ConvertSkillDescription(BaseParameter.SkillDescription, sideLogInfo, isSkipOption, buff, divergenceId, skillDescriptionValueList, (IsBuffDetail && sideLogInfo == null && ReplayBuffDetailSkillDescriptionValueList.Count > 0) ? ReplayBuffDetailSkillDescriptionValueList : ((sideLogDescriptionValueList != null) ? sideLogDescriptionValueList : ReplaySkillDescriptionValueList));
	}

	public virtual string EvoSkillDescription(BattlePlayerBase.SideLogInfo sideLogInfo = null, bool isSkipOption = false, BuffInfo buff = null, string divergenceId = "", List<int> skillDescriptionValueList = null, List<int> sideLogDescriptionValueList = null)
	{
		return ConvertSkillDescription(BaseParameter.EvoSkillDescription, sideLogInfo, isSkipOption, buff, divergenceId, skillDescriptionValueList, (IsBuffDetail && sideLogInfo == null && ReplayBuffDetailEvoSkillDescriptionValueList.Count > 0) ? ReplayBuffDetailEvoSkillDescriptionValueList : ((sideLogDescriptionValueList != null) ? sideLogDescriptionValueList : ReplayEvoSkillDescriptionValueList));
	}

	public virtual bool CantBeFocusedAttack(BattleCardBase attackCard)
	{
		if (SkillApplyInformation.IsSneak)
		{
			return true;
		}
		if (SkillApplyInformation.NotBeAttackedInfoList.Any((NotBeAttackedInfo s) => !s.CheckAttacked(attackCard)))
		{
			return true;
		}
		return false;
	}

	public bool CanEvolution(bool isSkill, bool isSelfBattlePlayer)
	{
		BattlePlayerBase battlePlayerBase = (isSelfBattlePlayer ? SelfBattlePlayer : OpponentBattlePlayer);
		if (isSkill)
		{
			if (!IsEvolution)
			{
				return true;
			}
		}
		else if (battlePlayerBase.EvolveWaitTurnCount <= 0 && battlePlayerBase.NowTurnEvol && (battlePlayerBase.CurrentEpCount - SkillApplyInformation.GetEp() >= 0 || battlePlayerBase.CheckNotConsumeEpCard(this)) && !IsEvolution && !SkillApplyInformation.CantEvolutionList.Any((int f) => (f & Skill_cant_evolution.BIT_FLAG_EPUSE) != 0))
		{
			return true;
		}
		return false;
	}

	public void IncrementSkillActivatedCount()
	{
		SkillActivatedCount++;
		ThisTurnSkillActivatedCount++;
	}

	public void SetSkillActivatedCount(int value)
	{
		SkillActivatedCount = value;
	}

	public void SetSkillActivatedCountWrapValue(int value)
	{
		_skillActivatedCountWrapValue = value;
	}

	public void ResetSkillActivateCountBySimultaneousDestroyedCardList()
	{
		ActiveSkillActivateCountBySimultaneousDestroyedCardList = false;
		AlreadyInactiveSkillActivateCountBySimultaneousDestroyedCardList = false;
	}

	public void InactiveSkillActivateCountBySimultaneousDestroyedCardList()
	{
		ActiveSkillActivateCountBySimultaneousDestroyedCardList = false;
		AlreadyInactiveSkillActivateCountBySimultaneousDestroyedCardList = true;
	}

	public void ResetSkillActivateCountBySimultaneousBuffingCards()
	{
		ActiveSkillActivateCountBySimultaneousBuffingCards = false;
		AlreadyInactiveSkillActivateCountBySimultaneousBuffingCards = false;
	}

	public void InactiveSkillActivateCountBySimultaneousBuffingCards()
	{
		ActiveSkillActivateCountBySimultaneousBuffingCards = false;
		AlreadyInactiveSkillActivateCountBySimultaneousBuffingCards = true;
	}

	public void ResetSkillActivateCountBySimultaneousSummonedCard()
	{
		ActiveSkillActivateCountBySimultaneousSummonedCard = false;
		AlreadyInactiveSkillActivateCountBySimultaneousSummonedCard = false;
	}

	public void InactiveSkillActivateCountBySimultaneousSummonedCard()
	{
		ActiveSkillActivateCountBySimultaneousSummonedCard = false;
		AlreadyInactiveSkillActivateCountBySimultaneousSummonedCard = true;
	}

	protected virtual bool GetIsMovableOnView()
	{
		if (IsOnDraw)
		{
			return false;
		}
		return Movable(isCheckOnDraw: false);
	}

	public virtual bool Movable(bool isCheckOnDraw = true, bool isSkipSelecting = false, CHECK_CONDITION_MUTATIONSKILL_TYPE type = CHECK_CONDITION_MUTATIONSKILL_TYPE.NONE, bool isRecording = false)
	{
		if (SelfBattlePlayer.Class.SkillApplyInformation.IsCantPlay(this, type))
		{
			return false;
		}
		if (!SelfBattlePlayer.HandCardList.Contains(this))
		{
			return false;
		}
		if (SelfBattlePlayer.Pp < Cost && FixedUseCost == -1)
		{
			return false;
		}
		if (!IsSelfTurn)
		{
			return false;
		}
		if (SelfBattlePlayer.Class.IsDead || OpponentBattlePlayer.Class.IsDead)
		{
			return false;
		}
		if (!InnerOptions.CheckMovable(SelfBattlePlayer.BattleView, BattleCardView, isCheckOnDraw && IsOnDraw, isSkipSelecting, isRecording))
		{
			return false;
		}
		if (!SelfBattlePlayer.IsPlayer && _buildInfo.BattleMgr.GameMgr.IsAdminWatch)
		{
			if (isCheckOnDraw && IsOnDraw)
			{
				return false;
			}
			if (BattleCardView._hasCardEnteredPlayQueue)
			{
				return false;
			}
			if (SelfBattlePlayer.BattleView.IsSelecting && !isSkipSelecting)
			{
				return false;
			}
		}
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(SelfBattlePlayer, OpponentBattlePlayer);
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		if (Skills.Any((SkillBase s) => s is Skill_can_play_self && !s.CheckCondition(playerInfoPair, checkerOption, isPrePlay: true)))
		{
			return false;
		}
		return true;
	}

	public Skill_transform GetAccelerateOrCrystallizeTransformSkill()
	{
		BattlePlayerReadOnlyInfoPair pair = new BattlePlayerReadOnlyInfoPair(SelfBattlePlayer, OpponentBattlePlayer);
		SkillConditionCheckerOption option = new SkillConditionCheckerOption();
		for (int i = 0; i < Skills.Count(); i++)
		{
			if (Skills.ElementAt(i) is Skill_pp_fixeduse { IsAccelerateOrCrystallize: false } skill_pp_fixeduse && !IsCantActivateFanfare && skill_pp_fixeduse.CheckCondition(pair, option, isPrePlay: true))
			{
				return null;
			}
		}
		return (Skill_transform)Skills.LastOrDefault((SkillBase s) => (s.OnWhenAccelerate != 0 || s.OnWhenCrystallize != 0) && s is Skill_transform && s.CheckCondition(pair, option, isPrePlay: true));
	}

	public bool IsMutationMovable(CHECK_CONDITION_MUTATIONSKILL_TYPE type)
	{
		if (type == CHECK_CONDITION_MUTATIONSKILL_TYPE.SELECT_ACCELERATE_SKILL_NOT_ACTIVE)
		{
			return false;
		}
		if (SelfBattlePlayer.InPlayCards.Count() >= 5)
		{
			switch (type)
			{
			case CHECK_CONDITION_MUTATIONSKILL_TYPE.CRYSTALLIZE_SKILL_ACTIVE:
			case CHECK_CONDITION_MUTATIONSKILL_TYPE.SELECT_CRYSTALLIZE_SKILL_NOT_ACTIVE:
				return false;
			default:
				return false;
			case CHECK_CONDITION_MUTATIONSKILL_TYPE.NONE:
			case CHECK_CONDITION_MUTATIONSKILL_TYPE.PLAY:
				break;
			}
		}
		return true;
	}

	public void SetDestroyedBySkillList(SkillBase skill)
	{
		DestroyedBySkillList = new List<DestroyedBySkillInfo>();
		if (skill == null)
		{
			return;
		}
		bool isDestroyedBySelf = skill.SkillPrm.ownerCard.SelfBattlePlayer.IsPlayer == SelfBattlePlayer.IsPlayer;
		if (skill.IsWhenPlaySkill)
		{
			DestroyedBySkillList.Add(new DestroyedBySkillInfo(DestroyedBySkillInfo.DestroyedBySkillAbility.WhenPlay, skill.SkillPrm.ownerCard.BaseParameter.BaseCardId, isDestroyedBySelf));
			TransformInformation transformInfo = skill.SkillPrm.ownerCard.TransformInfo;
			switch (transformInfo.Type)
			{
			case TransformType.Accelerate:
				DestroyedBySkillList.Add(new DestroyedBySkillInfo(DestroyedBySkillInfo.DestroyedBySkillAbility.Accelerate, transformInfo.OriginalCard.BaseParameter.BaseCardId, isDestroyedBySelf));
				break;
			case TransformType.Crystallize:
				DestroyedBySkillList.Add(new DestroyedBySkillInfo(DestroyedBySkillInfo.DestroyedBySkillAbility.Crystallize, transformInfo.OriginalCard.BaseParameter.BaseCardId, isDestroyedBySelf));
				break;
			}
		}
		else if (skill.IsWhenDestroySkill)
		{
			DestroyedBySkillList.Add(new DestroyedBySkillInfo(DestroyedBySkillInfo.DestroyedBySkillAbility.WhenDestroy, skill.SkillPrm.ownerCard.BaseParameter.BaseCardId, isDestroyedBySelf));
		}
		else
		{
			DestroyedBySkillList.Add(new DestroyedBySkillInfo(DestroyedBySkillInfo.DestroyedBySkillAbility.None, skill.SkillPrm.ownerCard.BaseParameter.BaseCardId, isDestroyedBySelf));
		}
	}

	public void SetBanishedInfo(BanishInfo.BanishPlace place)
	{
		BattlePlayerBase battlePlayerBase = (SelfBattlePlayer.IsSelfTurn ? SelfBattlePlayer : OpponentBattlePlayer);
		BanishedInfo = new BanishInfo(battlePlayerBase.Turn, battlePlayerBase.IsPlayer, place);
	}

	public void SetDiscardedSkill(SkillBase discardedSkill)
	{
		DiscardedSkill = discardedSkill;
	}

	public void SetReturnedSkill(SkillBase returnedSkill)
	{
		ReturnedSkill = returnedSkill;
	}

	public bool IsTribe(CardBasePrm.TribeType tribe)
	{
		if (Tribe != null)
		{
			return Tribe.Contains(tribe);
		}
		return false;
	}

	public bool HasMoreDamageThan(BattleCardBase other)
	{
		return SkillApplyInformation.HasMoreDamageThan(other.SkillApplyInformation);
	}

	protected void ResetPlayedCost()
	{
		_playedCost = -1;
	}

	protected void ResetLastCost()
	{
		_lastCost = -1;
	}

	public void GiveAttackCount(Skill_attack_count skill, int count)
	{
		if (skill.IsAddAttackCount())
		{
			attackCountinfo.Add(new AddAttackCountInfo(skill, count));
		}
		else if (skill.IsSetAttackCount())
		{
			attackCountinfo.Add(new SetAttackCountInfo(skill, count));
		}
	}

	public void DepriveAttackCount(Skill_attack_count skill)
	{
		attackCountinfo.RemoveAll((AttackCountInfo s) => s.Skill == skill);
	}

	public void ClearAttackCount()
	{
		attackCountinfo.Clear();
	}

	public void CallOnAttachSkill(BattleCardBase card, SkillBase skill)
	{
		if (card.IsClass)
		{
			skill.SetAndAddPublishedActiveSkillCount();
		}
		this.OnAttachSkill.Call(card, skill);
	}

	public Action ForceAttackOff()
	{
		if (_buildInfo.BattleMgr.IsRecovery)
		{
			return delegate
			{
			};
		}
		Func<bool> forceAttackOffFunc = () => false;
		OnCheckCanAttack += forceAttackOffFunc;
		return delegate
		{
			OnCheckCanAttack -= forceAttackOffFunc;
		};
	}

	public bool HasSkillWhenPlay(bool isOnlyNoSelect)
	{
		if (isOnlyNoSelect)
		{
			bool hasChoiceTransform = NormalSkills.Any((SkillBase s) => s.OnWhenChoicePlayStart != 0 && s is Skill_transform);
			if (NormalSkills.Any((SkillBase s) => s.IsWhenPlaySkill))
			{
				return !NormalSkills.Any((SkillBase s) => (s.IsWhenPlaySkill && (s.IsUserSelectType || s.IsBurialRite)) || (!hasChoiceTransform && s.OnWhenChoicePlayStart != 0));
			}
			return false;
		}
		return NormalSkills.Any((SkillBase s) => s.IsWhenPlaySkill);
	}

	public int CalcFixedUseCost(int currentPp, bool skipCondition = false)
	{
		int num = -1;
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(SelfBattlePlayer, OpponentBattlePlayer);
		for (int i = 0; i < Skills.Count(); i++)
		{
			if (Skills.ElementAt(i) is Skill_pp_fixeduse skill_pp_fixeduse && currentPp >= skill_pp_fixeduse._fixedUsePP)
			{
				bool flag = skill_pp_fixeduse.CheckCondition(playerInfoPair, new SkillConditionCheckerOption(), isPrePlay: true);
				bool num2 = flag && num < skill_pp_fixeduse._fixedUsePP;
				bool flag2 = (skipCondition || flag) && skill_pp_fixeduse._fixedUsePP < Cost && currentPp < Cost;
				if (num2 || flag2)
				{
					num = skill_pp_fixeduse._fixedUsePP;
				}
			}
		}
		return num;
	}

	public bool CheckConditionFixedUseCost(bool isPrePlay)
	{
		for (int i = 0; i < Skills.Count(); i++)
		{
			if (Skills.ElementAt(i) is Skill_pp_fixeduse skill_pp_fixeduse && skill_pp_fixeduse.CheckCondition(new BattlePlayerReadOnlyInfoPair(SelfBattlePlayer, OpponentBattlePlayer), new SkillConditionCheckerOption(), isPrePlay))
			{
				return true;
			}
		}
		return false;
	}

	protected bool IsFixedUseEnable(int labelCost)
	{
		if (Skills.Any((SkillBase s) => s is Skill_pp_fixeduse && (s as Skill_pp_fixeduse)._fixedUsePP == labelCost && ((s as Skill_pp_fixeduse)._fixedUsePP < Cost || !(s as Skill_pp_fixeduse).IsAccelerateOrCrystallize)))
		{
			return true;
		}
		return false;
	}

	public bool HasSkillDestroyTribe(CardBasePrm.TribeType tribe)
	{
		if (_normalSkillCollection != null && HasSkillDestroyTribeInner(tribe, _normalSkillCollection))
		{
			return true;
		}
		if (_evolveSkillCollection != null && HasSkillDestroyTribeInner(tribe, _evolveSkillCollection))
		{
			return true;
		}
		return false;
	}

	private bool HasSkillDestroyTribeInner(CardBasePrm.TribeType tribe, SkillCollectionBase skills)
	{
		if (skills == null)
		{
			return false;
		}
		for (int i = 0; i < skills.Count(); i++)
		{
			for (int j = 0; j < skills.ElementAt(i).PreprocessList.Count; j++)
			{
				if (skills.ElementAt(i).PreprocessList[j] is SkillPreprocessDestroyTribe skillPreprocessDestroyTribe && tribe == skillPreprocessDestroyTribe.GetDestroyTribe())
				{
					return true;
				}
			}
		}
		return false;
	}

	private CardParameter CreateClassParameter(bool isPlayer)
	{
		DataMgr dataMgr = _buildInfo.BattleMgr.GameMgr.GetDataMgr();
		return new CardParameter((CardBasePrm.ClanType)(isPlayer ? dataMgr.GetPlayerClassId() : dataMgr.GetEnemyClassId()));
	}

	public void ChangeClassClanParameter()
	{
		DataMgr dataMgr = _buildInfo.BattleMgr.GameMgr.GetDataMgr();
		BaseParameter.ChangeClanParameter((CardBasePrm.ClanType)(IsPlayer ? dataMgr.GetPlayerClassId() : dataMgr.GetEnemyClassId()));
	}

	public void CreateParameter()
	{
		if (CardMaster.IsClass(_buildInfo.CardId))
		{
			BaseParameter = CreateClassParameter(IsPlayer);
		}
		else
		{
			BaseParameter = CardMaster.GetInstanceForBattle().GetCardParameterFromId(_buildInfo.CardId);
		}
	}

	public BattleCardBase(BuildInfo buildInfo)
	{
		_buildInfo = buildInfo;
		IsTokenLoad = false;
		SelfBattlePlayer = buildInfo.SelfBattlePlayer;
		OpponentBattlePlayer = buildInfo.OpponentBattlePlayer;
		SelfBattlePlayerReadOnlyInfo = buildInfo.SelfBattlePlayerReadOnlyInfo;
		IsPlayer = buildInfo.IsPlayer;
		CreateParameter();
		CostModifierList = new List<ICardCostModifier>();
		attackCountinfo = new List<AttackCountInfo>();
		SpellChargeCount = 0;
		ExecutedFixedUseCostIndex = -1;
		DamagedCounter = new ItWasDamagedCounter();
		GetOffCards = new List<BattleCardBase>();
		IsSummonDrunkenness = true;
		IsOnMove = false;
		IsOnDraw = true;
		ResetPlayedCost();
		ResetLastCost();
		PlayedTurn = -1;
		DeathTypeInfo = new DeathTypeInformation();
		BuffInfoList = new List<BuffInfo>();
		SkillActivationList = new List<SkillActivationInfo>();
		_skillActivatedCount = 1;
		ThisTurnSkillActivatedCount = 0;
		InitSkillCollection();
		Skills = _normalSkillCollection;
		IsSkillLost = false;
		IsReanimate = false;
		HasAnySkill = (NormalSkills != null && NormalSkills.Any((SkillBase skill) => !(skill is Skill_none) && !skill.IsAttachedSkill)) || (EvolutionSkills != null && EvolutionSkills.Any((SkillBase skill) => !(skill is Skill_none) && !skill.IsAttachedSkill));
		HasSkillNecromance = (NormalSkills != null && NormalSkills.Any((SkillBase skill) => skill.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessNecromance) && !skill.IsAttachedSkill)) || (EvolutionSkills != null && EvolutionSkills.Any((SkillBase skill) => skill.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessNecromance) && !skill.IsAttachedSkill));
		OnPlay += delegate
		{
			BattleCardView.HideCanPlayEffect();
			return NullVfx.GetInstance();
		};
	}

	public virtual void Setup(bool createNullView = false, bool isRecreate = false)
	{
		BattleCardView = CreateView(CreateViewBuildInfo(_buildInfo), createNullView);
		if (!createNullView)
		{
			foreach (SkillBase normalSkill in NormalSkills)
			{
				normalSkill.SetInductionVoiceIndex();
			}
			foreach (SkillBase evolutionSkill in EvolutionSkills)
			{
				evolutionSkill.SetInductionVoiceIndex();
			}
		}
		if (SkillApplyInformation == null)
		{
			SkillApplyInformation = CreateSkillApplyInformation(this);
			SkillApplyInformation.InitializeInformation();
		}
		if (!isRecreate)
		{
			DrawTurn = -1;
		}
		DestroyedTurn = -1;
		DestroyedBySkillList = new List<DestroyedBySkillInfo>();
		BanishedInfo = new BanishInfo(-1, isSelfTurn: false, BanishInfo.BanishPlace.None);
	}

	protected virtual void InitSkillCollection()
	{
		_normalSkillCollection = CreateSkillCondition(_buildInfo.NormalSkillBuildInfos, _buildInfo.SelfBattlePlayer, _buildInfo.OpponentBattlePlayer, _buildInfo.ResourceMgr);
		_evolveSkillCollection = CreateSkillCondition(_buildInfo.EvolveSkillBuildInfos, _buildInfo.SelfBattlePlayer, _buildInfo.OpponentBattlePlayer, _buildInfo.ResourceMgr);
	}

	public void SetIndex(int setIndex)
	{
		_buildInfo.BattleCardIndex = setIndex;
	}

	public virtual void UpdateSkillCollection()
	{
	}

	protected virtual BattleCardView.BuildInfo CreateViewBuildInfo(BuildInfo baseBuildInfo)
	{
		Func<bool> getIsTouchable = null;
		if (baseBuildInfo.SelfBattlePlayer != null)
		{
			getIsTouchable = baseBuildInfo.SelfBattlePlayer.BattleView.IsTouchable;
		}
		return new BattleCardView.BuildInfo(this, new BattlePlayerReadOnlyInfoPair(SelfBattlePlayer, OpponentBattlePlayer), baseBuildInfo.GameObject, SelfBattlePlayer.BattleCamera, SelfBattlePlayer.BackGround, baseBuildInfo.ResourceMgr, getIsTouchable, () => GetIsMovableOnView(), () => IsOnMove, (int cost) => IsFixedUseEnable(cost), () => !IsActionCard, () => Attackable && IsInplay && IsSelfTurn, () => IsCantAttackClass, GetHandCardFrameEffectType);
	}

	protected virtual IBattleCardView CreateView(BattleCardView.BuildInfo buildInfo, bool IsNullView)
	{
		if (IsNullView)
		{
			return new NullBattleCardView(buildInfo);
		}
		return new BattleCardView(buildInfo);
	}

	protected SkillCollectionBase CreateSkillCondition(IEnumerable<SkillCreator.SkillBuildInfo> buildInfos, BattlePlayerBase selfBattlPlayer, BattlePlayerBase opponentBattlePlayer, IBattleResourceMgr resourceMgr)
	{
		if (buildInfos == null)
		{
			return null;
		}
		SkillCollectionBase skillCollectionBase = CreateSkillCollection();
		List<SkillPreprocessBase> list = null;
		SkillCreator skillCreator = CreateSkillCreator(selfBattlPlayer, opponentBattlePlayer, resourceMgr);
		foreach (SkillCreator.SkillBuildInfo buildInfo in buildInfos)
		{
			SkillBase skillBase = skillCreator.Create(buildInfo, list);
			skillCollectionBase.Add(skillBase);
			if (skillBase.PreprocessList.Any())
			{
				if (skillBase.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessReferencePrevious))
				{
					list.AddRange(skillBase.PreprocessList);
				}
				else
				{
					list = skillBase.PreprocessList.ToList();
				}
			}
		}
		skillCollectionBase.Complete();
		return skillCollectionBase;
	}

	public virtual SkillCreator CreateSkillCreator(BattlePlayerBase selfBattlPlayer, BattlePlayerBase opponentBattlePlayer, IBattleResourceMgr resourceMgr)
	{
		return new SkillCreator(this, selfBattlPlayer, opponentBattlePlayer, resourceMgr);
	}

	protected virtual SkillCollectionBase CreateSkillCollection()
	{
		return new SkillCollectionBase(this);
	}

	public virtual VfxBase TurnStart(SkillProcessor skillProcessor)
	{
		if (IsInplay)
		{
			this.OnTurnStart.Call();
			AttackableCount = MaxAttackableCount;
			IsFirstTurn = false;
			IsSummonDrunkenness = false;
		}
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(SelfBattlePlayer, OpponentBattlePlayer);
		skillProcessor.Register(Skills.CreateTurnStartInfo(skillProcessor, playerInfoPair));
		return Skills.RegisterAndProcessWhenTurnStartImmediateInfo(new BattlePlayerPair(SelfBattlePlayer, OpponentBattlePlayer));
	}

	public virtual VfxBase OpponentTurnStart(SkillProcessor skillProcessor)
	{
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(SelfBattlePlayer, OpponentBattlePlayer);
		skillProcessor.Register(Skills.CreateTurnStartInfo(skillProcessor, playerInfoPair));
		return Skills.RegisterAndProcessWhenTurnStartImmediateInfo(new BattlePlayerPair(SelfBattlePlayer, OpponentBattlePlayer));
	}

	public virtual void Necromance(BattleCardBase necromanceCard, SkillProcessor skillProcessor, int necromanceCount)
	{
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(SelfBattlePlayer, OpponentBattlePlayer);
		skillProcessor.Register(Skills.CreateNecromanceInfo(necromanceCard, skillProcessor, playerInfoPair, necromanceCount));
	}

	public virtual VfxBase TurnEndPostProcess()
	{
		return NullVfx.GetInstance();
	}

	public virtual void TurnEndSkillProcess(SkillProcessor skillProcessor)
	{
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(SelfBattlePlayer, OpponentBattlePlayer);
		skillProcessor.Register(Skills.CreateTurnEndInfo(skillProcessor, playerInfoPair));
		ThisTurnSkillActivatedCount = 0;
	}

	public void CheckPreviousTurnAttacked()
	{
		if (SelfBattlePlayer.IsSelfTurn && IsInplay)
		{
			IsPreviousTurnAttacked = AttackableCount < MaxAttackableCount;
		}
	}

	protected virtual VfxBase StartPlayCard()
	{
		foreach (BattleCardBase handCard in SelfBattlePlayer.HandCardList)
		{
			if (handCard != this)
			{
				handCard.BattleCardView.areArrowsForcedOff = false;
				handCard.BattleCardView.UpdateMovability();
			}
		}
		if (!_buildInfo.BattleMgr.GameMgr.IsNewReplayBattle)
		{
			SelfBattlePlayer.CantPlayChoiceBrave = false;
			SelfBattlePlayer.BattleView.UpdateChoiceBraveButtonPulsateEffectAndSprite();
		}
		BattleCardView.HideHandCardInfo();
		return NullVfx.GetInstance();
	}

	public VfxWith<SkillProcessor.ProcessInfo> PlayCard(SkillProcessor skillProcessor, SkillConditionCheckerOption option, bool isInplayGeneration = false, BattleCardBase originalCard = null)
	{
		SequentialVfxPlayer sequentialVfx = SequentialVfxPlayer.Create();
		IsSummonDrunkenness = true;
		AttackableCount = MaxAttackableCount;
		VfxBase instance = NullVfx.GetInstance();
		if (!IsChoiceBraveSkillCard)
		{
			int useCost = Cost;
			if (CheckConditionFixedUseCost(isPrePlay: true))
			{
				useCost = CalcFixedUseCost(SelfBattlePlayer.Pp);
				List<SkillBase> list = Skills.Where((SkillBase s) => s is Skill_pp_fixeduse && !(s as Skill_pp_fixeduse).IsAccelerateOrCrystallize).ToList();
				SkillBase item = list.SingleOrDefault((SkillBase s) => (s as Skill_pp_fixeduse)._fixedUsePP == useCost);
				ExecutedFixedUseCostIndex = list.IndexOf(item);
			}
			instance = SelfBattlePlayer.UsePp(useCost);
			_playedCost = useCost;
			_lastCost = useCost;
			SelfBattlePlayer.SummonedCards.Add(this);
		}
		else
		{
			instance = SelfBattlePlayer.UseBp(Cost, BaseParameter.IsVariableCost, IsPlayer);
			_playedCost = Cost;
		}
		VfxBase vfxBase = StartPlayCard();
		sequentialVfx.Register(ParallelVfxPlayer.Create(instance, vfxBase));
		sequentialVfx.Register(SetUpInplay());
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(SelfBattlePlayer, OpponentBattlePlayer);
		VfxWith<SkillProcessor.ProcessInfo> vfxWith = (isInplayGeneration ? new VfxWith<SkillProcessor.ProcessInfo>(NullVfx.GetInstance(), null) : Skills.CreateWhenPlayInfo(this, skillProcessor, playerInfoPair, option));
		VfxBase allFuncVfxResults = this.OnPlay.GetAllFuncVfxResults();
		sequentialVfx.Register(allFuncVfxResults);
		sequentialVfx.Register(vfxWith.Vfx);
		SelfBattlePlayer.CallOnPlayCard((originalCard != null) ? originalCard : this, this, IsChoiceBraveSkillCard);
		SelfBattlePlayer.AddRallyCount(SelfBattlePlayer.SummonedCards.Where((BattleCardBase c) => c.IsInplay && c.IsUnit).Count());
		sequentialVfx.Register(InstantVfx.Create(delegate
		{
			if (SelfBattlePlayer.HandCardList.Count <= 0)
			{
				sequentialVfx.Register(SelfBattlePlayer.BattleView.HandUnfocus());
			}
		}));
		sequentialVfx.Register(InstantVfx.Create(delegate
		{
			MotionUtils.SetLayerAll(BattleCardView.CardTemplate.CardNormalTemp.gameObject, 10);
		}));
		return new VfxWith<SkillProcessor.ProcessInfo>(sequentialVfx, vfxWith.Value);
	}

	public virtual VfxWith<SkillProcessor.ProcessInfo> PlayChoiceCard(SkillProcessor skillProcessor, SkillConditionCheckerOption option)
	{
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(SelfBattlePlayer, OpponentBattlePlayer);
		return Skills.CreateWhenChoicePlayInfo(skillProcessor, playerInfoPair, option);
	}

	public VfxBase FinishWhenPlaySkill()
	{
		VfxBase[] allFuncCallResults = this.OnFinishWhenPlaySkill.GetAllFuncCallResults();
		if (allFuncCallResults.IsNotNullOrEmpty())
		{
			return SequentialVfxPlayer.Create(allFuncCallResults);
		}
		return null;
	}

	public virtual VfxBase DestroyInPlay(SkillProcessor skillProcessor, bool useDestroy = true, SkillBase destroyedSkill = null)
	{
		if (IsDead)
		{
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
			if (IsChoiceEvolutionCard)
			{
				sequentialVfxPlayer.Register(InstantVfx.Create(delegate
				{
					UpdateBuildInfoAndSkillCollection(BaseParameter.BaseCardId, BaseParameter.IsFoil, isNotUpdateAtkLife: true);
				}));
			}
			sequentialVfxPlayer.Register(RemoveFromInPlay());
			VfxBase allFuncVfxResults = this.OnDestroy.GetAllFuncVfxResults(this, skillProcessor);
			if (!SelfBattlePlayer.ClassAndInPlayCardList.Contains(this))
			{
				DeathTypeInfo.WhenDestroy = Skills._skillTimingInfo.IsWhenDestroy;
				VfxBase vfx = NullVfx.GetInstance();
				sequentialVfxPlayer.Register(vfx);
			}
			sequentialVfxPlayer.Register(allFuncVfxResults);
			BattlePlayerBase battlePlayerBase = (SelfBattlePlayer.IsSelfTurn ? SelfBattlePlayer : OpponentBattlePlayer);
			DestroyedTurn = battlePlayerBase.Turn;
			IsDestroySelfTurn = battlePlayerBase.IsPlayer;
			return sequentialVfxPlayer;
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DestroyInHand(SkillProcessor skillProcessor)
	{
		BattlePlayerBase battlePlayerBase = (SelfBattlePlayer.IsSelfTurn ? SelfBattlePlayer : OpponentBattlePlayer);
		DestroyedTurn = battlePlayerBase.Turn;
		IsDestroySelfTurn = battlePlayerBase.IsPlayer;
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(StopSpellCharge());
		VfxBase allFuncVfxResults = this.OnDestroy.GetAllFuncVfxResults(this, skillProcessor);
		if (!SelfBattlePlayer.HandCardList.Contains(this))
		{
			VfxBase vfx = NullVfx.GetInstance();
			BattleCardView.HideCanPlayEffect();
			sequentialVfxPlayer.Register(vfx);
		}
		sequentialVfxPlayer.Register(allFuncVfxResults);
		return sequentialVfxPlayer;
	}

	public virtual VfxBase Banish(SkillProcessor skillProcessor, bool isReturn = false)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (IsChoiceEvolutionCard)
		{
			if (isReturn)
			{
				UpdateBuildInfoAndSkillCollection(BaseParameter.BaseCardId, BaseParameter.IsFoil);
				if (!SelfBattlePlayer.BattleMgr.IsRecovery || IsPlayer)
				{
					BattleCardView.CardTemplate.NormalNameLabelTemp.text = BaseParameter.CardName;
					Global.SetRepositionNameLabel(BattleCardView.CardTemplate.NormalNameLabelTemp, BaseParameter.CardName, is2D: false);
				}
			}
			else
			{
				sequentialVfxPlayer.Register(InstantVfx.Create(delegate
				{
					UpdateBuildInfoAndSkillCollection(BaseParameter.BaseCardId, BaseParameter.IsFoil, isNotUpdateAtkLife: true);
				}));
			}
		}
		sequentialVfxPlayer.Register(this.OnBanish.GetAllFuncVfxResults(this, skillProcessor));
		if (!isReturn)
		{
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
		}
		return sequentialVfxPlayer;
	}

	public virtual VfxWithLoading BanishInHand(SkillProcessor skillProcessor)
	{
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create(this.OnBanish.GetAllFuncVfxResults(this, skillProcessor));
		if (!SelfBattlePlayer.HandCardList.Contains(this))
		{
			vfxWithLoadingSequential.RegisterVfxWithLoading(NullVfxWithLoading.GetInstance());
		}
		return vfxWithLoadingSequential;
	}

	public virtual VfxBase BanishInDeck(SkillProcessor skillProcessor)
	{
		return this.OnBanish.GetAllFuncVfxResults(this, skillProcessor);
	}

	public virtual VfxBase GetOn(Transform vehicleCardTrans, IBattleCardView vehicleCardView, SkillProcessor skillProcessor, bool isReturn = false)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(this.OnGetOn.GetAllFuncVfxResults(this, skillProcessor));
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		return sequentialVfxPlayer;
	}

	public virtual VfxBase UniteInPlay(SkillProcessor skillProcessor, SkillBase skill)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(LoseSkill());
		sequentialVfxPlayer.Register(SkillApplyInformation.AllSkillEffectStop());
		sequentialVfxPlayer.Register(RemoveFromInPlay());
		sequentialVfxPlayer.Register(SelfBattlePlayer.UniteCard(this, skillProcessor, skill));
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		return sequentialVfxPlayer;
	}

	public virtual VfxWithLoading FusionMaterialized(SkillProcessor skillProcessor, BattleCardBase fusionCard, bool isFusionMetamorphose)
	{
		fusionCard.SkillApplyInformation.AddFusionIngredientCard(this);
		FusionedTurn = SelfBattlePlayer.Turn;
		DeathTypeInfo.UseFusionIngredient = true;
		DeathTypeInfo.UseFusionMetamorphoseIngredient = isFusionMetamorphose;
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create(this.OnBanish.GetAllFuncVfxResults(this, skillProcessor));
		vfxWithLoadingSequential.RegisterVfxWithLoading(NullVfxWithLoading.GetInstance());
		return vfxWithLoadingSequential;
	}

	public virtual VfxBase Metamorphose(SkillProcessor SkillProcessor)
	{
		return this.OnMetamorphose.GetAllFuncVfxResults(this, SkillProcessor);
	}

	public VfxWithLoadingSequential SkillPlayCard(bool isPlayer, SkillBaseSummon.SUMMON_TYPE summonType, SkillProcessor skillProcessor, SkillBase skill, bool isGetoff = false, bool isReanimate = false)
	{
		_lastCost = Cost;
		VfxBase vfxBase = SelfBattlePlayer.PickCard(this, skill, summonType, isGetoff, isReanimate);
		VfxBase vfxBase2 = SetUpInplay();
		IsReanimate = isReanimate;
		if (summonType == SkillBaseSummon.SUMMON_TYPE.HAND)
		{
			skillProcessor.Register(Skills.CreateWhenHandToNotPlayInfo(skillProcessor, new BattlePlayerReadOnlyInfoPair(SelfBattlePlayer, OpponentBattlePlayer), new SkillConditionCheckerOption()));
		}
		return VfxWithLoadingSequential.Create(vfxBase, vfxBase2);
	}

	public VfxBase StartHandEffect()
	{
		if (SelfBattlePlayer.IsPlayer || _buildInfo.BattleMgr.GameMgr.IsAdminWatch)
		{
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
			if (SpellChargeCount > 0 && HasSpellCharge && IsInHand)
			{
				sequentialVfxPlayer.Register(NullVfx.GetInstance());
			}
			return sequentialVfxPlayer;
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase RecoveryAttackCount()
	{
		AttackableCount = MaxAttackableCount;
		return InstantVfx.Create(delegate
		{
			BattleCardView._inPlayFrameEffect.UpdateCanAttackEffect();
		});
	}

	public virtual VfxBase StopSpellCharge()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase CreateMoveToHandVfx()
	{
		if (IsPlayer)
		{
			return new PlayerDrawCardToHandVfx(this);
		}
		return NullVfx.GetInstance();
	}

	public VfxBase CreateShowLogVfx(float time, SkillBase skill, bool isEvolve, string SkillDescription)
	{
		if (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SHOW_SIDE_LOG))
		{
			BattlePlayerBase.SideLogInfo sideLogInfo = new BattlePlayerBase.SideLogInfo(skill);
			return NullVfx.GetInstance();
		}
		return NullVfx.GetInstance();
	}

	public virtual AttackOpponentResult AttackOpponent(BattleCardBase target, DamageParam damageParam, SkillProcessor skillProcessor, bool IsChallenge)
	{
		return new AttackOpponentResult(NullVfx.GetInstance(), NullVfx.GetInstance(), null);
	}

	public void SetOnMove(bool move)
	{
		IsOnMove = move;
	}

	public void SetOnDraw(bool draw)
	{
		IsOnDraw = draw;
	}

	public IEnumerable<SkillBase> GetSelectTypeSkill(bool isEvolve = false, bool isFusion = false, bool isRegister = false, bool isEvolutionSimpleProcessor = false, bool isChoiceCheck = false)
	{
		SkillCollectionBase skillCollectionBase = (isEvolve ? EvolutionSkills : Skills);
		NetworkBattleManagerBase networkBattleManagerBase = SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase;
		GameMgr ins = _buildInfo.BattleMgr.GameMgr;
		if (!ins.IsAdminWatch && !ins.IsAINetwork && networkBattleManagerBase != null)
		{
			NetworkBattleReceiver.ReceiveData receiveData = networkBattleManagerBase.networkBattleData.GetReceiveData();
			if (Index == receiveData.playCardIndex && !isEvolutionSimpleProcessor && !isChoiceCheck)
			{
				List<int> list = new List<int>();
				for (int i = 0; i < receiveData.OpponentTargetDataList.Count; i++)
				{
					list.AddRange(receiveData.OpponentTargetDataList[i].SelectSkillIndexList);
				}
				list = list.Distinct().ToList();
				if (list.Count > 0)
				{
					List<SkillBase> list2 = new List<SkillBase>();
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j] < skillCollectionBase.Count())
						{
							list2.Add(skillCollectionBase.ElementAt(list[j]));
							continue;
						}
						LocalLog.AccumulateTraceLog("SelectSkillIndex " + list[j] + " out of range. CardId:" + CardId + " &&" + StackTraceUtility.ExtractStackTrace());
					}
					return list2;
				}
			}
		}
		BattlePlayerReadOnlyInfoPair readOnlyInfoPair = new BattlePlayerReadOnlyInfoPair(SelfBattlePlayer, OpponentBattlePlayer);
		BattleCardBase battleCardBase = SelfBattlePlayer.Class;
		bool isActivateFanfare = (!IsUnit || !battleCardBase.SkillApplyInformation.IsCantActivateFanfareUnit) && (!IsField || !battleCardBase.SkillApplyInformation.IsCantActivateFanfareField);
		IEnumerable<SkillBase> selectTypeSkill = skillCollectionBase.GetSelectTypeSkill(isEvolve, isFusion, isActivateFanfare, readOnlyInfoPair);
		if (isRegister && SelfBattlePlayer.BattleMgr is NetworkBattleManagerBase networkBattleManagerBase2)
		{
			for (int k = 0; k < selectTypeSkill.Count(); k++)
			{
				networkBattleManagerBase2.AddRegisterSelectTypeSkillIndexList(skillCollectionBase.IndexOf(selectTypeSkill.ElementAt(k)));
			}
		}
		return selectTypeSkill;
	}

	public virtual VfxBase StartAttack(BattleCardBase underAttackCard, BattlePlayerPair battlePlayerPair)
	{
		return NullVfx.GetInstance();
	}

	public virtual DamageResult ApplyDamage(SkillBase skill, DamageParam damage, bool doesAttackerPossessKiller, bool isReflectedDamage, SkillProcessor skillProcessor, BattleCardBase reflectCard)
	{
		DamagedCounter.AddDamageCount(SelfBattlePlayer.IsSelfTurn);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (this.OnDamageAfter != null)
		{
			sequentialVfxPlayer.Register(this.OnDamageAfter.GetAllFuncVfxResults(skillProcessor));
		}
		if (reflectCard != null && reflectCard.OnReflectionAfter != null)
		{
			sequentialVfxPlayer.Register(reflectCard.OnReflectionAfter.GetAllFuncVfxResults(skillProcessor));
		}
		BattleCardBase battleCardBase = damage.OwnerCard.SelfBattlePlayer.Class;
		if (battleCardBase != damage.OwnerCard && battleCardBase.SkillApplyInformation.AddDamageList.Count() > 0 && battleCardBase.OnGiveDamage != null)
		{
			sequentialVfxPlayer.Register(battleCardBase.OnGiveDamage.GetAllFuncVfxResults(skillProcessor));
		}
		if (damage.OwnerCard.OnGiveDamage != null)
		{
			sequentialVfxPlayer.Register(damage.OwnerCard.OnGiveDamage.GetAllFuncVfxResults(skillProcessor));
		}
		return new DamageResult(sequentialVfxPlayer, 0, 0);
	}

	public virtual HealResult ApplyHealing(HealParam healParam, SkillProcessor skillProcessor)
	{
		return new HealResult(-1, NullVfx.GetInstance());
	}

	protected VfxBase CreateVfxWithCardPlayabilityRefresh(VfxBase originalVfx)
	{
		return SequentialVfxPlayer.Create(originalVfx, InstantVfx.Create(delegate
		{
			SelfBattlePlayer.UpdateHandCardsPlayability();
		}));
	}

	public virtual VfxBase Evolution(bool isSkill, SkillProcessor skillProcessor, SkillConditionCheckerOption option, Func<BattleCardBase, IBattleResourceMgr, EvolveVfxBase> getEvolveVfxFunc = null)
	{
		return NullVfx.GetInstance();
	}

	public void CallOnFusionEvent(List<BattleCardBase> ingredientCards)
	{
		this.OnFusionEvent.Call(ingredientCards);
	}

	public VfxBase Fusion(SkillProcessor skillProcessor, List<BattleCardBase> ingredientCards, bool isFusionMetamorphose)
	{
		if (!isFusionMetamorphose)
		{
			SetActiveSkillCount();
		}
		skillProcessor.Register(Skills.CreateWhenFusionInfo(ingredientCards, skillProcessor, new BattlePlayerPair(SelfBattlePlayer, OpponentBattlePlayer)));
		for (int i = 0; i < ingredientCards.Count; i++)
		{
			skillProcessor.Register(ingredientCards[i].Skills.CreateWhenFusionedInfo(skillProcessor, new BattlePlayerPair(SelfBattlePlayer, OpponentBattlePlayer)));
		}
		BattlePlayerBase battlePlayerBase = (IsSelfTurn ? SelfBattlePlayer : OpponentBattlePlayer);
		BattlePlayerBase obj = (IsSelfTurn ? OpponentBattlePlayer : SelfBattlePlayer);
		battlePlayerBase.StartSkillWhenFusionOther(ingredientCards, skillProcessor);
		obj.StartSkillWhenFusionOther(ingredientCards, skillProcessor);
		return NullVfx.GetInstance();
	}

	public virtual void InitializeParameterOnWhenReturn()
	{
		InitSkillApplyInformationOnWhenReturn();
	}

	protected virtual void InitSkillApplyInformationOnWhenReturn()
	{
		SkillApplyInformation.InitializeInformation(isReturnCard: true);
		SkillApplyInformation.ClearParameterModifier();
		ClearCostModifier();
		TransformInfo = default(TransformInformation);
		int normalIndividualId = NormalIndividualId;
		int evolutionIndividualId = EvolutionIndividualId;
		SkillApplyInformation.AttachedSkillsInfo.Clear();
		_normalSkillCollection.Clear();
		_evolveSkillCollection.Clear();
		SkillCreator.CardSkillsBuildInfo cardSkillsBuildInfo = SkillCreator.CreateBuildInfo(CardMaster.GetInstanceForBattle().GetCardParameterFromId(CardId));
		_buildInfo.NormalSkillBuildInfos = cardSkillsBuildInfo.normalSkillBuildInfos;
		_buildInfo.EvolveSkillBuildInfos = cardSkillsBuildInfo.evolveSkillBuildInfos;
		foreach (SkillBase item in CreateSkillCondition(cardSkillsBuildInfo.normalSkillBuildInfos, SelfBattlePlayer, OpponentBattlePlayer, _buildInfo.ResourceMgr))
		{
			_normalSkillCollection.Add(item);
			item.SetInductionVoiceIndex();
			item.SetIndividualId(normalIndividualId);
		}
		foreach (SkillBase item2 in CreateSkillCondition(cardSkillsBuildInfo.evolveSkillBuildInfos, SelfBattlePlayer, OpponentBattlePlayer, _buildInfo.ResourceMgr))
		{
			_evolveSkillCollection.Add(item2);
			item2.SetInductionVoiceIndex();
			item2.SetIndividualId(evolutionIndividualId);
		}
		Skills = _normalSkillCollection;
		Skills.Complete();
	}

	public virtual VfxBase ReturnCard(SkillProcessor skillProcessor)
	{
		if (IsChoiceEvolutionCard)
		{
			ResetChoiceEvolutionCardBuildInfo();
		}
		InitSkill();
		SpellChargeCount = 0;
		SkillApplyInformation.ForceDepriveChantCount();
		SkillApplyInformation.ForceDepriveBuffLife();
		_skillActivatedCount = 1;
		ThisTurnSkillActivatedCount = 0;
		VfxBase[] allFuncCallResults = this.OnReturnCard.GetAllFuncCallResults(this, skillProcessor);
		ClearCostModifier();
		BattleCardView.InitHandParameter();
		UpdateCostViewStrategy(isForceUpdate: true);
		BattleCardView.UpdateCost(BattleCardView.GetUseCostList(BaseParameter.Cost), isGenerateInHand: false);
		ClearBuffInfo();
		ResetPlayedCost();
		ResetLastCost();
		ExecutedFixedUseCostIndex = -1;
		IsExecutedEarthRite = false;
		IsSkillLost = false;
		IsReanimate = false;
		DamagedCounter.Clear();
		GetOffCards.Clear();
		ResetUpdateBuildInfo();
		return ParallelVfxPlayer.Create(allFuncCallResults);
	}

	public void FlagCardAsDestroyedByKiller()
	{
		IsDestroyedByKiller = true;
		DeathTypeInfo.DestroyedByKiller = true;
	}

	public virtual void ResetFlagCardAsDestroyed()
	{
		IsDestroyedBySkill = false;
		IsDestroyedByKiller = false;
	}

	public virtual void FlagCardAsDestroyedBySkill()
	{
		IsDestroyedBySkill = true;
	}

	public VfxBase CreateMaskCardInPlayVfx()
	{
		return NullVfx.GetInstance();
	}

	public void AddCostModifier(ICardCostModifier modifier, SkillBase skill, bool eventCall = true)
	{
		if (eventCall && skill != null)
		{
			this.OnAddCostState.Call(skill, modifier);
		}
		if (modifier.IsClearBeforeModifier)
		{
			CostModifierList.RemoveAll((ICardCostModifier c) => !c.IsResidentModifier);
		}
		CostModifierList.Add(modifier);
	}

	public void RemoveCostModifier(SkillBase skill, ICardCostModifier modifier)
	{
		this.OnRemoveCostState.Call(skill, modifier);
		CostModifierList.Remove(modifier);
	}

	public void ClearCostModifier()
	{
		CostModifierList.Clear();
	}

	public int CalculateFinalDamageAmount(int damageAmount, bool isSkillDamage = false, bool isSpellDamage = false, ParallelVfxPlayer lifeLowerLimitEffectVfx = null)
	{
		bool flag = !isSkillDamage && !isSpellDamage;
		if (SkillApplyInformation.IsShieldAll || (isSkillDamage && SkillApplyInformation.IsShieldSkill) || (isSpellDamage && SkillApplyInformation.IsShieldSpell) || (flag && SkillApplyInformation.IsShieldAttack))
		{
			return 0;
		}
		damageAmount -= SkillApplyInformation.GetDamageCutAmount(isSkillDamage ? DamageCutInfo.DamageType.SKILL : DamageCutInfo.DamageType.ALL);
		damageAmount = SkillApplyInformation.GetClippingDamage(damageAmount, lifeLowerLimitEffectVfx);
		return Mathf.Max(damageAmount, 0);
	}

	public int HealLife(int healAmount, int turn, bool isSelfTurn)
	{
		int life = Life;
		SkillApplyInformation.HealLife(healAmount, turn, isSelfTurn);
		return Life - life;
	}

	public virtual string GetCardSkillDescription(BattlePlayerBase.SideLogInfo sideLogInfo, bool? isForceGetEvolveText = null)
	{
		return SkillDescription(sideLogInfo);
	}

	public VfxBase LoseSkill(SkillBase loseSkill = null, SkillProcessor skillProcessor = null)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		IsSkillLost = true;
		AttackableCount = ((AttackableCount >= MaxAttackableCount) ? 1 : 0);
		NormalSkills.Clear();
		NormalSkills.InitTimingInfo();
		EvolutionSkills.Clear();
		EvolutionSkills.InitTimingInfo();
		SkillApplyInformation.AttachedSkillsInfo.Clear();
		SkillApplyInformation.AttachedSkillsInfo.AttachedSkills.InitTimingInfo();
		_buildInfo.NormalSkillBuildInfos.Clear();
		_buildInfo.EvolveSkillBuildInfos.Clear();
		parallelVfxPlayer.Register(SkillApplyInformation.AllSkillEffectStop());
		SkillApplyInformation.InitializeInformationWithoutLifeOffenseModifier();
		parallelVfxPlayer.Register(ParallelVfxPlayer.Create(this.OnLoseSkillOneTime.GetAllFuncCallResults(loseSkill, skillProcessor, this)));
		this.OnLoseSkillOneTime = null;
		RemoveBuffInfo((BuffInfo buff) => !(buff.SkillFrom is Skill_powerup) && !(buff.SkillFrom is Skill_power_down));
		parallelVfxPlayer.Register(BattleCardView.InitializeBattleCardIcon(this, Skills));
		if (IsFirstTurn)
		{
			IsSummonDrunkenness = true;
		}
		else
		{
			IsSummonDrunkenness = false;
		}
		bool isSelfTurn = SelfBattlePlayer.IsSelfTurn;
		return SequentialVfxPlayer.Create(parallelVfxPlayer, SkillApplyInformation.AllSkillEffectRestart(), InstantVfx.Create(delegate
		{
			BattleCardView._inPlayFrameEffect.UpdateCanAttackEffect(null, isSelfTurn);
		}));
	}

	public CopySkillInfo CopySkill(BattleCardBase targetCard, string copySkillType, bool isRemain)
	{
		SkillFilterCreator.ContentKeyword skillType = (SkillFilterCreator.ContentKeyword)Enum.Parse(typeof(SkillFilterCreator.ContentKeyword), copySkillType, ignoreCase: true);
		List<SkillBase> copySkills = GetCopySkill(targetCard.Skills, skillType);
		bool isEvolutionSkill = targetCard.EvolutionSkills.Any((SkillBase skill) => skill == copySkills.FirstOrDefault());
		int num = 0;
		List<SkillBase> list = new List<SkillBase>();
		foreach (SkillBase item in copySkills)
		{
			if (!isRemain)
			{
				targetCard.Skills.Remove(item);
				if (!targetCard.IsEvolution)
				{
					List<SkillBase> list2 = new List<SkillBase>();
					foreach (SkillBase evolutionSkill in targetCard.EvolutionSkills)
					{
						if (item.IsSameSkill(evolutionSkill))
						{
							list2.Add(evolutionSkill);
						}
					}
					foreach (SkillBase item2 in list2)
					{
						targetCard.EvolutionSkills.Remove(item2);
					}
				}
			}
			if (item.GetAttachSkill == null)
			{
				num++;
			}
		}
		List<SkillCreator.SkillBuildInfo> copiedSkillBuildInfoList = new List<SkillCreator.SkillBuildInfo>();
		if (num > 0)
		{
			string timing = skillType.ToStringCustom();
			copiedSkillBuildInfoList.AddRange(SettingRobSkillInfo(targetCard, timing, targetCard.IsEvolution, isRemain));
		}
		foreach (SkillBase item3 in _normalSkillCollection)
		{
			if (item3.GetAttachSkill != null)
			{
				list.Add(item3);
			}
		}
		_normalSkillCollection.Clear();
		_evolveSkillCollection.Clear();
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(SkillApplyInformation.AllSkillEffectStop());
		CombineVirtualCardSkill(this);
		AttachedSkillInformation attachedSkillsInfo = targetCard.SkillApplyInformation.AttachedSkillsInfo;
		List<SkillBase> copySkill = GetCopySkill(attachedSkillsInfo.AttachedSkills, skillType);
		List<BuffInfo> list3 = new List<BuffInfo>();
		foreach (SkillBase item4 in copySkill)
		{
			int index = attachedSkillsInfo.AttachedSkills.IndexOf(item4);
			Skill_attach_skill creatorSkill = attachedSkillsInfo.CreatorSkillList[index] as Skill_attach_skill;
			string text = attachedSkillsInfo.OwnerCardNameList[index];
			int num2 = attachedSkillsInfo.OwnerCardIdList[index];
			long duplicateBanNum = attachedSkillsInfo.DuplicateBanNum[index];
			if (isRemain)
			{
				SkillBase attachSkill = targetCard.SkillApplyInformation.CloneAttachSkill(SkillApplyInformation as SkillApplyInformation, creatorSkill);
				SkillBase.BuffInfoContainer buffInfo = creatorSkill.GetBuffInfo(targetCard);
				if (buffInfo != null)
				{
					buffInfo = buffInfo.Clone();
					buffInfo._targetCard = this;
					buffInfo._attachSkill = attachSkill;
					buffInfo._buffInfo = buffInfo._buffInfo.Clone();
					buffInfo._buffInfo.IsCopied = true;
					buffInfo._buffInfo.IsCopiedEvolutionSkill = creatorSkill.SkillPrm.ownerCard.EvolutionSkills.Any((SkillBase s) => s == creatorSkill);
					buffInfo._buffInfo.SetPreviousOwner(creatorSkill.SkillPrm.ownerCard);
					AddBuffInfo(buffInfo._buffInfo);
					creatorSkill.AddBuffInfo(buffInfo);
					list3.Add(buffInfo._buffInfo);
				}
				else
				{
					BuffInfo buffInfo2 = creatorSkill.AddBuffInfoIfNeeded(this);
					buffInfo2.SetPreviousOwner(creatorSkill.SkillPrm.ownerCard);
					buffInfo2.IsCopied = true;
					buffInfo2.IsCopiedEvolutionSkill = creatorSkill.SkillPrm.ownerCard.EvolutionSkills.Any((SkillBase s) => s == creatorSkill);
					buffInfo = new SkillBase.BuffInfoContainer(this, buffInfo2, -1, "", null, 0L);
					buffInfo._attachSkill = attachSkill;
					creatorSkill.AddBuffInfo(buffInfo);
					list3.Add(buffInfo._buffInfo);
				}
			}
			else
			{
				int index2 = attachedSkillsInfo.CreatorSkillIndexList[index];
				item4.SkillPrm.ownerCard = this;
				item4.SkillPrm.selfBattlePlayer = SelfBattlePlayer;
				item4.SkillPrm.opponentBattlePlayer = OpponentBattlePlayer;
				SkillApplyInformation.AttachSkill(item4.SkillPrm.buildInfo, item4.SkillPrm.resourceMgr, text, num2, duplicateBanNum, creatorSkill);
				targetCard.SkillApplyInformation.AttachedSkillsInfo.Remove(item4, text, num2, duplicateBanNum, creatorSkill, index2);
				SkillBase.BuffInfoContainer buffInfoContainer = creatorSkill.PopBuffInfo(targetCard);
				if (buffInfoContainer == null)
				{
					CardParameter baseParameter = creatorSkill.SkillPrm.ownerCard.BaseParameter;
					BuffInfo buffInfo3 = new BuffInfo(baseParameter.BaseCardId, baseParameter.NormalCardId, creatorSkill);
					buffInfoContainer = new SkillBase.BuffInfoContainer(targetCard, buffInfo3, -1, "", null, 0L);
				}
				buffInfoContainer._buffInfo.IsCopied = true;
				buffInfoContainer._buffInfo.IsCopiedEvolutionSkill = creatorSkill.SkillPrm.ownerCard.EvolutionSkills.Any((SkillBase s) => s == creatorSkill);
				buffInfoContainer._buffInfo.SetPreviousOwner(creatorSkill.SkillPrm.ownerCard);
				targetCard.RemoveBuffInfo(buffInfoContainer._buffInfo);
				buffInfoContainer._targetCard = this;
				AddBuffInfo(buffInfoContainer._buffInfo);
				creatorSkill.AddBuffInfo(buffInfoContainer);
				list3.Add(buffInfoContainer._buffInfo);
			}
		}
		Skills.Complete();
		SetActiveSkillCount();
		targetCard.SkillApplyInformation.AttachedSkillsInfo.AttachedSkills.InitTimingInfo();
		SkillApplyInformation.AttachedSkillsInfo.AttachedSkills.InitTimingInfo();
		targetCard.NormalSkills.InitTimingInfo();
		NormalSkills.InitTimingInfo();
		targetCard.EvolutionSkills.InitTimingInfo();
		EvolutionSkills.InitTimingInfo();
		sequentialVfxPlayer.Register(targetCard.BattleCardView.InitializeBattleCardIcon(targetCard, targetCard.Skills));
		sequentialVfxPlayer.Register(BattleCardView.InitializeBattleCardIcon(this, Skills));
		sequentialVfxPlayer.Register(SkillApplyInformation.AllSkillEffectRestart());
		this.OnCopySkillComplete.Call(this);
		SkillBaseCopy copySkill2 = Skills.Where((SkillBase s) => s is SkillBaseCopy && s.OptionValue.GetString(SkillFilterCreator.ContentKeyword.ability, "NONE") == skillType.ToString()).FirstOrDefault() as SkillBaseCopy;
		List<SkillBase> copiedSkillList = Skills.Where((SkillBase s) => copiedSkillBuildInfoList.Contains(s.SkillPrm.buildInfo)).ToList();
		return new CopySkillInfo(sequentialVfxPlayer, isEvolutionSkill, copySkill2, copiedSkillList, list3);
	}

	private List<SkillCreator.SkillBuildInfo> SettingRobSkillInfo(BattleCardBase targetCard, string timing, bool isEvolution, bool isRemain)
	{
		List<SkillCreator.SkillBuildInfo> list = new List<SkillCreator.SkillBuildInfo>();
		bool flag = false;
		List<SkillCreator.SkillBuildInfo> list2 = (isEvolution ? targetCard._buildInfo.EvolveSkillBuildInfos : targetCard._buildInfo.NormalSkillBuildInfos);
		foreach (SkillCreator.SkillBuildInfo skillInfo in list2)
		{
			if (!(skillInfo._timing == timing))
			{
				continue;
			}
			flag = true;
			_buildInfo.NormalSkillBuildInfos.Add(skillInfo);
			if (_buildInfo.EvolveSkillBuildInfos.IsNotNullOrEmpty())
			{
				_buildInfo.EvolveSkillBuildInfos.Add(skillInfo);
			}
			if (skillInfo._previousSkillOwner == null)
			{
				skillInfo._previousSkillOwner = targetCard;
			}
			OnRemoveFromInPlayAfterOneTime += delegate
			{
				if (!IsDead)
				{
					_buildInfo.NormalSkillBuildInfos.Remove(skillInfo);
					if (_buildInfo.EvolveSkillBuildInfos.Contains(skillInfo))
					{
						_buildInfo.EvolveSkillBuildInfos.Remove(skillInfo);
					}
				}
				return NullVfx.GetInstance();
			};
			list.Add(skillInfo);
		}
		if (flag || !isEvolution)
		{
			if (!isRemain)
			{
				list2.RemoveAll((SkillCreator.SkillBuildInfo b) => b._timing == timing);
			}
		}
		else
		{
			list.AddRange(SettingRobSkillInfo(targetCard, timing, isEvolution: false, isRemain));
		}
		return list;
	}

	private List<SkillBase> GetCopySkill(SkillCollectionBase skills, SkillFilterCreator.ContentKeyword skillType)
	{
		List<SkillBase> list = new List<SkillBase>();
		foreach (SkillBase skill in skills)
		{
			switch (skillType)
			{
			case SkillFilterCreator.ContentKeyword.when_destroy:
				if (skill.IsWhenDestroySkill)
				{
					list.Add(skill);
				}
				break;
			case SkillFilterCreator.ContentKeyword.when_attack:
				if (skill.IsBeforAttackSkill)
				{
					list.Add(skill);
				}
				break;
			case SkillFilterCreator.ContentKeyword.when_fight:
				if (skill.IsWhenFightSkill)
				{
					list.Add(skill);
				}
				break;
			}
		}
		return list;
	}

	public virtual VfxBase RemoveFromInPlay()
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if (!IsClass)
		{
			parallelVfxPlayer.Register(InstantVfx.Create(delegate
			{
				BattleCardView.BattleCardIconAnimations.ClearAllSkillIcons();
			}));
			parallelVfxPlayer.Register(InstantVfx.Create(delegate
			{
				BattleCardView.HideAttackFinished();
			}));
		}
		return parallelVfxPlayer;
	}

	public virtual VfxBase RemoveFromInPlayAfter(SkillProcessor skillProcessor, bool isReturn = false)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create(this.OnRemoveFromInPlayAfterOneTime.GetAllFuncCallResults(isReturn, skillProcessor));
		this.OnRemoveFromInPlayAfterOneTime = null;
		ParallelVfxPlayer parallelVfxPlayer2 = ParallelVfxPlayer.Create(this.OnLoseSkillOneTime.GetAllFuncCallResults(null, skillProcessor, this));
		this.OnLoseSkillOneTime = null;
		return ParallelVfxPlayer.Create(parallelVfxPlayer, parallelVfxPlayer2);
	}

	protected virtual ISkillApplyInformation CreateSkillApplyInformation(BattleCardBase card)
	{
		return new SkillApplyInformation(card);
	}

	public void AddBuffInfo(BuffInfo buffInfo)
	{
		BuffInfoList.Add(buffInfo);
	}

	public void InsertBuffInfo(BuffInfo buffInfo, int index)
	{
		if (index >= BuffInfoList.Count)
		{
			index = BuffInfoList.Count;
		}
		BuffInfoList.Insert(index, buffInfo);
	}

	public void RemoveBuffInfo(BuffInfo buffInfo)
	{
		BuffInfoList.Remove(buffInfo);
	}

	public void RemoveBuffInfo(Predicate<BuffInfo> condition)
	{
		BuffInfoList.RemoveAll(condition);
	}

	public void ClearBuffInfo()
	{
		BuffInfoList.Clear();
	}

	public void ShallowCopyBuffInfoList(BattleCardBase originalCard)
	{
		BuffInfoList = originalCard.BuffInfoList;
		this.OnResetCardParameter = originalCard.OnResetCardParameter;
	}

	public VfxBase AfterAddDamage()
	{
		VfxBase allFuncVfxResults = this.OnAfterAddDamage.GetAllFuncVfxResults();
		this.OnAfterAddDamage = null;
		return allFuncVfxResults;
	}

	public virtual VfxBase SetUpInplay()
	{
		ResetCardParameter();
		IsFirstTurn = true;
		SetOnMove(move: false);
		AttackableCount = MaxAttackableCount;
		IsSummonDrunkenness = true;
		IsOnDraw = true;
		SetActiveSkillCount();
		return NullVfx.GetInstance();
	}

	public void SetActiveSkillCount()
	{
		_normalSkillCollection.SetAndAddPublishedActiveSkillsCount();
		_evolveSkillCollection.SetAndAddPublishedActiveSkillsCount();
	}

	public virtual void ResetCardParameter()
	{
		IsDestroyedByKiller = false;
		IsDestroyedBySkill = false;
		DeathTypeInfo.DestroyedByKiller = false;
		DeathTypeInfo.MysteriesDestroy = false;
		DeathTypeInfo.WhenDestroy = false;
		ClearCostModifier();
		this.OnResetCardParameter.Call();
	}

	public void ResetCardParameterInHand()
	{
		IsDestroyedByKiller = false;
		IsDestroyedBySkill = false;
		DeathTypeInfo.DestroyedByKiller = false;
		DeathTypeInfo.MysteriesDestroy = false;
		DeathTypeInfo.WhenDestroy = false;
	}

	public HandCardFrameEffectType GetHandCardFrameEffectType()
	{
		return GetHandCardFrameEffectType(isNewReplayRecord: false);
	}

	public HandCardFrameEffectType GetHandCardFrameEffectType(bool isNewReplayRecord)
	{
		HandCardFrameEffectType handCardFrameEffectType = HandCardFrameEffectType.NONE;
		BattlePlayerReadOnlyInfoPair pair = new BattlePlayerReadOnlyInfoPair(SelfBattlePlayer, OpponentBattlePlayer);
		SkillConditionCheckerOption option = new SkillConditionCheckerOption();
		List<SkillBase> list = new List<SkillBase>();
		list.AddRange(NormalSkills);
		list.AddRange(EvolutionSkills);
		for (int i = 0; i < list.Count(); i++)
		{
			SkillBase skill = list[i];
			if (skill == null)
			{
				continue;
			}
			HandCardFrameEffectType handCardFrameEffectType2 = skill.SkillPrm.buildInfo._handCardFrameEffectType;
			if ((handCardFrameEffectType < handCardFrameEffectType2 || (handCardFrameEffectType == HandCardFrameEffectType.LIGHT_BLUE && skill is Skill_pp_fixeduse)) && skill.VisualCheckCondition(pair, option, isPrePlay: true) && skill.PreprocessList.All((SkillPreprocessBase p) => p.IsRight(pair, option)))
			{
				handCardFrameEffectType = handCardFrameEffectType2;
				if ((_buildInfo.BattleMgr.GameMgr.IsWatchBattle || isNewReplayRecord) && handCardFrameEffectType == HandCardFrameEffectType.YELLOW && (skill.ConditionTargetFilter is SkillTargetDeckFilter || skill.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter c) => c.Text.Contains("deck") && RegisterSkillConditionCheck.IsSkillConditionCheck(skill))))
				{
					handCardFrameEffectType = HandCardFrameEffectType.NONE;
				}
			}
		}
		return handCardFrameEffectType;
	}

	protected string ConvertSkillDescription(string text, BattlePlayerBase.SideLogInfo sideLogInfo, bool isSkipOption, BuffInfo buff, string divergenceId, List<int> skillDescriptionValueList, List<int> replaySkillDescriptionValueList)
	{
		SelfBattlePlayer.SideLogSkill = sideLogInfo;
		OpponentBattlePlayer.SideLogSkill = sideLogInfo;
		bool num = sideLogInfo != null;
		bool isRobBuff = buff != null && buff.IsCopied;
		string text2 = (text.Contains("<<${") ? GetSkillDescriptionVariables(text) : text);
		bool flag = IsInHand || (text2.Contains("{me.inplay.class.count}") && (text2.Contains(SkillFilterCreator.ContentKeyword.hand.ToString()) || text2.Contains(SkillFilterCreator.ContentKeyword.deck.ToString())));
		if (num && !isSkipOption && !IsPlayer && !_buildInfo.BattleMgr.GameMgr.IsAdminWatch && flag)
		{
			isSkipOption = true;
		}
		Action<SkillOptionValue> setupOptionValue = (isSkipOption ? null : ((Action<SkillOptionValue>)delegate(SkillOptionValue optionValue)
		{
			BattleCardBase battleCardBase = ((!isRobBuff) ? this : ((buff.SkillFrom != null) ? buff.SkillFrom.SkillPrm.ownerCard : buff.OwnerCard));
			bool flag2 = isRobBuff && SelfBattlePlayer != battleCardBase.SelfBattlePlayer;
			BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(flag2 ? OpponentBattlePlayer : SelfBattlePlayer, flag2 ? SelfBattlePlayer : OpponentBattlePlayer);
			SkillBase skill = null;
			if (text.Contains(SkillFilterCreator.ContentKeyword.is_individual.ToString()))
			{
				skill = SelfBattlePlayer.Class.Skills.FirstOrDefault((SkillBase s) => s.HasIndividualId && s.IsAttachedSkill && s.GetAttachSkill.SkillPrm.ownerCard.Index == Index);
			}
			SkillCollectionBase.SetupOptionValue(optionValue, playerInfoPair, battleCardBase, skill);
		}));
		string result = ConvertSkillDescriptionText(text, setupOptionValue, IsPlayer, divergenceId, skillDescriptionValueList, replaySkillDescriptionValueList);
		SelfBattlePlayer.SideLogSkill = null;
		OpponentBattlePlayer.SideLogSkill = null;
		return result;
	}

	private string GetSkillDescriptionVariables(string originalText)
	{
		if (originalText == null || originalText == string.Empty)
		{
			return string.Empty;
		}
		string text = string.Empty;
		string text2 = originalText;
		int num = 0;
		while (true)
		{
			num++;
			if (num >= 21)
			{
				Debug.LogError("Maybe infinity loop. OriginalText=" + originalText);
				break;
			}
			string variableNumberText = GetVariableNumberText(text2);
			if (variableNumberText == string.Empty)
			{
				break;
			}
			if (variableNumberText.Contains('?'))
			{
				List<string> list = variableNumberText.Split('?').ToList();
				text2 = text2.Replace("<<" + list[0] + "?" + list[1] + "?" + list[2] + ">>", string.Empty);
				text = text + list[1] + list[2];
			}
		}
		return text;
	}

	private static string GetDefaultValue(string expression)
	{
		if (expression.Contains(SkillFilterCreator.ContentKeyword.union_burst_count.ToStringCustom()))
		{
			return 10.ToString();
		}
		if (expression.Contains(SkillFilterCreator.ContentKeyword.super_skybound_art_count.ToStringCustom()))
		{
			return 15.ToString();
		}
		if (expression.Contains(SkillFilterCreator.ContentKeyword.skybound_art_count.ToStringCustom()))
		{
			return 10.ToString();
		}
		if (expression.Contains(SkillFilterCreator.ContentKeyword.fixed_generic_value.ToStringCustom()))
		{
			return (-1).ToString();
		}
		if (expression.Contains(SkillFilterCreator.ContentKeyword.white_ritual_stack.ToStringCustom()))
		{
			return 1.ToString();
		}
		return "0";
	}

	private static string CreateDefaultOptionValue(string expression)
	{
		if (expression.Contains('{'))
		{
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			int count = 0;
			for (int i = 0; i < expression.Length; i++)
			{
				if (expression[i] == '{')
				{
					flag = true;
					num = i;
				}
				if (expression[i] == '}')
				{
					flag2 = true;
					count = ((i != expression.Length) ? (i - num + 1) : (i - num));
				}
				if (flag && flag2)
				{
					string defaultValue = GetDefaultValue(expression);
					expression = expression.Remove(num, count);
					expression = expression.Insert(num, defaultValue);
					i = 0;
					flag = false;
					flag2 = false;
				}
			}
		}
		return expression;
	}

	public static string ConvertSkillDescriptionText(string originalText, Action<SkillOptionValue> setupOptionValue = null, bool isPlayer = false, string divergenceId = "", List<int> valueList = null, List<int> replaySkillDescriptionValueList = null)
	{
		if (originalText == null || originalText == "")
		{
			return null;
		}
		string text = originalText;
		int num = 0;
		List<string> list = new List<string>();
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		while (true)
		{
			num++;
			if (num >= 21)
			{
				Debug.LogError("Maybe infinity loop. OriginalText=" + originalText);
				break;
			}
			Match match = Regex.Match(text, "<<([^>]+(>[^>])*)+>>");
			string variableNumberText = GetVariableNumberText(text);
			if (variableNumberText == string.Empty)
			{
				break;
			}
			string text2 = variableNumberText;
			bool flag = text2.Contains('?');
			List<string> list2 = null;
			if (flag)
			{
				list2 = text2.Split('?').ToList();
				text2 = list2[0];
			}
			bool flag2 = text2.StartsWith("$", StringComparison.Ordinal);
			if (flag && flag2)
			{
				text2 = text2.Remove(0, 1);
			}
			Action<SkillOptionValue> action = setupOptionValue;
			bool flag3 = action == null;
			if (flag3)
			{
				if (!flag2)
				{
					text2 = CreateDefaultOptionValue(text2);
				}
				action = SetupDefaultOptionValue;
			}
			bool isSkillDescriptionExpressionValueDefault = !flag2;
			if (flag)
			{
				isSkillDescriptionExpressionValueDefault = ((flag3 || true /* headless: IsNewReplayBattle is const-false, guard collapses */ || text2.Contains("(divergence_id=")) ? EvalExpressionAndCondition(isSkillDescriptionExpressionValueDefault, text2, flag2, flag3, divergenceId, action, dictionary) : (replaySkillDescriptionValueList[num - 1] == 1));
				string empty = string.Empty;
				empty = ((!isSkillDescriptionExpressionValueDefault) ? list2[2] : list2[1]);
				valueList?.Add(isSkillDescriptionExpressionValueDefault ? 1 : 0);
				text = text.Replace("<<" + list2[0] + "?" + list2[1] + "?" + list2[2] + ">>", empty);
				continue;
			}
			SkillOptionValue skillOptionValue = new SkillOptionValue("v=" + text2);
			action(skillOptionValue);
			isSkillDescriptionExpressionValueDefault = IsSkillDescriptionExpressionValueDefault(text2, action, dictionary);
			string text3 = "";
			string text4 = "";
			if (isSkillDescriptionExpressionValueDefault)
			{
				Match match2 = Regex.Match(text, "\\[[\\w\\d]+\\]([^[]*<<([^>])+>>[^[]*)\\[-\\]");
				MatchCollection matchCollection = Regex.Matches(text, "\\[[\\w\\d]+\\]([^[]*<<([^>])+>>[^[]*)\\[-\\]|<<([^>]+(>[^>])*)+>>");
				if (match2.Success && matchCollection[0].Value == match2.Value)
				{
					match = match2;
					string value = match.Groups[1].Value;
					Match match3 = Regex.Match(value, "<<([^>]+(>[^>])*)+>>");
					text3 = value.Substring(0, match3.Index);
					text4 = value.Substring(match3.Index + match3.Length);
				}
			}
			string text5 = text.Substring(0, match.Index) + text3;
			string text6 = text4 + text.Substring(match.Index + match.Length);
			if (Regex.Match(text, "{<<([^>])+>>@").Success)
			{
				text = text5 + list.Count + text6;
				int item = ((flag3 || true /* headless: IsNewReplayBattle is const-false, guard collapses */) ? skillOptionValue.GetInt(SkillFilterCreator.ContentKeyword.v) : replaySkillDescriptionValueList[num - 1]);
				list.Add(item.ToString());
				valueList?.Add(item);
			}
			else
			{
				int item2 = ((flag3 || true /* headless: IsNewReplayBattle is const-false, guard collapses */) ? skillOptionValue.GetInt(SkillFilterCreator.ContentKeyword.v) : replaySkillDescriptionValueList[num - 1]);
				text = text5 + item2 + text6;
				valueList?.Add(item2);
			}
		}
		text = Data.SystemText.Convert(text, list.ToArray());
		return text.Replace("\\n", "\n");
	}

	private static bool EvalExpressionAndCondition(bool isSkillDescriptionExpressionValueDefault, string expression, bool isNewExpression, bool isNotBattleScene, string divergenceId, Action<SkillOptionValue> setupOptVal, Dictionary<string, int> evalConditionCache)
	{
		string[] array = expression.Split('|');
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i];
			if (isNewExpression)
			{
				if (isNotBattleScene)
				{
					isSkillDescriptionExpressionValueDefault = false;
				}
				else if (text.StartsWith("(divergence_id="))
				{
					text = text.Substring(1, text.Length - 2);
					string text2 = text.Split('=')[1];
					isSkillDescriptionExpressionValueDefault |= text2 == divergenceId;
				}
				else
				{
					isSkillDescriptionExpressionValueDefault |= EvalNewExpressionAndCondition(text, setupOptVal, evalConditionCache);
				}
			}
			else
			{
				isSkillDescriptionExpressionValueDefault &= EvalOldExpressionAndCondition(text, setupOptVal, evalConditionCache);
			}
		}
		return isSkillDescriptionExpressionValueDefault;
	}

	private static bool EvalNewExpressionAndCondition(string expression, Action<SkillOptionValue> setupOptionValue, Dictionary<string, int> cache)
	{
		bool flag = true;
		string[] array = expression.Split('&');
		for (int i = 0; i < array.Length; i++)
		{
			SkillTextVariableComareFilter skillTextVariableComareFilter = new SkillTextVariableComareFilter(array[i]);
			SkillOptionValue skillOptionValue = new SkillOptionValue("v=" + skillTextVariableComareFilter.Lhs);
			setupOptionValue(skillOptionValue);
			if (cache.ContainsKey(skillTextVariableComareFilter.Lhs))
			{
				skillTextVariableComareFilter.LhsFilteringResult = cache[skillTextVariableComareFilter.Lhs];
			}
			else
			{
				cache[skillTextVariableComareFilter.Lhs] = skillTextVariableComareFilter.FilteringLhs(skillOptionValue);
			}
			if (cache.ContainsKey(skillTextVariableComareFilter.Rhs))
			{
				skillTextVariableComareFilter.RhsFilteringResult = cache[skillTextVariableComareFilter.Rhs];
			}
			else
			{
				cache[skillTextVariableComareFilter.Rhs] = skillTextVariableComareFilter.FilteringRhs(skillOptionValue);
			}
			flag &= skillTextVariableComareFilter.Filtering(skillOptionValue);
		}
		return flag;
	}

	private static bool EvalOldExpressionAndCondition(string expression, Action<SkillOptionValue> setupOptionValue, Dictionary<string, int> cache)
	{
		bool flag = false;
		string[] array = expression.Split('&');
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i];
			bool flag2 = false;
			if (!text.Contains("+1"))
			{
				text += "+1";
				flag2 = true;
			}
			SkillOptionValue obj = new SkillOptionValue("v=" + text);
			setupOptionValue(obj);
			bool flag3 = (flag2 ? (!IsSkillDescriptionExpressionValueDefault(text, setupOptionValue, cache)) : IsSkillDescriptionExpressionValueDefault(text, setupOptionValue, cache));
			flag = flag || flag3;
		}
		return flag;
	}

	private static string GetVariableNumberText(string text)
	{
		_extractedText.Length = 0;
		int num = 0;
		for (int i = 0; i < text.Length - 1; i++)
		{
			if (text[i] == '<' && text[i + 1] == '<')
			{
				if (num > 0)
				{
					_extractedText.Append(text[i]);
					_extractedText.Append(text[i + 1]);
				}
				i++;
				num++;
			}
			else if (text[i] == '>' && text[i + 1] == '>')
			{
				num--;
				if (num <= 0)
				{
					break;
				}
				_extractedText.Append(text[i]);
				_extractedText.Append(text[i + 1]);
				i++;
			}
			else if (num > 0)
			{
				_extractedText.Append(text[i]);
			}
		}
		return _extractedText.ToString();
	}

	private static void SetupDefaultOptionValue(SkillOptionValue optionValue)
	{
		optionValue.SetVariable("PLAY_COUNT", "0");
		optionValue.SetVariable("HAND_COUNT", "0");
		optionValue.SetVariable("HAND_SPACE_COUNT", "0");
		optionValue.SetVariable("CHANT_COUNT", "0");
		optionValue.SetVariable("CHARGE_COUNT", "0");
		optionValue.SetVariable("DROP_COUNT", "0");
		optionValue.SetVariable("RETURN_COUNT", "0");
		optionValue.SetVariable("INPLAY_ME_COUNT", "0");
		optionValue.SetVariable("INPLAY_OP_COUNT", "0");
		optionValue.SetVariable("INPLAY_UNIT_ME_COUNT", "0");
		optionValue.SetVariable("INPLAY_UNIT_OP_COUNT", "0");
		optionValue.SetVariable("CLASS_ME_LIFE", "0");
		optionValue.SetVariable("CLASS_OP_LIFE", "0");
		optionValue.SetVariable("ADD_CHARGE_COUNT", "0");
		optionValue.SetVariable("ADD_ODD_CHARGE_COUNT", "0");
		optionValue.SetVariable("ADD_EVEN_CHARGE_COUNT", "0");
	}

	private static bool IsSkillDescriptionExpressionValueDefault(string expression, Action<SkillOptionValue> setupOptionValue, Dictionary<string, int> cache)
	{
		string expression2 = string.Copy(expression);
		expression2 = CreateDefaultOptionValue(expression2);
		SkillOptionValue skillOptionValue = new SkillOptionValue("v=" + expression2);
		SetupDefaultOptionValue(skillOptionValue);
		int num;
		if (cache.ContainsKey(expression))
		{
			num = cache[expression];
		}
		else
		{
			SkillOptionValue skillOptionValue2 = new SkillOptionValue("v=" + expression);
			setupOptionValue(skillOptionValue2);
			num = (cache[expression] = skillOptionValue2.GetInt(SkillFilterCreator.ContentKeyword.v));
		}
		return skillOptionValue.GetInt(SkillFilterCreator.ContentKeyword.v) == num;
	}

	public virtual VfxBase LoadResource(bool isLogging = false)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		parallelVfxPlayer.Register(BattleCardView.LoadChoiceTransformCardsResources(this));
		parallelVfxPlayer.Register(BattleCardView.LoadResource());
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(parallelVfxPlayer);
		if (isLogging)
		{
			sequentialVfxPlayer.Register(InstantVfx.Create(delegate
			{
				LocalLog.AccumulateLastTraceLog("Loaded" + CardId);
			}));
		}
		return sequentialVfxPlayer;
	}

	public virtual VfxBase UnloadResource()
	{
		return BattleCardView.UnloadResource();
	}

	protected void OnEvolve(bool isSkill)
	{
		this.OnEvolveEvent(isSkill);
	}

	protected VfxBase OnBeforeEvolveEvent(SkillProcessor skillProcessor)
	{
		return this.OnBeforeEvolve.GetAllFuncVfxResults(skillProcessor);
	}

	protected void InitSkill()
	{
		Skills = _normalSkillCollection;
	}

	public virtual VfxBase RecoveryInPlay(int inPlayIndex, bool newReplayMoveTurn = false)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		sequentialVfxPlayer.Register(CreateMaskCardInPlayVfx());
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			BattleCardView.GameObject.transform.localPosition = InPlayCardControl.CalcPosition(SelfBattlePlayer.InPlayCards.Count(), inPlayIndex, IsPlayer);
			BattleCardView.GameObject.SetLayer(10, isSetChildren: true);
			BattleCardView.GameObject.SetActive(value: true);
			BattleCardView.isHiddenFromInPlayView = false;
			SetOnDraw(draw: false);
		}));
		if (!newReplayMoveTurn)
		{
			sequentialVfxPlayer.Register(BattleCardView.BattleCardIconAnimations.Initialize(this, Skills));
		}
		sequentialVfxPlayer.Register(SkillApplyInformation.AllSkillEffectStop());
		sequentialVfxPlayer.Register(SkillApplyInformation.AllSkillEffectRestart());
		return sequentialVfxPlayer;
	}

	public abstract BattleCardBase VirtualClone(BattlePlayerBase selfBattlePlayer, BattlePlayerBase opponentBattlePlayer);

	protected void CopyToVirtualCardBase(BattleCardBase target)
	{
		target.PlayedTurn = PlayedTurn;
		target.DeathTypeInfo = DeathTypeInfo.Clone();
		target.BaseParameter = BaseParameter;
		target.IsTokenLoad = IsTokenLoad;
		target.IsFirstTurn = IsFirstTurn;
		target.IsOnMove = IsOnMove;
		target.SpellChargeCount = SpellChargeCount;
		target.SkillActivatedCount = SkillActivatedCount;
		target.ThisTurnSkillActivatedCount = ThisTurnSkillActivatedCount;
		target.IsSummonDrunkenness = IsSummonDrunkenness;
		target.IsPreviousTurnAttacked = IsPreviousTurnAttacked;
		target.IsDestroyedByKiller = IsDestroyedByKiller;
		target.IsDestroyedBySkill = IsDestroyedBySkill;
		target.AttackableCount = AttackableCount;
		target._playedCost = _playedCost;
		target._lastCost = _lastCost;
		target.IsSkillLost = IsSkillLost;
		target.IsReanimate = IsReanimate;
		target.DamagedCounter = new ItWasDamagedCounter(DamagedCounter.GetDamageCount(selfTurn: true), DamagedCounter.GetDamageCount(selfTurn: false));
		target.GetOffCards = GetOffCards;
		target.TransformInfo = TransformInfo;
		for (int i = 0; i < CostModifierList.Count; i++)
		{
			target.CostModifierList.Add(CostModifierList[i].Clone());
		}
		for (int j = 0; j < attackCountinfo.Count; j++)
		{
			if (attackCountinfo[j].Skill.IsAddAttackCount())
			{
				target.attackCountinfo.Add(new AddAttackCountInfo(attackCountinfo[j].Skill, attackCountinfo[j].Count));
			}
			else if (attackCountinfo[j].Skill.IsSetAttackCount())
			{
				target.attackCountinfo.Add(new SetAttackCountInfo(attackCountinfo[j].Skill, attackCountinfo[j].Count));
			}
		}
		if (IsEvolution && EvolutionSkills.Count() > 0)
		{
			target.Skills = target.EvolutionSkills;
		}
		target.SkillApplyInformation = SkillApplyInformation.Clone(target);
		target._buildInfo = _buildInfo.VirtualClone(SelfBattlePlayer, OpponentBattlePlayer);
		target.HasSkillNecromance = HasSkillNecromance;
		target.Setup();
	}

	public virtual VfxBase CombineVirtualCardSkill(BattleCardBase target)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		IsSkillLost = false;
		foreach (SkillBase item in CreateSkillCondition(target._buildInfo.NormalSkillBuildInfos, SelfBattlePlayer, OpponentBattlePlayer, _buildInfo.ResourceMgr))
		{
			_normalSkillCollection.Add(item);
		}
		foreach (SkillBase item2 in CreateSkillCondition(target._buildInfo.EvolveSkillBuildInfos, SelfBattlePlayer, OpponentBattlePlayer, _buildInfo.ResourceMgr))
		{
			item2.ConditionCheckerList = item2.ConditionCheckerList.Where((ISkillConditionChecker c) => !(c is SkillPreprocessEvolutionEndStop)).ToList();
			item2.PreprocessList = item2.PreprocessList.Where((SkillPreprocessBase c) => !(c is SkillPreprocessEvolutionEndStop)).ToList();
			_evolveSkillCollection.Add(item2);
		}
		Skills = ((IsEvolution && EvolutionSkills.Count() > 0) ? _evolveSkillCollection : _normalSkillCollection);
		SkillApplyInformation.Combine(target.SkillApplyInformation);
		int count = target.BuffInfoList.Count;
		for (int num = 0; num < count; num++)
		{
			BuffInfo buffInfo = target.BuffInfoList[num];
			if (!(buffInfo.SkillFrom is Skill_powerup) && !(buffInfo.SkillFrom is Skill_power_down) && !BuffInfoList.Contains(buffInfo))
			{
				AddBuffInfo(buffInfo);
			}
		}
		Skills.Complete();
		CostModifierList.AddRange(target.CostModifierList);
		attackCountinfo.AddRange(target.attackCountinfo);
		if (!SelfBattlePlayer.BattleMgr.IsVirtualBattle && !SelfBattlePlayer.BattleMgr.IsRecovery)
		{
			parallelVfxPlayer.Register(SequentialVfxPlayer.Create(SkillApplyInformation.AllSkillEffectRestart(), InstantVfx.Create(delegate
			{
				BattleCardView._inPlayFrameEffect.UpdateCanAttackEffect();
			}), BattleCardView.BattleCardIconAnimations.Initialize(this, Skills)));
		}
		return parallelVfxPlayer;
	}

	public void SetSpellChargeCount(int num)
	{
		if (num != -1)
		{
			SpellChargeCount = num;
		}
	}

	public void AddSpellChargeCount(int num)
	{
		SpellChargeCount += num;
	}

	public VfxBase GetSpellChargeLoopEffect(int num)
	{
		return NullVfx.GetInstance();
	}

	public BattleCardBase GetDamageReflectionTarget(bool isSkillDamage)
	{
		if (SkillApplyInformation.IsReflectionClass && (isSkillDamage || SkillApplyInformation.ReflectionInfoList.Any((ReflectionInfo b) => b.Type == ReflectionInfo.DamageType.ALL)))
		{
			return OpponentBattlePlayer.Class;
		}
		return this;
	}

	public VfxBase CalcHandCost(bool playEffect = true, bool isOnlyFixedUseCost = false)
	{
		if (SelfBattlePlayer.BattleMgr.IsRecovery)
		{
			return NullVfx.GetInstance();
		}
		if (IsInHand)
		{
			List<int> costList = BattleCardView.GetUseCostList(Cost);
			return InstantVfx.Create(delegate
			{
				BattleCardView.UpdateCost(costList, isGenerateInHand: true, playEffect, isForceUpdate: false, isOnlyFixedUseCost);
			});
		}
		return NullVfx.GetInstance();
	}

	public void UpdateCostViewStrategy(bool isForceUpdate = false)
	{
		BattleCardView.UpdateCostViewStrategy(isForceUpdate);
	}

	public IEnumerable<BattleCardBase> AsIEnumerable()
	{
		yield return this;
	}

	public bool IsHoverActionCard()
	{
		if (IsClass)
		{
			return false;
		}
		if (IsOnMove)
		{
			return false;
		}
		if (IsOnDraw)
		{
			return false;
		}
		if (!_buildInfo.BattleMgr.GameMgr.IsAdmin && !IsPlayer && IsInHand)
		{
			return false;
		}
		return true;
	}

	public List<SkillBase> GetSelectSkillsNoDuplication(List<SkillBase> skills)
	{
		List<SkillBase> list = new List<SkillBase>();
		BattlePlayerPair playerInfoPair = new BattlePlayerPair(SelfBattlePlayer, OpponentBattlePlayer);
		SkillConditionCheckerOption option = new SkillConditionCheckerOption();
		for (int i = 0; i < skills.Count; i++)
		{
			SkillBase skillBase = skills[i];
			int val = ((skillBase.IsBurialRite && !skillBase.IsUserSelectType) ? 1 : skillBase.GetSelectableCards(playerInfoPair, option, isSkipForceSelect: true).Count());
			int num = Math.Min(skillBase.GetSkillSelectCount(), val);
			for (int j = 0; j < num; j++)
			{
				list.Add(skillBase);
			}
		}
		if (list.Count == 0)
		{
			list.AddRange(skills);
		}
		return list;
	}

	public int GetBurialRiteCount(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool isPrePlay)
	{
		if (_buildInfo.BattleMgr.BattlePlayer.PlayerBattleView._isEvolutionSkillSelect)
		{
			return EvolutionSkills.Count((SkillBase s) => s.CheckConditionWithoutBurialRite(playerInfoPair, option, isPrePlay));
		}
		return Skills.Count((SkillBase s) => s.CheckConditionWithoutBurialRite(playerInfoPair, option, isPrePlay));
	}

	public bool HasInductionSkill()
	{
		for (int i = 0; i < Skills.Count(); i++)
		{
			SkillBase skillBase = Skills.ElementAt(i);
			if (skillBase.IsInductionSkill && skillBase.SkillPrm.buildInfo._icon == "induction")
			{
				return true;
			}
		}
		return false;
	}

	public bool HasInductionNumberSkill()
	{
		for (int i = 0; i < Skills.Count(); i++)
		{
			SkillBase skillBase = Skills.ElementAt(i);
			if (skillBase.IsInductionSkill && skillBase.SkillPrm.buildInfo._icon != "induction" && skillBase.SkillPrm.buildInfo._icon.Contains("induction"))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasStackWhiteRitualAndOtherIconSkill()
	{
		if (HasSkillStackWhiteRitual)
		{
			if (!HasInductionSkill() && !HasInductionNumberSkill())
			{
				return HasSkillWhenDestroy;
			}
			return true;
		}
		return false;
	}
}
